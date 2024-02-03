using CMF;
using UnityEngine;
using UnityEngine.Serialization;

namespace Game.Entities.Character
{
    public sealed class CharacterWalkerController : MonoBehaviour, IController
    {
	    public ColliderSensor colliderSensor;
	    public CeilingDetector ceilingDetector;
	    public Transform targetTransform;

		//Movement speed;
		public float movementSpeed = 7f;

		//How fast the controller can change direction while in the air;
		//Higher values result in more air control;
		public float airControlRate = 2f;

		//Jump speed;
		public float jumpSpeed = 10f;

		//Jump duration variables;
		public float jumpDuration = 0.2f;
		float currentJumpStartTime = 0f;

		//'AirFriction' determines how fast the controller loses its momentum while in the air;
		//'GroundFriction' is used instead, if the controller is grounded;
		public float airFriction = 0.5f;
		public float groundFriction = 100f;

		//Current momentum;
		protected Vector3 momentum = Vector3.zero;

		//Saved velocity from last frame;
		Vector3 savedVelocity = Vector3.zero;

		//Saved horizontal movement velocity from last frame;
		Vector3 savedMovementVelocity = Vector3.zero;

		//Amount of downward gravity;
		public float gravity = 30f;
		[Tooltip("How fast the character will slide down steep slopes.")]
		public float slideGravity = 5f;
		
		//Acceptable slope angle limit;
		public float slopeLimit = 80f;

		[Tooltip("Whether to calculate and apply momentum relative to the controller's transform.")]
		public bool useLocalMomentum = false;

		//Enum describing basic controller states; 
		ControllerState _currentControllerState = ControllerState.Falling;

		[Tooltip("Optional camera transform used for calculating movement direction. If assigned, character movement will take camera view into account.")]
		public Transform cameraTransform;

		private Vector3 _inputMovement;
		//Current upwards (or downwards) velocity necessary to keep the correct distance to the ground;
		private Vector3 _currentGroundAdjustmentVelocity = Vector3.zero;
		
        public void FixedTick( Vector3 input )
        {
	        _inputMovement = input;
			ControllerUpdate();
		}

		//Update controller;
		//This function must be called every fixed update, in order for the controller to work correctly;
		private void ControllerUpdate()
		{
			//Reset ground adjustment velocity;
			_currentGroundAdjustmentVelocity = Vector3.zero;
			//Check if mover is grounded;
			colliderSensor.CheckForGround();
			//Set new ground adjustment velocity for the next frame;
			_currentGroundAdjustmentVelocity = targetTransform.up * ( colliderSensor.GetGroundAdjustmentDistance() / Time.fixedDeltaTime );

			//Determine controller state;
			_currentControllerState = DetermineControllerState();

			//Apply friction and gravity to 'momentum';
			HandleMomentum();

			//Check if the player has initiated a jump;
			HandleJumping();

			//Calculate movement velocity;
			Vector3 _velocity = Vector3.zero;
			if(_currentControllerState == ControllerState.Grounded)
				_velocity = CalculateMovementVelocity();
			
			//If local momentum is used, transform momentum into world space first;
			Vector3 _worldMomentum = momentum;
			if(useLocalMomentum)
				_worldMomentum = targetTransform.localToWorldMatrix * momentum;

			//Add current momentum to velocity;
			_velocity += _worldMomentum;
			
			//If player is grounded or sliding on a slope, extend mover's sensor range;
			//This enables the player to walk up/down stairs and slopes without losing ground contact;
			colliderSensor.SetExtendSensorRange(IsGrounded());

			//Set mover velocity;		
			colliderSensor.rigidbody.velocity = _velocity + _currentGroundAdjustmentVelocity;

			//Store velocity for next frame;
			savedVelocity = _velocity;
		
			//Save controller movement velocity;
			savedMovementVelocity = CalculateMovementVelocity();

			//Reset ceiling detector, if one is attached to this gameobject;
			if(ceilingDetector != null)
				ceilingDetector.ResetFlags();
		}
		
