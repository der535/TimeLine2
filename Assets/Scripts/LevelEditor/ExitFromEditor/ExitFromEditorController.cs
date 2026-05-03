using System.Collections.Generic;
using System.Linq;
using EventBus;
using TimeLine.EventBus.Events.Grid;
using TimeLine.LevelEditor.SpriteLoader;
using TimeLine.LevelEditor.TimeLineWindows.TimeLine.TimeLineObjects;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TimeLine.LevelEditor.ExitFromEditor
{
    public class ExitFromEditorController : MonoBehaviour
    {
        [SerializeField] private GameObject startScreen;
        [Space]
        [SerializeField] private GameObject exitPanel;
        [SerializeField] private Button exitPanelButtonYes;
        [SerializeField] private Button exitPanelButtonCancel;
        
        TrackObjectStorage _trackObjectStorage;
        TrackObjectRemover _trackObjectRemover;
        SpriteGallery _spriteGallery;
        SaveComposition _saveComposition;
        ActionMap _actionMap;
        PlayModeController _playModeController;
        private GameEventBus _gameEventBus;

        [Inject]
        private void Constructor(
            TrackObjectStorage trackObjectStorage,
            TrackObjectRemover trackObjectRemover,
            SpriteGallery spriteGallery,
            SaveComposition saveComposition, 
            ActionMap actionMap,
            PlayModeController playModeController,
            GameEventBus gameEventBus)
        {
            _trackObjectStorage = trackObjectStorage;
            _trackObjectRemover = trackObjectRemover;
            _spriteGallery = spriteGallery;
            _saveComposition = saveComposition;
            _actionMap = actionMap;
            _playModeController = playModeController;
            _gameEventBus = gameEventBus;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref EscapePressedEvent _) =>
            {
                if (!_playModeController.IsPlaying)
                {
                    exitPanel.SetActive(true);
                    _actionMap.Editor.Disable();
                }
            }, 3);
            exitPanelButtonYes.onClick.AddListener(() =>
            {
                exitPanel.SetActive(false);
                Exit();
            });
            exitPanelButtonCancel.onClick.AddListener(() =>
            {
                exitPanel.SetActive(false);
                _actionMap.Editor.Enable();
            });
        }

        private void Exit()
        {
            _trackObjectRemover.RemoveList(_trackObjectStorage.TrackObjects.ToList());
            _trackObjectRemover.RemoveList(new List<TrackObjectPacket>(_trackObjectStorage.TrackObjectGroups));
            _spriteGallery.RemoveAllCards();
            _saveComposition.RemoveAll();
            startScreen.gameObject.SetActive(true);
        }
    }
}