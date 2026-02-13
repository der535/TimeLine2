using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.RadialSunburstDrawer;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.Tabs.InspectorTab.Keyframe.AnimationDatas.TransformComponent.Position;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class RadialSunburstDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer = null;
        private KeyframeCreator _keyframeCreator = null;
        private TrackObjectStorage _trackObjectStorage = null;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
            _trackObjectStorage = trackObjectStorage;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(RadialSunburstMaterial);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, true);
            // var id = target.GetComponent<SceneObjectLink>().trackObjectData.sceneObjectID;
            string id = _trackObjectStorage.GetTrackObjectDataOrParentGroupBySceneObject(target).sceneObjectID;
            SceneObjectLink link = target.GetComponent<SceneObjectLink>();
            if (component is RadialSunburstMaterial componentComponent)
            {
                //todo дописать скрипт
                _customInspectorDrawer.CreateColorField(componentComponent.Color1, () =>
                {
                    _keyframeCreator.CreateKeyframe(new RadialSunburstMaterialColor1(componentComponent.Color1.Value),
                        target,
                        componentComponent.GetType().Name, componentComponent.Color1);
                }, id);
                _customInspectorDrawer.CreateColorField(componentComponent.Color2, () =>
                {
                    _keyframeCreator.CreateKeyframe(new RadialSunburstMaterialColor2(componentComponent.Color2.Value),
                        target,
                        componentComponent.GetType().Name, componentComponent.Color2);
                }, id);
                _customInspectorDrawer.CreateIntField(componentComponent.SegmentsCount, null);
                _customInspectorDrawer.CreateFloatField(componentComponent.TwistIntensity,                    link.trackObjectData,
                    (BaseParameterComponent)component, id, null);
                _customInspectorDrawer.CreateFloatField(componentComponent.RotationSpeed,                    link.trackObjectData,
                    (BaseParameterComponent)component, id, () =>
                {
                    _keyframeCreator.CreateKeyframe(new RadialSunburstMaterialRotationSpeed(componentComponent.RotationSpeed.Value),
                        target,
                        componentComponent.GetType().Name, componentComponent.RotationSpeed);
                });
            }
        }
    }
}