using TimeLine;
using UnityEngine;

public class FieldLineData
{
    public RectTransform RectTransform;
    public TreeNode Node;
    public AnimationFieldLine AnimationFieldLine;
    public SelectFieldLine SelectFieldLine;
        
    public FieldLineData(RectTransform rectTransform, TreeNode node, AnimationFieldLine animationFieldLine, SelectFieldLine selectFieldLine)
    {
        RectTransform = rectTransform;
        Node = node;
        AnimationFieldLine = animationFieldLine;
        SelectFieldLine = selectFieldLine;
    }
}