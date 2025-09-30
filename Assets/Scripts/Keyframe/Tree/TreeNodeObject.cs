// using TMPro;
// using UnityEngine;
// using UnityEngine.Serialization;
// using UnityEngine.UI;
//
// public class TreeNodeObject : MonoBehaviour
// {
//     [SerializeField] private TextMeshProUGUI text;
//     [SerializeField] private RectTransform namePanel;
//     [SerializeField] private RectTransform rootRect;
//     [SerializeField] private Image nodeImage;
//     [SerializeField] private Button expandButton;
//     [SerializeField] private Image icon;
//     [SerializeField] private Toggle selectionToggle;
//     
//     // Ссылка на логический узел
//     private TreeNode _logicalNode;
//
//     public TextMeshProUGUI Text => text;
//     public RectTransform RootRect => rootRect;
//     public Image NodeImage => nodeImage;
//     public Button ExpandButton => expandButton;
//     public Image Icon => icon;
//     public Toggle SelectionToggle => selectionToggle;
//     public TreeNode LogicalNode => _logicalNode;
//
//     public void Initialize(TreeNode node, string customName = null)
//     {
//         _logicalNode = node;
//         text.text = customName != null ? customName : node.Name;
//     }
//
//     public void AddOffset(float offset)
//     {
//         namePanel.anchoredPosition += Vector2.right * offset;
//     }
//
//     // Метод для получения кастомных компонентов
//     public T GetCustomComponent<T>() where T : Component
//     {
//         return GetComponentInChildren<T>();
//     }
// }