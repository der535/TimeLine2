using EventBus;
using TimeLine.EventBus.Events.Misc;
using TimeLine.LevelEditor.ValueEditor.Fields;
using TimeLine.LevelEditor.ValueEditor.Test;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor
{
    public class ContentConstructor : MonoBehaviour
    {
        [SerializeField] private ContentInputColor contentInputColor;
        [SerializeField] private ContentInputSprite contentInputSprite;
        [SerializeField] private ContentInputFloat contentInputFloatPrefab;
        [SerializeField] private ContentButton contentButton;

        private DiContainer _diContainer;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(DiContainer container, GameEventBus gameEvent)
        {
            _diContainer = container;
            _gameEventBus = gameEvent;
        }

        private ContentInputFloat CreateContentInputFloat(RectTransform root)
        {
            return _diContainer.InstantiatePrefab(contentInputFloatPrefab, root).GetComponent<ContentInputFloat>();
        }

        private ContentInputColor CreateContentInputColor(RectTransform root)
        {
            return _diContainer.InstantiatePrefab(contentInputColor, root).GetComponent<ContentInputColor>();
        }
        
        private ContentInputSprite CreateContentInputSprite(RectTransform root)
        {
            return _diContainer.InstantiatePrefab(contentInputSprite, root).GetComponent<ContentInputSprite>();
        }

        private ContentButton CreateButton(RectTransform root)
        {
            return _diContainer.InstantiatePrefab(contentButton, root).GetComponent<ContentButton>();
        }

        internal void CreateFields(global::NodeLogic logic, Node node, RectTransform root)
        {
            if (logic is FloatLogic constantLogic)
            {
                CreateContentInputFloat(root).Setup(0f, (value) => { constantLogic.Value = value; });
            }

            if (logic is RandomFromListLogic _)
            {
                var button = CreateButton(root);
                button.Setup("Add input port", () => { node.AddInputDynamic("Input", DataType.Float); });
            }

            if (logic is ComponentFieldLogic componentFieldLogic)
            {
                var selectField = CreateButton(root);
                var removeField = CreateButton(root);
                
                selectField.Setup("Select Field", () =>
                {
                    _gameEventBus.Raise(new ListeningParameterEvent());
                    EventBinder binder = new EventBinder();
                    binder.Add(_gameEventBus, (ref GetParameterEvent data) =>
                    {
                        MapParameterStorage.Add(componentFieldLogic.idMap, data._map);
                        componentFieldLogic.Entity = data._trackObjectPacket.entity;
                        node.AddOutPutDynamic(data._map.ParameterID,
                            TypeToDataType.Convert(typeof(float))); //Стоит float по умолчанию
                        binder.Dispose();
                    });
                    selectField.SetActiveButton(false);
                    removeField.SetActiveButton(true);
                });
                removeField.Setup("Remove field", () =>
                {
                    node.RemoveOutputDynamic(logic.OutputDefinitions.Count-1);
                    MapParameterStorage.Remove(componentFieldLogic.idMap);
                    selectField.SetActiveButton(true);
                    removeField.SetActiveButton(false);
                });

                if (componentFieldLogic.OutputDefinitions.Count > 0)
                {
                    removeField.SetActiveButton(true);
                    selectField.SetActiveButton(false);
                }
                else
                {
                    removeField.SetActiveButton(false);
                    selectField.SetActiveButton(true);
                }
                
            }

            // ВАЖНО: Если ты используешь AddInputDynamic, 
            // этот цикл ниже должен срабатывать ТОЛЬКО при первом создании ноды.
            // Если нода уже имеет порты, мы настраиваем их один раз.
            for (var index = 0; index < node.inputPorts.Count; index++)
            {
                var port = node.inputPorts[index];

                // Проверка: если в порту уже есть инпут, не создаем дубликат
                if (port.GetInputValueRoot().childCount > 0) continue;
                
                // Debug.Log(port.type);
                if (port.type == DataType.Float)
                {
                    SetupManualInputFloat(logic, port, index);
                }

                if (port.type == DataType.Color)
                {
                    SetupManualInputColor(logic, port, index);
                }
                
                if (port.type == DataType.Sprite)
                {
                    SetupManualInputSprite(logic, port, index);
                }
            }
        }

        internal void SetupManualInputFloat(global::NodeLogic nodeLogic, Port port, int index)
        {
            var input = CreateContentInputFloat(port.GetInputValueRoot());

            RectTransform rectTransform = input.transform as RectTransform;
            rectTransform.anchoredPosition = new Vector2(-rectTransform.sizeDelta.x / 2, 0);

            // Инициализируем значение в логике, если его там еще нет
            if (!nodeLogic.ManualValues.ContainsKey(index))
            {
                nodeLogic.ManualValues[index] = 0f;
            }

            // Устанавливаем начальное значение в UI
            // Debug.Log(nodeLogic.ManualValues[index]);
            // Debug.Log(nodeLogic.ManualValues[index].GetType());
            input.Setup((float)nodeLogic.ManualValues[index], (value) => { nodeLogic.ManualValues[index] = value; });

            port.SetActiveDefaultInputValue(true);
        }

        internal void SetupManualInputColor(global::NodeLogic nodeLogic, Port port, int index)
        {
            var input = CreateContentInputColor(port.GetInputValueRoot());

            RectTransform rectTransform = input.transform as RectTransform;
            rectTransform.anchoredPosition = new Vector2(-rectTransform.sizeDelta.x / 2, 0);

            // Инициализируем значение в логике, если его там еще нет
            if (!nodeLogic.ManualValues.ContainsKey(index))
            {
                nodeLogic.ManualValues[index] = new Color(1, 1, 1);
            }

            // Устанавливаем начальное значение в UI
            input.Setup((Color)nodeLogic.ManualValues[index], "Color",
                (value) => { nodeLogic.ManualValues[index] = value; });

            port.SetActiveDefaultInputValue(true);
        }
        
        internal void SetupManualInputSprite(global::NodeLogic nodeLogic, Port port, int index)
        {
            var input = CreateContentInputSprite(port.GetInputValueRoot());

            RectTransform rectTransform = input.transform as RectTransform;
            rectTransform.anchoredPosition = new Vector2(-rectTransform.sizeDelta.x / 2, 0);

            // Инициализируем значение в логике, если его там еще нет
            if (!nodeLogic.ManualValues.ContainsKey(index))
            {
                nodeLogic.ManualValues[index] = new Color(1, 1, 1);
            }

            // Устанавливаем начальное значение в UI
            input.Setup((string)nodeLogic.ManualValues[index], "Sprite",
                (value) => { nodeLogic.ManualValues[index] = value; });

            port.SetActiveDefaultInputValue(true);
        }
    }
}