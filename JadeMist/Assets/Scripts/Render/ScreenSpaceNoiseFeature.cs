using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

public class ScreenSpaceNoiseFeature : ScriptableRendererFeature
{
    public class RenderPass : ScriptableRenderPass
    {
        class RenderPassData
        {
            public TextureHandle bufferTexture;
        }

        private Shader blitShader = Shader.Find("Hidden/Custom/NoiseBlit");
        private Material blitMaterial;

        public RenderPass()
        { }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            if (blitMaterial == null)
                blitMaterial = new Material(blitShader);

            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();

            TextureHandle bufferTexture = renderGraph.CreateTexture(resourceData.activeColorTexture);

            renderGraph.AddCopyPass(resourceData.activeColorTexture, bufferTexture);

            using (var builder = renderGraph.AddRasterRenderPass<RenderPassData>("Noise Postprocess", out var passData))
            {
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.None);
                builder.UseTexture(bufferTexture, AccessFlags.ReadWrite);

                passData.bufferTexture = bufferTexture;
                builder.SetRenderFunc((RenderPassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, passData.bufferTexture, new Vector4(1, 1, 0, 0), blitMaterial, 0);
                });
            }
        }
    }

    RenderPass renderPass;

    public override void Create()
    {
        renderPass = new RenderPass { renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(renderPass);
    }
}
