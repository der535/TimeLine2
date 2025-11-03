using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullscreenRenderPass : ScriptableRenderPass
{
    private Material material;
    private RTHandle tempColor;

    public FullscreenRenderPass(Material material)
    {
        this.material = material;
        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!material) return;

        var cmd = CommandBufferPool.Get("FullscreenEffect");
        var color = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // Создаём дескриптор на основе текущего буфера
        var desc = color.rt.descriptor;
        desc.depthBufferBits = 0; // убираем depth

        // Выделяем RTHandle — без ref!
        tempColor = RTHandles.Alloc(desc);

        // Копируем текущий экран во временный буфер
        Blit(cmd, color, tempColor);

        // Применяем шейдер
        Blit(cmd, tempColor, color, material);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        tempColor?.Release(); // безопасное освобождение
    }
}