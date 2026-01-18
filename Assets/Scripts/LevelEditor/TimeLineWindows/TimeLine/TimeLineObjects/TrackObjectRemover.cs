using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
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
        }
        
        private void Remove()
        {
            if(!windowsFocus.IsFocused) return;
            
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

        internal void SingleRemove(TrackObjectData select)
        {
            Disassemble(select.sceneObject);
            
            foreach (var nodes in select.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }
            
            Destroy(select.trackObject.gameObject);
            Destroy(select.sceneObject);

            _trackObjectStorage.Remove(select);
        }
        
        internal void SingleRemoveNoStorage(TrackObjectData select, bool disassemble = true)
        {
           if(disassemble) Disassemble(select.sceneObject);

            foreach (var nodes in select.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }

            Destroy(select.trackObject.gameObject);
            Destroy(select.sceneObject);
        }
        
        void Disassemble(GameObject parentObject)
        {
            // Используем ToList() из System.Linq, так как мы будем менять родителя (child.parent = null)
            // Если менять иерархию внутри обычного foreach, итератор может "сломаться"
            foreach (Transform child in parentObject.transform.Cast<Transform>().ToList())
            {
                // ВАЖНО: вызываем у child!
                if (child.TryGetComponent(out SceneObjectLink link))
                {
                    link.trackObjectData.trackObject._parentID = string.Empty;
                    child.SetParent(null); // Отцепляем
                }
            }
        }

        
        internal void SingleRemove(TrackObjectGroup select, bool disassemble = true)
        {
            if(disassemble)
                Disassemble(select.sceneObject);
            
            foreach (var nodes in select.branch.Nodes)
            {
                _keyframeTrackStorage.RemoveTrack(nodes);
            }

            Destroy(select.trackObject.gameObject);
            Destroy(select.sceneObject);
            
            _trackObjectStorage.Remove(select);
        }

        internal void ListRemove(TrackObjectGroup list)
        {
            foreach (var item in list.TrackObjectDatas)
            {
                if (item is TrackObjectGroup group)
                {
                    ListRemove(group);
                    SingleRemove(item);
                }
                else
                {
                    SingleRemove(item);
                }
            }
            SingleRemove(list);
        }
    }
}