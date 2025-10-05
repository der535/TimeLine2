using System;
using TimeLine.Bezier_curve;
using TimeLine.Installers;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class BezierPoint : MonoBehaviour
    {
        [SerializeField] private RectTransform point;
        [SerializeField] private BezierDragPoint _bezierDragPoint;
        [SerializeField] private BezierSelectPoint bezierSelectPoint;
        [Space]
        [SerializeField] private RectTransform tangentLeft;
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

        private MainObjects _mainObjects;
        private Main _main;
        private TimeLineSettings _timeLineSettings;

        public Keyframe.Keyframe PrevKey;
        public Keyframe.Keyframe NextKey;

        [Inject]
        private void Construct(MainObjects mainObject, Main main, TimeLineSettings timeLineSettings)
        {
            _mainObjects = mainObject;
            _main = main;
            _timeLineSettings = timeLineSettings;
        }

        public void Setup(
            Keyframe.Keyframe keyframe,
            Action sortKeyframes,
            Keyframe.Keyframe prevKey = null,
            Keyframe.Keyframe nextKey = null,
            float pan = 100f,
            float verticalScale = 50f)
        {
            pan += _timeLineSettings.DistanceBetweenBeatLines;
            
            PrevKey = prevKey;
            NextKey = nextKey;
            
            double currentTime = _main.TicksToSeconds(keyframe.Ticks);
            double currentValue = keyframe.GetData().GetValue() is float val ? val : 0f;

            // ---------- IN ----------
            double inWeight = keyframe.InWeight;
            double inTangent = keyframe.InTangent;

            tangentLeft.gameObject.SetActive(prevKey != null);
            tangentLineLeft.gameObject.SetActive(prevKey != null);
            
            tangentRight.gameObject.SetActive(nextKey != null);
            tangentLineRight.gameObject.SetActive(nextKey != null);
            
            if (prevKey != null)
            {
                double prevTime = _main.TicksToSeconds(prevKey.Ticks);
                double prevValue = prevKey.GetData().GetValue() is float pVal ? pVal : 0f;

                double deltaTime = currentTime - prevTime;

                // нормализованный вес -> абсолютное смещение по времени
                double inTimeOffset = deltaTime * inWeight;
                double inValueOffset = inTimeOffset * inTangent;

                tangentLeft.anchoredPosition = new Vector2(
                    -(float)(inTimeOffset * pan * (_main.MusicDataSo.bpm / 60f)),
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
                double nextTime = _main.TicksToSeconds(nextKey.Ticks);
                double nextValue = nextKey.GetData().GetValue() is float nVal ? nVal : 0f;

                double deltaTime = nextTime - currentTime;

                double outTimeOffset = deltaTime * outWeight;
                double outValueOffset = outTimeOffset * outTangent;

                tangentRight.anchoredPosition = new Vector2(
                    (float)(outTimeOffset * pan * (_main.MusicDataSo.bpm / 60f)),
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

            _bezierDragPoint.Setup(keyframe, sortKeyframes);
            bezierPointTangleLineDrawer.UpdatePosition();
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
            
            double currentTime = _main.TicksToSeconds(keyframe.Ticks);
            double currentValue = keyframe.GetData().GetValue() is float val ? val : 0f;

            // ---------- IN ----------
            double inWeight = keyframe.InWeight;
            double inTangent = keyframe.InTangent;
            
            tangentLeft.gameObject.SetActive(prevKey != null);
            tangentLineLeft.gameObject.SetActive(prevKey != null);
            
            tangentRight.gameObject.SetActive(nextKey != null);
            tangentLineRight.gameObject.SetActive(nextKey != null);

            if (prevKey != null)
            {
                double prevTime = _main.TicksToSeconds(prevKey.Ticks);
                double prevValue = prevKey.GetData().GetValue() is float pVal ? pVal : 0f;

                double deltaTime = currentTime - prevTime;

                // нормализованный вес -> абсолютное смещение по времени
                double inTimeOffset = deltaTime * inWeight;
                double inValueOffset = inTimeOffset * inTangent;

                tangentLeft.anchoredPosition = new Vector2(
                    -(float)(inTimeOffset * pan * (_main.MusicDataSo.bpm / 60f)),
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
                double nextTime = _main.TicksToSeconds(nextKey.Ticks);
                double nextValue = nextKey.GetData().GetValue() is float nVal ? nVal : 0f;

                double deltaTime = nextTime - currentTime;

                double outTimeOffset = deltaTime * outWeight;
                double outValueOffset = outTimeOffset * outTangent;

                tangentRight.anchoredPosition = new Vector2(
                    (float)(outTimeOffset * pan * (_main.MusicDataSo.bpm / 60f)),
                    (float)(outValueOffset * verticalScale)
                );
            }
            else
            {
                tangentRight.anchoredPosition = Vector2.zero;
            }

            bezierPointTangleLineDrawer.UpdatePosition();
            // Debug.Log($"[BezierPoint.Setup] key={keyframe} " +
            //           $"InWeight={inWeight}, InTangent={inTangent}, tangentLeft={tangentLeft.anchoredPosition} " +
            //           $"OutWeight={outWeight}, OutTangent={outTangent}, tangentRight={tangentRight.anchoredPosition}");
        }
    }
}