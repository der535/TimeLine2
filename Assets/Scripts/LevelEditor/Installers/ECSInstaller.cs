using TimeLine.LevelEditor.InspectorTab.Components.BoxCollider;
using TimeLine.LevelEditor.LevelEffects;
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
        [SerializeField] private Sprite quadSprite;
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SpriteRendererInstaller>().AsSingle().WithArguments(baseMaterial, baseMesh);
            Container.BindInterfacesAndSelfTo<SunBurstMaterialInstaller>().AsSingle().WithArguments(subBurstMaterial, baseMesh, quadSprite);
            Container.BindInterfacesAndSelfTo<BoxColliderInstaller>().AsSingle();
            Container.BindInterfacesAndSelfTo<CircleColliderInstaller>().AsSingle();
            Container.BindInterfacesAndSelfTo<PolygoneColliderInstaller>().AsSingle();
            Container.BindInterfacesAndSelfTo<ShakeCameraInstaller>().AsSingle();
            Container.BindInterfacesAndSelfTo<EntityComponentController>().AsSingle();
            
            
            Container.Bind<ColliderDrawer>().FromInstance(colliderDrawer).AsSingle();
            
            
            Container.Bind<SetupAnimationDataResolver>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<ShakeCameraController>().AsSingle().NonLazy();

            
            Container.BindInterfacesAndSelfTo<SaveSpriteRenderer>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveBoxCollider>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveCircleCollider>().AsSingle();
            Container.BindInterfacesAndSelfTo<SavePolygonCollider>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveCompositionOffset>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveSunBurstMaterial>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveShakeCamera>().AsSingle();
            Container.BindInterfacesAndSelfTo<SaveTransform>().AsSingle();
        }
    }
}