using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects
{
    public class TrackObjectRemover : MonoBehaviour
    {
        [SerializeField] private WindowsFocus windowsFocus;

        private TrackObjectStorage _trackObjectStorage;
        private KeyframeTrackStorage _keyframeTrackStorage;
        private SelectObjectController _selectObjectController;
        private ActionMap _actionMap;
        private GameEventBus _gameEventBus;
        private EntityManager _entityManager;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController,
            ActionMap actionMap,
            GameEventBus gameEventBus)
        {
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _selectObjectController = selectObjectController;
            _actionMap = actionMap;
            _gameEventBus = gameEventBus;
        }

        private void Start()
        {
            _actionMap.Editor.X.started += _ => Remove();
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void Remove()
        {
            if (!windowsFocus.IsFocused) return;

            foreach (var select in _selectObjectController.SelectObjects)
            {
                if (select is TrackObjectGroup group)
                {
                    ListRemove(group);
                }
                else
                {
                    SingleRemove(select);
                }
            }

            _gameEventBus.Raise(new DeselectAllObjectEvent());
        }

        internal void SingleRemove(TrackObjectPacket select)
        {
            // Disassemble(select.sceneObject);

            foreach (var nodes in select.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }


// Если они разные, удаление не сработает!
            
            select.components.Dispose();
            _entityManager.DestroyEntity(select.entity);

            _trackObjectStorage.Remove(select);
        }

        internal void SingleRemoveNoStorage(TrackObjectPacket select)
        {
            // if(disassemble) Disassemble(select.sceneObject);

            foreach (var nodes in select.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }

            select.components.Dispose();
            _entityManager.DestroyEntity(select.entity);

        }


        internal void SingleRemove(TrackObjectGroup select, bool removeFromStorage)
        {
            foreach (var nodes in select.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }

            select.components.Dispose();
            _entityManager.DestroyEntity(select.entity);
            if(removeFromStorage)_trackObjectStorage.Remove(select);
        }

        internal void ListRemove(TrackObjectGroup list)
        {
            foreach (var item in list.TrackObjectDatas)
            {
                if (item is TrackObjectGroup group)
                {
                    ListRemove(group);
                    
                }
                
                Debug.Log($"remove {item.entity.Version}:{item.entity.Index}");
                SingleRemove(item);
            }

            SingleRemove(list);
        }
    }
}