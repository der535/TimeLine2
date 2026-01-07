using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.Grid;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class SelectFieldLineController : MonoBehaviour
    {
        private List<FieldLineData> _fieldLines = new();
        
        private GameEventBus _eventBus;
        private ActionMap _actionMap;

        [Inject]
        private void Construct(GameEventBus eventBus, ActionMap actionMap)
        {
            _eventBus = eventBus;
            _actionMap = actionMap;
        }

        internal bool CheckActive(TreeNode node)
        {
            if (node == null)
            {
                // print("CheckActive: node is null → returning false");
                return false;
            }

            // print($"CheckActive: checking node '{node.Name}'");

            if (_fieldLines.Count == 0)
            {
                // print("CheckActive: _fieldLines is empty → returning true (all nodes considered active)");
                return true;
            }

            bool isNodeInFieldLines = _fieldLines.Any(field => field.Node == node);
            if (isNodeInFieldLines)
            {
                // print($"CheckActive: node '{node.Name}' is directly in _fieldLines → returning true");
                return true;
            }
            else
            {
                // print($"CheckActive: node '{node.Name}' is NOT in _fieldLines → checking children...");
                // bool result = CheckActiveChildren(node);
                // print($"CheckActive: result for node '{node.Name}' after checking children = {result}");
                return false;
            }
        }

        private bool CheckActiveChildren(TreeNode node)
        {
            if (node == null)
            {
                print("CheckActiveChildren: node is null → returning false");
                return false;
            }

            if (node.Children == null || node.Children.Count == 0)
            {
                print($"CheckActiveChildren: node '{node.Name}' has no children → returning false");
                return false;
            }

            print($"CheckActiveChildren: checking {node.Children.Count} children of '{node.Name}'");

            foreach (var child in node.Children)
            {
                if (child == null)
                {
                    print("CheckActiveChildren: encountered null child → skipping");
                    continue;
                }

                print($"CheckActiveChildren: recursing into child '{child.Name}'");
                if (CheckActive(child))
                {
                    print($"CheckActiveChildren: child '{child.Name}' is active → returning true immediately");
                    return true;
                }
            }

            print($"CheckActiveChildren: none of the children of '{node.Name}' are active → returning false");
            return false;
        }

        private void Start()
        {
            _eventBus.SubscribeTo((ref SelectFieldLineEvent selectFieldLineEvent) =>
            {
                if (_actionMap.Editor.LeftShift.IsPressed())
                {
                    if (!_fieldLines.Contains(selectFieldLineEvent.Data))
                        _fieldLines.Add(selectFieldLineEvent.Data);
                }
                else
                {
                    // Если объект уже выделен — ничего не делаем (не сбрасываем выделение)
                    if (_fieldLines.Contains(selectFieldLineEvent.Data))
                        return;

                    // Иначе — сбрасываем текущее выделение и выделяем новый объект
                    Deselect();
                    _fieldLines.Add(selectFieldLineEvent.Data);
                }
            });
        }

        public void Deselect()
        {
            foreach (var field in _fieldLines)
            {
                field.SelectFieldLine?.Deselect();
            }
            _fieldLines.Clear();
            _eventBus.Raise(new DeselectFieldLineEvent());
        }
    }
}
