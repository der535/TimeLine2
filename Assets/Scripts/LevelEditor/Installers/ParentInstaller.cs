using TimeLine.LevelEditor.Parent.New;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Core
{
    public class ParentInstaller : MonoInstaller
    {
        [SerializeField] private ParentView parentView;
        public override void InstallBindings()
        {
            Container.Bind<ParentView>().FromInstance(parentView).AsSingle();
            Container.BindInterfacesAndSelfTo<ParentController>().AsSingle().NonLazy();

        }
    }
}