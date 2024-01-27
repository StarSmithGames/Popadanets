using Game.Entities.Character.Controller;
using Game.Managers.CreateManager;
using UnityEngine;
using Zenject;

namespace Game.Entities.Character
{
    public sealed class CharacterView : MonoBehaviour
    {
        public Animator Animator;
        public CharacterController Controller;
        
        public CharacterController3D.Settings ControllerSettings;

        private CharacterViewModel _viewModel;

        [ Inject ] private CreateManager _createManager;
        
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