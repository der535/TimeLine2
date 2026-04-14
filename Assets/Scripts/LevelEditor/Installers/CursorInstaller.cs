using TimeLine.Cursor;
using TimeLine.LevelEditor.Parent.New;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Core
{
    public class CursorInstaller : MonoInstaller
    {
        [SerializeField] private CursorController cursorController;
        public override void InstallBindings()
        {
            Container.Bind<CursorController>().FromInstance(cursorController).AsSingle();
        }
    }
}