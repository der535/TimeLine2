using System;
using System.Collections;
using TimeLine.EventBus.Events.KeyframeTimeLine;
using TimeLine.TimeLine;
using UnityEngine;
using UnityEngine.Networking;

namespace TimeLine.LevelEditor.Core.MusicLoader
{
    public class M_MusicLoaderService
    {
        internal IEnumerator LoadAudioClip(string filePath, Action<AudioClip> onLoaded)
        {
            // Определяем расширение файла и сопоставляем с AudioType
            AudioType audioType = TimeLineConverter.GetAudioTypeFromPath(filePath);

            if (audioType == AudioType.UNKNOWN)
            {
                Debug.LogError("Неизвестный формат аудиофайла: " + filePath);
                yield break;
            }

            using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(filePath, audioType))
            {
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                    onLoaded.Invoke(clip);
                }
                else
                {
                    Debug.LogError("Ошибка загрузки аудио: " + www.error);
                }
            }
        }
    }
}