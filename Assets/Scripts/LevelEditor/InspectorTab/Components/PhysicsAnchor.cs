using TimeLine;
using TimeLine.LevelEditor.EditorWindows.RightPanel.InspectorTab.Components;
using UnityEngine;

public class PhysicsAnchor : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rb;
    private TransformComponent _targetToFollow;
    private ActiveObjectControllerComponent _activeObjectControllerComponent;
    bool cuurentActive = false;

    public void Setup(ActiveObjectControllerComponent activeObjectControllerComponent, TransformComponent transformComponent)
    {
        _targetToFollow = transformComponent;
        _activeObjectControllerComponent = activeObjectControllerComponent;
        // Базовая настройка для четкого следования
        _rb.bodyType = RigidbodyType2D.Static;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        _rb.useFullKinematicContacts = true;

        _activeObjectControllerComponent.IsActiveChanged += actibeObject =>
        {
            if(actibeObject == cuurentActive) return;
            
            cuurentActive = actibeObject;
            transform.position = _targetToFollow.gameObject.transform.position;
            transform.rotation = _targetToFollow.gameObject.transform.rotation;
            transform.localScale = _targetToFollow.gameObject.transform.lossyScale;
            
            if (actibeObject)
            {
                _rb.bodyType = RigidbodyType2D.Kinematic;
            }
            else
            {
                _rb.bodyType = RigidbodyType2D.Static;
            }
        };
    }

    void FixedUpdate()
    {
        if (_rb.bodyType != RigidbodyType2D.Static)
        {
            // 1. Позиция (Мировая) - работает идеально
            _rb.MovePosition(_targetToFollow.gameObject.transform.position);

            // 2. Поворот (Мировой) - работает идеально
            _rb.MoveRotation(_targetToFollow.gameObject.transform.rotation);
    
            // 3. Масштаб (Глобальный)
            // Используем lossyScale цели, чтобы наш объект стал такого же размера в мире
            if (transform.localScale != _targetToFollow.gameObject.transform.lossyScale)
            {
                transform.localScale = _targetToFollow.gameObject.transform.lossyScale;
            }
        }
        else if(_activeObjectControllerComponent.IsActive)
        {
            transform.position = _targetToFollow.gameObject.transform.position;
            transform.rotation = _targetToFollow.gameObject.transform.rotation;
            transform.localScale = _targetToFollow.gameObject.transform.lossyScale;
        }
    }
}