
namespace TimeLine.Keyframe
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Track
    {
        public GameObject targetObject;
        public string trackName;
        public List<Keyframe> keyframes = new List<Keyframe>();
        private int lastFoundIndex = 0;

        public Track(GameObject target, string trackName)
        {
            this.trackName = trackName;
            targetObject = target;
        }

        public Keyframe AddKeyframe(float time, AnimationData adata = null)
        {
            Keyframe newKeyframe = new Keyframe(time);

            // if (adata == null)
            // {
            //     foreach (IAnimatable animatable in targetObject.GetComponents<IAnimatable>())
            //     {
            //         foreach (AnimationData data in animatable.GetAnimationData())
            //         {
            //             newKeyframe.AddData(data);
            //         }
            //     }
            // }
            newKeyframe.AddData(adata);
            

            keyframes.Add(newKeyframe);
            SortKeyframes();
            return newKeyframe;
        }

        public void Evaluate(float time)
        {
            if (keyframes.Count == 0 || targetObject == null) return;
        
            // Находим текущий и следующий ключевые кадры
            Keyframe prev = keyframes.LastOrDefault(k => k.time <= time);
            Keyframe next = keyframes.FirstOrDefault(k => k.time >= time);
            
            Debug.Log(prev?.time);
            Debug.Log(next?.time);
        
            if (prev == null && next == null) return;
            
            Debug.Log("Супер");
        
            // Если только один ключевой кадр
            if (prev == null) next.Apply(targetObject);
            else if (next == null) prev.Apply(targetObject);
            // Интерполяция между кадрами
            else if (prev != next)
            {
                float t = Mathf.InverseLerp(prev.time, next.time, time);
                Debug.Log("Интерполяция");
                prev.Interpolate(next, targetObject, t);
            }
            else
            {
                Debug.Log("Применение");
                prev.Apply(targetObject);
            }
        }

        private void SortKeyframes()
        {
            keyframes = keyframes.OrderBy(k => k.time).ToList();
        }
    }
}
