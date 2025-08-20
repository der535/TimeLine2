using System;
using UnityEngine;

namespace TimeLine
{
    public class TransformComponent : MonoBehaviour, ICopyableComponent
    {
        public FloatParameter XPosition = new("Position-X", 0);
        public FloatParameter YPosition = new("Position-Y", 0);

        public FloatParameter XRotation = new("Rotation-X", 0);
        public FloatParameter YRotation = new("Rotation-Y", 0);
        public FloatParameter ZRotation = new("Rotation-Z", 0);
        
        public FloatParameter XScale = new("Scale-X", 1);
        public FloatParameter YScale = new("Scale-Y", 1);
        
        private void Awake()
        {
            XPosition.OnValueChanged += () => transform.position = new Vector3(XPosition.Value, transform.position.y, transform.position.z);
            YPosition.OnValueChanged += () => transform.position = new Vector3(transform.position.x, YPosition.Value, transform.position.z);

            XRotation.OnValueChanged += () => transform.rotation = Quaternion.Euler(XRotation.Value, transform.rotation.y, transform.rotation.z);
            YRotation.OnValueChanged += () => transform.rotation = Quaternion.Euler(transform.rotation.x,YRotation.Value, transform.rotation.z);
            ZRotation.OnValueChanged += () => transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, ZRotation.Value);
            
            XScale.OnValueChanged += () => transform.localScale = new Vector3(XScale.Value, transform.localScale.y, transform.localScale.z);
            YScale.OnValueChanged += () => transform.localScale = new Vector3(transform.localScale.x, YScale.Value, transform.localScale.z);
        }

        public void CopyTo(Component targetComponent)
        {
            if (targetComponent is TransformComponent other)
            {
                other.XPosition.Value = XPosition.Value;
                print($"Оп вставил {other.XPosition.Value}");
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
            print("Оп скопировал");
            return component;
        }
    }
}