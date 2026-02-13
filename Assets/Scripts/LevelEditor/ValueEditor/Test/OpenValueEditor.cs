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
            if (!string.IsNullOrEmpty(_editKeyframe.GetData().Graph))
            {
                _editKeyframe.GetData().Logic = _saveNodes.LoadGraph(_editKeyframe.GetData().Graph, 
                    _editKeyframe.GetData().Logic.DataType, _editKeyframe.GetData().initializedNodes);
            }
            else
            {
                _nodeCreator.CreateNode(_editKeyframe.GetData().Logic);
            }
        }
    }
}