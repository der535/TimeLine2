using System.Collections.Generic;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.Keyframe;
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
        
        private ActionMap _actionMap;

        [Inject]
        private void Constructor(
            TrackObjectClipboard clipboard,
            ObjectFactory factory,
            ObjectLoader loader,
            ActionMap actionMap,
            EditingCompositionController editingCompositionController,
            SelectObjectController selectObjectController)
        {
            _clipboard = clipboard;
            _factory = factory;
            _loader = loader;
            _actionMap = actionMap;
            _editingCompositionController = editingCompositionController;
            _selectObjectController = selectObjectController;
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

        internal (TrackObjectData, GameObject, Branch, List<Track>) LoadObject(GameObjectSaveData data,
            bool addToStorage = true)
        {
            return _loader.LoadObject(data, addToStorage, loadGraph: false);
        }

        internal void CreateSceneObjectAndAddSprite(Sprite sprite)
        {
            _factory.CreateSceneObjectAndAddSprite(sprite);
        }
        
        internal (TrackObjectData, GameObject, Branch) LoadComposition(GroupGameObjectSaveData data,
            string compositionID, bool generateNewSceneID,
            GroupGameObjectSaveData compositionData = null, bool addToStorage = true, string lastEditID = null)
        {
            return _loader.LoadComposition(data, compositionID, compositionData, addToStorage, lastEditID, generateNewSceneID: generateNewSceneID);
        }
    }
}