using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Tabs.InspectorTab.Keyframe.KeyframeTimeLine
{
    public class KeyframeSelectController : MonoBehaviour
    {
        [SerializeField] private KeyfeameVizualizer keyframeVizualizer;
        public List<KeyframeObjectData> SelectedKeyframe { get; private set; }
        
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
        }

        private void Awake()
        {
            SelectedKeyframe = new List<KeyframeObjectData>();
            _gameEventBus.SubscribeTo<SelectKeyframeEvent>(SelectKeyframe);
        }

        private void SelectKeyframe(ref SelectKeyframeEvent selectKeyframeEvent)
        {
            if (_actionMap.Editor.LeftShift.IsPressed())
            {
                if (SelectedKeyframe.Contains(selectKeyframeEvent.Keyframe))
                {
                    SelectedKeyframe.Remove(selectKeyframeEvent.Keyframe);
                }
                else
                {
                    SelectedKeyframe.Add(selectKeyframeEvent.Keyframe);
                }
            }
            else
            {
                if (!SelectedKeyframe.Contains(selectKeyframeEvent.Keyframe))
                {
                    SelectedKeyframe.Clear();
                    SelectedKeyframe.Add(selectKeyframeEvent.Keyframe);
                }
            }
            
            print(SelectedKeyframe.Count);

            foreach (var keyframe in keyframeVizualizer.Keyframes)
            {
                keyframe.KeyframeSelect.SelectColor(false);
            }

            foreach (var sData in SelectedKeyframe)
            {
                sData.KeyframeSelect.SelectColor(true);
            }
        }

        private void OnDestroy()
        {
            _gameEventBus.UnsubscribeFrom<SelectKeyframeEvent>(SelectKeyframe);
        }
    }
}