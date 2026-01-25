using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;


public class ParticleFogRenderFeature : ScriptableRendererFeature
{
    public class RenderPass: ScriptableRenderPass
    {
        class AdditivePassData
        {
            public RendererListHandle rendererList;
        }

        class RenderPassData
        {
            public TextureHandle cameraTexture;
        }

        private Shader blitShader = Shader.Find("Hidden/Custom/ParticleFogBlit");
        private Material blitMaterial;
        private ShaderTagId shaderFrontTagId = new ShaderTagId("ParticleFog");
        private int ParticleFogBufferId = Shader.PropertyToID("_ParticleFogBuffer");

        public RenderPass()
        { }

        private RendererListParams CreateRenderListParams(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData, ShaderTagId tag)
        {
            SortingCriteria sortingCriteria = SortingCriteria.None;
            DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(tag, renderingData, cameraData, lightData, sortingCriteria);
            var filteringSettings = new FilteringSettings(RenderQueueRange.all);
            return new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            if (blitMaterial == null)
                blitMaterial = new Material(blitShader);

            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
            UniversalRenderingData universalRenderingData = frameContext.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
            UniversalLightData lightData = frameContext.Get<UniversalLightData>();

            TextureHandle particleFogBuffer = renderGraph.CreateTexture(resourceData.cameraColor);

            using (var builder = renderGraph.AddRasterRenderPass<AdditivePassData>("Particle fog additive pass", out var passData))
            {
                builder.SetRenderAttachment(particleFogBuffer, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.cameraDepthTexture, AccessFlags.Read);
                builder.SetGlobalTextureAfterPass(particleFogBuffer, ParticleFogBufferId);

                var rendererListParams = CreateRenderListParams(universalRenderingData, cameraData, lightData, shaderFrontTagId);
                passData.rendererList = renderGraph.CreateRendererList(rendererListParams);
                builder.UseRendererList(passData.rendererList);

                builder.SetRenderFunc(static (AdditivePassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(false, true, Color.black);
                    context.cmd.DrawRendererList(data.rendererList);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<RenderPassData>("Particle fog render pass", out var passData))
            {
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.None);
                builder.UseTexture(particleFogBuffer, AccessFlags.Read);

                TextureDesc desc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
                Vector2 scale = desc.scale;
                passData.cameraTexture = resourceData.activeColorTexture;
                builder.SetRenderFunc((RenderPassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.cameraTexture, new Vector4(1, 1, 0, 0), blitMaterial, 0);
                });
            }
        }
    }

    [Range(0, 1)]
    public float fogGlobalK = 0.5f;
    public Color fogGlobalColor = Color.white;

    RenderPass renderPass;

    public override void Create()
    {
        renderPass = new RenderPass();
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Shader.SetGlobalFloat("_ParticleFogGlobalK", fogGlobalK);
        Shader.SetGlobalColor("_ParticleFogGlobalColor", fogGlobalColor);

        renderer.EnqueuePass(renderPass);
    }
}
