using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.KeyframeType
{
    public class C_KeyframeActiveTypeController : MonoBehaviour
    {
        [SerializeField] private KeyframeActiveTypeView keyframeActiveTypeView;
        
        private GameEventBus _gameEventBus;
        private KeyframeVizualizer _keyframeVisualizer;
        private BezierController _bezierController;
        
        private M_KeyframeActiveTypeData _mKeyframeActiveTypeData;

        [Inject]
        private void Construct(GameEventBus eventBus, M_KeyframeActiveTypeData mKeyframeActiveTypeData)
        {
            _gameEventBus = eventBus;
            _mKeyframeActiveTypeData = mKeyframeActiveTypeData;
        }
        private void Start()
        {
            keyframeActiveTypeView.OnKeyframeSelected += () =>
            {
                if (_mKeyframeActiveTypeData.ActiveType == M_KeyframeType.Keyframe) return;
                _mKeyframeActiveTypeData.ActiveType = M_KeyframeType.Keyframe;
                _gameEventBus.Raise(new KeyframeTypeChangeEvent(_mKeyframeActiveTypeData.ActiveType));
            };
            keyframeActiveTypeView.OnBezierSelected += () =>
            {
                if (_mKeyframeActiveTypeData.ActiveType == M_KeyframeType.Bezier) return;
                _mKeyframeActiveTypeData.ActiveType = M_KeyframeType.Bezier;
                _gameEventBus.Raise(new KeyframeTypeChangeEvent(_mKeyframeActiveTypeData.ActiveType));
            };
            
            _gameEventBus.Raise(new KeyframeTypeChangeEvent(_mKeyframeActiveTypeData.ActiveType)); //Сетаем ключевые кадры со старта
        }
    }
}
