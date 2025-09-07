using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TimeLine.Keyframe
{
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
            // Удаляем существующий ключевой кадр с таким же временем
            RemoveKeyframeAtTime(time);
            
            Keyframe newKeyframe = new Keyframe(time);
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
            // Удаляем существующий ключевой кадр с таким же временем
            RemoveKeyframeAtTime(newKeyframe.Ticks);
            
            Keyframes.Add(newKeyframe);
            SortKeyframes();
        }

        // Новый метод для удаления ключевых кадров по времени
        private void RemoveKeyframeAtTime(double time)
        {
            Keyframes.RemoveAll(k => k.Ticks == time);
        }

        // Остальные методы без изменений
        public void Evaluate(double time)
        {
            if (Keyframes.Count == 0 || TargetObject == null) return;
        
            Keyframe prev = Keyframes.LastOrDefault(k => k.Ticks <= time);
            Keyframe next = Keyframes.FirstOrDefault(k => k.Ticks >= time);

            if (prev == null && next == null) return;
            
            if (prev == null) next.Apply(TargetObject);
            else if (next == null) prev.Apply(TargetObject);
            else if (prev != next)
            {
                double t = Mathf.InverseLerp((float)prev.Ticks, (float)next.Ticks, (float)time);
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
                keyframe.Ticks += Math.Round(ticks);
            }
        }
        
        public Track Copy(GameObject target)
        {
            Track newTrack = new Track(target, this.TrackName);
            newTrack.Keyframes = this.Keyframes.Select(kf => kf.Clone()).ToList();
            return newTrack;
        }

        public void SortKeyframes()
        {
            Keyframes = Keyframes.OrderBy(k => k.Ticks).ToList();
        }
    }
}