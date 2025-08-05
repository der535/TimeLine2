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
            // Ищем существующий ключевой кадр
            Keyframe keyframe = keyframes.FirstOrDefault(k => Mathf.Approximately(k.time, time));
            if (keyframe == null)
            {
                keyframe = new Keyframe(time);
                keyframes.Add(keyframe);
                SortKeyframes();
            }

            if (adata != null)
            {
                keyframe.AddData(adata);
            }
            return keyframe;
        }

        public void Evaluate(float time)
        {
            if (keyframes.Count == 0 || targetObject == null) return;

            // Бинарный поиск ближайшего ключевого кадра
            int low = 0;
            int high = keyframes.Count - 1;
            int closestIndex = 0;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                if (keyframes[mid].time < time)
                {
                    closestIndex = mid;
                    low = mid + 1;
                }
                else
                {
                    high = mid - 1;
                }
            }

            // Используем оптимизацию для последовательного воспроизведения
            if (closestIndex < lastFoundIndex) lastFoundIndex = 0;
            closestIndex = Mathf.Max(lastFoundIndex, closestIndex);
            lastFoundIndex = closestIndex;

            Keyframe prev = keyframes[closestIndex];
            Keyframe next = (closestIndex < keyframes.Count - 1) ? keyframes[closestIndex + 1] : null;

            if (prev == null && next == null) return;

            // Если только один ключевой кадр
            if (prev == null) next.Apply(targetObject);
            else if (next == null) prev.Apply(targetObject);
            // Интерполяция между кадрами
            else if (prev != next)
            {
                float t = Mathf.InverseLerp(prev.time, next.time, time);
                prev.Interpolate(next, targetObject, t);
            }
            else
            {
                prev.Apply(targetObject);
            }
        }

        private void SortKeyframes()
        {
            keyframes = keyframes.OrderBy(k => k.time).ToList();
        }
    }
}