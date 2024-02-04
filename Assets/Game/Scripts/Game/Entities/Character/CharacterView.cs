using Game.Managers.CreateManager;
using UnityEngine;
using Zenject;

namespace Game.Entities.Character
{
    public sealed class CharacterView : MonoBehaviour
    {
        [Tooltip("Optional camera transform used for calculating movement direction. If assigned, character movement will take camera view into account.")]
        public Transform CameraTransform;
        
        public Animator Animator;
        public CharacterController Controller;
        
        public CharacterWalkerController.Settings ControllerSettings;

        private CharacterViewModel _viewModel;

        [ Inject ]
        private CreateManager _createManager;
        
        private void Awake()
        {
            _viewModel = _createManager.Container.Instantiate< CharacterViewModel >();
            _viewModel.Initialize( this );
        }

        private void OnDestroy()
        {
            _viewModel?.Dispose();
        }
    }
}