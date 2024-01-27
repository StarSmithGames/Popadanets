using UnityEngine;
using Zenject;

namespace Game.Managers.InputManager
{
    [ CreateAssetMenu( fileName = "InputManagerInstaller", menuName = "Installers/InputManagerInstaller") ]
    public sealed class InputManagerInstaller : ScriptableObjectInstaller< InputManagerInstaller >
    {
        public InputSettings Settings;
        
        public override void InstallBindings()
        {
            Container.BindInstance( Settings ).WhenInjectedInto< InputManager >();

            Container.BindInterfacesAndSelfTo< InputService >().AsSingle();
            Container.BindInterfacesAndSelfTo< InputManager >().AsSingle().NonLazy();
        }
    }
}