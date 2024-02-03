using UnityEngine;

namespace Game.Entities.Character
{
    public abstract class ColliderSensor : MonoBehaviour
    {
	    public abstract Collider Collider { get; }
	    public Sensor Sensor { get; private set; }
	    public int CurrentLayer { get; private set; } = -1;
	    public bool IsGrounded { get; private set; }= false;
	    
		[Header("Options :")]
		[Range(0f, 1f)]
		public float stepHeightRatio = 0.25f;
		[Header("Collider Options :")]
		public float colliderHeight = 2f;
		public float colliderThickness = 1f;
		public Vector3 colliderOffset = Vector3.zero;

		[Header("Sensor Options :")]
		public Sensor.CastType sensorType = Sensor.CastType.Raycast;
		public bool isInDebugMode = false;
		[Header("Sensor Array Options :")]
		[Range(1, 5)]
		public int sensorArrayRows = 1;
		[Range(3, 10)]
		public int sensorArrayRayCount = 6;
		public bool sensorArrayRowsAreOffset = false;
		
		[Header("Variables :")]
		public Transform targetTransform;
		public Rigidbody rigidbody;
		
		[HideInInspector] public Vector3[] raycastArrayPreviewPositions;

		protected float _sensorRadiusModifier = 0.8f;
		
		//Sensor range variables;
		private bool _isUsingExtendedSensorRange  = true;
		private float _baseSensorRange = 0f;



		private void Awake()
		{
			//Freeze rigidbody rotation and disable rigidbody gravity;
			rigidbody.freezeRotation = true;
			rigidbody.useGravity = false;

			//Initialize sensor;
			Sensor = new Sensor(targetTransform, Collider);
			RecalculateColliderDimensions();
			RecalibrateSensor();
		}

		private void OnValidate()
		{
			//Recalculate collider dimensions;
			if ( gameObject.activeInHierarchy )
			{
				RecalculateColliderDimensions();
			}

			//Recalculate raycast array preview positions;
			if ( sensorType == Sensor.CastType.RaycastArray )
			{
				raycastArrayPreviewPositions = Sensor.GetRaycastStartPositions(sensorArrayRows, sensorArrayRayCount, sensorArrayRowsAreOffset, 1f);
			}
		}

		private void LateUpdate()
		{
			if ( isInDebugMode )
			{
				Sensor.DrawDebug();
			}
		}

		//Recalculate collider height/width/thickness;
		private void RecalculateColliderDimensions()
		{
			RecalculateCollider();

			//Recalibrate sensor variables to fit new collider dimensions;
			if(Sensor != null)
				RecalibrateSensor();
		}
		
		//Recalibrate sensor variables;
		private void RecalibrateSensor()
		{
			//Set sensor ray origin and direction;
			Sensor.SetCastOrigin( Collider.bounds.center );
			Sensor.SetCastDirection(Sensor.CastDirection.Down);

			//Calculate sensor layermask;
			RecalculateSensorLayerMask();

			//Set sensor cast type;
			Sensor.castType = sensorType;

			//Calculate sensor radius/width;

			//Multiply all sensor lengths with '_safetyDistanceFactor' to compensate for floating point errors;
			float _safetyDistanceFactor = 0.001f;

			//Set sensor radius;
			Sensor.sphereCastRadius = GetColliderRadius( _safetyDistanceFactor ) * targetTransform.localScale.x;

			//Calculate and set sensor length;
			float _length = 0f;
			_length += (colliderHeight * (1f - stepHeightRatio)) * 0.5f;
			_length += colliderHeight * stepHeightRatio;
			_baseSensorRange = _length * (1f + _safetyDistanceFactor) * targetTransform.localScale.x;
			Sensor.castLength = _length * targetTransform.localScale.x;

			//Set sensor array variables;
			Sensor.ArrayRows = sensorArrayRows;
			Sensor.arrayRayCount = sensorArrayRayCount;
			Sensor.offsetArrayRows = sensorArrayRowsAreOffset;
			Sensor.isInDebugMode = isInDebugMode;

			//Set sensor spherecast variables;
			Sensor.calculateRealDistance = true;
			Sensor.calculateRealSurfaceNormal = true;

			//Recalibrate sensor to the new values;
			Sensor.RecalibrateRaycastArrayPositions();
		}
		
		protected abstract void RecalculateCollider();

		protected abstract float GetColliderRadius( float safetyDistanceFactor );

		//Recalculate sensor layermask based on current physics settings;
		private void RecalculateSensorLayerMask()
		{
			int _layerMask = 0;
			int _objectLayer = gameObject.layer;
 
			//Calculate layermask;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(_objectLayer, i)) 
					_layerMask = _layerMask | (1 << i);
			}

			//Make sure that the calculated layermask does not include the 'Ignore Raycast' layer;
			if(_layerMask == (_layerMask | (1 << LayerMask.NameToLayer("Ignore Raycast"))))
			{
				_layerMask ^= (1 << LayerMask.NameToLayer("Ignore Raycast"));
			}
 
			//Set sensor layermask;
            Sensor.layermask = _layerMask;

			//Save current layer;
			CurrentLayer = _objectLayer;
		}

		public float GetGroundAdjustmentDistance()
		{
			float _distance = Sensor.GetDistance();
			float _upperLimit = ((colliderHeight * targetTransform.localScale.x) * (1f - stepHeightRatio)) * 0.5f;
			float _middle = _upperLimit + (colliderHeight * targetTransform.localScale.x) * stepHeightRatio;
			float _distanceToGo = _middle - _distance;

			return _distanceToGo;
		}

		//Check if mover is grounded;
		//Store all relevant collision information for later;
		//Calculate necessary adjustment velocity to keep the correct distance to the ground;
		public void CheckForGround()
		{
			//Check if object layer has been changed since last frame;
			//If so, recalculate sensor layer mask;
			if ( CurrentLayer != gameObject.layer )
			{
				RecalculateSensorLayerMask();
			}

			//Set sensor length;
			Sensor.castLength = _isUsingExtendedSensorRange ? _baseSensorRange + (colliderHeight * targetTransform.localScale.x) * stepHeightRatio : _baseSensorRange;
			Sensor.Cast();

			//If sensor has not detected anything, set flags and return;
			if ( !Sensor.HasDetectedHit() )
			{
				IsGrounded = false;
				return;
			}

			//Set flags for ground detection;
			IsGrounded = true;
		}
		
		#region Sets

		//Set whether sensor range should be extended;
		public void SetExtendSensorRange(bool _isExtended)
		{
			_isUsingExtendedSensorRange = _isExtended;
		}

		//Set height of collider;
		public void SetColliderHeight(float _newColliderHeight)
		{
			if(colliderHeight == _newColliderHeight)
				return;

			colliderHeight = _newColliderHeight;
			RecalculateColliderDimensions();
		}

		//Set thickness/width of collider;
		public void SetColliderThickness(float _newColliderThickness)
		{
			if(colliderThickness == _newColliderThickness)
				return;

			if(_newColliderThickness < 0f)
				_newColliderThickness = 0f;

			colliderThickness = _newColliderThickness;
			RecalculateColliderDimensions();
		}

		//Set acceptable step height;
		public void SetStepHeightRatio(float _newStepHeightRatio)
		{
			_newStepHeightRatio = Mathf.Clamp(_newStepHeightRatio, 0f, 1f);
			stepHeightRatio = _newStepHeightRatio;
			RecalculateColliderDimensions();
		}
		#endregion
    }
}