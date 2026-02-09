using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.ContextMenu
{
    public class ContextMenuController : MonoBehaviour
    {
        [SerializeField] private RectTransform panel;

        [FormerlySerializedAs("root")] [SerializeField]
        private RectTransform rootContent;

        [SerializeField] private CanvasScaler canvas;
        [SerializeField] private Camera camera;
        [SerializeField] private ContextMenuItemData contextMenuButton;

        private List<ContextMenuItemData> _buttons = new();
        private ActionMap _actionMap;

        [Inject]
        private void Construct(ActionMap actionMap)
        {
            _actionMap = actionMap;
        }

        private void Start()
        {
            _actionMap.Editor.MouseLeft.performed += _ => { OnGlobalClick(); };
        }

        private void OnGlobalClick()
        {
            // Если меню и так закрыто, ничего не делаем
            if (!panel.gameObject.activeSelf) return;

            // Проверяем, находится ли мышь в момент клика над RectTransform нашей панели
            if (!RectTransformUtility.RectangleContainsScreenPoint(panel, Mouse.current.position.ReadValue(), camera))
            {
                panel.gameObject.SetActive(false);
            }
        }

        internal void ShowMenu()
        {
            panel.gameObject.SetActive(true);
            PositionMenu();
        }

        private void PositionMenu()
        {
            // Принудительно обновляем размеры, если кнопки только что созданы
            LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

            Vector2 localMousePos;
            RectTransform canvasRect = canvas.transform as RectTransform;

            // Переводим позицию мыши в координаты внутри Canvas (где центр - это 0,0)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                UnityEngine.Input.mousePosition,
                camera,
                out localMousePos
            );

            float menuWidth = panel.rect.width;
            float menuHeight = panel.rect.height;

            // Границы Canvas (отрицательные слева/снизу, положительные справа/сверху)
            float canvasMinX = canvasRect.rect.xMin;
            float canvasMaxX = canvasRect.rect.xMax;
            float canvasMinY = canvasRect.rect.yMin;
            float canvasMaxY = canvasRect.rect.yMax;

            // Рассчитываем X
            float targetX = localMousePos.x + menuWidth / 2; // Сдвиг вправо от курсора
            if (targetX + menuWidth / 2 > canvasMaxX) // Если правый край вылез
            {
                targetX = localMousePos.x - menuWidth / 2; // Прыгаем влево от курсора
            }

            // Рассчитываем Y
            float targetY = localMousePos.y - menuHeight / 2; // Сдвиг вниз от курсора
            if (targetY - menuHeight / 2 < canvasMinY) // Если нижний край вылез
            {
                targetY = localMousePos.y + menuHeight / 2; // Прыгаем вверх от курсора
            }

            panel.anchoredPosition = new Vector2(targetX, targetY);
        }

        public void Setup(List<(Action, string, bool)> items)
        {
            foreach (var button in _buttons)
            {
                Destroy(button.gameObject);
            }
            _buttons.Clear();

            foreach (var item in items)
            {
                var contextMenuItem = Instantiate(contextMenuButton, rootContent);
                _buttons.Add(contextMenuItem);
                contextMenuItem.Button.onClick.AddListener(() =>
                {
                    panel.gameObject.SetActive(false);
                    item.Item1.Invoke();
                });
                contextMenuItem.Text.text = item.Item2;

                if (item.Item3 == false)
                {
                    contextMenuItem.Button.interactable = false;
                    contextMenuItem.Text.color = Color.gray;
                }

            }
        }
    }
}