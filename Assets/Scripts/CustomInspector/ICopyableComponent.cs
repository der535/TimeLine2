using UnityEngine;

namespace TimeLine
{
    public interface ICopyableComponent
    {
        void CopyTo(Component targetComponent);
        Component Copy(GameObject targetGameObject);
    }
}