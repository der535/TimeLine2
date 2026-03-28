using System.Collections.Generic;
using EventBus;
using NaughtyAttributes;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.outline;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.SceneView.Outline
{
    public class OutlineController : MonoBehaviour
    {
        [SerializeField] private TrackObjectStorage trackObjectStorage;
        [SerializeField] private GameObject outlinePrefab;
        [SerializeField] private float stroke;
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private Color outlineColor;

        private List<Entity> _outlines = new();

        GameEventBus _gameEventBus;
        SelectObjectController _selectObjectController;
        private AddAnEntitySprite _addAnEntitySprite;
        private EntityManager _entityManager;

        [Inject]
        private void Construct(GameEventBus eventBus, SelectObjectController selectObjectController,
             AddAnEntitySprite addAnEntitySprite)
        {
            _gameEventBus = eventBus;
            _selectObjectController = selectObjectController;
            _addAnEntitySprite = addAnEntitySprite;
        }

        private void Start()
        {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            _gameEventBus.SubscribeTo((ref SelectObjectEvent data) =>
            {
                Clear();
                foreach (var track in data.Tracks)
                {
                    DrawOutline(track.entity);
                }
            });
            _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
            {
                Clear();
                foreach (var track in data.SelectedObjects)
                {
                    DrawOutline(track.entity);
                }
            });
            _gameEventBus.SubscribeTo((ref SelectedNewSpriteEvent data) =>
            {
                Clear();
                foreach (var track in _selectObjectController.SelectObjects)
                {
                    DrawOutline(track.entity);
                }
            });
        
            _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => Clear());
        }

        private void DrawOutline(Entity selectedObject)
        {
            if (trackObjectStorage.GetTrackObjectData(selectedObject) is TrackObjectGroup trackObjectGroup)
            {
                CheckGroup(trackObjectGroup);
            }

            CheckSpriteRenderer(selectedObject);
        }

        internal void ReDrawOutline()
        {
            Clear();
            foreach (var track in _selectObjectController.SelectObjects)
            {
                DrawOutline(track.entity);
            }
        }

        private void CheckGroup(TrackObjectGroup trackObjectGroup)
        {
            foreach (var trackObject in trackObjectGroup.TrackObjectDatas)
            {
                if (trackObject is TrackObjectGroup trackObjectGroup2)
                {
                    CheckGroup(trackObjectGroup2);
                }
                else
                {
                    CheckSpriteRenderer(trackObject.entity);
                }
            }
        }

        void CheckSpriteRenderer(Entity selectedObject)
        {
            if (_entityManager.HasComponent<RenderMeshArray>(selectedObject))
            {
                Material currentMat = null;

                RenderMeshArray rma = _entityManager.GetSharedComponentManaged<RenderMeshArray>(selectedObject);  
                if (_entityManager.HasComponent<MaterialMeshInfo>(selectedObject))  
                {  
                    var meshInfo = _entityManager.GetComponentData<MaterialMeshInfo>(selectedObject);  
  
                    // Получаем текущий материал  
                    currentMat = rma.GetMaterial(meshInfo);  
                }
                
                _outlines.Add(CreateChildEntity(selectedObject, currentMat.mainTexture));
            }
        }

        
        public Entity CreateChildEntity(Entity parentEntity, Texture texture)
        {
            // 1. Создаем новую сущность (ребенка)
            Entity childEntity = _entityManager.CreateEntity();

            _addAnEntitySprite.SetupSpriteRender(childEntity, texture, outlineMaterial);

            // 2. Добавляем LocalTransform (локальные координаты относительно родителя)
            _entityManager.AddComponentData(childEntity, LocalTransform.FromPosition(new float3(0, 0, 0)));

            // 3. Устанавливаем родителя
            // Это автоматически добавит компонент Parent
            _entityManager.AddComponentData(childEntity, new Unity.Transforms.Parent { Value = parentEntity });
            return childEntity;
        }
        
        [Button]
        private void Clear()
        {
            foreach (var part in _outlines)
            {
                // Debug.Log(part);
                _entityManager.DestroyEntity(part);
            }

            _outlines.Clear();
        }
    }
}