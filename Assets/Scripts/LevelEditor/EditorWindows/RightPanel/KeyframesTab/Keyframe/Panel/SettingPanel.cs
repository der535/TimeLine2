using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TimeLine
{
    public class SettingPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField X;
        [SerializeField] private TMP_InputField Y;
        [SerializeField] private TMP_InputField Z;
        [Space] 
        [SerializeField] private GameObject panel;
        [Space] 
        [SerializeField] private Button addKeyframe;
        
        private Transform _selectedTransform;

        public void Select(Transform transform, Action addKeyframeAction)
        {
            // Удаляем старые слушатели перед добавлением новых
            ClearEventListeners();
            
            addKeyframe.onClick.RemoveAllListeners();
            addKeyframe.onClick.AddListener(new UnityAction(addKeyframeAction));
            
            _selectedTransform = transform;
            
            X.text = _selectedTransform.position.x.ToString();
            Y.text = _selectedTransform.position.y.ToString();
            Z.text = _selectedTransform.position.z.ToString();
            
            // Добавляем новые слушатели с проверкой ввода
            X.onEndEdit.AddListener(HandleXChanged);
            Y.onEndEdit.AddListener(HandleYChanged);
            Z.onEndEdit.AddListener(HandleZChanged);
            
            panel.SetActive(true);
        }

        private void HandleXChanged(string value)
        {
            if (float.TryParse(value, out float x))
            {
                Vector3 pos = _selectedTransform.position;
                pos.x = x;
                _selectedTransform.position = pos;
            }
        }

        private void HandleYChanged(string value)
        {
            if (float.TryParse(value, out float y))
            {
                Vector3 pos = _selectedTransform.position;
                pos.y = y;
                _selectedTransform.position = pos;
            }
        }

        private void HandleZChanged(string value)
        {
            if (float.TryParse(value, out float z))
            {
                Vector3 pos = _selectedTransform.position;
                pos.z = z;
                _selectedTransform.position = pos;
            }
        }

        // Очистка всех слушателей событий
        private void ClearEventListeners()
        {
            X.onEndEdit.RemoveAllListeners();
            Y.onEndEdit.RemoveAllListeners();
            Z.onEndEdit.RemoveAllListeners();
        }
    }
}