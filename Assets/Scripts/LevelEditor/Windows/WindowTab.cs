using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TimeLine
{
    public class WindowTab : MonoBehaviour,
        IDragHandler, IBeginDragHandler, IEndDragHandler,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _imageBg;
        [SerializeField] private Image _image;
        [SerializeField] private TMP_Text _nameText;

        [Space]
        [SerializeField] private Color _baseColor;
        [SerializeField] private Color _pointerEnterColor;
        [SerializeField] private Color _dragColor;

        private RectTransform _rectTransform;
        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private FlyingWindowTab _flyingWindowTab;

        private RectTransform _originalParent;
        private int _originalSiblingIndex;
        private RectTransform _placeholder;
        private TabBar _currentBar;
        private bool _isDragging;

        private void Awake()
        {
            _rectTransform = (RectTransform)transform;
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            _flyingWindowTab = FindAnyObjectByType<FlyingWindowTab>();
        }

        public void OnPointerEnter(PointerEventData _) { if (!_isDragging) _imageBg.color = _pointerEnterColor; }
        public void OnPointerExit(PointerEventData _) { if (!_isDragging) _imageBg.color = _baseColor; }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
            _originalParent = (RectTransform)transform.parent;
            _originalSiblingIndex = transform.GetSiblingIndex();

            // ---- placeholder на наше место ----
            var phGO = new GameObject("TabPlaceholder",
                typeof(RectTransform), typeof(LayoutElement));
            _placeholder = (RectTransform)phGO.transform;
            var image = phGO.AddComponent<Image>();
            image.color = new Color(0.341f, 0.369f, 0.427f, 1f);
            _placeholder.SetParent(_originalParent, false);
            _placeholder.SetSiblingIndex(_originalSiblingIndex);

            var le = phGO.GetComponent<LayoutElement>();
            le.preferredWidth = _rectTransform.rect.width;
            le.preferredHeight = _rectTransform.rect.height;
            le.flexibleWidth = 0;

            // ---- сами уезжаем на корень канваса, чтобы не мешать layout-у ----
            _rectTransform.SetParent(_canvas.transform, true);
            _rectTransform.SetAsLastSibling();

            // визуально нас представл€ет FlyingTab, поэтому пр€чемс€
            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;

            _currentBar = _originalParent.GetComponentInParent<TabBar>();

            _flyingWindowTab.StartMove(_nameText.text, _image.sprite);
            _imageBg.color = _dragColor;
            UnityEngine.Cursor.visible = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            var screenPos = eventData.position;
            var cam = eventData.pressEventCamera;

            var over = FindBestBar(screenPos, cam);

            if (over != null)
            {
                if (_placeholder.parent != over.Container)
                    _placeholder.SetParent(over.Container, false);

                int idx = over.CalculateInsertIndex(screenPos, cam, _placeholder);
                if (_placeholder.GetSiblingIndex() != idx)
                    _placeholder.SetSiblingIndex(idx);

                if (!_placeholder.gameObject.activeSelf)
                    _placeholder.gameObject.SetActive(true);

                _currentBar = over;
            }
            else
            {
                if (_placeholder.gameObject.activeSelf)
                    _placeholder.gameObject.SetActive(false);
                _currentBar = null;
            }
        }

        private TabBar FindBestBar(Vector2 screenPos, Camera cam)
        {
            TabBar strict = null;
            TabBar nearestInflated = null;
            float nearestDist = float.MaxValue;

            foreach (var bar in TabBar.All)
            {
                if (strict == null && bar.ContainsScreenPointStrict(screenPos, cam))
                {
                    strict = bar;
                    continue;
                }
                if (bar.ContainsScreenPointInflated(screenPos, cam))
                {
                    float d = bar.DistanceToScreenPoint(screenPos, cam);
                    if (d < nearestDist) { nearestDist = d; nearestInflated = bar; }
                }
            }
            return strict ?? nearestInflated;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_currentBar != null && _placeholder.gameObject.activeSelf)
            {
                int idx = _placeholder.GetSiblingIndex();
                _rectTransform.SetParent(_currentBar.Container, false);
                _rectTransform.SetSiblingIndex(idx);
            }
            else
            {
                // дропнули в пустоту Ч возвращаем как было
                // (тут же можно вместо этого спавнить новое плавающее окно а-л€ Unity Editor)
                _rectTransform.SetParent(_originalParent, false);
                _rectTransform.SetSiblingIndex(_originalSiblingIndex);
            }

            Destroy(_placeholder.gameObject);
            _placeholder = null;

            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

            _flyingWindowTab.StopMove();
            _imageBg.color = _baseColor;
            UnityEngine.Cursor.visible = true;
            _isDragging = false;
            _currentBar = null;
        }
    }
}