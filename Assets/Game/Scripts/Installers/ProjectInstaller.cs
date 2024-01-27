using Game.Managers.CreateManager;
using Zenject;

namespace Game
{
    public sealed class ProjectInstaller : MonoInstaller< ProjectInstaller >
    {
        public override void InstallBindings()
        {
            SignalBusInstaller.Install( Container );
            CreateManagerInstaller.Install( Container );
        }
    }
}