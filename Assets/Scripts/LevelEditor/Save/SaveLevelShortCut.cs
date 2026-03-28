using System;
using EventBus;
using TimeLine.LevelEditor.Save;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Core
{
    public class SaveLevelShortCut : MonoBehaviour
    {
        private GameEventBus _gameEventBus;
        private ActionMap _actionMap;
        private SaveLevel _saveLevel;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, ActionMap actionMap, SaveLevel saveLevel)
        {
            _gameEventBus = gameEventBus;
            _actionMap = actionMap;
            _saveLevel = saveLevel;
        }

        private void Start()
        {
            _actionMap.Editor.S.started += context =>
            {
                if (_actionMap.Editor.LeftCtrl.IsPressed())
                {
                    _saveLevel.Save();
                }
            };
        }
    }
}