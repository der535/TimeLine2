using UnityEngine;

namespace TimeLine
{
    public class C_EdgeColliderSelectionCube : MonoBehaviour
    {
        [SerializeField] private GameObject selectionCube;
        [SerializeField] private float baseScale = 0.1f; // Желаемый размер на экране
        [SerializeField] private Camera sceneCamera;
        
        private void UpdateScale()
        {
            if (sceneCamera.orthographic)
            {
                float scale = sceneCamera.orthographicSize * baseScale;
                selectionCube.transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        public void SetActive(bool active)
        {
            selectionCube.SetActive(active);
            if (active) UpdateScale(); // Сразу обновляем при включении
        }

        public void SetPosition(Vector3 position)
        {
            selectionCube.transform.position = position;
            UpdateScale();
        }

        public Vector3 GetPosition()
        {
            return selectionCube.transform.position;
        }
    }
}