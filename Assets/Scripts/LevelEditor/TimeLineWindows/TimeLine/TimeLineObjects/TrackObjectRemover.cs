using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.ActionHistory;
using TimeLine.LevelEditor.ActionHistory.Commands;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning;
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
        private SaveLevel _saveLevel;
        private FacadeObjectSpawner _facadeObjectSpawner;

        [Inject]
        private void Construct(
            TrackObjectStorage trackObjectStorage,
            KeyframeTrackStorage keyframeTrackStorage,
            SelectObjectController selectObjectController,
            ActionMap actionMap,
            GameEventBus gameEventBus,
            SaveLevel saveLevel,
            FacadeObjectSpawner facadeObjectSpawner)
        {
            _trackObjectStorage = trackObjectStorage;
            _keyframeTrackStorage = keyframeTrackStorage;
            _selectObjectController = selectObjectController;
            _actionMap = actionMap;
            _gameEventBus = gameEventBus;
            _saveLevel = saveLevel;
            _facadeObjectSpawner = facadeObjectSpawner;
        }

        private void Start()
        {
            _actionMap.Editor.X.started += _ => Remove();
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void Remove()
        {
            if (!windowsFocus.IsFocused || _selectObjectController.SelectObjects.Count <= 0) return;
            
            CommandHistory.ExecuteCommand(new DeleteObjectCommand(_saveLevel,_facadeObjectSpawner,this,_selectObjectController.SelectObjects,""));
        }

        internal void RemoveList(List<TrackObjectPacket> target)
        {
            foreach (var select in target)
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
                
                SingleRemove(item);
            }

            SingleRemove(list);
        }
    }
}