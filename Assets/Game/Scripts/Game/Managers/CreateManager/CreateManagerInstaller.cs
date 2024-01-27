using Zenject;

namespace Game.Managers.CreateManager
{
    public class CreateManagerInstaller : Installer< CreateManagerInstaller >
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo< CreateManager >().AsSingle();
        }
    }
}