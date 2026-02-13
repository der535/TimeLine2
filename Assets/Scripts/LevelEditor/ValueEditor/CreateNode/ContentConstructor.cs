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
                        componentFieldLogic._parameter = data.Parameter.Item1;
                        componentFieldLogic._Map = data.Parameter.Item2;
                        node.AddOutPutDynamic(data.Parameter.Item1.Name,
                            TypeToDataType.Convert(data.Parameter.Item1.ValueType));
                        binder.Dispose();
                    });
                    selectField.SetActiveButton(false);
                    removeField.SetActiveButton(true);
                });
                removeField.Setup("Remove field", () =>
                {
                    node.RemoveOutputDynamic(logic.OutputDefinitions.Count-1);
                    componentFieldLogic._Map = null;
                    componentFieldLogic._parameter = null;
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

                if (port.type == DataType.Float)
                {
                    SetupManualInputFloat(logic, port, index);
                }

                if (port.type == DataType.Color)
                {
                    SetupManualInputColor(logic, port, index);
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
    }
}