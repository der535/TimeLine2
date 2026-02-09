using TimeLine.LevelEditor.ValueEditor.Fields;
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

        [Inject]
        private void Constructor(DiContainer container)
        {
            _diContainer = container;
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

        internal void CreateFields(NodeLogic logic, Node node, RectTransform root)
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

        internal void SetupManualInputFloat(NodeLogic nodeLogic, Port port, int index)
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

        internal void SetupManualInputColor(NodeLogic nodeLogic, Port port, int index)
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
            input.Setup((Color)nodeLogic.ManualValues[index], "Color", (value) => { nodeLogic.ManualValues[index] = value; });

            port.SetActiveDefaultInputValue(true);
        }
    }
}