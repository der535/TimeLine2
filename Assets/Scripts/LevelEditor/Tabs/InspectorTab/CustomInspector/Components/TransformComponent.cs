using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TransformComponent : BaseParameterComponent
    {
        public FloatParameter XPosition = new("Position-X", 0, Color.red);
        public BoolParameter XPositionActive = new("x-p Active", true, Color.gray);
        public FloatParameter YPosition = new("Position-Y", 0, Color.green);
        public BoolParameter YPositionActive = new("y-p Active", true, Color.gray);
        
        public FloatParameter XPositionOffset = new("Offset-Position-X", 0, Color.black);
        public FloatParameter YPositionOffset = new("Offset-Position-Y", 0, Color.black);
        
        public FloatParameter XRotation = new("Rotation-X", 0, new Color(0.8301887f, 0.2310431f, 0.2310431f));
        public BoolParameter XRotationActive = new("x-r Active", true, Color.gray);
        public FloatParameter YRotation = new("Rotation-Y", 0, new Color(0.2584544f, 0.8313726f, 0.2313726f));
        public BoolParameter YRotationActive = new("y-r Active", true, Color.gray);
        public FloatParameter ZRotation = new("Rotation-Z", 0, new Color(0.2313726f, 0.5932469f, 0.8313726f));
        public BoolParameter ZRotationActive = new("z-r Active", true, Color.gray);
        
        public FloatParameter XScale = new("Scale-X", 1, new Color(0.6132076f, 0.1918755f, 0.1918755f));
        public BoolParameter XScaleActive = new("x-s Active", true, Color.gray);
        public FloatParameter YScale = new("Scale-Y", 1, new Color(0.2226184f, 0.6117647f, 0.172549f));
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
            XPosition.OnValueChanged += () => transform.localPosition = new Vector3(XPosition.Value + XPositionOffset.Value, transform.localPosition.y, transform.localPosition.z);
            YPosition.OnValueChanged += () => transform.localPosition = new Vector3(transform.localPosition.x, YPosition.Value + YPositionOffset.Value, transform.localPosition.z);
            
            XPositionOffset.OnValueChanged += () => transform.localPosition = new Vector3(XPosition.Value + XPositionOffset.Value, transform.localPosition.y, transform.localPosition.z);
            YPositionOffset.OnValueChanged += () => transform.localPosition = new Vector3(transform.localPosition.x, YPosition.Value + YPositionOffset.Value, transform.localPosition.z);

            XRotation.OnValueChanged += () => transform.localRotation = Quaternion.Euler(XRotation.Value, transform.rotation.y, transform.rotation.z);
            YRotation.OnValueChanged += () => transform.localRotation = Quaternion.Euler(transform.rotation.x,YRotation.Value, transform.rotation.z);
            ZRotation.OnValueChanged += () => transform.localRotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, ZRotation.Value);
            
            XScale.OnValueChanged += () => transform.localScale = new Vector3(XScale.Value, transform.localScale.y, transform.localScale.z);
            YScale.OnValueChanged += () => transform.localScale = new Vector3(transform.localScale.x, YScale.Value, transform.localScale.z);

            XPositionActive.OnValueChanged += () =>
            {
                TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                TreeNode node = data.branch.AddNode(this.GetType().Name,XPosition.Name);
                _keyframeTrackStorage.SetActiveTrack(node, XPositionActive.Value);
            };
            
            YPositionActive.OnValueChanged += () =>
            {
                TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                TreeNode node = data.branch.AddNode(this.GetType().Name,YPosition.Name);
                _keyframeTrackStorage.SetActiveTrack(node ,YPositionActive.Value);
            };
            
            XRotationActive.OnValueChanged += () =>
            {
                TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                TreeNode node = data.branch.AddNode(this.GetType().Name,XRotation.Name);
                _keyframeTrackStorage.SetActiveTrack(node ,XRotationActive.Value);
            };
            
            YRotationActive.OnValueChanged += () =>
            {
                TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                TreeNode node = data.branch.AddNode(this.GetType().Name,YRotation.Name);
                _keyframeTrackStorage.SetActiveTrack(node ,YRotationActive.Value);
            };
            
            ZRotationActive.OnValueChanged += () =>
            {
                TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                TreeNode node = data.branch.AddNode(this.GetType().Name,ZRotation.Name);
                _keyframeTrackStorage.SetActiveTrack(node ,ZRotationActive.Value);
            };
            
            XScaleActive.OnValueChanged += () =>
            {
                TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                TreeNode node = data.branch.AddNode(this.GetType().Name,XScale.Name);
                _keyframeTrackStorage.SetActiveTrack(node ,XScaleActive.Value);
            };
            
            YScaleActive.OnValueChanged += () =>
            {
                TrackObjectData data = _trackObjectStorage.GetTrackObjectData(gameObject);
                TreeNode node = data.branch.AddNode(this.GetType().Name,YScale.Name);
                _keyframeTrackStorage.SetActiveTrack(node ,YScaleActive.Value);
            };
        }
        
        // public override void CopyTo(Component targetComponent)
        // {
        //     if (targetComponent is TransformComponent other)
        //     {
        //         other.XPosition.Value = XPosition.Value;
        //         other.YPosition.Value = YPosition.Value;
        //         
        //         other.XRotation.Value = XRotation.Value;
        //         other.YRotation.Value = YRotation.Value;
        //         other.ZRotation.Value = ZRotation.Value;
        //         
        //         other.XScale.Value = XScale.Value;
        //         other.YScale.Value = YScale.Value;
        //     }
        //     else
        //     {
        //         throw new ArgumentException("Target component must be of type TransformComponent");
        //     }
        // }
        //
        // public override Component Copy(GameObject targetGameObject)
        // {
        //     var component = targetGameObject.GetComponent<TransformComponent>();
        //     CopyTo(component);
        //     // print("Оп скопировал");
        //     return component;
        // }
    }
}