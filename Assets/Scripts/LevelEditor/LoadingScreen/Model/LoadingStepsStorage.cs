using System;
using System.Collections.Generic;

namespace TimeLine.LevelEditor.LoadingScreen.Model
{
    public class LoadingStepData
    {
        public string Description;
        public Action Action;
        public Func<bool> WaitCondition;
    }

    [Serializable]
    public class LoadingStepsStorage
    {
        public List<LoadingStepData> Value = new();

        public void AddStep(string desc, Action action) => Value.Add(new LoadingStepData { Description = desc, Action = action });
        public void AddStep(string desc, Action action, Func<bool> condition) => Value.Add(new LoadingStepData { Description = desc, Action = action, WaitCondition = condition });
        public int GetCountSteps() => Value.Count;
        public LoadingStepData GetStep(int i) => Value[i];
    }
}