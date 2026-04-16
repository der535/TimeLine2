using TimeLine.LevelEditor.TransformationSquare;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Installers
{
    public class TransformationSquareInstaller : MonoInstaller
    {
        [SerializeField] private TransformationSquareController transformationSquareController;

        public override void InstallBindings()
        {
            Container.Bind<TransformationSquareController>().FromInstance(transformationSquareController);
        }
    }
}