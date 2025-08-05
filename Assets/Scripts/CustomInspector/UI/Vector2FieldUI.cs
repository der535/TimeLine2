using System;
using EventBus;
using TMPro;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class Vector2FieldUI : FieldUIBase<Vector2>
    {
        [SerializeField] private TMP_InputField x;
        [SerializeField] private TMP_InputField y;
        
        public override void Setup(IField<Vector2> field, Action onValueChanged, GameObject target, string component, SceneObjectAddKeyFrame sceneObjectAddKeyFrame, GameEventBus eventBus)
        {
            base.Setup(field, onValueChanged, target, component, sceneObjectAddKeyFrame, eventBus);
            
            x.text = field.Value.ToString();
            x.onEndEdit.AddListener(arg0 =>
            {
                field.Value = new Vector2(float.Parse(arg0), field.Value.y);
                onValueChanged.Invoke();
            });
            
            y.text = field.Value.ToString();
            y.onEndEdit.AddListener(arg0 =>
            {
                field.Value = new Vector2(field.Value.x, float.Parse(arg0));
                onValueChanged.Invoke();
            });
        }
    }
}