		//Calculate and return movement direction based on player input;
		//This function can be overridden by inheriting scripts to implement different player controls;
		private Vector3 CalculateMovementDirection()
		{
			Vector3 _velocity = Vector3.zero;

			//If no camera transform has been assigned, use the character's transform axes to calculate the movement direction;
			if(cameraTransform == null)
			{
				_velocity += targetTransform.right * _inputMovement.x;
				_velocity += targetTransform.forward * _inputMovement.z;
			}
			else
			{
				//If a camera transform has been assigned, use the assigned transform's axes for movement direction;
				//Project movement direction so movement stays parallel to the ground;
				_velocity += Vector3.ProjectOnPlane(cameraTransform.right, targetTransform.up).normalized * _inputMovement.x;
				_velocity += Vector3.ProjectOnPlane(cameraTransform.forward, targetTransform.up).normalized * _inputMovement.z;
			}

			//If necessary, clamp movement vector to magnitude of 1f;
			if(_velocity.magnitude > 1f)
				_velocity.Normalize();

			return _velocity;
		}

		//Calculate and return movement velocity based on player input, controller state, ground normal [...];
		private Vector3 CalculateMovementVelocity()
		{
			//Calculate (normalized) movement direction;
			Vector3 _velocity = CalculateMovementDirection();

			//Multiply (normalized) velocity with movement speed;
			_velocity *= movementSpeed;

			return _velocity;
		}

