using System;
using System.Collections.Generic;
using System.Linq;
using TimeLine.LevelEditor.ECS.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.Keyframe;
using TimeLine.LevelEditor.Save;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject;
using Unity.Entities;
using UnityEngine;

namespace TimeLine.Keyframe
{
    public class Track
    {
        public Entity TargetEntity;
        public ActiveObjectControllerComponent activeObjectControllerComponent;
        public string TrackName;
        public Color AnimationColor;
        public List<Keyframe> Keyframes = new();
        public TrackObjectData groupObject;
        public Component cachedComponent;


        // public Track(GameObject target, string trackName, Color animationColor)
        // {
        //     this.TrackName = trackName;
        //     // TargetObject = target;
        //     AnimationColor = animationColor;
        //     // if(target != null)
        //         // activeObjectControllerComponent = TargetObject.GetComponent<ActiveObjectControllerComponent>();
        // }

        public Track(Entity target, string trackName, Color animationColor)
        {
            this.TrackName = trackName;
            TargetEntity = target;
            AnimationColor = animationColor;
        }
        
        // public Keyframe AddKeyframe(double time, AnimationData adata)
        // {
        //     // Debug.Log(time);
        //     // Удаляем существующий ключевой кадр с таким же временем
        //     var type = RemoveKeyframeAtTime(time);
        //
        //     Keyframe newKeyframe;
        //     if (type is { } typ2e)
        //         newKeyframe = new Keyframe(time, typ2e);
        //     else
        //         newKeyframe = new Keyframe(time, Keyframe.InterpolationType.Linear);
        //     newKeyframe.AddData(adata);
        //
        //     Keyframes.Add(newKeyframe);
        //     SortKeyframes();
        //     if (cachedComponent == null)
        //     {
        //         Type t = adata.GetComponentType();
        //         // cachedComponent = TargetObject.GetComponent(t);
        //     }
        //
        //     return newKeyframe;
        // }
        
        public Keyframe AddKeyframe(double time, EntityAnimationData adata)
        {
            // Debug.Log(time);
            // Удаляем существующий ключевой кадр с таким же временем
            var type = RemoveKeyframeAtTime(time);

            Keyframe newKeyframe;
            if (type is { } typ2e)
                newKeyframe = new Keyframe(time, typ2e);
            else
                newKeyframe = new Keyframe(time, Keyframe.InterpolationType.Linear);
            newKeyframe.AddData(adata);

            Keyframes.Add(newKeyframe);
            SortKeyframes();

            return newKeyframe;
        }


        public void SetParent(TrackObjectData trackObject = null)
        {
            groupObject = trackObject;
        }

        public (IAnimationApplyer, Component) GetApplyer()
        {
            return (Keyframes[0].GetEntityData(), cachedComponent);
        }

        public void RemoveKeyframe(Keyframe keyframe)
        {
            Keyframes.Remove(keyframe);
            SortKeyframes();
        }

        public void AddKeyframe(Keyframe newKeyframe)
        {
            // Удаляем существующий ключевой кадр с таким же временем
            var type = RemoveKeyframeAtTime(newKeyframe.Ticks);
            if (type is { } typ2e)
                newKeyframe.Interpolation = typ2e;

            Keyframes.Add(newKeyframe);
            SortKeyframes();
        }

        // Новый метод для удаления ключевых кадров по времени
        private Keyframe.InterpolationType? RemoveKeyframeAtTime(double time)
        {
            // Ищем последний кадр, попадающий в диапазон погрешности
            var target = Keyframes.FindLast(k => Math.Abs(k.Ticks - time) < 0.1);

            if (target == null)
            {
                // Если кадр не найден, возвращаем значение по умолчанию
                return null;
            }

            // Запоминаем тип интерполяции найденного кадра
            Keyframe.InterpolationType lastType = target.Interpolation;

            // Удаляем все кадры в этой временной точке
            Keyframes.RemoveAll(k => Math.Abs(k.Ticks - time) < 0.1);

            return lastType;
        }

        public double GetOffset()
        {
            if (groupObject == null)
                return 0;
            else
            {
                return groupObject.GetKeyframeTrackOffset();
            }
        }

        public void Evaluate(double time)
        {
            if (Keyframes.Count == 0) return;

            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;

            double offset = groupObject?.GetKeyframeTrackOffset() ?? 0;

            // 1. Сброс всех флагов ТОЛЬКО при выключении объекта
            // Это позволит кадрам "переинициализироваться" при следующем включении

            bool isActive = manager.IsComponentEnabled<EntityActiveTag>(TargetEntity);
            if (!isActive)
            {
                for (int i = 0; i < Keyframes.Count; i++) 
                    Keyframes[i].IsInitialized = false;
                return;
            }

            // 2. Находим текущую пару кадров
            Keyframe prev = Keyframes.LastOrDefault(k => k.Ticks + offset <= time);
            Keyframe next = Keyframes.FirstOrDefault(k => k.Ticks + offset > time);

            // 3. Инициализируем только то, что нужно сейчас
            // Инициализируем prev (прошлый/текущий)
            if (prev != null && !prev.IsInitialized)
            {
                prev.Initialize?.Invoke();
                prev.IsInitialized = true;
            }

            // Инициализируем next (будущий, для плавной интерполяции)
            if (next != null && !next.IsInitialized)
            {
                next.Initialize?.Invoke();
                next.IsInitialized = true;
            }

            // 4. Применяем значения
            if (prev != null && next != null)
            {
                double start = prev.Ticks + offset;
                double end = next.Ticks + offset;
        
                if (Math.Abs(start - end) < 0.0001)
                    prev.Apply(TargetEntity);
                else
                {
                    double t = (time - start) / (end - start);
                    prev.Interpolate(next, TargetEntity, t);
                }
            }
            else if (prev != null)
            {
                prev.Apply(TargetEntity);
            }
            else if (next != null)
            {
                next.Apply(TargetEntity);
            }
        }
        public void AddOffsetKeyframes(double ticks)
        {
            foreach (var keyframe in Keyframes)
            {
                keyframe.Ticks += ticks;
            }
        }

        public List<KeyframeSaveData> SaveKeyframes()
        {
            List<KeyframeSaveData> keyframes = new();
            foreach (var keyframe in Keyframes)
            {
                keyframes.Add(keyframe.ToSaveData());
            }

            return keyframes;
        }

        public EntityAnimationData GetAnimationData() => Keyframes[0].GetEntityData();

        public Track Copy(Entity target)
        {
            Track newTrack = new Track(target, this.TrackName, this.AnimationColor);
            newTrack.Keyframes = this.Keyframes.Select(kf => kf.Clone()).ToList();
            return newTrack;
        }

        public void SortKeyframes()
        {
            Keyframes = Keyframes.OrderBy(k => k.Ticks).ToList();
        }
    }
}