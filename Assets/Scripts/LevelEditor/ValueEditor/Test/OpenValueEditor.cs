using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using TimeLine.LevelEditor.ValueEditor.Save;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.Test
{
    public class OpenValueEditor : MonoBehaviour
    {
        public RectTransform panel;
        private NodeCreator _nodeCreator;

        private Keyframe.Keyframe _selectedKeyframe;
        private Keyframe.Keyframe _editKeyframe;

        private GameEventBus _gameEventBus;
        private ClearWorkPlace _clearWorkPlace;
        private SaveNodes _saveNodes;
        public Keyframe.Keyframe GetEditKeyframe() => _editKeyframe;
        public bool IsPanelActive() => panel.gameObject.activeSelf;
        public RectTransform GetPanel() => panel;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, ClearWorkPlace clearWorkPlace, NodeCreator nodeCreator,
            SaveNodes saveNodes)
        {
            _gameEventBus = gameEventBus;
            _clearWorkPlace = clearWorkPlace;
            _nodeCreator = nodeCreator;
            _saveNodes = saveNodes;
        }

        private void Start()
        {
            _gameEventBus.SubscribeTo((ref SelectKeyframeEvent data) => { _selectedKeyframe = data.Keyframe; });
        }

        public void Open()
        {
            _editKeyframe = _selectedKeyframe;
            _clearWorkPlace.Clear();
            panel.gameObject.SetActive(true);
            if (!string.IsNullOrEmpty(_editKeyframe.GetEntityData().Graph))
            {
                _editKeyframe.GetEntityData().Logic = _saveNodes.LoadGraph(_editKeyframe.GetEntityData().Graph, 
                    _editKeyframe.GetEntityData().Logic.DataType, _editKeyframe.GetEntityData().initializedNodes);
            }
            else
            {
                _nodeCreator.CreateNode(_editKeyframe.GetEntityData().Logic);
            }
        }
    }
}