using System;
using System.Collections.Generic;
using System.Linq;
using TimeLine.Components;
using TimeLine.Keyframe;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components.ComponentsLogic;
using TimeLine.LevelEditor.TimeLineWindows.Composition.Components.EntityComponent;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using Unity.Entities;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.CopyComponent
{
    public class CopyComponentController
    {
        private Type _copyComponent;
        private List<(Track, string)> _copyTrack = new();
        private Dictionary<string, ParameterPacket> _copyParemetersData = new();


        private KeyframeTrackStorage _keyframeTrackStorage;
        private TrackObjectStorage _trackObjectStorage;

        private TrackObject _trackObject;
        
        private DiContainer _container;

        [Inject]
        private void Construct(KeyframeTrackStorage keyframeTrackStorage, TrackObjectStorage _trackObjectStorage, DiContainer container)
        {
            _keyframeTrackStorage = keyframeTrackStorage;
            this._trackObjectStorage = _trackObjectStorage;
            _container = container;
        }

        public bool CompareTypes(BaseParameterComponent target)
        {
            if (target == null || _copyComponent == null) return false;
            return target.GetType() == _copyComponent;
        }
        
        public Type GetCopyComponent() => _copyComponent;

        // public void Copy(BaseParameterComponent component)
        // {
        //     _copyComponent = component.GetType();
        //     _copyParemetersData = component.GetParameterData();
        //     foreach (var track in _keyframeTrackStorage.GetTracks())
        //     {
        //         if (component.gameObject == track.Track.TargetObject)
        //         {
        //             _copyTrack.Add((track.Track.Copy(track.TrackObjectData., track.TreeNode.Path));
        //         }
        //     }
        // }

        // public void PasteNewComponent(Entity targetToPaste)
        // {
        //     // 1. Проверка на наличие данных в буфере
        //     if (_copyComponent == null || _copyParemetersData == null) return;
        //
        //     var component =  (BaseParameterComponent) EntityComponentController.AddComponentSafely(_copyComponent.Name, targetToPaste, _container);
        //     
        //     var trackObjectData = _trackObjectStorage.GetTrackObjectData(targetToPaste);
        //     
        //     
        //     // Применяем параметры (числа, строки и т.д.)
        //     component.SetParameterData(_copyParemetersData);
        //     
        //     
        //     if (trackObjectData == null) return;
        //
        //     foreach (var (sourceTrack, path) in _copyTrack)
        //     {
        //         // СЛУЧАЙ А: Трека еще нет — создаем полностью
        //         TreeNode newTreeNode = trackObjectData.branch.AddNode(path);
        //         Track newTrackData = sourceTrack;
        //         newTrackData.TargetEntity = targetToPaste;
        //         newTrackData.cachedComponent = component;
        //
        //
        //         
        //         _keyframeTrackStorage.AddTrack(
        //             newTreeNode,
        //             newTrackData,
        //             trackObjectData.components.Data,
        //             trackObjectData.branch.ID
        //         );
        //     }
        // }

        // public void PasteValues(GameObject targetToPaste, BaseParameterComponent component)
        // {
        //     // 1. Проверка на наличие данных в буфере
        //     if (_copyComponent == null || _copyParemetersData == null) return;
        //
        //     // 2. (Опционально) Проверка на соответствие типов, чтобы не вставить данные 
        //     // от Transform в компонент Light, например.
        //     if (component.GetType() != _copyComponent)
        //     {
        //         Debug.LogWarning("Тип целевого компонента не совпадает со скопированным.");
        //         return;
        //     }
        //
        //     // Применяем параметры (числа, строки и т.д.)
        //     component.SetParameterData(_copyParemetersData);
        //
        //     var trackObjectData = _trackObjectStorage.GetTrackObjectData(targetToPaste);
        //     if (trackObjectData == null) return;
        //
        //     foreach (var (sourceTrack, path) in _copyTrack)
        //     {
        //         var searchResult = trackObjectData.branch.FindNode(path);
        //
        //         if (searchResult.node == null)
        //         {
        //             // СЛУЧАЙ А: Трека еще нет — создаем полностью
        //             TreeNode newTreeNode = trackObjectData.branch.AddNode(path);
        //             Track newTrackData = sourceTrack;
        //             newTrackData.TargetObject = targetToPaste;
        //             newTrackData.cachedComponent = component;
        //             newTrackData.activeObjectControllerComponent = targetToPaste.GetComponent<ActiveObjectControllerComponent>();
        //
        //
        //             _keyframeTrackStorage.AddTrack(
        //                 newTreeNode,
        //                 newTrackData,
        //                 trackObjectData.components.Data,
        //                 trackObjectData.branch.ID
        //             );
        //         }
        //         else
        //         {
        //             // СЛУЧАЙ Б: Трек уже есть — обновляем только ключевые кадры
        //             var existingTrack = _keyframeTrackStorage.GetTrack(searchResult.node);
        //             if (existingTrack != null)
        //             {
        //                 Debug.Log(existingTrack.Keyframes.Count);
        //                 // Важно: копируем именно список кадров, чтобы не было ссылочной связи с оригиналом
        //                 existingTrack.Keyframes = sourceTrack.Keyframes;
        //                 Debug.Log(existingTrack.Keyframes.Count);
        //                 existingTrack.cachedComponent = component; // Обновляем ссылку на компонент
        //             }
        //         }
        //     }
        // }
    }
}