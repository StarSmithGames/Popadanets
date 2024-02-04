using Game.Managers.CreateManager;
using UnityEngine;
using Zenject;

namespace Game.Entities.Character
{
    public sealed class CharacterView : MonoBehaviour
    {
        public CharacterAvatar Avatar;
        [Space]
        public Animator Animator;
        [Space]
        public CharacterController3D.Settings ControllerSettings;
        public TurnController.Settings TurnSettings;

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