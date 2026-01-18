using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.Keyframe;
using TimeLine.Keyframe.AnimationDatas.TransformComponent;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Rotation;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using TimeLine.LevelEditor.Tabs.InspectorTab.Keyframe.AnimationDatas.TransformComponent.Position;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using BoolParameter = TimeLine.CustomInspector.Logic.Parameter.BoolParameter;
using FloatParameter = TimeLine.CustomInspector.Logic.Parameter.FloatParameter;

namespace TimeLine
{
    public class TransformComponent : BaseParameterComponent
    {
        public BoolParameter isActiveTrackObject = new("isActive TrackObject", true, Color.black);
        public BoolParameter isTempTrackObject = new("isTempTrackObject", false, Color.black);

        public FloatParameter XPosition = new("Position/X", 0, Color.red);
        public BoolParameter XPositionActive = new("x-p Active", true, Color.gray);
        public FloatParameter YPosition = new("Position/Y", 0, Color.green);
        public BoolParameter YPositionActive = new("y-p Active", true, Color.gray);

        public FloatParameter XPositionOffset = new("Offset-Position-X", 0, Color.black);
        public FloatParameter YPositionOffset = new("Offset-Position-Y", 0, Color.black);

        public FloatParameter XRotation = new("Rotation/X", 0, new Color(0.8301887f, 0.2310431f, 0.2310431f));
        public BoolParameter XRotationActive = new("x-r Active", true, Color.gray);
        public FloatParameter YRotation = new("Rotation/Y", 0, new Color(0.2584544f, 0.8313726f, 0.2313726f));
        public BoolParameter YRotationActive = new("y-r Active", true, Color.gray);
        public FloatParameter ZRotation = new("Rotation/Z", 0, new Color(0.2313726f, 0.5932469f, 0.8313726f));
        public BoolParameter ZRotationActive = new("z-r Active", true, Color.gray);

        public FloatParameter XScale = new("Scale/X", 1, new Color(0.6132076f, 0.1918755f, 0.1918755f));
        public BoolParameter XScaleActive = new("x-s Active", true, Color.gray);
        public FloatParameter YScale = new("Scale/Y", 1, new Color(0.2226184f, 0.6117647f, 0.172549f));
        public BoolParameter YScaleActive = new("y-s Active", true, Color.gray);


        private KeyframeTrackStorage _keyframeTrackStorage;
        private TrackObjectStorage _trackObjectStorage;

        [Inject]
        private void Construct(KeyframeTrackStorage keyframeTrackStorage, TrackObjectStorage trackObjectStorage)
        {
            _keyframeTrackStorage = keyframeTrackStorage;
            _trackObjectStorage = trackObjectStorage;
        }


        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return isActiveTrackObject;
            yield return XPosition;
            yield return XPositionActive;
            yield return YPosition;
            yield return YPositionActive;
            yield return XRotation;
            yield return XRotationActive;
            yield return YRotation;
            yield return YRotationActive;
            yield return ZRotation;
            yield return ZRotationActive;
            yield return XScale;
            yield return XScaleActive;
            yield return YScale;
            yield return YScaleActive;
        }

        private void Awake()
        {
            Bind<float, FloatParameter>(XPosition,
                val => new XPositionData(val),
                val => transform.localPosition =
                    new Vector3(val, transform.localPosition.y, transform.localPosition.z));

            Bind<float, FloatParameter>(YPosition,
                val => new YPositionData(val),
                val => transform.localPosition =
                    new Vector3(transform.localPosition.x, val, transform.localPosition.z));

            Bind<float, FloatParameter>(XPositionOffset,
                val => null,
                val => transform.localPosition = new Vector3(XPosition.Value + val, transform.localPosition.y,
                    transform.localPosition.z));

            Bind<float, FloatParameter>(YPositionOffset,
                val => null,
                val => transform.localPosition = new Vector3(transform.localPosition.x, YPosition.Value + val,
                    transform.localPosition.z));

            Bind<float, FloatParameter>(XRotation,
                val => new XRotationData(val),
                val => transform.localRotation = Quaternion.Euler(val, transform.localRotation.y, transform.localRotation.z));

            Bind<float, FloatParameter>(YRotation,
                val => new YRotationData(val),
                val => transform.localRotation = Quaternion.Euler(transform.localRotation.x, val, transform.localRotation.z));

            Bind<float, FloatParameter>(ZRotation,
                val => new ZRotationData(val),
                val =>
                {
                    // Извлекаем текущие углы в градусах
                    Vector3 currentEuler = transform.localEulerAngles;
        
                    // Создаем новое вращение, меняя только Z
                    Quaternion newRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, val);
        
                    transform.localRotation = newRotation;
                });

