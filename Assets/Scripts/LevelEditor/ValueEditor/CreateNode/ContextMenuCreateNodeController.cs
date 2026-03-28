using System;
using System.Collections.Generic;
using TimeLine.Installers;
using TimeLine.LevelEditor.ContextMenu;
using TimeLine.LevelEditor.Core;
using TimeLine.LevelEditor.ValueEditor.NodeLogic;
using TimeLine.LevelEditor.ValueEditor.Test;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.ValueEditor.CreateNode
{
    public class ContextMenuCreateNodeController : MonoBehaviour
    {
        private ContextMenuController _contextMenu;
        private NodeCreator _nodeCreator;
        private OpenValueEditor _openValueEditor;
        private CameraReferences _cameraReferences;

        [Inject]
        private void Construct(ContextMenuController contextMenuController, NodeCreator nodeCreator,
            OpenValueEditor openValueEditor, CameraReferences cameraReferences)
        {
            _contextMenu = contextMenuController;
            _nodeCreator = nodeCreator;
            _openValueEditor = openValueEditor;
            _cameraReferences = cameraReferences;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space) && _openValueEditor.IsPanelActive() && MouseIsInsidePanel())
            {
                _contextMenu.Setup(new List<(Action, string, bool)>()
                {
                    (() => _nodeCreator.CreateNode(new FloatLogic(), "Float", new Vector2(0, 0)), "Float", true),
                    (() => _nodeCreator.CreateNode(new RandomRangeLogic(), "Random range", new Vector2(0, 0)),
                        "Random range", true),
                    (() => _nodeCreator.CreateNode(new PlayerPositionLogic(), "Player position", new Vector2(0, 0)),
                        "Player position", true),
                    (() => _nodeCreator.CreateNode(new ComponentFieldLogic(), "Component field", new Vector2(0, 0)),
                        "Component field", true),
                    (() => _nodeCreator.CreateNode(new InitializeLogic(), "Initialize value", new Vector2(0, 0)),
                        "Initialize value", true),
                    (() => _nodeCreator.CreateNode(new SubtractionLogic(), "Subtraction", new Vector2(0, 0)),
                        "Subtraction", true),
                    (() => _nodeCreator.CreateNode(new MultiplicationLogic(), "MultiplicationLogic", new Vector2(0, 0)),
                        "MultiplicationLogic", true),
                    (() => _nodeCreator.CreateNode(new DivisionLogic(), "DivisionLogic", new Vector2(0, 0)),
                        "DivisionLogic", true),
                    (() => _nodeCreator.CreateNode(new ModLogic(), "ModuleLogic", new Vector2(0, 0)), "ModuleLogic",
                        true),
                    (() => _nodeCreator.CreateNode(new AddLogic(), "Add", new Vector2(0, 0)), "Add", true),
                });
                _contextMenu.ShowMenu();
            }
        }

        private bool MouseIsInsidePanel()
        {
            return RectTransformUtility.RectangleContainsScreenPoint(_openValueEditor.panel,
                UnityEngine.Input.mousePosition, _cameraReferences.editUICamera);
        }
    }
}