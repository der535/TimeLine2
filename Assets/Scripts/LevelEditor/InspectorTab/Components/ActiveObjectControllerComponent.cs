using System;
using System.Collections.Generic;
using NaughtyAttributes;
using TimeLine.CustomInspector.Logic.Parameter;
using TimeLine.LevelEditor.Tabs.InspectorTab.CustomInspector.Logic;
using UnityEngine;

namespace TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components
{
    public class ActiveObjectControllerComponent : BaseParameterComponent
    {
        /// <summary>
        /// false - disabled
        /// true - enabled
        /// </summary>
        public Action<bool> IsActiveChanged;
        public bool IsActive = false;

        protected override IEnumerable<InspectableParameter> GetParameters()
        {
            yield return new StringParameter(string.Empty, string.Empty);
        }

        public void Turn(bool active)
        {
            if (active) TurnOn();
            else TurnOff();
        }
        
        [Button]
        private void TurnOff()
        {
            IsActiveChanged?.Invoke(false);
            IsActive = false;
        }

        [Button]
        private void TurnOn()
        {
            IsActiveChanged?.Invoke(true);
            IsActive = true;
        }
    }
}