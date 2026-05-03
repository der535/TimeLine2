using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.Player.PlayerMove.PlayerFreeMove;
using TimeLine.LevelEditor.Player.PlayerMoveNew.PlayerFreeMove.View;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Installers
{
    public class SpriteGalleryInstaller : MonoInstaller
    {
        [SerializeField] private SpriteGallery spriteGallery;

        public override void InstallBindings()
        {
            Container.Bind<SpriteGallery>().FromInstance(spriteGallery);

            // Container.BindInterfacesAndSelfTo<PlayerHitAnimation>().AsSingle().NonLazy();

        }
    }
}