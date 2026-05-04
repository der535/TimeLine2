using UnityEngine;
using UnityEngine.InputSystem;

namespace TimeLine
{
    public class WindowsTester : MonoBehaviour
    {
        private void Update()
        {
            if(Keyboard.current.f1Key.wasPressedThisFrame)
            {
                Debug.Log("Создать окно");
            }
        }
    }
}
