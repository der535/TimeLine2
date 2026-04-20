using TimeLine.LevelEditor.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.Cursor
{
    public class CursorController : MonoBehaviour
    {
        [SerializeField] private Image cursorImage;
        [Space]
        [SerializeField] private Sprite cursorIdel;
        [SerializeField] private Sprite cursorHover;
        [SerializeField] private Sprite cursorResizeHorizontal;
        [SerializeField] private Sprite cursorResizeVertical;
        [SerializeField] private Sprite cursorResizeDiagonallyLeft;
        [SerializeField] private Sprite cursorResizeDiagonallyRight;

        private CameraReferences _cameraReferences;
        private RectTransform _cursorRectTransform;

        public RectTransform canvasRect;

        
        [Inject]
        private void Construct(CameraReferences cameraReferences)
        {
            _cameraReferences = cameraReferences;
        }

        private void Start()
        {
            UnityEngine.Cursor.visible = false;

            cursorImage.sprite = cursorIdel;
            _cursorRectTransform = (RectTransform)cursorImage.transform;
            cursorImage.SetNativeSize();
        }

        public void SetIdel()
        {
            cursorImage.sprite = cursorIdel;
            cursorImage.SetNativeSize();
        }
        
        public void SetHover()
        {
            cursorImage.sprite = cursorHover;
            cursorImage.SetNativeSize();
        }
        
        public void SetResizeHorizontal()
        {
            cursorImage.sprite = cursorResizeHorizontal;
            cursorImage.SetNativeSize();
        }
        
        public void SetResizeVertical()
        {
            cursorImage.sprite = cursorResizeVertical;
            cursorImage.SetNativeSize();
        }
        
        public void SetResizeDiagonallyLeft()
        {
            cursorImage.sprite = cursorResizeDiagonallyLeft;
            cursorImage.SetNativeSize();
        }

        public void SetResizeDiagonallyRight()
        {
            cursorImage.sprite = cursorResizeDiagonallyRight;
            cursorImage.SetNativeSize();
        }
    
        void Update()
        {
            Vector2 localPoint;
        
            // Преобразуем позицию мыши в локальную точку Canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, 
                Mouse.current.position.ReadValue(), 
                _cameraReferences.editUICamera, // Укажите камеру, если Canvas в режиме Screen Space - Camera
                out localPoint
            );

            if (cursorImage.sprite == cursorResizeHorizontal)
            {
                _cursorRectTransform.anchoredPosition = localPoint + new Vector2(0, -_cursorRectTransform.sizeDelta.y/2);
            }
            else if (cursorImage.sprite == cursorResizeVertical)
            {
                _cursorRectTransform.anchoredPosition = localPoint + new Vector2(_cursorRectTransform.sizeDelta.x/2, 0);
            }
            else
            {
                _cursorRectTransform.anchoredPosition = localPoint + new Vector2(_cursorRectTransform.sizeDelta.x/2, -_cursorRectTransform.sizeDelta.y/2);
            }
            // Устанавливаем позицию объекта
        }
    }
}