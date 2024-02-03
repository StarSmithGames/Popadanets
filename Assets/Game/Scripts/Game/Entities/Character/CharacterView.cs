using Game.Entities.Character.Controller;
using Game.Managers.CreateManager;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UniRx;
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

        [Button]
        private void Test()
        {
            var list = new List< Action >();

            for ( var i = 0; i < 5; i++ )
            {
                int count = i;
                list.Add( () => Debug.LogError( count ) );
            }

            foreach ( var func in list )
            {
                func();
            }
        }
    }
}