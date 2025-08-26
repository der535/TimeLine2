using UnityEngine;

namespace TimeLine.CustomInspector.UI
{
    public interface IComponentDrawer
    {
        public void Setup(CustomInspectorDrawer drawer, KeyframeCreater keyframeCreater);
        public bool GetComponent(Component component);
        public void Draw(Component component, GameObject target);
    }
}