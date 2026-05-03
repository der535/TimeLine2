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
        internal ComponentNames _copyComponent;
        private List<(string, Track)> _copyTrack = new();
        private Dictionary<string, object> _copyParemetersData = new();
        
        private KeyframeTrackStorage _keyframeTrackStorage;
        private TrackObjectStorage _trackObjectStorage;
        private EntityComponentController _entityComponentController;

        private TrackObject _trackObject;
        
        [Inject]
        private void Construct(KeyframeTrackStorage keyframeTrackStorage, TrackObjectStorage _trackObjectStorage,
            DiContainer container, EntityComponentController entityComponentController)
        {
            _keyframeTrackStorage = keyframeTrackStorage;
            this._trackObjectStorage = _trackObjectStorage;
            _entityComponentController = entityComponentController;
        }

        public bool CheckAvailabilityType(ComponentNames target)
        {
            return _copyComponent == target;
        }
        
        public bool CompareTypes(ComponentNames target)
        {
            if(_copyParemetersData.Count == 0) return false;
            return CheckAvailabilityType(target);
        }
        

        public void Copy(ComponentNames componentName, Entity entity)
        {
            _copyTrack.Clear();
            _copyParemetersData.Clear();

            _copyComponent = componentName;
            _copyParemetersData = _entityComponentController.Save(entity, componentName);
            foreach (var track in _keyframeTrackStorage.GetTracks())
            {
                if (track.Track.ComponentNames == componentName && track.Track.TargetEntity == entity)
                {
                    _copyTrack.Add((track.TreeNode.Path, track.Track.Copy(entity)));
                }
            }
        }

        public (ComponentNames componentNames, Dictionary<string, object> copyParemetersData, List<(string, Track)> copyTrack ) CopyReturn(ComponentNames componentName, Entity entity)
        {
            var componentNames = componentName;
            var copyParemetersData = _entityComponentController.Save(entity, componentName);
            List<(string, Track)> copyTrack = new();
            foreach (var track in _keyframeTrackStorage.GetTracks())
            {
                if (track.Track.ComponentNames == componentName && track.Track.TargetEntity == entity)
                {
                    copyTrack.Add((track.TreeNode.Path, track.Track.Copy(entity)));
                }
            }
            return (componentNames, copyParemetersData, copyTrack);
        }

        public void PasteNewComponent(Entity targetToPaste)
        {
            // 1. Проверка на наличие данных в буфере
            if (_copyParemetersData == null) return;

            _entityComponentController.AddComponentSafely(_copyComponent, targetToPaste);

            var trackObjectData = _trackObjectStorage.GetTrackObjectData(targetToPaste);


            // Применяем параметры (числа, строки и т.д.)
            _entityComponentController.Load(targetToPaste, new Dictionary<ComponentNames, Dictionary<string, object>>()
            {
                { _copyComponent, _copyParemetersData }
            });


            if (trackObjectData == null) return;

            foreach (var track in _copyTrack)
            {
                // СЛУЧАЙ А: Трека еще нет — создаем полностью
                TreeNode newTreeNode = trackObjectData.branch.AddNode(track.Item1);
                Track newTrackData = track.Item2;
                newTrackData.TargetEntity = targetToPaste;
                newTrackData.ComponentNames = _copyComponent;

                _keyframeTrackStorage.AddTrack(
                    newTreeNode,
                    newTrackData,
                    trackObjectData.components.Data,
                    trackObjectData.branch.ID, false
                );
            }
        }
        
        public void PasteNewComponent(Entity targetToPaste, ComponentNames _copyComponent, Dictionary<string, object> _copyParemetersData)
        {
            // 1. Проверка на наличие данных в буфере
            if (_copyParemetersData == null) return;

            _entityComponentController.AddComponentSafely(_copyComponent, targetToPaste);

            var trackObjectData = _trackObjectStorage.GetTrackObjectData(targetToPaste);


            // Применяем параметры (числа, строки и т.д.)
            _entityComponentController.Load(targetToPaste, new Dictionary<ComponentNames, Dictionary<string, object>>()
            {
                { _copyComponent, _copyParemetersData }
            });


            if (trackObjectData == null) return;

            foreach (var track in _copyTrack)
            {
                // СЛУЧАЙ А: Трека еще нет — создаем полностью
                TreeNode newTreeNode = trackObjectData.branch.AddNode(track.Item1);
                Track newTrackData = track.Item2;
                newTrackData.TargetEntity = targetToPaste;
                newTrackData.ComponentNames = _copyComponent;

                _keyframeTrackStorage.AddTrack(
                    newTreeNode,
                    newTrackData,
                    trackObjectData.components.Data,
                    trackObjectData.branch.ID, false
                );
            }
        }

        public void PasteValues(ComponentNames componentName, Entity entity)
        {
            // 1. Проверка на наличие данных в буфере
            if (_copyParemetersData == null || _copyParemetersData.Count == 0) return;
        
            // 2. (Опционально) Проверка на соответствие типов, чтобы не вставить данные 
            // от Transform в компонент Light, например.
            if (componentName != _copyComponent)
            {
                Debug.LogWarning("Тип целевого компонента не совпадает со скопированным.");
                return;
            }
        
            _entityComponentController.Load(entity, new Dictionary<ComponentNames, Dictionary<string, object>>()
            {
                { _copyComponent, _copyParemetersData }
            });
        
            var trackObjectData = _trackObjectStorage.GetTrackObjectData(entity);
            if (trackObjectData == null) return;
        
            foreach (var (path, sourceTrack) in _copyTrack)
            {
                var searchResult = trackObjectData.branch.FindNode(path);
        
                if (searchResult == null)
                {
                    // СЛУЧАЙ А: Трека еще нет — создаем полностью
                    TreeNode newTreeNode = trackObjectData.branch.AddNode(path);
                    Track newTrackData = sourceTrack;
                    newTrackData.TargetEntity = entity;
                    newTrackData.ComponentNames = componentName;
        
        
                    _keyframeTrackStorage.AddTrack(
                        newTreeNode,
                        newTrackData,
                        trackObjectData.components.Data,
                        trackObjectData.branch.ID, false
                    );
                }
                else
                {
                    // СЛУЧАЙ Б: Трек уже есть — обновляем только ключевые кадры
                    var existingTrack = _keyframeTrackStorage.GetTrack(searchResult);
                    if (existingTrack != null)
                    {
                        // Важно: копируем именно список кадров, чтобы не было ссылочной связи с оригиналом
                        existingTrack.Keyframes = sourceTrack.Keyframes;
                        existingTrack.ComponentNames = componentName; // Обновляем ссылку на компонент
                    }
                }
            }
        }
        
         public void PasteValues(ComponentNames componentName, Entity entity, Dictionary<string, object> _copyParemetersData,  List<(string, Track)> _copyTrack )
        {
            // 1. Проверка на наличие данных в буфере
            if (_copyParemetersData == null || _copyParemetersData.Count == 0) return;
        
            // 2. (Опционально) Проверка на соответствие типов, чтобы не вставить данные 
            // от Transform в компонент Light, например.

            _entityComponentController.Load(entity, new Dictionary<ComponentNames, Dictionary<string, object>>()
            {
                { componentName, _copyParemetersData }
            });
        
            var trackObjectData = _trackObjectStorage.GetTrackObjectData(entity);
            if (trackObjectData == null) return;
        
            foreach (var (path, sourceTrack) in _copyTrack)
            {
                var searchResult = trackObjectData.branch.FindNode(path);
        
                if (searchResult == null)
                {
                    // СЛУЧАЙ А: Трека еще нет — создаем полностью
                    TreeNode newTreeNode = trackObjectData.branch.AddNode(path);
                    Track newTrackData = sourceTrack;
                    newTrackData.TargetEntity = entity;
                    newTrackData.ComponentNames = componentName;
        
        
                    _keyframeTrackStorage.AddTrack(
                        newTreeNode,
                        newTrackData,
                        trackObjectData.components.Data,
                        trackObjectData.branch.ID, false
                    );
                }
                else
                {
                    // СЛУЧАЙ Б: Трек уже есть — обновляем только ключевые кадры
                    var existingTrack = _keyframeTrackStorage.GetTrack(searchResult);
                    if (existingTrack != null)
                    {
                        // Важно: копируем именно список кадров, чтобы не было ссылочной связи с оригиналом
                        existingTrack.Keyframes = sourceTrack.Keyframes;
                        existingTrack.ComponentNames = componentName; // Обновляем ссылку на компонент
                    }
                }
            }
        }
    }
}