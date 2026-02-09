using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.SelectController;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Core
{
    public class ValueEditorInstaller : MonoInstaller
    {
        [SerializeField] private ContentConstructor _contentConstructor;
        [SerializeField] private NodeConnector _nodeConnector;
        [SerializeField] private NodeCreator _nodeCreator;
        [SerializeField] private SelectConnectionController _selectConnectionController;
        [SerializeField] private SelectNodeController _selectNodeController;
        public override void InstallBindings()
        {
            Container.Bind<ContentConstructor>().FromInstance(_contentConstructor);
            Container.Bind<NodeConnector>().FromInstance(_nodeConnector);
            Container.Bind<NodeCreator>().FromInstance(_nodeCreator);
            Container.Bind<SelectConnectionController>().FromInstance(_selectConnectionController);
            Container.Bind<SelectNodeController>().FromInstance(_selectNodeController);
        }
    }
}