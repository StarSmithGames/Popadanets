using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game.Entities.Character
{
	public sealed class CeilingDetector
	{
		private bool ceilingWasHit = false;
		private float debugDrawDuration = 2.0f;

		private Settings _settings;
		private Transform _transform;
		private CancellationTokenSource _cancellation;

		public CeilingDetector(
			Settings settings,
			Transform transform
			)
		{
			_settings = settings ?? throw new ArgumentNullException( nameof(settings) );
			_transform = transform ?? throw new ArgumentNullException( nameof(transform) );

			_cancellation = new();

			_transform.OnCollisionEnterAsObservable().Subscribe( CollisionEnterHandler ).AddTo( _cancellation.Token );
			_transform.OnCollisionEnterAsObservable().Subscribe( CollisionStayHandler ).AddTo( _cancellation.Token );
		}

		~CeilingDetector()
		{
			_cancellation?.Cancel();
			_cancellation?.Dispose();
			_cancellation = null;
		}
		
		//Return whether ceiling was hit during the last frame;
		public bool HitCeiling()
		{
			return ceilingWasHit;
		}

		//Reset ceiling hit flags;
		public void ResetFlags()
		{
			ceilingWasHit = false;
		}

		//Check if a given collision qualifies as a ceiling hit;
		private void CheckCollisionAngles( Collision _collision )
		{
			float _angle = 0f;

			if ( _settings.ceilingDetectionMethod == CeilingDetectionMethod.OnlyCheckFirstContact )
			{
				//Calculate angle between hit normal and character;
				_angle = Vector3.Angle( -_transform.up, _collision.contacts[ 0 ].normal );

				//If angle is smaller than ceiling angle limit, register ceiling hit;
				if ( _angle < _settings.ceilingAngleLimit )
					ceilingWasHit = true;
#if UNITY_EDITOR
				//Draw debug information;
				if ( _settings.isInDebugMode )
					Debug.DrawRay( _collision.contacts[ 0 ].point, _collision.contacts[ 0 ].normal, Color.red, debugDrawDuration );
#endif
			}

			if ( _settings.ceilingDetectionMethod == CeilingDetectionMethod.CheckAllContacts )
			{
				for ( int i = 0; i < _collision.contacts.Length; i++ )
				{
					//Calculate angle between hit normal and character;
					_angle = Vector3.Angle( -_transform.up, _collision.contacts[ i ].normal );

					//If angle is smaller than ceiling angle limit, register ceiling hit;
					if ( _angle < _settings.ceilingAngleLimit )
						ceilingWasHit = true;

#if UNITY_EDITOR
					//Draw debug information;
					if ( _settings.isInDebugMode )
						Debug.DrawRay( _collision.contacts[ i ].point, _collision.contacts[ i ].normal, Color.red, debugDrawDuration );
#endif
				}
			}

			if ( _settings.ceilingDetectionMethod == CeilingDetectionMethod.CheckAverageOfAllContacts )
			{
				for ( int i = 0; i < _collision.contacts.Length; i++ )
				{
					//Calculate angle between hit normal and character and add it to total angle count;
					_angle += Vector3.Angle( -_transform.up, _collision.contacts[ i ].normal );

#if UNITY_EDITOR
					//Draw debug information;
					if ( _settings.isInDebugMode )
						Debug.DrawRay( _collision.contacts[ i ].point, _collision.contacts[ i ].normal, Color.red, debugDrawDuration );
#endif
				}

				//If average angle is smaller than the ceiling angle limit, register ceiling hit;
				if ( _angle / ( (float)_collision.contacts.Length ) < _settings.ceilingAngleLimit )
					ceilingWasHit = true;
			}
		}

		private void CollisionEnterHandler( Collision _collision )
		{
			CheckCollisionAngles( _collision );
		}

		private void CollisionStayHandler( Collision _collision )
		{
			CheckCollisionAngles( _collision );
		}

		[ System.Serializable ]
		public class Settings
		{
			//Angle limit for ceiling hits;
			public float ceilingAngleLimit = 10f;

			public CeilingDetectionMethod ceilingDetectionMethod;
			//If enabled, draw debug information to show hit positions and hit normals;
			public bool isInDebugMode = false;
		}
	}
}