using TimeLine.Components;
using TimeLine.CustomInspector.UI.FieldUI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    /// <summary>
    /// Скрипт отвечает за кнопку которая появляется в конце всех компонентов в инспекторе
    /// </summary>
    public class AddComponentButton : MonoBehaviour, IGetFieldHeight
    {
         [SerializeField] private RectTransform rectTransform;
         [SerializeField] private Button button;
         
         private AddComponentWindowsController _addComponentWindowsData;

         [Inject]
         private void Construct(AddComponentWindowsController addComponentWindowsData)
         {
             _addComponentWindowsData = addComponentWindowsData;
         }
         
         internal void Setup(GameObject target)
         {
             button.onClick.AddListener(() =>
             {
                 _addComponentWindowsData.UpdateComponents(target);
                 _addComponentWindowsData.SetActiveComponentWindow(true);
             });
         }
         
         public float GetFieldHeight()
         {
             return rectTransform.rect.height;
         }
    }
}