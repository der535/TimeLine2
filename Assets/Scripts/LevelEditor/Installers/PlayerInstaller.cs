using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace TimeLine.LevelEditor.Core
{
    public class PlayerInstaller : MonoInstaller
    {
        // [SerializeField] private PlayerInputView playerInputView;
        // [SerializeField] private PlayerFreeMoveRigidbodyView playerFreeMoveRigidbodyView;
        // [SerializeField] private GroundCheckController groundCheckController;
        // [SerializeField] private PlayerFreeMoveAnimationView playerFreeMoveAnimationView;

        public override void InstallBindings()
        {
            // Container.Bind<PlayerInputView>().FromInstance(playerInputView);
            // Container.Bind<PlayerFreeMoveRigidbodyView>().FromInstance(playerFreeMoveRigidbodyView).AsSingle();
            // Container.Bind<GroundCheckController>().FromInstance(groundCheckController).AsSingle();
            // Container.Bind<PlayerFreeMoveAnimationView>().FromInstance(playerFreeMoveAnimationView).AsSingle();
            //
            //
            // Container.BindInterfacesAndSelfTo<PlayerFreeMoveController>().AsSingle();
            // Container.BindInterfacesAndSelfTo<PlayerPlatformerController>().AsSingle();
            
            
            
            Container.BindInterfacesAndSelfTo<PlayerHitAnimation>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerHitController>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ResurrectPlayer>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<PlayerComponents>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<TimeLineRestartAnimation>().AsSingle().NonLazy();
        }
    }
}