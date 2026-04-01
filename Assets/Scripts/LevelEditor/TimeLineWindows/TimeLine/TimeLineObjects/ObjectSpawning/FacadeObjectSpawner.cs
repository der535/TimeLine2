using System.Collections.Generic;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.Save;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.ObjectSpawning
{
    public class FacadeObjectSpawner : MonoBehaviour
    {
        [SerializeField] private WindowsFocus timeLineFocus;
        
        private TrackObjectClipboard _clipboard;
        private ObjectFactory _factory;
        private ObjectLoader _loader;
        
        private EditingCompositionController _editingCompositionController;
        private SelectObjectController _selectObjectController;
        private SaveComposition _saveComposition;
        
        private ActionMap _actionMap;

        [Inject]
        private void Constructor(
            TrackObjectClipboard clipboard,
            ObjectFactory factory,
            ObjectLoader loader,
            ActionMap actionMap,
            EditingCompositionController editingCompositionController,
            SelectObjectController selectObjectController, SaveComposition saveComposition)
        {
            _clipboard = clipboard;
            _factory = factory;
            _loader = loader;
            _actionMap = actionMap;
            _editingCompositionController = editingCompositionController;
            _selectObjectController = selectObjectController;
            _saveComposition = saveComposition;
        }

        private void Start()
        {
            _actionMap.Editor.C.started += _ =>
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed() && timeLineFocus.IsFocused)
                    _clipboard.CopyObjects(_selectObjectController.SelectObjects); //Todo потом сделать по нормальному
            };
            _actionMap.Editor.V.started += _ =>
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed() && timeLineFocus.IsFocused)
                {
                    if (_clipboard.PasteValidCheck(_editingCompositionController.EditionCompositionID))
                        _clipboard.PasteObjects();
                }
            };
        }

        internal List<TrackObjectPacket> LoadObjects(List<GameObjectSaveData> data)
        {
            var trackObjects = new List<TrackObjectPacket>();
            
            foreach (var saveData in data)
            {
                if (saveData is GroupGameObjectSaveData group)
                {
                    Debug.Log("is group");
                    
                    trackObjects.Add(LoadComposition(group, group.compositionID, false, compositionData:_saveComposition.FindCompositionDataById(group.compositionID)).Item1);
                }
                else
                {
                    trackObjects.Add(LoadObject(saveData, true, true).Item1);
                }
            }
            
            return trackObjects;
        }

        internal (TrackObjectPacket,  Branch, List<Track>) LoadObject(GameObjectSaveData data,
            bool addToStorage = true, bool loadGraph = false)
        {
            return _loader.LoadObject(data, addToStorage, loadGraph: loadGraph);
        }

        internal void CreateSceneObjectAndAddSprite(Sprite sprite)
        {
            _factory.CreateSceneObjectAndAddSprite(sprite);
        }
        
        internal (TrackObjectPacket, GameObject, Branch) LoadComposition(GroupGameObjectSaveData data,
            string compositionID, bool generateNewSceneID,
            GroupGameObjectSaveData compositionData = null, bool addToStorage = true, string lastEditID = null)
        {
            return _loader.LoadComposition(data, compositionID, compositionData, addToStorage, lastEditID, generateNewSceneID: generateNewSceneID);
        }
    }
}