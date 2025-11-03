using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullscreenEffectFeature : ScriptableRendererFeature
{
    [SerializeField] Shader shader;

    private FullscreenRenderPass renderPass;
    private Material material;

    public override void Create()
    {
        material = new Material(shader);
        material.hideFlags = HideFlags.HideAndDontSave;
        renderPass = new FullscreenRenderPass(material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(material);
    }
}