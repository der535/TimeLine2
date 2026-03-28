using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Scale;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.InspectorTab.InspectorView.FieldUI;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class TransformComponentDrawer : IComponentDrawer
    {
        private KeyframeCreator _keyframeCreator;
        private CustomInspectorDrawer _customInspectorDrawer;
        private TrackObjectStorage _trackObjectStorage;
        private ToolsController _toolsController;
        
        

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator, ToolsController toolsController)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
            _trackObjectStorage = trackObjectStorage;
            _toolsController = toolsController;
        }

        public bool GetComponent(ComponentType component)
        {
            return component.GetManagedType() == typeof(LocalTransform);
        }

        public bool GetComponent(List<ComponentType> component)
        {
            return
            (
                CheckIfComponentTypeInList.Check(component, typeof(LocalTransform))
            );
        }

        public void Draw(Entity target)
        {
            _customInspectorDrawer.CreateComponent(ComponentNames.Transform, target, false);
            var manager = World.DefaultGameObjectInjectionWorld.EntityManager;


            LocalTransform transform = manager.GetComponentData<LocalTransform>(target);
            RotationData rotationData = manager.GetComponentData<RotationData>(target);
            ObjectPositionOffsetData objectPositionOffsetData = manager.GetComponentData<ObjectPositionOffsetData>(target);
            TrackObjectPacket trackObjectPacket = _trackObjectStorage.GetTrackObjectData(target);

            
            var posX = new FloatParameter("posX", 0, Color.white);
            var posY = new FloatParameter("posY", 0, Color.white);
            var rotZ = new FloatParameter("rotZ", 0, Color.white);
            var scaleX = new FloatParameter("scaleX", 0, Color.white);
            var scaleY = new FloatParameter("scaleY", 0, Color.white);
            
            _toolsController.OnValueChanged += () =>
            {
                LocalTransform transform = manager.GetComponentData<LocalTransform>(target);
                PostTransformMatrix postTransformMatrix = manager.GetComponentData<PostTransformMatrix>(target);
                RotationData rotationData = manager.GetComponentData<RotationData>(target);

                float3 scale = GetScaleFromMatrix.Get(postTransformMatrix.Value); 
                
                posX.Value = transform.Position.x;
                posY.Value = transform.Position.y;
                rotZ.Value = rotationData.RotateZ;
                scaleX.Value = scale.x;
                scaleY.Value = scale.y;
            };
            
            
            // --- POSITION ---
            _customInspectorDrawer.CreateFloatField(transform.Position.x, "Position/X",
                () =>
                {
                    _keyframeCreator.CreateKeyframe(new EntityXPositionData(transform.Position.x), target, "Position/X",
                        Color.red,
                        "Transform");
                }, (newValue) =>
                {
                    transform = manager.GetComponentData<LocalTransform>(target);
                    PositionData positionData = manager.GetComponentData<PositionData>(target);
                    ObjectPositionOffsetData objectPositionOffsetData = manager.GetComponentData<ObjectPositionOffsetData>(target);
                
                    positionData.Position.x = newValue;
                    transform.Position.x = newValue + objectPositionOffsetData.Offset.x;
                    
                    Debug.Log(transform.Position.x );
                
                    manager.SetComponentData(target, transform);
                    manager.SetComponentData(target, positionData);
                    manager.SetComponentData(target, objectPositionOffsetData);
                },  trackObjectPacket, "Transform.Position.X", posX);

            _customInspectorDrawer.CreateFloatField(transform.Position.y, "Position/Y", () =>
            {
                _keyframeCreator.CreateKeyframe(new EntityYPositionData(transform.Position.y), target, "Position/Y",
                    Color.blue,
                    "Transform");
            }, (newValue) =>
            {
                transform = manager.GetComponentData<LocalTransform>(target);
                PositionData positionData = manager.GetComponentData<PositionData>(target);
                ObjectPositionOffsetData objectPositionOffsetData = manager.GetComponentData<ObjectPositionOffsetData>(target);
                
                positionData.Position.y = newValue;
                transform.Position.y = newValue + objectPositionOffsetData.Offset.y;
                
                manager.SetComponentData(target, transform);
                manager.SetComponentData(target, positionData);
                manager.SetComponentData(target, objectPositionOffsetData);
            },  trackObjectPacket, "Transform.Position.Y", posY);

            _customInspectorDrawer.AddSpace(10);

            // --- ROTATION ---
            // Используем eulerAngles для получения углов в радианах
            float3 eulerRadians = math.Euler(transform.Rotation);
            float3 eulerDegrees = math.degrees(eulerRadians);

            // _customInspectorDrawer.CreateFloatField(eulerDegrees.x, "Rotation/X", () =>
            // {
            //     _keyframeCreator.CreateKeyframe(new EntityXRotationData(GetDegree.FromQuaternion(transform.Rotation).x),
            //         target, "Rotation/X",
            //         Color.red,
            //         "Transform");
            // }, (newValue) =>
            // {
            //     transform = manager.GetComponentData<LocalTransform>(target);
            //     eulerDegrees.x = newValue;
            //     transform.Rotation = quaternion.Euler(math.radians(eulerDegrees));
            //     manager.SetComponentData(target, transform);
            // },  trackObjectPacket, "Transform.Rotation.X");
            //
            // _customInspectorDrawer.CreateFloatField(eulerDegrees.y, "Rotation/Y", () =>
            // {
            //     _keyframeCreator.CreateKeyframe(new EntityYRotationData(GetDegree.FromQuaternion(transform.Rotation).y),
            //         target, "Rotation/Y",
            //         Color.blue,
            //         "Transform");
            // }, (newValue) =>
            // {
            //     transform = manager.GetComponentData<LocalTransform>(target);
            //     eulerDegrees.y = newValue;
            //     transform.Rotation = quaternion.Euler(math.radians(eulerDegrees));
            //     manager.SetComponentData(target, transform);
            // },  trackObjectPacket, "Transform.Rotation.Y");

            _customInspectorDrawer.CreateFloatField(rotationData.RotateZ, "Rotation/Z", () =>
            {
                RotationData rotationData = manager.GetComponentData<RotationData>(target);
                _keyframeCreator.CreateKeyframe(new EntityZRotationData(rotationData.RotateZ),
                    target, "Rotation/z",
                    Color.green,
                    "Transform");
            }, (newValue) =>
            {
                transform = manager.GetComponentData<LocalTransform>(target);
                eulerDegrees.z = newValue;
                transform.Rotation = quaternion.Euler(math.radians(eulerDegrees));
                rotationData.RotateZ = newValue;
                manager.SetComponentData(target, transform);
                manager.SetComponentData(target, rotationData);
            },  trackObjectPacket, "Transform.Rotation.Z", rotZ);

            _customInspectorDrawer.AddSpace(10);

            // --- SCALE (NON-UNIFORM) ---
            if (!manager.HasComponent<PostTransformMatrix>(target))
            {
                manager.AddComponentData(target, new PostTransformMatrix { Value = float4x4.identity });
            }

            if (!Mathf.Approximately(transform.Scale, 1.0f))
            {
                transform = manager.GetComponentData<LocalTransform>(target);
                transform.Scale = 1.0f;
                manager.SetComponentData(target, transform);
            }

            var postMatrix = manager.GetComponentData<PostTransformMatrix>(target);
            float3 nonUniformScale =
                new float3(postMatrix.Value.c0.x, postMatrix.Value.c1.y, postMatrix.Value.c2.z);

            _customInspectorDrawer.CreateFloatField(nonUniformScale.x, "Scale/X", () =>
            {
                _keyframeCreator.CreateKeyframe(new EntityXScaleData(nonUniformScale.x), target, "Scale/X",
                    Color.red,
                    "Transform");
            }, (newValue) =>
            {
                transform = manager.GetComponentData<LocalTransform>(target);
                nonUniformScale.x = newValue;
                postMatrix.Value = float4x4.Scale(nonUniformScale);
                manager.SetComponentData(target, postMatrix);
            }, trackObjectPacket, "Transform.Scale.X", scaleX);

            _customInspectorDrawer.CreateFloatField(nonUniformScale.y, "Scale/Y", () =>
            {
                _keyframeCreator.CreateKeyframe(new EntityYScaleData(nonUniformScale.y), target, "Scale/Y",
                    Color.blue,
                    "Transform");
            }, (newValue) =>
            {
                transform = manager.GetComponentData<LocalTransform>(target);
                nonUniformScale.y = newValue;
                postMatrix.Value = float4x4.Scale(nonUniformScale);
                manager.SetComponentData(target, postMatrix);
            }, trackObjectPacket, "Transform.Scale.Y", scaleY);
        }
    }
}