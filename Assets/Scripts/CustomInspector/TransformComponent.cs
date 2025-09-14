using System;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.Keyframe;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TransformComponent : MonoBehaviour, ICopyableComponent
    {
        public FloatParameter XPosition = new("Position-X", 0);
        public BoolParameter XPositionActive = new("x-p Active", true);
        public FloatParameter YPosition = new("Position-Y", 0);
        public BoolParameter YPositionActive = new("x-p Active", true);

        public FloatParameter XRotation = new("Rotation-X", 0);
        public BoolParameter XRotationActive = new("x-r Active", true);
        public FloatParameter YRotation = new("Rotation-Y", 0);
        public BoolParameter YRotationActive = new("y-r Active", true);
        public FloatParameter ZRotation = new("Rotation-Z", 0);
        public BoolParameter ZRotationActive = new("z-r Active", true);
        
        public FloatParameter XScale = new("Scale-X", 1);
        public BoolParameter XScaleActive = new("x-s Active", true);
        public FloatParameter YScale = new("Scale-Y", 1);
        public BoolParameter YScaleActive = new("y-s Active", true);

        private KeyframeTrackStorage _keyframeTrackStorage;
        private TrackObjectStorage _trackObjectStorage;

        [Inject]
        private void Construct(KeyframeTrackStorage keyframeTrackStorage, TrackObjectStorage trackObjectStorage)
        {
            _keyframeTrackStorage = keyframeTrackStorage;
            _trackObjectStorage = trackObjectStorage;
        }
        
        private void Awake()
        {
            XPosition.OnValueChanged += () => transform.position = new Vector3(XPosition.Value, transform.position.y, transform.position.z);
            YPosition.OnValueChanged += () => transform.position = new Vector3(transform.position.x, YPosition.Value, transform.position.z);

            XRotation.OnValueChanged += () => transform.rotation = Quaternion.Euler(XRotation.Value, transform.rotation.y, transform.rotation.z);
            YRotation.OnValueChanged += () => transform.rotation = Quaternion.Euler(transform.rotation.x,YRotation.Value, transform.rotation.z);
            ZRotation.OnValueChanged += () => transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, ZRotation.Value);
            
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

        public void CopyTo(Component targetComponent)
        {
            if (targetComponent is TransformComponent other)
            {
                other.XPosition.Value = XPosition.Value;
                other.YPosition.Value = YPosition.Value;
                
                other.XRotation.Value = XRotation.Value;
                other.YRotation.Value = YRotation.Value;
                other.ZRotation.Value = ZRotation.Value;
                
                other.XScale.Value = XScale.Value;
                other.YScale.Value = YScale.Value;
            }
            else
            {
                throw new ArgumentException("Target component must be of type TransformComponent");
            }
        }

        public Component Copy(GameObject targetGameObject)
        {
            var component = targetGameObject.GetComponent<TransformComponent>();
            CopyTo(component);
            // print("Оп скопировал");
            return component;
        }
    }
}