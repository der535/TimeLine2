using System;
using TimeLine.LevelEditor.Misk;
using TimeLine.LevelEditor.Player;
using TimeLine.LevelEditor.ValueEditor;
using TimeLine.LevelEditor.ValueEditor.Save;
using TimeLine.LevelEditor.ValueEditor.SelectController;
using TimeLine.LevelEditor.ValueEditor.Test;
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
        [SerializeField] private ClearWorkPlace _clearWorkPlace;
        [SerializeField] private OpenValueEditor _openValueEditor;
        [SerializeField] private SaveGraphToKeyFrame _saveGraphToKeyFrame;
        [SerializeField] private SaveNodes _saveNodes;
        [SerializeField] private ValueEditorReferences _valueEditorReferences;
        public override void InstallBindings()
        {
            Container.Bind<ValueEditorReferences>().FromInstance(_valueEditorReferences);
            Container.Bind<ContentConstructor>().FromInstance(_contentConstructor);
            Container.Bind<NodeConnector>().FromInstance(_nodeConnector);
            Container.Bind<NodeCreator>().FromInstance(_nodeCreator);
            Container.Bind<SelectConnectionController>().FromInstance(_selectConnectionController);
            Container.Bind<SelectNodeController>().FromInstance(_selectNodeController);
            Container.Bind<ClearWorkPlace>().FromInstance(_clearWorkPlace);
            Container.Bind<OpenValueEditor>().FromInstance(_openValueEditor);
            Container.Bind<SaveGraphToKeyFrame>().FromInstance(_saveGraphToKeyFrame);
            Container.Bind<SaveNodes>().FromInstance(_saveNodes);
            
            Container.Bind<LoadGraphLogic>().AsSingle();
            Container.Bind<FindField>().AsSingle();
        }
    }
}
[Serializable]
internal class ValueEditorReferences
{
    public RectTransform nodesRootContainer;
}