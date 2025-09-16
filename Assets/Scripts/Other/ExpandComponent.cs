using UnityEngine;

namespace TimeLine
{
    public class ExpandComponent : MonoBehaviour
    {
        [SerializeField] private GameObject triangle;
        [SerializeField] private ComponentUI componentUI;

        private bool _isExpanded = true;
        
        public void ChangeVisibility()
        {
            _isExpanded = !_isExpanded;

            if (!_isExpanded)
            {
                triangle.transform.localRotation = Quaternion.Euler(0, 0, 270);
                componentUI.Hide();
            }
            else
            {
                triangle.transform.localRotation = Quaternion.Euler(0, 0, 180);
                componentUI.Show();
            }
        }
    }
}
