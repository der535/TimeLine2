using UnityEngine;

namespace TimeLine
{
    public class EditorCameraMover : MonoBehaviour
    {
        [SerializeField] private Camera editorCamera;
        [SerializeField] private float moveSpeed = 5f;

        void Update()
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (UnityEngine.Input.GetKey(KeyCode.LeftArrow))
                horizontal = -1f;
            else if (UnityEngine.Input.GetKey(KeyCode.RightArrow))
                horizontal = 1f;

            if (UnityEngine.Input.GetKey(KeyCode.UpArrow))
                vertical = 1f;
            else if (UnityEngine.Input.GetKey(KeyCode.DownArrow))
                vertical = -1f;

            Vector3 movement = new Vector3(horizontal, vertical, 0f) * moveSpeed * Time.deltaTime;
            editorCamera.transform.Translate(movement);
        }
    }
}