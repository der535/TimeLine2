using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.LevelEffects;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
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

        private ShakeCameraController _shakeCameraController;
        private TrackObjectStorage _trackObjectStorage;
        private Main _main;
        
        private SceneObjectLink _sceneObjectLink;
        private InitializedComponentController _initializedComponentController;

        private bool isShakeActive;
        
        [Inject]
        private void Construct(ShakeCameraController shakeCameraController, TrackObjectStorage trackObjectStorage, Main main, InitializedComponentController initializedComponentController)
        {
            _shakeCameraController = shakeCameraController;
            _trackObjectStorage = trackObjectStorage;
            _main = main;
            _initializedComponentController = initializedComponentController;
        }

        private void Start()
        {
            _sceneObjectLink = gameObject.GetComponent<SceneObjectLink>();
            _initializedComponentController.Add(this, _sceneObjectLink.trackObjectData.trackObject);
        }

        private void Update()
        {
            if (TimeLineConverter.Instance.TicksCurrentTime() > _sceneObjectLink.trackObjectData.trackObject.GetGlobalTime() && isShakeActive == false)
            {
                isShakeActive = true;
                
                _shakeCameraController.Shake(ShakeStrength.Value, Duration.Value, Vibrato.Value, Randomness.Value);
            }
        }


        public override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return ShakeStrength;
            yield return Duration;
            yield return Vibrato;
            yield return Randomness;
        }
        

        public void Initialized()
        {
            isShakeActive = false;
        }

        public void OnDestroy()
        {
            _initializedComponentController.Remove(this);
        }
    }
}