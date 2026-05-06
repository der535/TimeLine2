using System;
using System.Runtime.InteropServices;
using TimeLine.LevelEditor.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.Cursor
{
    public class CursorController : MonoBehaviour
    {
        [SerializeField] private RectTransform canvasRect;

        [Space]
        [SerializeField] private Image cursorImage;
        
        [Space]
        [SerializeField] private Sprite cursorArrow;
        [SerializeField] private Sprite cursorHand;
        [SerializeField] private Sprite cursorResizeEW;
        [SerializeField] private Sprite cursorResizeNS;
        [SerializeField] private Sprite cursorResizeNWSE;
        [SerializeField] private Sprite cursorResizeNESW;

        private CameraReferences _cameraReferences;
        private RectTransform _cursorRectTransform;

        private int _currentState;
        private IntPtr _currentCursorHandle;

        [Inject]
        private void Construct(CameraReferences cameraReferences)
        {
            _cameraReferences = cameraReferences;
        }

        private void Start()
        {
            cursorImage.gameObject.SetActive(false);
#if UNITY_EDITOR
            cursorImage.gameObject.SetActive(true);
            UnityEngine.Cursor.visible = false;
            cursorImage.sprite = cursorArrow;
            cursorImage.SetNativeSize();
            _cursorRectTransform = (RectTransform)cursorImage.transform;
#endif
        }

        [DllImport("user32.dll")]
        static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

        [DllImport("user32.dll")]
        static extern IntPtr SetCursor(IntPtr hCursor);

        public void SetState(WindowsCursorID state)
        {
#if UNITY_EDITOR
            switch (state)
            {
                case WindowsCursorID.Arrow:
                    cursorImage.sprite = cursorArrow;
                    cursorImage.SetNativeSize();
                    break;
                case WindowsCursorID.Hand:
                    cursorImage.sprite = cursorHand;
                    cursorImage.SetNativeSize();
                    break;
                case WindowsCursorID.SizeNS:
                    cursorImage.sprite = cursorResizeNS;
                    cursorImage.SetNativeSize();
                    break;
                case WindowsCursorID.SizeWE:
                    cursorImage.sprite = cursorResizeEW;
                    cursorImage.SetNativeSize();
                    break;
                case WindowsCursorID.SizeNWSE:
                    cursorImage.sprite = cursorResizeNWSE;
                    cursorImage.SetNativeSize();
                    break;
                case WindowsCursorID.SizeNESW:
                    cursorImage.sprite = cursorResizeNESW;
                    cursorImage.SetNativeSize();
                    break;
            }
#endif

#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            _currentState = (int)state;
            _currentCursorHandle = LoadCursor(IntPtr.Zero, _currentState);
            SetCursor(_currentCursorHandle);
#endif
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


            if (cursorImage.sprite == cursorArrow || cursorImage.sprite == cursorHand)
            {
                _cursorRectTransform.anchoredPosition = localPoint + new Vector2(_cursorRectTransform.sizeDelta.x / 2, -_cursorRectTransform.sizeDelta.y / 2);
            }
            else
            {
                _cursorRectTransform.anchoredPosition = localPoint;
            }
            // Устанавливаем позицию объекта
        }
    }

    public enum WindowsCursorID : int
    {
        /// <summary>
        /// Стандартный курсор-стрелка.
        /// </summary>
        Arrow = 32512,

        /// <summary>
        /// I-образный курсор. Появляется при наведении на текст.
        /// </summary>
        IBeam = 32513,

        /// <summary>
        /// Курсор ожидания (песочные часы или вращающийся круг). 
        /// Указывает, что система занята и не принимает ввод.
        /// </summary>
        Wait = 32514,

        /// <summary>
        /// Перекрестие. Обычно используется для точного выделения или в графических редакторах.
        /// </summary>
        Cross = 32515,

        /// <summary>
        /// Вертикальная стрелка, указывающая строго вверх.
        /// </summary>
        UpArrow = 32516,

        /// <summary> 
        /// Диагональное изменение размера (Северо-Запад — Юго-Восток). ↖↘
        /// </summary>
        SizeNWSE = 32642,

        /// <summary>
        /// Диагональное изменение размера (Северо-Восток — Юго-Запад). ↗↙
        /// </summary>
        SizeNESW = 32643,

        /// <summary>
        /// Горизонтальное изменение размера (Запад — Восток).
        /// </summary>
        SizeWE = 32644,

        /// <summary>
        /// Вертикальное изменение размера (Север — Юг).
        /// </summary>
        SizeNS = 32645,

        /// <summary>
        /// Перемещение. Четыре стрелки, указывающие во всех направлениях.
        /// </summary>
        SizeAll = 32646,

        /// <summary>
        /// Перечеркнутый круг. Указывает на то, что действие недоступно или запрещено.
        /// </summary>
        No = 32648,

        /// <summary>
        /// Указывающая рука. Стандарт для гиперссылок и интерактивных элементов.
        /// </summary>
        Hand = 32649,

        /// <summary>
        /// Фоновая работа. Стрелка вместе с маленьким индикатором ожидания. 
        /// Приложение занято, но ввод по-прежнему возможен.
        /// </summary>
        AppStarting = 32650,

        /// <summary>
        /// Справка. Стрелка с вопросительным знаком.
        /// </summary>
        Help = 32651,

        /// <summary>
        /// Булавка. Выбор местоположения на карте или в системе (Windows 10+).
        /// </summary>
        Pin = 32671,

        /// <summary>
        /// Личность/Человек. Выбор контакта или пользователя (Windows 10+).
        /// </summary>
        Person = 32672
    }
}