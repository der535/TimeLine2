using UnityEngine;

namespace TimeLine
{
    public static class FindTopmostParentWithComponent
    {
        public static T Find<T>(Transform current) where T : MonoBehaviour
        {
            T lastFound = null;
            Transform parent = current.parent;

            while (parent != null)
            {
                T component = parent.GetComponent<T>();
                if (component != null)
                    lastFound = component;
                parent = parent.parent;
            }

            return lastFound;
        }
    }
}