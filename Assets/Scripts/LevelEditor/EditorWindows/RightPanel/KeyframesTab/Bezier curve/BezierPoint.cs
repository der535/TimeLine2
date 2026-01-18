using System;
using TimeLine.Bezier_curve;
using TimeLine.Installers;
using TimeLine.LevelEditor.Core.MusicData;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class BezierPoint : MonoBehaviour
    {
        [SerializeField] private RectTransform point;
        [SerializeField] private BezierDragPoint _bezierDragPoint;
        [SerializeField] private BezierSelectPoint bezierSelectPoint;
        [Space] [SerializeField] private RectTransform tangentLeft;
        [SerializeField] private RectTransform tangentLineLeft;
        [SerializeField] private RectTransform tangentRight;
        [SerializeField] private RectTransform tangentLineRight;

        [SerializeField] private BezierPointTangleLineDrawer bezierPointTangleLineDrawer;


        public Action onValueChanged;
        public RectTransform RectTransform => point;
        public BezierDragPoint BezierDragPoint => _bezierDragPoint;
        public BezierSelectPoint BezierSelectPoint => bezierSelectPoint;

        public Vector3 Point => point.anchoredPosition;
        public Vector3 TangentLeft => tangentLeft.anchoredPosition + point.anchoredPosition;
        public Vector3 TangentRight => tangentRight.anchoredPosition + point.anchoredPosition;
        
        private TimeLineSettings _timeLineSettings;
        private M_MusicData _musicData;

        public Keyframe.Keyframe PrevKey;
        public Keyframe.Keyframe NextKey;

        private bool isSelected;

        [Inject]
        private void Construct(Main main, TimeLineSettings timeLineSettings, M_MusicData musicData)
        {
            _timeLineSettings = timeLineSettings;
            _musicData = musicData;
        }

        public void Setup(
            Keyframe.Keyframe keyframe,
            Action sortKeyframes,
            Keyframe.Keyframe prevKey = null,
            Keyframe.Keyframe nextKey = null,
            float pan = 100f,
            float verticalScale = 50f,
            Keyframe.Keyframe keyframeOriginal = null)
        {
            pan += _timeLineSettings.DistanceBetweenBeatLines;

            PrevKey = prevKey;
            NextKey = nextKey;

            double currentTime = TimeLineConverter.Instance.TicksToSeconds(keyframe.Ticks);
            // double currentValue = keyframe.GetData().GetValue() is float val ? val : 0f;

            // ---------- IN ----------
            double inWeight = keyframe.InWeight;
            double inTangent = keyframe.InTangent;

            if (prevKey != null)
            {
                double prevTime =  TimeLineConverter.Instance.TicksToSeconds(prevKey.Ticks);
                // double prevValue = prevKey.GetData().GetValue() is float pVal ? pVal : 0f;

                double deltaTime = currentTime - prevTime;

                // нормализованный вес -> абсолютное смещение по времени
                double inTimeOffset = deltaTime * inWeight;
                double inValueOffset = inTimeOffset * inTangent;

                tangentLeft.anchoredPosition = new Vector2(
                    -(float)(inTimeOffset * pan * (_musicData.bpm / 60f)),
                    -(float)(inValueOffset * verticalScale)
                );
            }
            else
            {
                tangentLeft.anchoredPosition = Vector2.zero;
            }

            // ---------- OUT ----------
            double outWeight = keyframe.OutWeight;
            double outTangent = keyframe.OutTangent;

            if (nextKey != null)
            {
                double nextTime =  TimeLineConverter.Instance.TicksToSeconds(nextKey.Ticks);
                // double nextValue = nextKey.GetData().GetValue() is float nVal ? nVal : 0f;

                double deltaTime = nextTime - currentTime;

                double outTimeOffset = deltaTime * outWeight;
                double outValueOffset = outTimeOffset * outTangent;

                tangentRight.anchoredPosition = new Vector2(
                    (float)(outTimeOffset * pan * (_musicData.bpm / 60f)),
                    (float)(outValueOffset * verticalScale)
                );
            }
            else
            {
                tangentRight.anchoredPosition = Vector2.zero;
            }

            // Debug.Log($"[BezierPoint.Setup] key={keyframe} " +
            //           $"InWeight={inWeight}, InTangent={inTangent}, tangentLeft={tangentLeft.anchoredPosition} " +
            //           $"OutWeight={outWeight}, OutTangent={outTangent}, tangentRight={tangentRight.anchoredPosition}");

            _bezierDragPoint.Setup(keyframe, keyframeOriginal,sortKeyframes);
            bezierPointTangleLineDrawer.UpdatePosition();
        }

        public void Select(bool select)
        {
            isSelected = select;
            
            tangentLeft.gameObject.SetActive(select && PrevKey != null);
            tangentLineLeft.gameObject.SetActive(select && PrevKey != null);

            tangentRight.gameObject.SetActive(select && NextKey != null);
            tangentLineRight.gameObject.SetActive(select && NextKey != null);
        }


        public void UpdatePosition(
            Keyframe.Keyframe keyframe,
            Keyframe.Keyframe prevKey = null,
            Keyframe.Keyframe nextKey = null,
            float pan = 100f,
            float verticalScale = 50f)
        {
            pan += _timeLineSettings.DistanceBetweenBeatLines;
            PrevKey = prevKey;
            NextKey = nextKey;
            
            Select(isSelected);

            double currentTime =  TimeLineConverter.Instance.TicksToSeconds(keyframe.Ticks);
            // double currentValue = keyframe.GetData().GetValue() is float val ? val : 0f;

            // ---------- IN ----------
            double inWeight = keyframe.InWeight;
            double inTangent = keyframe.InTangent;
            

            if (prevKey != null)
            {
                double prevTime =  TimeLineConverter.Instance.TicksToSeconds(prevKey.Ticks);
                // double prevValue = prevKey.GetData().GetValue() is float pVal ? pVal : 0f;

                double deltaTime = currentTime - prevTime;

                // нормализованный вес -> абсолютное смещение по времени
                double inTimeOffset = deltaTime * inWeight;
                double inValueOffset = inTimeOffset * inTangent;

                tangentLeft.anchoredPosition = new Vector2(
                    -(float)(inTimeOffset * pan * (_musicData.bpm / 60f)),
                    -(float)(inValueOffset * verticalScale)
                );
            }
            else
            {
                tangentLeft.anchoredPosition = Vector2.zero;
            }

            // ---------- OUT ----------
            double outWeight = keyframe.OutWeight;
            double outTangent = keyframe.OutTangent;

            if (nextKey != null)
            {
                double nextTime =  TimeLineConverter.Instance.TicksToSeconds(nextKey.Ticks);
                double nextValue = nextKey.GetData().GetValue() is float nVal ? nVal : 0f;

                double deltaTime = nextTime - currentTime;

                double outTimeOffset = deltaTime * outWeight;
                double outValueOffset = outTimeOffset * outTangent;

                tangentRight.anchoredPosition = new Vector2(
                    (float)(outTimeOffset * pan * (_musicData.bpm / 60f)),
                    (float)(outValueOffset * verticalScale)
                );
            }
            else
            {
                tangentRight.anchoredPosition = Vector2.zero;
            }

            bezierPointTangleLineDrawer.UpdatePosition();

        }
    }
}