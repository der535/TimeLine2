using TimeLine.Parent;
using TMPro;
using UnityEngine;

namespace TimeLine.CustomInspector.UI.Drawers
{
    public class ParentDrawer : IComponentDrawer
    {
        private CustomInspectorDrawer _customInspectorDrawer;

        public void Setup(CustomInspectorDrawer customInspectorDrawer, KeyframeCreater keyframeCreater)
        {
            _customInspectorDrawer = customInspectorDrawer;
        }

        public bool GetComponent(Component component)
        {
            return component.GetType() == typeof(ParentComponent);
        }

        public void Draw(Component component, GameObject target)
        {
            _customInspectorDrawer.CreateComponent(component.GetType().Name);

            if (component is ParentComponent parentComponent)
            {
                TMP_Dropdown dropdown = _customInspectorDrawer.CreateDropDownField("Parent object");
                parentComponent.Setup(dropdown);
            }
        }
    }
}