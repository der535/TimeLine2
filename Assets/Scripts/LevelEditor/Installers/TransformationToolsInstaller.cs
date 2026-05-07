using TimeLine.LevelEditor.EditorWindows.SceneView.TransformTools.Position;
using TimeLine.LevelEditor.TransformationSquare;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Installers
{
    public class TransformationToolsInstaller : MonoInstaller
    {
        [SerializeField] private TransformationSquareController transformationSquareController;
        [SerializeField] private RotationController rotationController; 
        [SerializeField] private PositionController positionController;
        [SerializeField] private PositionTool positionTool;

        public override void InstallBindings()
        {
            Container.Bind<TransformationSquareController>().FromInstance(transformationSquareController);
            Container.Bind<RotationController>().FromInstance(rotationController);
            Container.Bind<PositionController>().FromInstance(positionController);
            Container.Bind<PositionTool>().FromInstance(positionTool);
        }
    }
}