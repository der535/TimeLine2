namespace TimeLine.Keyframe
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    public class Keyframe
    {
        public float time;
        private List<AnimationData> animationData = new List<AnimationData>();
    
        public Keyframe(float time)
        {
            this.time = time;
        }
    
// В классе Keyframe
        private Dictionary<string, CustomParameterData> parameterMap;

        public void AddData(AnimationData data)
        {
            if (data is CustomParameterData customData)
            {
                if (parameterMap == null) 
                    parameterMap = new Dictionary<string, CustomParameterData>();
        
                parameterMap[customData.ParameterName] = customData;
            }
            else
            {
                // Обработка других типов данных
            }
        }
    
        public void Apply(GameObject target)
        {
            foreach (AnimationData data in animationData)
            {
                data.Apply(target);
            }
        }
    
        public void Interpolate(Keyframe next, GameObject target, float t)
        {
            var allParams = new HashSet<string>();
    
            // Собираем все уникальные имена параметров
            foreach (var data in animationData.Concat(next.animationData))
            {
                if (data is CustomParameterData customData)
                {
                    allParams.Add(customData.ParameterName);
                }
            }
            
            Debug.Log(allParams.Count);
    
            // Интерполяция для каждого параметра
            foreach (var paramName in allParams)
            {
                AnimationData current = animationData
                    .FirstOrDefault(d => d is CustomParameterData c && c.ParameterName == paramName);
        
                AnimationData nextData = next.animationData
                    .FirstOrDefault(d => d is CustomParameterData c && c.ParameterName == paramName);
        
                if (current != null && nextData != null)
                {
                    current.Interpolate(nextData, t).Apply(target);
                }
                else if (current != null)
                {
                    current.Apply(target);
                }
                else if (nextData != null)
                {
                    nextData.Apply(target);
                }
            }
        }
    }
}