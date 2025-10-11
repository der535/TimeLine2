using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TimeLine
{
    public class RandomTransformComponent : BaseParameterComponent
    {
        public BoolParameter ComponentActive = new("Enabled", true, Color.gray);
        
        public Vector2Parameter XRandomPosition = new("Random-Position-x", "min", "max", Vector2.zero);
        public BoolParameter XRandomPositionActive = new("p-x", false, Color.gray);
        public Vector2Parameter YRandomPosition = new("Random-Position-y", "mix", "max", Vector2.zero);
        public BoolParameter YRandomPositionActive = new("p-y", false, Color.gray);

        public Vector2Parameter XRandomRotation = new("Random-Rotation-x", "mix", "max", Vector2.zero);
        public BoolParameter XRandomRotationActive = new ("r-x", false, Color.gray);
        public Vector2Parameter YRandomRotation = new("Random-Rotation-y", "mix", "max", Vector2.zero);
        public BoolParameter YRandomRotationActive = new ("r-y", false, Color.gray);
        public Vector2Parameter ZRandomRotation = new("Random-Rotation-z", "mix", "max", Vector2.zero);
        public BoolParameter ZRandomRotationActive = new ("r-z", false, Color.gray);

        public Vector2Parameter XRandomScale = new("Random-Scale-x", "mix", "max", Vector2.zero);
        public BoolParameter XRandomScaleActive = new ("Active-x", false, Color.gray);
        public Vector2Parameter YRandomScale = new("Random-Scale-y", "mix", "max", Vector2.zero);
        public BoolParameter YRandomScaleActive = new ("Active-y", false, Color.gray);

        private TransformComponent _transform;

        public void Awake()
        {
            _transform = GetComponent<TransformComponent>();
        }

        public void Initialized()
        {
            if(ComponentActive.Value == false) return;
            
            if (XRandomPositionActive.Value)
                _transform.XPosition.Value = Random.Range(XRandomPosition.Value.x, XRandomPosition.Value.y);
            if (YRandomPositionActive.Value)
                _transform.YPosition.Value = Random.Range(YRandomPosition.Value.x, YRandomPosition.Value.y);
            if (XRandomRotationActive.Value)
                _transform.XRotation.Value = Random.Range(XRandomRotation.Value.x, XRandomRotation.Value.y);
            if (YRandomRotationActive.Value)
                _transform.YRotation.Value = Random.Range(YRandomRotation.Value.x, YRandomRotation.Value.y);
            if (ZRandomRotationActive.Value)
                _transform.ZRotation.Value = Random.Range(ZRandomRotation.Value.x, ZRandomRotation.Value.y);
            if (XRandomScaleActive.Value)
                _transform.XScale.Value = Random.Range(XRandomScale.Value.x, XRandomScale.Value.y);
            if (YRandomScaleActive.Value)
                _transform.YScale.Value = Random.Range(YRandomScale.Value.x, YRandomScale.Value.y);
        }

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return ComponentActive;
            yield return XRandomPosition;
            yield return XRandomPositionActive;
            yield return YRandomPosition;
            yield return YRandomPositionActive;
            yield return XRandomRotation;
            yield return XRandomRotationActive;
            yield return YRandomRotation;
            yield return YRandomPositionActive;
            yield return ZRandomRotation;
            yield return ZRandomRotationActive;
            yield return XRandomScale;
            yield return XRandomScaleActive;
            yield return YRandomScale;
            yield return YRandomScaleActive;
        }

        public override void CopyTo(Component targetComponent)
        {
            if (targetComponent is RandomTransformComponent other)
            {
                other.XRandomPosition.Value = XRandomPosition.Value;
                other.YRandomPosition.Value = YRandomPosition.Value;
                other.XRandomRotation.Value = XRandomRotation.Value;
                other.YRandomRotation.Value = YRandomRotation.Value;
                other.ZRandomRotation.Value = ZRandomRotation.Value;
                other.XRandomScale.Value = XRandomScale.Value;
                other.YRandomScale.Value = YRandomScale.Value;
            }
            else
            {
                throw new ArgumentException("Target component must be of type TransformComponent");
            }
        }

        public override Component Copy(GameObject targetGameObject)
        {
            print("RandomTransformComponent copy");
            if (targetGameObject.TryGetComponent(out RandomTransformComponent component))
            {
                CopyTo(component);
            }
            else
            {
                component = targetGameObject.AddComponent<RandomTransformComponent>();
                CopyTo(component);
            }

            return component;
        }
    }
}