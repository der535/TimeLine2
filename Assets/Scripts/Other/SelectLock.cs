using UnityEngine;

namespace TimeLine
{
    public class SelectLock : MonoBehaviour
    {
        public bool IsLocked = false;

        public void SetLock(bool value)
        {
            IsLocked = value;
        }
    }
}
