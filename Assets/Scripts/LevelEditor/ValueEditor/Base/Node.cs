using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Zenject;
using UnityEngine.EventSystems; // Добавь это для интерфейсов мыши

namespace TimeLine.LevelEditor.ValueEditor
{
    // Добавляем интерфейсы событий
    public class Node : MonoBehaviour, IDragHandler, IPointerDownHandler
    {
        [Header("UI Containers")] 
        [SerializeField] private SelectNode selectNode;
        [SerializeField] private TextMeshProUGUI textMeshProUGUI;
        [Space] 
        [SerializeField] private RectTransform inputContainer;
        [SerializeField] private RectTransform outputContainer;
        [SerializeField] private RectTransform contentContainer;

        [Header("Prefabs")] 
        [SerializeField] private Port inputPortPrefab;
        [SerializeField] private Port outputPortPrefab;

        public List<Port> inputPorts = new();
        public List<Port> outputPorts = new();

        public NodeLogic Logic { get; private set; }

        private ContentConstructor _contentConstructor;
        private DiContainer _container;
        private bool _isDeleted;
        
        // Кэшируем компоненты для перемещения
        private RectTransform _rectTransform;
        private Canvas _canvas;

        [Inject]
        private void Constructor(ContentConstructor contentConstructor, DiContainer container)
        {
            _contentConstructor = contentConstructor;
            _container = container;
        }

        internal SelectNode GetSelectNode() => selectNode;
        
        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas = GetComponentInParent<Canvas>();
        }
        
        public bool GetIsDeleted() => _isDeleted;

        // Вызывается при клике по ноде
        public void OnPointerDown(PointerEventData eventData)
        {
            // Выносим ноду на передний план среди соседей
            _rectTransform.SetAsLastSibling();
        }
        
        // Вызывается при зажатии ЛКМ и движении
        public void OnDrag(PointerEventData eventData)
        {
            // Перемещаем ноду. Делим на scaleFactor, чтобы скорость была 
            // одинаковой при любом масштабе интерфейса или зуме
            _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        }

        /// <summary>
        /// Метод добовляет порты, вызывается ТОЛЬКО при иницаиализации.
        /// </summary>
        /// <param name="label">Название порта</param>
        /// <param name="isInput">True - входящи порт False - выходящий</param>
        /// <param name="type">Тип порта, например int, float, string</param>
        /// <returns></returns>
        private void AddPort(string label, bool isInput, DataType type)
        {
            var container = isInput ? inputContainer : outputContainer;
            var port = Instantiate(isInput ? inputPortPrefab : outputPortPrefab, container);
            _container.Inject(port);
            port.Setup(type, this, label);

            if (isInput) inputPorts.Add(port);
            else outputPorts.Add(port);
        }
        
        /// <summary>
        /// Динамическое добовляет новый инпут порт
        /// </summary>
        /// <param name="label"></param>
        /// <param name="type"></param>
        public void AddInputDynamic(string label, DataType type)
        {
            // Добовляет в логику порт
            Logic.AddInputDefinition();
            
            //Создаем визуальный порт
            AddPort(label, true, type); 
    
            //Получаем индекс только что созданного порта
            int newIndex = inputPorts.Count - 1;
    
            // Настраиваем только этот конкретный порт
            _contentConstructor.SetupManualInputFloat(Logic, inputPorts[newIndex], newIndex);
        }

        
        /// <summary>
        /// Инициализатор записывает логику ноды, название ноды и инициализирует поля
        /// </summary>
        public void Initialize(NodeLogic logic, string title, bool isDeleted)
        {
            _isDeleted = isDeleted;
            Logic = logic;
            textMeshProUGUI.text = title;
            SetupPorts();
            SetupContent();
        }

        /// <summary>
        /// Создаёт порты в ноде
        /// </summary>
        private void SetupPorts()
        {
            foreach (var p in inputPorts) if(p != null) Destroy(p.gameObject);
            inputPorts.Clear();

            foreach (var p in outputPorts) if(p != null) Destroy(p.gameObject);
            outputPorts.Clear();

            foreach (var def in Logic.InputDefinitions)
                AddPort(def.name, true, def.type);
            
            foreach (var def in Logic.OutputDefinitions)
                AddPort(def.name, false, def.type);
        }

        /// <summary>
        /// Создаёт тело ноды обращаясь к конструктору
        /// </summary>
        private void SetupContent()
        {
            foreach (Transform child in contentContainer) Destroy(child.gameObject);
            _contentConstructor.CreateFields(Logic, this, contentContainer);
        }
    }
}