            Bind<float, FloatParameter>(XScale,
                val => new XScaleData(val),
                val => transform.localScale = new Vector3(val, transform.localScale.y, transform.localScale.z));

            Bind<float, FloatParameter>(YScale,
                val => new YScaleData(val),
                val => transform.localScale = new Vector3(transform.localScale.x, val, transform.localScale.z));

            Bind<bool, BoolParameter>(XPositionActive,
                val => null,
                val =>
                {
                    TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                    TreeNode node = data.branch.AddNode(this.GetType().Name + "/" + XPosition.Name);
                    _keyframeTrackStorage.SetActiveTrack(node, val);
                });

            Bind<bool, BoolParameter>(YPositionActive,
                val => null,
                val =>
                {
                    TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                    TreeNode node = data.branch.AddNode(this.GetType().Name + "/" + YPosition.Name);
                    _keyframeTrackStorage.SetActiveTrack(node, val);
                });

            Bind<bool, BoolParameter>(XRotationActive,
                val => null,
                val =>
                {
                    TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                    TreeNode node = data.branch.AddNode(this.GetType().Name + "/" + XRotation.Name);
                    _keyframeTrackStorage.SetActiveTrack(node, val);
                });

            Bind<bool, BoolParameter>(YRotationActive,
                val => null,
                val =>
                {
                    TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                    TreeNode node = data.branch.AddNode(this.GetType().Name + "/" + YRotation.Name);
                    _keyframeTrackStorage.SetActiveTrack(node, val);
                });

            Bind<bool, BoolParameter>(ZRotationActive,
                val => null,
                val =>
                {
                    TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                    TreeNode node = data.branch.AddNode(this.GetType().Name + "/" + ZRotation.Name);
                    _keyframeTrackStorage.SetActiveTrack(node, val);
                });

            Bind<bool, BoolParameter>(XScaleActive,
                val => null,
                val =>
                {
                    TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                    TreeNode node = data.branch.AddNode(this.GetType().Name + "/" + XScale.Name);
                    _keyframeTrackStorage.SetActiveTrack(node, val);
                });

            Bind<bool, BoolParameter>(YScaleActive,
                val => null,
                val =>
                {
                    TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                    TreeNode node = data.branch.AddNode(this.GetType().Name + "/" + YScale.Name);
                    _keyframeTrackStorage.SetActiveTrack(node, val);
                });

            Bind<bool, BoolParameter>(isActiveTrackObject,
                val => null,
                val =>
                {
                    TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                    data.trackObject.isActive = val;
                });

            Bind<bool, BoolParameter>(isTempTrackObject,
                val => null,
                val =>
                {
                    TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                    data.trackObject.isTemp = val;
                });
            
        }

        private void OnDestroy()
        {
            isActiveTrackObject = null;
            isTempTrackObject = null;

            XPosition = null;
            XPositionActive = null;
            YPosition = null;
            YPositionActive = null;

            XPositionOffset = null;
            YPositionOffset = null;

            XRotation = null;
            XRotationActive = null;
            YRotation = null;
            YRotationActive = null;
            ZRotation = null;
            ZRotationActive = null;

            XScale = null;
            XScaleActive = null;
            YScale = null;
            YScaleActive = null;
        }
    }
}