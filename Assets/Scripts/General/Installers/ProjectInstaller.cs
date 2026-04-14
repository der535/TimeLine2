using TimeLine.Cursor;
using UnityEngine;
using Zenject;

namespace TimeLine.General.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private FontStorage fontStorage;
        public override void InstallBindings()
        {
            Container.Bind<FontStorage>().FromInstance(fontStorage).AsSingle();
        }
    }
}