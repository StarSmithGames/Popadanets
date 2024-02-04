using CMF;
using System;
using UnityEngine;

namespace Game.Entities.Character
{
    public sealed class CharacterController3D : IController
    {
	    public event Action< Vector3 > OnLanded;
	    
	    //Returns 'true' if controller is grounded (or sliding down a slope);
	    public bool IsGrounded => _currentControllerState == ControllerState.Grounded || _currentControllerState == ControllerState.Sliding;
	    public bool IsSliding => _currentControllerState == ControllerState.Sliding;

		private float currentJumpStartTime = 0f;

		//Saved velocity from last frame;
		private Vector3 _cachedVelocity = Vector3.zero;

		//Saved horizontal movement velocity from last frame;
		private Vector3 _cachedMovementVelocity = Vector3.zero;

		//Enum describing basic controller states; 
		private ControllerState _currentControllerState = ControllerState.Falling;
		
		private Vector3 _movementDirection;
		private Vector3 _momentum = Vector3.zero;

		private CeilingDetector _ceilingDetector;
		
		private Settings _settings;
		private Transform _root;
		
		public CharacterController3D(
			Settings settings,
			Transform root
			)
		{
			_settings = settings ?? throw new ArgumentNullException( nameof(settings) );
			_root = root ?? throw new ArgumentNullException( nameof(root) );

			_ceilingDetector = new( _settings.ceilingSettings, _root );

			_settings.colliderSensor.Initialize();
		}
		
        public void FixedTick( Vector3 movementDirection )
        {
	        _movementDirection = movementDirection;
			ControllerUpdate();
		}
        
		//Update controller;
		//This function must be called every fixed update, in order for the controller to work correctly;
		private void ControllerUpdate()
		{
			//Check if mover is grounded;
			_settings.colliderSensor.CheckForGround();

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
			Vector3 _worldMomentum = _momentum;
			if(_settings.useLocalMomentum)
				_worldMomentum = _root.localToWorldMatrix * _momentum;

			//Add current momentum to velocity;
			_velocity += _worldMomentum;
			
			//If player is grounded or sliding on a slope, extend mover's sensor range;
			//This enables the player to walk up/down stairs and slopes without losing ground contact;
			_settings.colliderSensor.SetExtendSensorRange( IsGrounded );

			//Set mover velocity;		
			_settings.colliderSensor.SetVelocity( _velocity );

			//Store velocity for next frame;
			_cachedVelocity = _velocity;
		
			//Save controller movement velocity;
			_cachedMovementVelocity = CalculateMovementVelocity();

			//Reset ceiling detector, if one is attached to this gameobject;
			if(_ceilingDetector != null)
				_ceilingDetector.ResetFlags();
		}

		//Calculate and return movement velocity based on player input, controller state, ground normal [...];
		private Vector3 CalculateMovementVelocity() => _movementDirection * _settings.movementSpeed;

