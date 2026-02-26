using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects.TrackObject
{
    public class TrackObjectView : MonoBehaviour, ITrackObjectView
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private RectTransform rect;

        public bool GetActive() => rect.gameObject.activeSelf;

        public void Destroy()
        {
            Destroy(rect.gameObject);
        }

        /// <summary>
        /// Прячет трекобжект
        /// </summary>
        public void Hide()
        {
            rect.gameObject.SetActive(false);
        }

        /// <summary>
        /// Показывает трекобжект
        /// </summary>
        public void Show()
        {
            rect.gameObject.SetActive(true);
        }

        public Transform GetParent()
        {
            return transform.parent;
        }

        public void SetParent(Transform parent)
        {
            transform.parent = parent;
        }

        public Vector2 GetSizeDelta()
        {
            return rect.sizeDelta;
        }

        public Vector2 GetAnchorPosition()
        {
            return rect.anchoredPosition;
        }

        public Vector2 GetOffsetMax()
        {
            return rect.offsetMax;
        }

        public Vector2 GetOffsetMin()
        {
            return rect.offsetMin;
        }

        public RectTransform GetRectTransform()
        {
            return rect;
        }

        public void SetOffsetMax(Vector2 offset)
        {
            rect.offsetMax = offset;
        }

        public void SetOffsetMin(Vector2 offset)
        {
            rect.offsetMin = offset;
        }

        public void SetSizeDelta(Vector2 size)
        {
            rect.sizeDelta = size;
        }

        public void SetAnchorPosition(Vector2 position)
        {
            rect.anchoredPosition = position;
        }

        public void SetColor(Color c)
        {
            image.color = c;
        }

        /// <summary>
        /// Переименовывает трек обжект
        /// </summary>
        /// <param name="name"></param>
        public void Rename(string name)
        {
            nameText.text = name;
        }
    }
}