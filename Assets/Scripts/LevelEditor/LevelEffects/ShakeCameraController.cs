using DG.Tweening;
using NaughtyAttributes;
using TimeLine.LevelEditor.Core;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.LevelEffects
{
    public class ShakeCameraController : MonoBehaviour
    {
        private ShakeCameraData _playCameraData = new();
        private ShakeCameraData _editorCameraData = new();
        private CameraReferences _cameraReferences;

        [Inject]
        private void Construct(CameraReferences references)
        {
            _cameraReferences = references;
        }

        private void Start()
        {
            _playCameraData.SaveStartPosition(_cameraReferences.playCamera.transform);
            _editorCameraData.SaveStartPosition(_cameraReferences.editSceneCamera.transform);
        }


        [Button]
        public void Shake(Vector2 shakeStrength, float duration, int vibrato, float randomness)
        {
            if(_playCameraData.ShakeTween == null || !_playCameraData.ShakeTween.IsPlaying())
                _playCameraData.SaveStartPosition(_cameraReferences.playCamera.transform);
            if(_editorCameraData.ShakeTween == null || !_editorCameraData.ShakeTween.IsPlaying())
                _editorCameraData.SaveStartPosition(_cameraReferences.editSceneCamera.transform);
            
            ShakeCameraService.Shake(_playCameraData, _cameraReferences.playCamera.transform, shakeStrength, duration, vibrato,
                randomness);
            ShakeCameraService.Shake(_editorCameraData, _cameraReferences.editSceneCamera.transform, shakeStrength, duration, vibrato,
                randomness);
        }
    }
}