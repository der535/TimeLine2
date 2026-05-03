using TimeLine.LevelEditor.EscInput;
using TimeLine.LevelEditor.Player;
using Zenject;

namespace TimeLine.LevelEditor.Installers
{
    public class ServiceInstaller : MonoInstaller
    {

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<EscInputTrigger>().AsSingle().NonLazy();
        }
    }
}