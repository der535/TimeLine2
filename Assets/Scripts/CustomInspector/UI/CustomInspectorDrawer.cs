using System;
using NaughtyAttributes;
using TimeLine;
using UnityEngine;

public class CustomInspectorDrawer : MonoBehaviour
{
    [SerializeField] private RectTransform rootObject;
    
    [SerializeField] private ComponentUI componentUIPrefab;
    
    [Header("Fields")]
    [SerializeField] private FloatFieldUI floatFieldUIPrefab;
    
    private ComponentUI currentComponent;

    [Button]
    public void Clear()
    {
        for (int i = 0; i < rootObject.childCount; i++)
        {
            Destroy(rootObject.GetChild(i).gameObject);
        }
    }

    public void CreateComponent(string componentName)
    {
        currentComponent = Instantiate(componentUIPrefab, rootObject);
        currentComponent.SetName(componentName);
    }

    public void CreateFloatField(FloatParameter floatParameter, Action createKeyframe)
    {
        Instantiate(floatFieldUIPrefab, currentComponent.RootObject).Setup(floatParameter, createKeyframe);
    }
}