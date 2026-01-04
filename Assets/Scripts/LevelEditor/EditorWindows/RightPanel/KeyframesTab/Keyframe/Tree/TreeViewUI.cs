using System.Collections.Generic;
using EventBus;
using TimeLine;
using TimeLine.EventBus.Events.TrackObject;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class TreeViewUI : MonoBehaviour
{
    [SerializeField] private AnimationLineController animationLineController;
    [SerializeField] private RectTransform root;
    [SerializeField] private RectTransform rightPanel;
    [SerializeField] private Color[] levelColors;
    

    public AnimationLineController AnimationLineController => animationLineController;
    
    private Branch CurrentBranch { get; set; }
    private GameEventBus _gameEventBus;

    [Inject]
    private void Construct(GameEventBus gameEventBus)
    {
        _gameEventBus = gameEventBus;
    }

    private void Awake()
    {
        animationLineController.Clear();
        _gameEventBus.SubscribeTo<AddTrackEvent>(RebuildBranch, 1);

        _gameEventBus.SubscribeTo((ref SelectObjectEvent data) => BuildBranch(data.Tracks[^1].branch), 1);
        _gameEventBus.SubscribeTo((ref DeselectObjectEvent data) =>
        {
            if (data.SelectedObjects.Count != 0)
            {
                BuildBranch(data.SelectedObjects[^1].branch);
            }
            else
            {
                ClearContent();
            }
        }, 1);
        
        _gameEventBus.SubscribeTo((ref DeselectAllObjectEvent data) => ClearContent());

    }

    public void BuildBranch(Branch branch)
    {
        CurrentBranch = branch;
        ClearContent();
        
        animationLineController.Clear();
        
        if(CurrentBranch == null) return;
        
        BuildNodeRecursive(branch.Root, root, 0, CurrentBranch.Name);
    }

    private void RebuildBranch(ref AddTrackEvent addTrackEvent)
    {
        ClearContent();
        
        animationLineController.Clear();
        
        if(CurrentBranch == null) return;
        
        BuildNodeRecursive(CurrentBranch.Root, root, 0, CurrentBranch.Name);
    }

    private void BuildNodeRecursive(TreeNode node, Transform parent, int level, string customName = null)
    {
        animationLineController.AddLine(customName ?? node.Name, node, level);

        // Рекурсивное создание дочерних узлов
        foreach (TreeNode child in node.Children)
        {
            BuildNodeRecursive(child, parent, level + 1);
        }
    }

    // private void SetupNodeVisuals(TreeNodeObject uiNode, int level)
    // {
    //     // Настройка отступов
    //     uiNode.AddOffset(level * horizontalOffset);
    //     
    //     uiNode.RootRect.sizeDelta = new Vector2(rightPanel.sizeDelta.x, uiNode.RootRect.sizeDelta.y);
    //
    //     // Настройка цвета фона
    //     if (uiNode.NodeImage != null && level < levelColors.Length)
    //     {
    //         uiNode.NodeImage.color = levelColors[level];
    //     }
    // }

    private void ClearContent()
    {
        foreach (Transform child in root)
        {
            Destroy(child.gameObject);
        }
    }
}