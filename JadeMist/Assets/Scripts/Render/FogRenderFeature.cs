using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;


public class FogRenderFeature : ScriptableRendererFeature
{
    public class FogRenderPass: ScriptableRenderPass
    {
        class FrontBackPassData
        {
            public RendererListHandle rendererList;
        }

        class ResetNativeRenderPassFrameData
        {
        }

        private LayerMask layerMask;
        private Shader fogBlitShader = Shader.Find("Hidden/Custom/RenderFogBlit");
        private Material fogBlitMaterial;
        private int _FogDepthBack = Shader.PropertyToID("_FogDepthBack");
        private int _FogDepthFront = Shader.PropertyToID("_FogDepthFront");
        private ShaderTagId shaderFrontTagId = new ShaderTagId("FogFront");
        private ShaderTagId shaderBackTagId = new ShaderTagId("FogBack");

        public FogRenderPass(LayerMask layerMask)
        {
            this.layerMask = layerMask;
            fogBlitMaterial = new Material(fogBlitShader);
        }

        private RendererListParams CreateRenderListParams(UniversalRenderingData renderingData, UniversalCameraData cameraData, UniversalLightData lightData, ShaderTagId tag)
        {
            SortingCriteria sortingCriteria = SortingCriteria.None;
            DrawingSettings drawingSettings = RenderingUtils.CreateDrawingSettings(tag, renderingData, cameraData, lightData, sortingCriteria);
            var filteringSettings = new FilteringSettings(RenderQueueRange.all, -1);
            return new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameContext)
        {
            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
            UniversalRenderingData universalRenderingData = frameContext.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
            UniversalLightData lightData = frameContext.Get<UniversalLightData>();

            TextureHandle fogDepthBack  = renderGraph.CreateTexture(resourceData.cameraDepthTexture);
            TextureHandle fogDepthFront = renderGraph.CreateTexture(resourceData.cameraDepthTexture);

            using (var builder = renderGraph.AddRasterRenderPass<FrontBackPassData>("Fog back pass", out var passData))
            {
                builder.SetRenderAttachmentDepth(fogDepthBack, AccessFlags.Write);
                builder.UseTexture(resourceData.cameraDepthTexture, AccessFlags.Read);
                builder.SetGlobalTextureAfterPass(fogDepthBack, _FogDepthBack);

                var rendererListParams = CreateRenderListParams(universalRenderingData, cameraData, lightData, shaderBackTagId);
                passData.rendererList = renderGraph.CreateRendererList(rendererListParams);
                builder.UseRendererList(passData.rendererList);

                builder.SetRenderFunc(static (FrontBackPassData data, RasterGraphContext context) =>
                {
                    float nearClipZ = SystemInfo.usesReversedZBuffer ? -1 : 1; // TODO: ???
                    context.cmd.ClearRenderTarget(true, false, Color.green, nearClipZ);
                    context.cmd.DrawRendererList(data.rendererList);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<FrontBackPassData>("Fog front pass", out var passData))
            {
                builder.SetRenderAttachmentDepth(fogDepthFront, AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(fogDepthFront, _FogDepthFront);

                var rendererListParams = CreateRenderListParams(universalRenderingData, cameraData, lightData, shaderFrontTagId);
                passData.rendererList = renderGraph.CreateRendererList(rendererListParams);
                builder.UseRendererList(passData.rendererList);

                builder.SetRenderFunc(static (FrontBackPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(true, false, Color.green);
                    context.cmd.DrawRendererList(data.rendererList);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<ResetNativeRenderPassFrameData>("Fog render pass", out var passData))
            {
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);
                builder.UseTexture(fogDepthBack, AccessFlags.Read);
                builder.UseTexture(fogDepthFront, AccessFlags.Read);

                builder.SetRenderFunc((ResetNativeRenderPassFrameData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, new Vector4(1, 1, 0, 0), fogBlitMaterial, 0);
                });
            }
        }
    }

    FogRenderPass fogRenderPass;
    // CopyDepthPass copyDepthPass;

    public override void Create()
    {
        // copyDepthPass = new CopyDepthPass(RenderPassEvent.AfterRenderingTransparents, Shader.Find("Hidden/Universal Render Pipeline/CopyDepth"), false, true);
        fogRenderPass = new FogRenderPass(2500);
        fogRenderPass.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(fogRenderPass);
    }
}
