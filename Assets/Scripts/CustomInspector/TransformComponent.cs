using System;
using System.Collections.Generic;
using UnityEngine;

namespace TimeLine
{
    public class TransformComponent : MonoBehaviour, IFieldProvider
    {
        [SerializeField] private FloatField xPosition = new("Position/X", 0);
        [SerializeField] private FloatField yPosition = new("Position/Y", 0);
        [Space]
        [SerializeField] private FloatField xRotation = new("Rotation/X", 0);
        [SerializeField] private FloatField yRotation = new("Rotation/Y", 0);
        [SerializeField] private FloatField zRotation = new("Rotation/Z", 0);
        // [Space]
        // [SerializeField] private FloatField xScale = new("Scale/X", 0);
        // [SerializeField] private FloatField yScale = new("Scale/Y", 0);
        // [SerializeField] private FloatField zScale = new("Scale/Z", 0);

        public Action OnChangeCustomInspector { get; set; }

        private void Awake()
        {
            OnChangeCustomInspector += (() =>
            {
                XPosition = xPosition.Value;
                YPosition = yPosition.Value;
                XRotation = xRotation.Value;
                YRotation = yRotation.Value;
                ZRotation = zRotation.Value;
            });
        }

        public IEnumerable<IField> GetFields()
        {
            yield return xPosition;
            yield return yPosition;
            yield return xRotation;
            yield return yRotation;
            yield return zRotation;
            // yield return xScale;
            // yield return yScale;
            // yield return zScale;
        }
        
        public float XPosition
        {
            get => transform.position.x;
            set
            {
                transform.position = new Vector3(value, transform.position.y, transform.position.z);
                xPosition.Value = value;
            }
        }

        public float YPosition
        {
            get => transform.position.y;
            set
            {
                transform.position = new Vector3(transform.position.x, value, transform.position.z);
                yPosition.Value = value;
            }
        }

        public float XRotation
        {
            get => transform.rotation.x;
            set
            {
                transform.rotation = Quaternion.Euler(value, transform.rotation.y, transform.rotation.z);
                yRotation.Value = value;
            }
        }

        public float YRotation
        {
            get => transform.rotation.y;
            set
            {
                transform.rotation = Quaternion.Euler(transform.rotation.x, value, transform.rotation.z);
                yRotation.Value = value;
            }
        }

        public float ZRotation
        {
            get => transform.rotation.z;
            set
            {
                transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, value);
                zRotation.Value = value;
            }
        }
    }
}