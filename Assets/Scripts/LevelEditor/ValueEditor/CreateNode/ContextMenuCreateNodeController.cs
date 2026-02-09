using System;
using System.Collections.Generic;
using TimeLine.LevelEditor.ContextMenu;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.CreateNode
{
    public class ContextMenuCreateNodeController : MonoBehaviour
    {
        private ContextMenuController _contextMenu;
        private NodeCreator _nodeCreator;
        
        [Inject]
        private void Construct(ContextMenuController contextMenuController, NodeCreator nodeCreator)
        {
            _contextMenu = contextMenuController;
            _nodeCreator = nodeCreator;
        }
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                _contextMenu.Setup(new List<(Action, string, bool)>()
                {
                    (() => _nodeCreator.CreateNode(new FloatLogic(), "Float", new Vector2(0,0)), "Float", true), 
                    (() => _nodeCreator.CreateNode(new RandomRangeLogic(), "Random range", new Vector2(0,0)), "Random range", true), 
                    (() => _nodeCreator.CreateNode(new RandomFromListLogic(), "Random from list", new Vector2(0,0)), "Random from list", true), 
                });
                _contextMenu.ShowMenu();
            }
        }
    }
}