using UnityEngine;

namespace TimeLine
{
    public class EdgeColliderSelectionCube : MonoBehaviour
    {
        [SerializeField] private GameObject selectionCube;

        public void SetActive(bool active)
        {
            selectionCube.SetActive(active);
        }

        public void SetPosition(Vector3 position)
        {
            selectionCube.transform.position = position;
        }

        public Vector3 GetPosition()
        {
            return selectionCube.transform.position;
        }
    }
}
