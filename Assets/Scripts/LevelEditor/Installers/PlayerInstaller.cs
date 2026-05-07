using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using TimeLine.LevelEditor.Player.PlayerMoveNew.PlayerFreeMove;
using TimeLine.LevelEditor.Player.PlayerMoveNew.PlayerFreeMove.View;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Installers
{
    public class PlayerInstaller : MonoInstaller
    {
        [SerializeField] private PlayerInputView playerInputView;
        [SerializeField] private PlayerTrailContoller playerTrailContoller;
        [SerializeField] private PlayerDeath playerDeath;
        [SerializeField] private PlayerMover playerMover;

        public override void InstallBindings()
        {
            Container.Bind<PlayerInputView>().FromInstance(playerInputView);
            Container.Bind<PlayerTrailContoller>().FromInstance(playerTrailContoller);
            Container.Bind<PlayerDeath>().FromInstance(playerDeath);
            Container.Bind<PlayerMover>().FromInstance(playerMover);
            
            Container.BindInterfacesAndSelfTo<PlayerTakeDamageAnimation>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerInvulnerabilityHandler>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ResurrectPlayer>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerComponents>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TimeLineRestartAnimation>().AsSingle().NonLazy();
        }
    }
}