using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.Installers;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using TimeLine.TimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class ShakeComponent : BaseParameterComponent, IInitializedComponent
    {
        public Vector2Parameter ShakeStrength = new("Shake strength", "x", "y", Vector2.one);
        public FloatParameter Duration = new("Duration", 1, Color.black);
        public IntParameter Vibrato = new("Vibrato", 10, Color.white);
        public FloatParameter Randomness = new("Randomness", 90, Color.magenta);

        private ShakeCamera _shakeCamera;
        private TrackObjectStorage _trackObjectStorage;
        private Main _main;
        
        private TrackObjectData _trackObjectData;

        private bool isShakeActive;
        
        [Inject]
        private void Construct(ShakeCamera shakeCamera, TrackObjectStorage trackObjectStorage, Main main)
        {
            _shakeCamera = shakeCamera;
            _trackObjectStorage = trackObjectStorage;
            _main = main;
        }

        private void Start()
        {
            _trackObjectData = _trackObjectStorage.GetTrackObjectData(gameObject);
        }

        private void Update()
        {
            if (TimeLineConverter.Instance.TicksCurrentTime() > _trackObjectData.trackObject.StartTimeInTicks && isShakeActive == false)
            {
                isShakeActive = true;
                print(isShakeActive);


                _shakeCamera.Shake(ShakeStrength.Value, Duration.Value, Vibrato.Value, Randomness.Value);

            }
        }

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return ShakeStrength;
            yield return Duration;
            yield return Vibrato;
            yield return Randomness;
        }
        

        public void Initialized()
        {
            isShakeActive = false;
            print(isShakeActive);
        }
    }
}