		//Determine current controller state based on current momentum and whether the controller is grounded (or not);
		//Handle state transitions;
		private ControllerState DetermineControllerState()
		{
			//Check if vertical momentum is pointing upwards;
			bool _isRising = IsRisingOrFalling() && (VectorMath.GetDotProduct(GetMomentum(), _root.up) > 0f);
			//Check if controller is sliding;
			bool _isSliding = _settings.colliderSensor.IsGrounded && IsGroundTooSteep();
			
			//Grounded;
			if(_currentControllerState == ControllerState.Grounded)
			{
				if(_isRising){
					OnGroundContactLost();
					return ControllerState.Rising;
				}
				if(!_settings.colliderSensor.IsGrounded){
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
				if(_settings.colliderSensor.IsGrounded && !_isSliding){
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
				if(!_settings.colliderSensor.IsGrounded){
					OnGroundContactLost();
					return ControllerState.Falling;
				}
				if(_settings.colliderSensor.IsGrounded && !_isSliding){
					OnGroundContactRegained();
					return ControllerState.Grounded;
				}
				return ControllerState.Sliding;
			}

			//Rising;
			if(_currentControllerState == ControllerState.Rising)
			{
				if(!_isRising){
					if(_settings.colliderSensor.IsGrounded && !_isSliding){
						OnGroundContactRegained();
						return ControllerState.Grounded;
					}
					if(_isSliding){
						return ControllerState.Sliding;
					}
					if(!_settings.colliderSensor.IsGrounded){
						return ControllerState.Falling;
					}
				}

				//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
				if(_ceilingDetector != null)
				{
					if(_ceilingDetector.HitCeiling())
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
				if((Time.time - currentJumpStartTime) > _settings.jumpDuration)
					return ControllerState.Rising;

				//If a ceiling detector has been attached to this gameobject, check for ceiling hits;
				if(_ceilingDetector != null)
				{
					if(_ceilingDetector.HitCeiling())
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
        void HandleJumping()
        {
            // if (_currentControllerState == ControllerState.Grounded)
            // {
            //     if ((jumpKeyIsPressed == true || jumpKeyWasPressed) && !jumpInputIsLocked)
            //     {
            //         //Call events;
            //         OnGroundContactLost();
            //         OnJumpStart();
            //
            //         currentControllerState = ControllerState.Jumping;
            //     }
            // }
        }

        //Apply friction to both vertical and horizontal momentum based on 'friction' and 'gravity';
		//Handle movement in the air;
        //Handle sliding down steep slopes;
        private void HandleMomentum()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(_settings.useLocalMomentum)
				_momentum = _root.localToWorldMatrix * _momentum;

			Vector3 _verticalMomentum = Vector3.zero;
			Vector3 _horizontalMomentum = Vector3.zero;

			//Split momentum into vertical and horizontal components;
			if(_momentum != Vector3.zero)
			{
				_verticalMomentum = VectorMath.ExtractDotVector(_momentum, _root.up);
				_horizontalMomentum = _momentum - _verticalMomentum;
			}

			//Add gravity to vertical momentum;
			_verticalMomentum -= _root.up * _settings.gravity * Time.deltaTime;

			//Remove any downward force if the controller is grounded;
			if(_currentControllerState == ControllerState.Grounded && VectorMath.GetDotProduct(_verticalMomentum, _root.up) < 0f)
				_verticalMomentum = Vector3.zero;

			//Manipulate momentum to steer controller in the air (if controller is not grounded or sliding);
			if(!IsGrounded)
			{
				Vector3 _movementVelocity = CalculateMovementVelocity();

				//If controller has received additional momentum from somewhere else;
				if(_horizontalMomentum.magnitude > _settings.movementSpeed)
				{
					//Prevent unwanted accumulation of speed in the direction of the current momentum;
					if(VectorMath.GetDotProduct(_movementVelocity, _horizontalMomentum.normalized) > 0f)
						_movementVelocity = VectorMath.RemoveDotVector(_movementVelocity, _horizontalMomentum.normalized);
					
					//Lower air control slightly with a multiplier to add some 'weight' to any momentum applied to the controller;
					float _airControlMultiplier = 0.25f;
					_horizontalMomentum += _movementVelocity * Time.deltaTime * _settings.airControlRate * _airControlMultiplier;
				}
				//If controller has not received additional momentum;
				else
				{
					//Clamp _horizontal velocity to prevent accumulation of speed;
					_horizontalMomentum += _movementVelocity * Time.deltaTime * _settings.airControlRate;
					_horizontalMomentum = Vector3.ClampMagnitude(_horizontalMomentum, _settings.movementSpeed);
				}
			}

			//Steer controller on slopes;
			if(_currentControllerState == ControllerState.Sliding)
			{
				//Calculate vector pointing away from slope;
				Vector3 _pointDownVector = Vector3.ProjectOnPlane(_settings.colliderSensor.GetGroundNormal(), _root.up).normalized;

				//Calculate movement velocity;
				Vector3 _slopeMovementVelocity = CalculateMovementVelocity();
				//Remove all velocity that is pointing up the slope;
				_slopeMovementVelocity = VectorMath.RemoveDotVector(_slopeMovementVelocity, _pointDownVector);

				//Add movement velocity to momentum;
				_horizontalMomentum += _slopeMovementVelocity * Time.fixedDeltaTime;
			}

			//Apply friction to horizontal momentum based on whether the controller is grounded;
			if(_currentControllerState == ControllerState.Grounded)
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, _settings.groundFriction, Time.deltaTime, Vector3.zero);
			else
				_horizontalMomentum = VectorMath.IncrementVectorTowardTargetVector(_horizontalMomentum, _settings.airFriction, Time.deltaTime, Vector3.zero); 

			//Add horizontal and vertical momentum back together;
			_momentum = _horizontalMomentum + _verticalMomentum;

			//Additional momentum calculations for sliding;
			if(_currentControllerState == ControllerState.Sliding)
			{
				//Project the current momentum onto the current ground normal if the controller is sliding down a slope;
				_momentum = Vector3.ProjectOnPlane(_momentum, _settings.colliderSensor.GetGroundNormal());

				//Remove any upwards momentum when sliding;
				if(VectorMath.GetDotProduct(_momentum, _root.up) > 0f)
					_momentum = VectorMath.RemoveDotVector(_momentum, _root.up);

				//Apply additional slide gravity;
				Vector3 _slideDirection = Vector3.ProjectOnPlane(-_root.up, _settings.colliderSensor.GetGroundNormal()).normalized;
				_momentum += _slideDirection * _settings.slideGravity * Time.deltaTime;
			}
			
			//If controller is jumping, override vertical velocity with jumpSpeed;
			if(_currentControllerState == ControllerState.Jumping)
			{
				_momentum = VectorMath.RemoveDotVector(_momentum, _root.up);
				_momentum += _root.up * _settings.jumpSpeed;
			}

			if(_settings.useLocalMomentum)
				_momentum = _root.worldToLocalMatrix * _momentum;
		}

		//Events;

		//This function is called when the player has initiated a jump;
		private void OnJumpStart()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(_settings.useLocalMomentum)
				_momentum = _root.localToWorldMatrix * _momentum;

			//Add jump force to momentum;
			_momentum += _root.up * _settings.jumpSpeed;

			//Set jump start time;
			currentJumpStartTime = Time.time;

    //         //Lock jump input until jump key is released again;
    //         jumpInputIsLocked = true;
    //
    //         //Call event;
    //         if (OnJump != null)
				// OnJump(momentum);

			if(_settings.useLocalMomentum)
				_momentum = _root.worldToLocalMatrix * _momentum;
		}

		//This function is called when the controller has lost ground contact, i.e. is either falling or rising, or generally in the air;
		private void OnGroundContactLost()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(_settings.useLocalMomentum)
				_momentum = _root.localToWorldMatrix * _momentum;

			//Get current movement velocity;
			Vector3 _velocity = GetMovementVelocity();

			//Check if the controller has both momentum and a current movement velocity;
			if(_velocity.sqrMagnitude >= 0f && _momentum.sqrMagnitude > 0f)
			{
				//Project momentum onto movement direction;
				Vector3 _projectedMomentum = Vector3.Project(_momentum, _velocity.normalized);
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
			_momentum += _velocity;

			if(_settings.useLocalMomentum)
				_momentum = _root.worldToLocalMatrix * _momentum;
		}

		//This function is called when the controller has landed on a surface after being in the air;
		private void OnGroundContactRegained()
		{
			Vector3 _collisionVelocity = _momentum;
			//If local momentum is used, transform momentum into world coordinates first;
			if ( _settings.useLocalMomentum )
				_collisionVelocity = _root.localToWorldMatrix * _collisionVelocity;

			OnLanded?.Invoke( _collisionVelocity );
		}

		//This function is called when the controller has collided with a ceiling while jumping or moving upwards;
		private void OnCeilingContact()
		{
			//If local momentum is used, transform momentum into world coordinates first;
			if(_settings.useLocalMomentum)
				_momentum = _root.localToWorldMatrix * _momentum;

			//Remove all vertical parts of momentum;
			_momentum = VectorMath.RemoveDotVector(_momentum, _root.up);

			if(_settings.useLocalMomentum)
				_momentum = _root.worldToLocalMatrix * _momentum;
		}

		//Helper functions;

		//Returns 'true' if vertical momentum is above a small threshold;
		private bool IsRisingOrFalling()
		{
			//Calculate current vertical momentum;
			Vector3 _verticalMomentum = VectorMath.ExtractDotVector(GetMomentum(), _root.up);

			//Setup threshold to check against;
			//For most applications, a value of '0.001f' is recommended;
			float _limit = 0.001f;

			//Return true if vertical momentum is above '_limit';
			return(_verticalMomentum.magnitude > _limit);
		}

		//Returns true if angle between controller and ground normal is too big (> slope limit), i.e. ground is too steep;
		private bool IsGroundTooSteep()
		{
			if(!_settings.colliderSensor.IsGrounded)
				return true;

			return (Vector3.Angle(_settings.colliderSensor.GetGroundNormal(), _root.up) > _settings.colliderSensor.slopeLimit);
		}

		//Get current momentum;
		public Vector3 GetMomentum()
		{
			Vector3 _worldMomentum = _momentum;
			if(_settings.useLocalMomentum)
				_worldMomentum = _root.localToWorldMatrix * _momentum;

			return _worldMomentum;
		}

		//Add momentum to controller;
		public void AddMomentum (Vector3 _momentum)
		{
			if(_settings.useLocalMomentum)
				_momentum = _root.localToWorldMatrix * _momentum;

			_momentum += _momentum;	

			if(_settings.useLocalMomentum)
				_momentum = _root.worldToLocalMatrix * _momentum;
		}

		//Set controller momentum directly;
		public void SetMomentum(Vector3 _newMomentum)
		{
			if(_settings.useLocalMomentum)
				_momentum = _root.worldToLocalMatrix * _newMomentum;
			else
				_momentum = _newMomentum;
		}
		
		
		//Get last frame's velocity;
		public Vector3 GetVelocity() => _cachedVelocity;

		public Vector3 GetNormalVelocity() => _cachedVelocity / _settings.movementSpeed;
		
		//Get last frame's movement velocity (momentum is ignored);
		public Vector3 GetMovementVelocity() => _cachedMovementVelocity;
		
		[ System.Serializable ]
		public sealed class Settings
		{
			public ColliderSensor colliderSensor;
			public CeilingDetector.Settings ceilingSettings;
			
			//Movement speed;
			public float movementSpeed = 7f;

			//How fast the controller can change direction while in the air;
			//Higher values result in more air control;
			public float airControlRate = 2f;

			//Jump speed;
			public float jumpSpeed = 10f;
			public float jumpDuration = 0.2f;
			
			//'AirFriction' determines how fast the controller loses its momentum while in the air;
			//'GroundFriction' is used instead, if the controller is grounded;
			public float airFriction = 0.5f;
			public float groundFriction = 100f;
			
			//Amount of downward gravity;
			public float gravity = 30f;
			[Tooltip("How fast the character will slide down steep slopes.")]
			public float slideGravity = 5f;
		
			[Tooltip("Whether to calculate and apply momentum relative to the controller's transform.")]
			public bool useLocalMomentum = false;
		}
    }
}