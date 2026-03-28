using TimeLine.LevelEditor.InspectorTab.Components.BoxCollider;
using TimeLine.LevelEditor.Parent.New;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentInstaller;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.EntityComponentSaver;
using TimeLine.LevelEditor.ValueEditor.FieldNodeTest;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Core
{
    public class ECSInstaller : MonoInstaller
    {
        [SerializeField] private Material baseMaterial;
        [SerializeField] private Mesh baseMesh;
        [SerializeField] private ColliderDrawer colliderDrawer;
        [Space]
        [SerializeField] private Material subBurstMaterial;
        public override void InstallBindings()
        {
            Container.Bind<SpriteRendererInstaller>().AsSingle().WithArguments(baseMaterial, baseMesh);
            Container.Bind<SunBurstMaterialInstaller>().AsSingle().WithArguments(subBurstMaterial, baseMesh);
            Container.Bind<BoxColliderInstaller>().AsSingle();
            Container.Bind<CircleColliderInstaller>().AsSingle();
            Container.Bind<PolygoneColliderInstaller>().AsSingle();
            Container.Bind<EntityComponentController>().AsSingle();
            Container.Bind<ColliderDrawer>().FromInstance(colliderDrawer).AsSingle();
            
            
            Container.Bind<SetupAnimationDataResolver>().AsSingle().NonLazy();

            Container.Bind<SaveSpriteRenderer>().AsSingle();
            Container.Bind<SaveBoxCollider>().AsSingle();
            Container.Bind<SaveCircleCollider>().AsSingle();
            Container.Bind<SavePolygonCollider>().AsSingle();
            Container.Bind<SaveCompositionOffset>().AsSingle();
            Container.Bind<SaveTransform>().AsSingle();
        }
    }
}