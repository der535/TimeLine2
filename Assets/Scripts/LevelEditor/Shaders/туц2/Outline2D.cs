// Assets/Scripts/Outline2D.cs
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Outline2D : MonoBehaviour
{
    public Color outlineColor = Color.clear; // clear = использовать настройки из Render Feature
    public float outlineWidth = 0f; // 0 = использовать настройки из Render Feature
    
    public static Outline2D[] instances = new Outline2D[100];
    public static int _instanceCount;

    void OnEnable() => Register();
    void OnDisable() => Unregister();

    void Register()
    {
        if (_instanceCount >= instances.Length)
        {
            Outline2D[] newInstances = new Outline2D[instances.Length * 2];
            System.Array.Copy(instances, newInstances, instances.Length);
            instances = newInstances;
        }
        instances[_instanceCount++] = this;
    }

    void Unregister()
    {
        for (int i = 0; i < _instanceCount; i++)
        {
            if (instances[i] == this)
            {
                instances[i] = instances[--_instanceCount];
                break;
            }
        }
    }
}