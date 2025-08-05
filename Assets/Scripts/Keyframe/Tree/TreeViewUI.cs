using System.Collections.Generic;
using EventBus;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using Zenject;

public class TreeViewUI : MonoBehaviour
{
    [SerializeField] private RectTransform root;
    [SerializeField] private RectTransform rightPanel;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private int horizontalOffset = 30;
    [SerializeField] private Color[] levelColors;
    [SerializeField] private bool autoRefresh = true;
    
    public List<TreeNodeObject> NodeObjects { get; private set; }

    private Branch CurrentBranch { get; set; }
    private GameEventBus _gameEventBus;

    [Inject]
    private void Construct(GameEventBus gameEventBus)
    {
        _gameEventBus = gameEventBus;
    }

    private void Awake()
    {
        _gameEventBus.SubscribeTo<AddTrackEvent>(RebuildBranch, 1);
        _gameEventBus.SubscribeTo((ref SelectTrackObjectEvent data) => BuildBranch(data.Track.branch), 1);
    }

    public void BuildBranch(Branch branch)
    {
        if(CurrentBranch == branch) return;
        
        CurrentBranch = branch;
        ClearContent();
        
        NodeObjects = new List<TreeNodeObject>();
        
        BuildNodeRecursive(branch.Root, root, 1);
    }

    private void RebuildBranch(ref AddTrackEvent addTrackEvent)
    {
        ClearContent();
        
        NodeObjects = new List<TreeNodeObject>();
        
        BuildNodeRecursive(CurrentBranch.Root, root, 1);
    }

    private void BuildNodeRecursive(TreeNode node, Transform parent, int level)
    {
        GameObject nodeObj = Instantiate(nodePrefab, parent);
        TreeNodeObject uiNode = nodeObj.GetComponent<TreeNodeObject>();
        NodeObjects.Add(uiNode);

        if (uiNode != null)
        {
            uiNode.Initialize(node);
            SetupNodeVisuals(uiNode, level);
        }

        // Рекурсивное создание дочерних узлов
        foreach (TreeNode child in node.Children)
        {
            BuildNodeRecursive(child, parent, level + 1);
        }
    }

    private void SetupNodeVisuals(TreeNodeObject uiNode, int level)
    {
        // Настройка отступов
        uiNode.AddOffset(level * horizontalOffset);
        
        uiNode.RootRect.sizeDelta = new Vector2(rightPanel.sizeDelta.x, uiNode.RootRect.sizeDelta.y);

        // Настройка цвета фона
        if (uiNode.NodeImage != null && level < levelColors.Length)
        {
            uiNode.NodeImage.color = levelColors[level];
        }
    }

    private void ClearContent()
    {
        foreach (Transform child in root)
        {
            Destroy(child.gameObject);
        }
    }
}