		//Determine current controller state based on current momentum and whether the controller is grounded (or not);
		//Handle state transitions;
		private ControllerState DetermineControllerState()
		{
			//Check if vertical momentum is pointing upwards;
			bool _isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), targetTransform.up) > 0f);
			//Check if controller is sliding;
			bool _isSliding = colliderSensor.IsGrounded && IsGroundTooSteep();
			
			//Grounded;
			if(_currentControllerState == ControllerState.Grounded)
			{
				if(_isRising){
					OnGroundContactLost();
					return ControllerState.Rising;
				}
				if(!colliderSensor.IsGrounded){
					OnGroundContactLost();
					return ControllerState.Falling;
				}
				if(_isSliding){
					OnGroundContactLost();
					return ControllerState.Sliding;
				}
				return ControllerState.Grounded;
			}

			//Falling;
			if(_currentControllerState == ControllerState.Falling)
			{
				if(_isRising){
					return ControllerState.Rising;
				}
				if(colliderSensor.IsGrounded && !_isSliding){
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}
				if(_isSliding){
					return ControllerState.Sliding;
				}
				return ControllerState.Falling;
			}
			
			//Sliding;
			if(_currentControllerState == ControllerState.Sliding)
			{	
				if(_isRising){
					OnGroundContactLost();
					return ControllerState.Rising;
				}
				if(!colliderSensor.IsGrounded){
					OnGroundContactLost();
					return ControllerState.Falling;
				}
				if(colliderSensor.IsGrounded && !_isSliding){
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}
				return ControllerState.Sliding;
			}

			//Rising;
			if(_currentControllerState == ControllerState.Rising)
			{
				if(!_isRising){
					if(colliderSensor.IsGrounded && !_isSliding){
						OnGroundContactRegained();
						return ControllerState.Grounded;
					}
					if(_isSliding){
						return ControllerState.Sliding;
					}
					if(!colliderSensor.IsGrounded){
						return ControllerState.Falling;
					}
				}

				//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
				if(ceilingDetector != null)
				{
					if(ceilingDetector.HitCeiling())
					{
						OnCeilingContact();
						return ControllerState.Falling;
					}
				}
				return ControllerState.Rising;
			}

			//Jumping;
			if(_currentControllerState == ControllerState.Jumping)
			{
				//Check for jump timeout;
				if((Time.time - currentJumpStartTime) > jumpDuration)
					return ControllerState.Rising;

				//Check if jump key was let go;
				// if(jumpKeyWasLetGo)
				// 	return ControllerState.Rising;

				//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
				if(ceilingDetector != null)
				{
					if(ceilingDetector.HitCeiling())
					{
						OnCeilingContact();
						return ControllerState.Falling;
					}
				}
				return ControllerState.Jumping;
			}
			
			return ControllerState.Falling;
		}

        //Check if player has initiated a jump;
        private void HandleJumping()
        {
            // if (_currentControllerState == ControllerState.Grounded)
            // {
            //     if ((jumpKeyIsPressed == true || jumpKeyWasPressed) && !jumpInputIsLocked)
            //     {
            //         //Call events;
            //         OnGroundContactLost();
            //         OnJumpStart();
            //
            //         _currentControllerState = ControllerState.Jumping;
            //     }
            // }
        }

        //Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
		//Handle movement in the air;
        //Handle sliding down steep slopes;
        private void HandleMomentum()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = targetTransform.localToWorldMatrix * momentum;

			Vector3 _verticalMomentum = Vector3.zero;
			Vector3 _horizontalMomentum = Vector3.zero;

			//Split momentum into vertical and horizontal components;
			if(momentum != Vector3.zero)
			{
				_verticalMomentum = VectorMath.ExtractDotVector(momentum, targetTransform.up);
				_horizontalMomentum = momentum - _verticalMomentum;
			}

			//Add gravity to vertical momentum;
			_verticalMomentum -= targetTransform.up * gravity * Time.deltaTime;

			//Remove any downward force if the controller is grounded;
			if(_currentControllerState == ControllerState.Grounded && VectorMath.GetDotProduct(_verticalMomentum, targetTransform.up) < 0f)
				_verticalMomentum = Vector3.zero;

			//Manipulate momentum to steer controller in the air (if controller is not grounded or sliding);
			if(!IsGrounded())
			{
				Vector3 _movementVelocity = CalculateMovementVelocity();

				//If controller has received additional momentum from somewhere else;
				if(_horizontalMomentum.magnitude > movementSpeed)
				{
					//Prevent unwanted accumulation of speed in the direction of the current momentum;
					if(VectorMath.GetDotProduct(_movementVelocity, _horizontalMomentum.normalized) > 0f)
						_movementVelocity = VectorMath.RemoveDotVector(_movementVelocity, _horizontalMomentum.normalized);
					
					//Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller;
					float _airControlMultiplier = 0.25f;
					_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate * _airControlMultiplier;
				}
				//If controller has not received additional momentum;
				else
				{
					//Clamp _horizontal velocity to prevent accumulation of speed;
					_horizontalMomentum += _movementVelocity * Time.deltaTime * airControlRate;
					_horizontalMomentum = Vector3.ClampMagnitude(_horizontalMomentum, movementSpeed);
				}
			}

			//Steer controller on slopes;
			if(_currentControllerState == ControllerState.Sliding)
			{
				//Calculate vector pointing away from slope;
				Vector3 _pointDownVector = Vector3.ProjectOnPlane(colliderSensor.Sensor.GetNormal(), targetTransform.up).normalized;

				//Calculate movement velocity;
				Vector3 _slopeMovementVelocity = CalculateMovementVelocity();
				//Remove all velocity that is pointing up the slope;
				_slopeMovementVelocity = VectorMath.RemoveDotVector(_slopeMovementVelocity, _pointDownVector);

				//Add movement velocity to momentum;
				_horizontalMomentum += _slopeMovementVelocity * Time.fixedDeltaTime;
			}

			//Apply friction to horizontal momentum based on whether the controller is grounded;
			if(_currentControllerState == ControllerState.Grounded)
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, groundFriction, Time.deltaTime, Vector3.zero);
			else
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, airFriction, Time.deltaTime, Vector3.zero); 

			//Add horizontal and vertical momentum back together;
			momentum = _horizontalMomentum + _verticalMomentum;

			//Additional momentum calculations for sliding;
			if(_currentControllerState == ControllerState.Sliding)
			{
				//Project the current momentum onto the current ground normal if the controller is sliding down a slope;
				momentum = Vector3.ProjectOnPlane(momentum, colliderSensor.Sensor.GetNormal());

				//Remove any upwards momentum when sliding;
				if(VectorMath.GetDotProduct(momentum, targetTransform.up) > 0f)
					momentum = VectorMath.RemoveDotVector(momentum, targetTransform.up);

				//Apply additional slide gravity;
				Vector3 _slideDirection = Vector3.ProjectOnPlane(-targetTransform.up, colliderSensor.Sensor.GetNormal()).normalized;
				momentum += _slideDirection * slideGravity * Time.deltaTime;
			}
			
			//If controller is jumping, override vertical velocity with jumpSpeed;
			if(_currentControllerState == ControllerState.Jumping)
			{
				momentum = VectorMath.RemoveDotVector(momentum, targetTransform.up);
				momentum += targetTransform.up * jumpSpeed;
			}

			if(useLocalMomentum)
				momentum = targetTransform.worldToLocalMatrix * momentum;
		}

		//Events;

		//This function is called when the player has initiated a jump;
		void OnJumpStart()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = targetTransform.localToWorldMatrix * momentum;

			//Add jump force to momentum;
			momentum += targetTransform.up * jumpSpeed;

			//Set jump start time;
			currentJumpStartTime = Time.time;

            //Lock jump input until jump key is released again;
    //         jumpInputIsLocked = true;
    //
    //         //Call event;
    //         if (OnJump != null)
				// OnJump(momentum);

			if(useLocalMomentum)
				momentum = targetTransform.worldToLocalMatrix * momentum;
		}

		//This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
		void OnGroundContactLost()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = targetTransform.localToWorldMatrix * momentum;

			//Get current movement velocity;
			Vector3 _velocity = GetMovementVelocity();

			//Check if the controller has both momentum and a current movement velocity;
			if(_velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
			{
				//Project momentum onto movement direction;
				Vector3 _projectedMomentum = Vector3.Project(momentum, _velocity.normalized);
				//Calculate dot product to determine whether momentum and movement are aligned;
				float _dot = VectorMath.GetDotProduct(_projectedMomentum.normalized, _velocity.normalized);

				//If current momentum is already pointing in the same direction as movement velocity,
				//Don't add further momentum (or limit movement velocity) to prevent unwanted speed accumulation;
				if(_projectedMomentum.sqrMagnitude >= _velocity.sqrMagnitude && _dot > 0f)
					_velocity = Vector3.zero;
				else if(_dot > 0f)
					_velocity -= _projectedMomentum;	
			}

			//Add movement velocity to momentum;
			momentum += _velocity;

			if(useLocalMomentum)
				momentum = targetTransform.worldToLocalMatrix * momentum;
		}

		//This function is called when the controller has landed on a surface after being in the air;
		void OnGroundContactRegained()
		{
			//Call 'OnLand' event;
			// if(OnLand != null)
			// {
			// 	Vector3 _collisionVelocity = momentum;
			// 	//If local momentum is used, transform momentum into world coordinates first;
			// 	if(useLocalMomentum)
			// 		_collisionVelocity = targetTransform.localToWorldMatrix * _collisionVelocity;
			//
			// 	OnLand(_collisionVelocity);
			// }
			// 	
		}

		//This function is called when the controller has collided with a ceiling while jumping or moving upwards;
		void OnCeilingContact()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(useLocalMomentum)
				momentum = targetTransform.localToWorldMatrix * momentum;

			//Remove all vertical parts of momentum;
			momentum = VectorMath.RemoveDotVector(momentum, targetTransform.up);

			if(useLocalMomentum)
				momentum = targetTransform.worldToLocalMatrix * momentum;
		}

		//Helper functions;

		//Returns 'true' if vertical momentum is above a small threshold;
		private bool IsRisingOrFalling()
		{
			//Calculate current vertical momentum;
			Vector3 _verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), targetTransform.up);

			//Setup threshold to check against;
			//For most applications, a value of '0.001f' is recommended;
			float _limit = 0.001f;

			//Return true if vertical momentum is above '_limit';
			return(_verticalMomentum.magnitude > _limit);
		}

		//Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
		private bool IsGroundTooSteep()
		{
			if(!colliderSensor.IsGrounded)
				return true;

			return (Vector3.Angle(colliderSensor.Sensor.GetNormal(), targetTransform.up) > slopeLimit);
		}

		//Getters;

		//Get last frame's velocity;
		public Vector3 GetVelocity ()
		{
			return savedVelocity;
		}

		//Get last frame's movement velocity (momentum is ignored);
		public Vector3 GetMovementVelocity()
		{
			return savedMovementVelocity;
		}

		//Get current momentum;
		public Vector3 GetMomentum()
		{
			Vector3 _worldMomentum = momentum;
			if(useLocalMomentum)
				_worldMomentum = targetTransform.localToWorldMatrix * momentum;

			return _worldMomentum;
		}

		//Returns 'true' if controller is grounded (or sliding down a slope);
		public bool IsGrounded()
		{
			return(_currentControllerState == ControllerState.Grounded || _currentControllerState == ControllerState.Sliding);
		}

		//Returns 'true' if controller is sliding;
		public bool IsSliding()
		{
			return(_currentControllerState == ControllerState.Sliding);
		}

		//Add momentum to controller;
		public void AddMomentum (Vector3 _momentum)
		{
			if(useLocalMomentum)
				momentum = targetTransform.localToWorldMatrix * momentum;

			momentum += _momentum;	

			if(useLocalMomentum)
				momentum = targetTransform.worldToLocalMatrix * momentum;
		}

		//Set controller momentum directly;
		public void SetMomentum(Vector3 _newMomentum)
		{
			if(useLocalMomentum)
				momentum = targetTransform.worldToLocalMatrix * _newMomentum;
			else
				momentum = _newMomentum;
		}
    }
}