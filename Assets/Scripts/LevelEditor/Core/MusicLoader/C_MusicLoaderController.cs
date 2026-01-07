using System;
using EventBus;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using UnityEngine;
using Zenject;

namespace TimeLine.LevelEditor.Core.MusicLoader
{
    public class C_MusicLoaderController : MonoBehaviour
    {
        private M_MusicLoaderService _mMusicLoaderService;
        private GameEventBus _gameEventBus;
        private Main _main;

        [Inject]
        private void Constructor(GameEventBus gameEventBus, M_MusicLoaderService mMusicLoaderService, Main main)
        {
            _gameEventBus = gameEventBus;
            this._mMusicLoaderService = mMusicLoaderService;
            this._main = main;
        }

        private void Awake()
        {
            _gameEventBus.SubscribeTo((ref OpenEditorEvent data) =>
            {
                StartCoroutine(_mMusicLoaderService.LoadAudioClip(
                    $"{Application.persistentDataPath}/Levels/{data.LevelInfo.levelName}/{data.LevelInfo.songName}",
                    (clip) =>
                    {
                        _gameEventBus.Raise(new MusicLoadedEvent(clip));
                        _main.SetTimeInTicks(0);
                    }));
            });
        }
    }
}