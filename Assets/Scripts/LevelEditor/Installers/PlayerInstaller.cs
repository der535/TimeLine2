using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Installers
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerInputView playerInputView;

        public override void InstallBindings()
        {
            Container.Bind<PlayerInputView>().FromInstance(playerInputView);
            
            Container.BindInterfacesAndSelfTo<PlayerHitAnimation>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerHitController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ResurrectPlayer>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerComponents>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TimeLineRestartAnimation>().AsSingle().NonLazy();
        }
    }
}