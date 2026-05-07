using UnityEngine;

namespace TimeLine
{
    [ExecuteInEditMode]
    public class ShaderBase : MonoBehaviour
    {
        public Shader shader;
        Material postEffectMat;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Awake()
        {
            postEffectMat = new Material(shader);
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            int width = source.width;
            int height = source.height;
            
            RenderTexture startRenderTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32);
            
            Graphics.Blit(source, startRenderTexture, postEffectMat, 5);
            Graphics.Blit(startRenderTexture, destination);
            RenderTexture.ReleaseTemporary(startRenderTexture);
        }

       
    }
}
