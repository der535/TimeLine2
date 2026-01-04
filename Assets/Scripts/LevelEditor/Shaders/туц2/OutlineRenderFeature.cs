using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class OutlineRenderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class OutlineSettings
    {
        [Range(0.1f, 10f)]
        public float outlineWidth = 2f;
        public Color outlineColor = Color.black;
    }

    [SerializeField] private OutlineSettings settings = new OutlineSettings();
    [SerializeField] private Shader outlineShader;
    
    private Material outlineMaterial;
    private OutlineRenderPass outlineRenderPass;

    public override void Create()
    {
        if (outlineShader == null)
        {
            Debug.LogError("Outline Shader not assigned in Render Feature!");
            return;
        }
        
        outlineMaterial = CoreUtils.CreateEngineMaterial(outlineShader);
        outlineRenderPass = new OutlineRenderPass(outlineMaterial, settings);
        outlineRenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Только для игровой камеры
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;
            
        if (outlineRenderPass == null || outlineMaterial == null)
            return;
            
        outlineRenderPass.UpdateSettings(settings);
        renderer.EnqueuePass(outlineRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(outlineMaterial);
    }

    private class OutlineRenderPass : ScriptableRenderPass
    {
        private const string k_OutlinePassName = "OutlineRenderPass";
        private static readonly int outlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int outlineWidthId = Shader.PropertyToID("_OutlineWidth");
        
        private Material outlineMaterial;
        private OutlineSettings settings;

        public OutlineRenderPass(Material material, OutlineSettings settings)
        {
            this.outlineMaterial = material;
            this.settings = settings;
        }

        public void UpdateSettings(OutlineSettings newSettings)
        {
            this.settings = newSettings;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            // Защита от рендеринга в back buffer
            if (resourceData.isActiveTargetBackBuffer)
                return;
                
            TextureHandle cameraColor = resourceData.activeColorTexture;
            if (!cameraColor.IsValid())
                return;

            // Обновление глобальных параметров
            if (outlineMaterial != null)
            {
                outlineMaterial.SetColor(outlineColorId, settings.outlineColor);
                outlineMaterial.SetFloat(outlineWidthId, settings.outlineWidth);
            }

            // Сбор объектов с обводкой
            Outline2D[] outlineObjects = Outline2D.instances;
            int validCount = 0;
            
            for (int i = 0; i < Outline2D._instanceCount; i++)
            {
                if (i >= outlineObjects.Length) break;
                
                Outline2D target = outlineObjects[i];
                if (target == null || !target.isActiveAndEnabled) continue;
                
                SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
                if (renderer == null || !renderer.isVisible) continue;
                
                validCount++;
            }
            
            if (validCount == 0)
                return;

            // ✅ УПРОЩЕННАЯ ВЕРСИЯ ДЛЯ 2D - БЕЗ DEPTH TEXTURE
            using (var builder = renderGraph.AddRasterRenderPass<OutlinePassData>(k_OutlinePassName, out var passData))
            {
                // Только цветовое вложение
                builder.SetRenderAttachment(cameraColor, 0, AccessFlags.Write);

                passData.outlineMaterial = outlineMaterial;
                passData.outlineObjects = outlineObjects;
                passData.validCount = validCount;
                
                builder.SetRenderFunc((OutlinePassData data, RasterGraphContext context) =>
                {
                    for (int i = 0; i < data.validCount; i++)
                    {
                        if (i >= data.outlineObjects.Length) break;
                        
                        Outline2D target = data.outlineObjects[i];
                        if (target == null || !target.isActiveAndEnabled) continue;
                        
                        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
                        if (renderer == null || !renderer.isVisible) continue;

                        // Индивидуальные настройки для объекта
                        Color finalColor = target.outlineColor != Color.clear ? target.outlineColor : settings.outlineColor;
                        float finalWidth = target.outlineWidth > 0 ? target.outlineWidth : settings.outlineWidth;
                        
                        data.outlineMaterial.SetColor(outlineColorId, finalColor);
                        data.outlineMaterial.SetFloat(outlineWidthId, finalWidth);
                        
                        // Рисуем обводку ПОД оригиналом
                        context.cmd.DrawRenderer(renderer, data.outlineMaterial);
                    }
                });
            }
        }

        private class OutlinePassData
        {
            public Material outlineMaterial;
            public Outline2D[] outlineObjects;
            public int validCount;
        }
    }
}