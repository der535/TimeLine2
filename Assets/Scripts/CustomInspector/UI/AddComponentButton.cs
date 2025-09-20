using TimeLine.Components;
using TimeLine.CustomInspector.UI.FieldUI;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine
{
    public class AddComponentButton : MonoBehaviour, IGetFieldHeight
    {
         [SerializeField] private RectTransform rectTransform;
         [SerializeField] private Button button;
         
         private AddComponentWindowsData _addComponentWindowsData;

         [Inject]
         private void Construct(AddComponentWindowsData addComponentWindowsData)
         {
             _addComponentWindowsData = addComponentWindowsData;
             print(_addComponentWindowsData);
         }
         
         internal void Setup(GameObject target)
         {
             button.onClick.AddListener(() =>
             {
                 _addComponentWindowsData.controller.UpdateComponents(target);
                 _addComponentWindowsData.windows.gameObject.SetActive(true);
             });
         }
         
         public float GetFieldHeight()
         {
             return rectTransform.rect.height;
         }
    }
}