using System;
using System.Collections.Generic;
using System.Linq;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using Unity.Mathematics;
using UnityEngine;

namespace TimeLine.Keyframe
{
    public class Track
    {
        public GameObject TargetObject;
        public string TrackName;
        public Color AnimationColor;
        public List<Keyframe> Keyframes = new();
        public TrackObject groupObject;
        public Component cachedComponent;


        public Track(GameObject target, string trackName, Color animationColor)
        {
            this.TrackName = trackName;
            TargetObject = target;
            AnimationColor = animationColor;
        }

        public Keyframe AddKeyframe(double time, AnimationData adata)
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
            if (cachedComponent == null)
            {
                Type t = adata.GetComponentType();
                cachedComponent = TargetObject.GetComponent(t);
            }
            
            return newKeyframe;
        }

        public void SetParent(TrackObject trackObject = null)
        {
            groupObject = trackObject;
        }

        public (IAnimationApplyer, Component) GetApplyer()
        {
            return (Keyframes[0].GetData(), cachedComponent);
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
            // Debug.Log(type);
            if (type is { } typ2e)
                newKeyframe.Interpolation = typ2e;
            // Debug.Log(newKeyframe.Interpolation);
            if (cachedComponent == null)
            {
                Type t = newKeyframe.GetData().GetComponentType();
                cachedComponent = TargetObject.GetComponent(t);
            }
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
            
            if (Keyframes.Count == 0 || TargetObject == null) return;
            

            double offset;

            if (groupObject == null)
                offset = 0;
            else
            {
                offset = groupObject.GetKeyframeTrackOffset();
            }


            Keyframe prev = Keyframes.LastOrDefault(k => k.Ticks + offset <= time);
            Keyframe next = Keyframes.FirstOrDefault(k => k.Ticks + offset >= time);
            

            if (prev == null && next == null) return;

            if (prev == null)
            {
                next.Apply(cachedComponent);
            }
            else if (next == null)
            {
                prev.Apply(cachedComponent);
            }
            else if (prev != next)
            {
                double start = prev.Ticks + offset;
                double end = next.Ticks + offset;

                // Защита от деления на ноль
                if (start == end)
                {
                    prev.Apply(cachedComponent);
                }
                else
                {
                    double t = (time - start) / (end - start);
                    prev.Interpolate(next, cachedComponent, t);
                }
            }
            else
            {
                prev.Apply(cachedComponent);
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

        public AnimationData GetAnimationData() => Keyframes[0].GetData();
        
        public Track Copy(GameObject target)
        {
            Track newTrack = new Track(target, this.TrackName, this.AnimationColor);
            newTrack.Keyframes = this.Keyframes.Select(kf => kf.Clone()).ToList();
            return newTrack;
        }

        public void SortKeyframes()
        {
            Keyframes = Keyframes.OrderBy(k => k.Ticks).ToList();
        }
        
        public void Apply(float4 value)
        {
            Keyframes[0].GetData().Apply(cachedComponent, value);
        }
    }
}
