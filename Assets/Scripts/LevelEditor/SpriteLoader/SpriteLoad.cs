using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using TimeLine.LevelEditor.SpriteLoader;

public static class SpriteLoad
{
    public static IEnumerator LoadSpriteFromPath(string filePath, TextureData textureData, System.Action<Sprite> callback)
    {

        using (UnityWebRequest request = UnityWebRequest.Get(filePath))
        {
            yield return request.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            if (request.result != UnityWebRequest.Result.Success)
#else
            if (request.isNetworkError || request.isHttpError)
#endif
            {
                string errorMsg = request.error ?? "Unknown error";
                callback?.Invoke(null);
                yield break;
            }


            Texture2D tex = new Texture2D(2, 2);
            bool loadSuccess = tex.LoadImage(request.downloadHandler.data);
            if (!loadSuccess)
            {
                callback?.Invoke(null);
                yield break;
            }

            tex.filterMode = textureData.FilterMode;

            Rect rect = new Rect(0, 0, tex.width, tex.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            float pixelsPerUnit = textureData.PixelsPerUnit;


            Sprite sprite = Sprite.Create(tex, rect, pivot, pixelsPerUnit);
            
            sprite.name = textureData.Id;

            if (sprite == null)
            {
                callback?.Invoke(null);
                yield break;
            }

            callback?.Invoke(sprite);
        }
    }
}