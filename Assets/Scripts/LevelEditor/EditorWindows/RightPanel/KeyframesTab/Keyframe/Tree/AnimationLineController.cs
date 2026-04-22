using System;
using System.Collections.Generic;
using TimeLine.LevelEditor.EditorWindows.RightPanel.KeyframesTab.line;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class AnimationLineController : MonoBehaviour
    {
        [SerializeField] private AnimationFieldLine fieldLine;
        [SerializeField] private RectTransform keyframeLine;
        [Space]
        [SerializeField] private RectTransform fieldPanel;
        [SerializeField] private RectTransform keyframePanel;
        [Space]
        [SerializeField] private Color firstColor;
        [SerializeField] private Color secondColor;
        [Space]
        [SerializeField] private float heightLine;

        public List<AnimationLineData> Lines = new List<AnimationLineData>();
        
        private DiContainer _container;

        [Inject]
        private void Construct(DiContainer container)
        {
            _container = container;
        }

        internal void Clear()
        {
            foreach (var line in Lines.ToArray())
            {
               if(line.FieldLine!=null) Destroy(line.FieldLine.gameObject);
                Destroy(line.KeyframeLine.gameObject);
            }
            
            Lines.Clear();
        }
        
        internal void AddLine(string name, TreeNode node, int level)
        {
            AnimationFieldLine fLine = _container.InstantiatePrefab(fieldLine, fieldPanel).GetComponent<AnimationFieldLine>();
            fLine.Setup(name, heightLine, level, node);
            RectTransform kLine = Instantiate(keyframeLine, keyframePanel);
            kLine.sizeDelta = new Vector2(kLine.sizeDelta.x, kLine.sizeDelta.y);
            
            Lines.Add(new AnimationLineData()
            {
                LogicalNode = node,
                FieldLine = fLine,
                KeyframeLine = kLine
            });
        }
    }

    [Serializable]
    public class AnimationLineData
    {
        public TreeNode LogicalNode;
        public AnimationFieldLine FieldLine;
        public RectTransform KeyframeLine;
    }
}
