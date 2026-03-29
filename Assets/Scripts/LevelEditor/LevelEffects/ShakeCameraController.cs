using DG.Tweening;
using NaughtyAttributes;
using TimeLine.LevelEditor.Core;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.LevelEffects
{
    public class ShakeCameraController : IInitializable
    {
        private ShakeCameraDataOLD _playCameraDataOld = new();
        private ShakeCameraDataOLD _editorCameraDataOld = new();
        private CameraReferences _cameraReferences;
        
        private bool isInitialized;

        [Inject]
        private void Construct(CameraReferences references)
        {
            _cameraReferences = references;
        }
        
        public void Shake(Vector2 shakeStrength, float duration, int vibrato, float randomness)
        {
            if(!isInitialized) return;
            
            if(_playCameraDataOld.ShakeTween == null || !_playCameraDataOld.ShakeTween.IsPlaying())
                _playCameraDataOld.SaveStartPosition(_cameraReferences.playCamera.transform);
            if(_editorCameraDataOld.ShakeTween == null || !_editorCameraDataOld.ShakeTween.IsPlaying())
                _editorCameraDataOld.SaveStartPosition(_cameraReferences.editSceneCamera.transform);
            
            ShakeCameraService.Shake(_playCameraDataOld, _cameraReferences.playCamera.transform, shakeStrength, duration, vibrato,
                randomness);
            ShakeCameraService.Shake(_editorCameraDataOld, _cameraReferences.editSceneCamera.transform, shakeStrength, duration, vibrato,
                randomness);
        }

        public void Initialize()
        {
            _playCameraDataOld.SaveStartPosition(_cameraReferences.playCamera.transform);
            _editorCameraDataOld.SaveStartPosition(_cameraReferences.editSceneCamera.transform);
            isInitialized = true;
        }
    }
}