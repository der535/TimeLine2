using TimeLine.Keyframe.AnimationDatas.TransformComponent;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.Keyframe.AnimationDatas.TransformComponent.Rotation;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.Tabs.InspectorTab.Keyframe.AnimationDatas.TransformComponent.Position;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class TransformComponentDrawer : IComponentDrawer
    {
        private KeyframeCreator _keyframeCreator;
        private CustomInspectorDrawer _customInspectorDrawer;
        private TrackObjectStorage _trackObjectStorage;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
            _trackObjectStorage = trackObjectStorage;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(TransformComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component, false);
            string id = _trackObjectStorage.GetTrackObjectDataOrParentGroupBySceneObject(target).sceneObjectID;

            if (component is TransformComponent tc)
            {
                // Системные флаги
                _customInspectorDrawer.CreateBoolField(tc.isActiveTrackObject);
                _customInspectorDrawer.CreateBoolField(tc.isTempTrackObject);

                _customInspectorDrawer.AddSpace(10);

                // Позиция
                _customInspectorDrawer.CreateFloatField(tc.XPosition, id ,() =>
                    tc.CreateKeyframeManual(new XPositionData(tc.XPosition.Value), tc.XPosition));
                _customInspectorDrawer.CreateBoolField(tc.XPositionActive);

                _customInspectorDrawer.CreateFloatField(tc.YPosition, id,() =>
                    tc.CreateKeyframeManual(new YPositionData(tc.YPosition.Value), tc.YPosition));
                _customInspectorDrawer.CreateBoolField(tc.YPositionActive);

                _customInspectorDrawer.AddSpace(30);

                // Офсеты (без ключей, так как передаем null в callback)
                _customInspectorDrawer.CreateFloatField(tc.XPositionOffset, id,null);
                _customInspectorDrawer.CreateFloatField(tc.YPositionOffset, id,null);

                _customInspectorDrawer.AddSpace(30);

                // Ротация
                _customInspectorDrawer.CreateFloatField(tc.XRotation, id,() =>
                    tc.CreateKeyframeManual(new XRotationData(tc.XRotation.Value), tc.XRotation));
                _customInspectorDrawer.CreateBoolField(tc.XRotationActive);

                _customInspectorDrawer.CreateFloatField(tc.YRotation, id,() =>
                    tc.CreateKeyframeManual(new YRotationData(tc.YRotation.Value), tc.YRotation));
                _customInspectorDrawer.CreateBoolField(tc.YRotationActive);

                _customInspectorDrawer.CreateFloatField(tc.ZRotation, id,() =>
                    tc.CreateKeyframeManual(new ZRotationData(tc.ZRotation.Value), tc.ZRotation));
                _customInspectorDrawer.CreateBoolField(tc.ZRotationActive);

                _customInspectorDrawer.AddSpace(30);

                // Масштаб
                _customInspectorDrawer.CreateFloatField(tc.XScale, id,() =>
                    tc.CreateKeyframeManual(new XScaleData(tc.XScale.Value), tc.XScale));
                _customInspectorDrawer.CreateBoolField(tc.XScaleActive);

                _customInspectorDrawer.CreateFloatField(tc.YScale, id,() =>
                    tc.CreateKeyframeManual(new YScaleData(tc.YScale.Value), tc.YScale));
                _customInspectorDrawer.CreateBoolField(tc.YScaleActive);
            }
        }
    }
}