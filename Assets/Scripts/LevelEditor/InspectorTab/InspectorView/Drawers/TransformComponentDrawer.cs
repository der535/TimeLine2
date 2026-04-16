using System;
using System.Collections.Generic;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.ECS;
using TimeLine.LevelEditor.ECS.Services;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Position;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe.AnimationDatas.TransformComponent.Scale;
using TimeLine.LevelEditor.General;
using TimeLine.LevelEditor.InspectorTab.InspectorView.Drawers;
using TimeLine.LevelEditor.InspectorTab.InspectorView.FieldUI;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.UI.Drawers;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent.Components;
using TimeLine.LevelEditor.TransformationSquare;
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
        private TransformationSquareController _transformationSquareController;
        private TimeLineRecorder _timeLineRecorder;

        public Action ToolsControllerAction;
        
        public Action OnStopPositionX;
        public Action OnStopPositionY;
        public Action OnStopRotation;
        public Action OnStopScaleX;
        public Action OnStopScaleY;

        public TransformComponentDrawer (TransformationSquareController transformationSquareController)
        {
            _transformationSquareController = transformationSquareController;
            
            _transformationSquareController.OnStopPositionX += () => OnStopPositionX?.Invoke();
            _transformationSquareController.OnStopPositionY += () => OnStopPositionY?.Invoke();
            _transformationSquareController.OnStopRotation += () => OnStopRotation?.Invoke();
            _transformationSquareController.OnStopScaleX += () => OnStopScaleX?.Invoke();
            _transformationSquareController.OnStopScaleY += () => OnStopScaleY?.Invoke();
        }
        

        public void Setup(CustomInspectorDrawer customInspectorDrawer, TrackObjectStorage trackObjectStorage,
            KeyframeCreator keyframeCreator, ToolsController toolsController, TimeLineRecorder timeLineRecorder)
        {
            _customInspectorDrawer = customInspectorDrawer;
            _keyframeCreator = keyframeCreator;
            _trackObjectStorage = trackObjectStorage;
            _toolsController = toolsController;
            _timeLineRecorder = timeLineRecorder;
            
            // _toolsController.OnValueChanged += () => ToolsControllerAction.Invoke();
            

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
          
            
            
            ToolsControllerAction = null;
            ToolsControllerAction += () =>
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

            Action createKeyFramePositionX =() =>
            {
                LocalTransform transform2 = manager.GetComponentData<LocalTransform>(target);
                _keyframeCreator.CreateKeyframe(new EntityXPositionData(transform2.Position.x), target, "Position/X",
                    Color.red,
                    "Transform", ComponentNames.Transform);
            };
                
            _customInspectorDrawer.CreateFloatField(transform.Position.x, "Position/X",
                createKeyFramePositionX, (newValue) =>
                {
                    transform = manager.GetComponentData<LocalTransform>(target);
                    PositionData positionData = manager.GetComponentData<PositionData>(target);
                    ObjectPositionOffsetData objectPositionOffsetData = manager.GetComponentData<ObjectPositionOffsetData>(target);
                
                    positionData.Position.x = newValue;
                    transform.Position.x = newValue + objectPositionOffsetData.Offset.x;
                
                    manager.SetComponentData(target, transform);
                    manager.SetComponentData(target, positionData);
                    manager.SetComponentData(target, objectPositionOffsetData);
                    
                    if (_timeLineRecorder.IsRecording()) createKeyFramePositionX.Invoke();
                    
                },  trackObjectPacket, "Transform.Position.X", posX);

                        
            Action createKeyFramePositionY =() =>
            {
                LocalTransform transform2 = manager.GetComponentData<LocalTransform>(target);
                _keyframeCreator.CreateKeyframe(new EntityYPositionData(transform2.Position.y), target, "Position/Y",
                    Color.blue,
                    "Transform", ComponentNames.Transform);
            };
            
            _customInspectorDrawer.CreateFloatField(transform.Position.y, "Position/Y", createKeyFramePositionY, (newValue) =>
            {
                transform = manager.GetComponentData<LocalTransform>(target);
                PositionData positionData = manager.GetComponentData<PositionData>(target);
                ObjectPositionOffsetData objectPositionOffsetData = manager.GetComponentData<ObjectPositionOffsetData>(target);
                
                positionData.Position.y = newValue;
                transform.Position.y = newValue + objectPositionOffsetData.Offset.y;
                
                manager.SetComponentData(target, transform);
                manager.SetComponentData(target, positionData);
                manager.SetComponentData(target, objectPositionOffsetData);
                
                if(_timeLineRecorder.IsRecording()) createKeyFramePositionY.Invoke();
                
            },  trackObjectPacket, "Transform.Position.Y", posY);

            
            
            
            
            
            _customInspectorDrawer.AddSpace(10);
            
            
            
            
            

            // --- ROTATION ---
            // Используем eulerAngles для получения углов в радианах
            float3 eulerRadians = math.Euler(transform.Rotation);
            float3 eulerDegrees = math.degrees(eulerRadians);
            
            Action createKeyFrameRotationZ =() =>
            {
                RotationData rotationData2 = manager.GetComponentData<RotationData>(target);
                _keyframeCreator.CreateKeyframe(new EntityZRotationData(rotationData2.RotateZ),
                    target, "Rotation/z",
                    Color.green,
                    "Transform", ComponentNames.Transform);
            };
            
            _customInspectorDrawer.CreateFloatField(rotationData.RotateZ, "Rotation/Z", createKeyFrameRotationZ, (newValue) =>
            {
                transform = manager.GetComponentData<LocalTransform>(target);
                eulerDegrees.z = newValue;
                transform.Rotation = quaternion.Euler(math.radians(eulerDegrees));
                rotationData.RotateZ = newValue;
                manager.SetComponentData(target, transform);
                manager.SetComponentData(target, rotationData);
                
                if(_timeLineRecorder.IsRecording()) createKeyFrameRotationZ.Invoke();
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

            
            Action createKeyFrameScaleX =() =>
            {
                var postMatrix2 = manager.GetComponentData<PostTransformMatrix>(target);
                float3 nonUniformScale2 =
                    new float3(postMatrix2.Value.c0.x, postMatrix2.Value.c1.y, postMatrix2.Value.c2.z);
                _keyframeCreator.CreateKeyframe(new EntityXScaleData(nonUniformScale2.x), target, "Scale/X",
                    Color.red,
                    "Transform", ComponentNames.Transform);
            };

            
            _customInspectorDrawer.CreateFloatField(nonUniformScale.x, "Scale/X", createKeyFrameScaleX, (newValue) =>
            {
                transform = manager.GetComponentData<LocalTransform>(target);
                nonUniformScale.x = newValue;
                postMatrix.Value = float4x4.Scale(nonUniformScale);
                manager.SetComponentData(target, postMatrix);
                
                if(_timeLineRecorder.IsRecording()) createKeyFrameScaleX.Invoke();
            }, trackObjectPacket, "Transform.Scale.X", scaleX);

            
                        
            Action createKeyFrameScaleY = () =>
            {
                var postMatrix2 = manager.GetComponentData<PostTransformMatrix>(target);
                float3 nonUniformScale2 =
                    new float3(postMatrix2.Value.c0.x, postMatrix2.Value.c1.y, postMatrix2.Value.c2.z);
                _keyframeCreator.CreateKeyframe(new EntityYScaleData(nonUniformScale2.y), target, "Scale/Y",
                    Color.blue,
                    "Transform", ComponentNames.Transform);
            };
            
            _customInspectorDrawer.CreateFloatField(nonUniformScale.y, "Scale/Y", createKeyFrameScaleY, (newValue) =>
            {
                transform = manager.GetComponentData<LocalTransform>(target);
                nonUniformScale.y = newValue;
                postMatrix.Value = float4x4.Scale(nonUniformScale);
                manager.SetComponentData(target, postMatrix);
                
                if(_timeLineRecorder.IsRecording()) createKeyFrameScaleY.Invoke();
            }, trackObjectPacket, "Transform.Scale.Y", scaleY);



            OnStopPositionX = null;
            OnStopPositionX += () =>
            {if(_timeLineRecorder.IsRecording())
                createKeyFramePositionX.Invoke();
            };
            
            OnStopPositionY = null;
            OnStopPositionY += () =>
            {
                if (_timeLineRecorder.IsRecording())
                    createKeyFramePositionY.Invoke();
            };

            OnStopRotation = null;
            OnStopRotation += () =>
            {
                if (_timeLineRecorder.IsRecording())
                    createKeyFrameRotationZ.Invoke();
            };
            
            OnStopScaleX = null;
            OnStopScaleX += () =>
            {
                if (_timeLineRecorder.IsRecording())
                    createKeyFrameScaleX.Invoke();
            };

            OnStopScaleY = null;
            OnStopScaleY += () =>
            {
                if (_timeLineRecorder.IsRecording())
                    createKeyFrameScaleY.Invoke();
            };


        }
    }
}