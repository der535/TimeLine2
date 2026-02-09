using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class RadialSunburstMaterial : BaseParameterComponent
    {
        public ColorParameter Color1 = new("Color1", Color.white, Color.white);
        public ColorParameter Color2 = new("Color2", Color.black, Color.white);
        public IntParameter SegmentsCount = new IntParameter("Segments Count", 10, Color.white);
        public FloatParameter TwistIntensity = new FloatParameter("Twist intensity", 0, Color.white);
        public FloatParameter RotationSpeed = new FloatParameter("Rotation Speed", 0, Color.white);


        private SpriteRenderer _spriteRenderer;
        private Material _material;

        [Inject]
        private void Construct(SelectSpriteController selectSpriteController, DiContainer container,
            CustomSpriteStorage customSpriteStorage, TrackObjectStorage trackObjectStorage)
        {
            _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            Material myMaterial = new Material(Shader.Find("Custom/RadialSunburst_Twist"));
            _spriteRenderer.material = myMaterial;
            Color1.OnValueChanged += () => myMaterial.SetColor("_ColorA", Color1.Value);
            Color2.OnValueChanged += () => myMaterial.SetColor("_ColorB", Color2.Value);
            SegmentsCount.OnValueChanged += () => myMaterial.SetInt("_Segments", SegmentsCount.Value);
            TwistIntensity.OnValueChanged += () => myMaterial.SetFloat("_Twist", TwistIntensity.Value);
            _material = myMaterial;
        }


        private void OnDestroy()
        {
            //todo отписака
        }

        private float _currentAngle = 0f;

        void Update()
        {
            // Увеличиваем угол на основе времени
            _currentAngle += RotationSpeed.Value * Time.deltaTime;

            // Ограничиваем значение от 0 до 360
            _currentAngle %= 360f;
            
            _material.SetFloat("_Angle", _currentAngle);
        }

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return Color1;
            yield return Color2;
            yield return SegmentsCount;
            yield return TwistIntensity;
        }
    }
}