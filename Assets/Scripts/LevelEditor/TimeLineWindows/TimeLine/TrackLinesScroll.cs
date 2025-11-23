using TimeLine.Installers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class TrackLinesScroll : MonoBehaviour
    {
        [SerializeField] private TrackStorage trackStorage;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform scrollContent;
        [SerializeField] private RectTransform scrollview;
        [Space] 
        [SerializeField] private float speed;
        [SerializeField] private float deltatime = 0.01f;

        private ActionMap _actionMap;
        private MainObjects _mainObjects;

        [Inject]
        private void Construct(ActionMap actionMap, MainObjects mainObjects)
        {
            _actionMap = actionMap;
            _mainObjects = mainObjects;
        }

        private void Start()
        {
            _actionMap.Editor.MouseScroll.started += context =>
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed() && CheckMouseInWindow())
                {
                    float scrollDelta = -context.ReadValue<float>();
        
                    float viewportHeight = scrollRect.viewport.rect.height;
                    float contentHeight = scrollContent.rect.height;

                    if (contentHeight <= viewportHeight) return;

                    float scrollFactor = viewportHeight / contentHeight;
                    float delta = scrollDelta * speed * deltatime * scrollFactor;

                    scrollRect.verticalNormalizedPosition = Mathf.Clamp01(
                        scrollRect.verticalNormalizedPosition + delta
                    );
                }
            };
        }
        
        private bool CheckMouseInWindow()
        {
            return RectTransformUtility.RectangleContainsScreenPoint(scrollview,  
                UnityEngine.Input.mousePosition, _mainObjects.MainCamera);  
        }
    }
}