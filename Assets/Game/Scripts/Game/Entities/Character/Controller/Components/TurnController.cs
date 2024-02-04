using CMF;
using System;
using UnityEngine;

namespace Game.Entities.Character
{
    public sealed class TurnController
    {
        //Current (local) rotation around the (local) y axis of this gameobject;
        private float _currentYRotation = 0f;
        //If the angle between the current and target direction falls below 'fallOffAngle', 'turnSpeed' becomes progressively slower (and eventually approaches '0f');
        //This adds a smoothing effect to the rotation;
        private float _fallOffAngle = 90f;

        private Settings _settings;
        private IController _controller;
        private Transform _root;
        private Transform _model;
        
        public TurnController(
            Settings settings,
            IController controller,
            Transform root,
            Transform model
            )
        {
            _settings = settings ?? throw new ArgumentNullException( nameof(settings) );
            _controller = controller ?? throw new ArgumentNullException( nameof(controller) );
            _root = root ?? throw new ArgumentNullException( nameof(root) );
            _model = model ?? throw new ArgumentNullException( nameof(model) );
            
            _currentYRotation = _model.localEulerAngles.y;
        }

        public void LateTick()
        {
            Vector3 _velocity = _settings.ignoreControllerMomentum ?  _controller.GetMovementVelocity() :_controller.GetVelocity();

            //Project velocity onto a plane defined by the 'up' direction of the parent transform;
            _velocity = Vector3.ProjectOnPlane(_velocity, _root.up);

            float _magnitudeThreshold = 0.001f;

            //If the velocity's magnitude is smaller than the threshold, return;
            if(_velocity.magnitude < _magnitudeThreshold)
                return;

            //Normalize velocity direction;
            _velocity.Normalize();

            //Get current 'forward' vector;
            Vector3 _currentForward = _model.forward;

            //Calculate (signed) angle between velocity and forward direction;
            float _angleDifference = VectorMath.GetAngle(_currentForward, _velocity, _root.up);

            //Calculate angle factor;
            float _factor = Mathf.InverseLerp(0f, _fallOffAngle, Mathf.Abs(_angleDifference));

            //Calculate this frame's step;
            float _step = Mathf.Sign(_angleDifference) * _factor * Time.deltaTime * _settings.turnSpeed;

            //Clamp step;
            if(_angleDifference < 0f && _step < _angleDifference)
                _step = _angleDifference;
            else if(_angleDifference > 0f && _step > _angleDifference)
                _step = _angleDifference;

            //Add step to current y angle;
            _currentYRotation += _step;

            //Clamp y angle;
            if(_currentYRotation > 360f)
                _currentYRotation -= 360f;
            if(_currentYRotation < -360f)
                _currentYRotation += 360f;

            //Set transform rotation using Quaternion.Euler;
            _model.localRotation = Quaternion.Euler(0f, _currentYRotation, 0f);
        }
        
        [ System.Serializable ]
        public class Settings
        {
            public float turnSpeed = 1500f;
            //Whether the current controller momentum should be ignored when calculating the new direction;
            public bool ignoreControllerMomentum = false;
        }
    }
}