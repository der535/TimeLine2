
using System;

namespace TimeLine.Keyframe
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Track
    {
        public GameObject TargetObject;
        public string TrackName;
        public List<Keyframe> Keyframes = new List<Keyframe>();
        private int _lastFoundIndex = 0;

        public Track(GameObject target, string trackName)
        {
            this.TrackName = trackName;
            TargetObject = target;
        }

        public Keyframe AddKeyframe(double time, AnimationData adata = null)
        {
            var time2 = Mathf.RoundToInt((float)time);
            // Debug.Log(time2);
            Keyframe newKeyframe = new Keyframe(time2);
            
            newKeyframe.AddData(adata);
            
            Keyframes.Add(newKeyframe);
            SortKeyframes();
            return newKeyframe;
        }

        public void RemoveKeyframe(Keyframe keyframe)
        {
            Keyframes.Remove(keyframe);
            SortKeyframes();
        }
        
        public void AddKeyframe(Keyframe newKeyframe)
        {
            Keyframes.Add(newKeyframe);
            SortKeyframes();
        }

        public void Evaluate(double time)
        {
            // Debug.Log(TrackName);
            if (Keyframes.Count == 0 || TargetObject == null) return;
        
            // Находим текущий и следующий ключевые кадры
            Keyframe prev = Keyframes.LastOrDefault(k => k.ticks <= time);
            Keyframe next = Keyframes.FirstOrDefault(k => k.ticks >= time);

            // Debug.Log(prev);
            // Debug.Log(next);
            
            if (prev == null && next == null) return;
            
            // Если только один ключевой кадр
            if (prev == null) next.Apply(TargetObject);
            else if (next == null) prev.Apply(TargetObject);
            
            // Интерполяция между кадрами
            else if (prev != next)
            {
                double t = Mathf.InverseLerp((float)prev.ticks, (float)next.ticks, (float)time);
                // Debug.Log(t);
                prev.Interpolate(next, TargetObject, t);
            }
            else
            {
                prev.Apply(TargetObject);
            }
        }

        public void AddOffsetKeyframes(double ticks)
        {
            foreach (var keyframe in Keyframes)
            {
                keyframe.ticks += Math.Round(ticks);
            }
        }
        
        // Добавленный метод копирования трека
        public Track Copy(GameObject target)
        {
            // Создаем новый трек с теми же параметрами
            Track newTrack = new Track(target, this.TrackName);
            
            // Глубокое копирование ключевых кадров
            newTrack.Keyframes = this.Keyframes.Select(kf => kf.Clone()).ToList();
            return newTrack;
        }

        public void SortKeyframes()
        {
            Keyframes = Keyframes.OrderBy(k => k.ticks).ToList();
        }
    }
}
