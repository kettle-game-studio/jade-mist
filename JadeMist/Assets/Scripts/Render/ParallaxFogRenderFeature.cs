using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;


public class ParallaxFogRenderFeature : ScriptableRendererFeature
{
    public class RenderPass: ScriptableRenderPass
    {
        class NormalsPassData
        {
            public RendererListHandle rendererList;
        }

        class InternalCounterPassData
        {
            public RendererListHandle rendererListFront;
            public RendererListHandle rendererListBack;
        }

        class RenderPassData
        {
            public TextureHandle cameraTexture;
            public TextureHandle sceneDepthTexture;
        }

        private Shader blitShader = Shader.Find("Hidden/Custom/ParallaxFogBlit");
        private Material blitMaterial;
        private ShaderTagId shaderNormalsFrontTagId = new ShaderTagId("ParallaxFogNormalsFront");
        private ShaderTagId shaderNormalsBackTagId = new ShaderTagId("ParallaxFogNormalsBack");
        private ShaderTagId shaderInternalCounterFrontTagId = new ShaderTagId("ParallaxFogInternalCounterFront");
        private ShaderTagId shaderInternalCounterBackTagId = new ShaderTagId("ParallaxFogInternalCounterBack");
        private int ParallaxFogNormalsFrontBufferId = Shader.PropertyToID("_ParallaxFogNormalsFront");
        private int ParallaxFogNormalsBackBufferId = Shader.PropertyToID("_ParallaxFogNormalsBack");
        private int ParallaxFogDepthFrontBufferId = Shader.PropertyToID("_ParallaxFogDepthFront");
        private int ParallaxFogDepthBackBufferId = Shader.PropertyToID("_ParallaxFogDepthBack");
        private int ParallaxFogInternalCounterBufferId = Shader.PropertyToID("_ParallaxFogInternalCounter");
        private int ParallaxFogSceneDepthBufferId = Shader.PropertyToID("_ParallaxFogSceneDepth");

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

            ProfilingSampler sampler = new ProfilingSampler("Parallax Fog");
            renderGraph.BeginProfilingSampler(sampler);

            UniversalResourceData resourceData = frameContext.Get<UniversalResourceData>();
            UniversalRenderingData universalRenderingData = frameContext.Get<UniversalRenderingData>();
            UniversalCameraData cameraData = frameContext.Get<UniversalCameraData>();
            UniversalLightData lightData = frameContext.Get<UniversalLightData>();

            TextureDesc fogNormalsDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
            TextureDesc internalCounterDesc = fogNormalsDesc;
            fogNormalsDesc.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
            fogNormalsDesc.name = "_ParallaxFogNormalsTexture";
            internalCounterDesc.colorFormat = GraphicsFormat.R16_SFloat;
            internalCounterDesc.name = "_ParallaxInternalCounterTexture";

            TextureHandle fogFrontDepth = renderGraph.CreateTexture(resourceData.cameraDepthTexture);
            TextureHandle fogBackDepth = renderGraph.CreateTexture(resourceData.cameraDepthTexture);
            TextureHandle fogFrontNormals = renderGraph.CreateTexture(fogNormalsDesc);
            TextureHandle fogBackNormals = renderGraph.CreateTexture(fogNormalsDesc);
            TextureHandle internalCounter = renderGraph.CreateTexture(internalCounterDesc);

            using (var builder = renderGraph.AddRasterRenderPass<NormalsPassData>("Front Normals", out var passData))
            {
                builder.SetRenderAttachment(fogFrontNormals, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(fogFrontDepth, AccessFlags.Write); // TODO: copy depth
                builder.SetGlobalTextureAfterPass(fogFrontNormals, ParallaxFogNormalsFrontBufferId);
                builder.SetGlobalTextureAfterPass(fogFrontDepth, ParallaxFogDepthFrontBufferId);

                var rendererListParams = CreateRenderListParams(universalRenderingData, cameraData, lightData, shaderNormalsFrontTagId);
                passData.rendererList = renderGraph.CreateRendererList(rendererListParams);
                builder.UseRendererList(passData.rendererList);

                builder.SetRenderFunc(static (NormalsPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(true, true, new Color(0, 0, 0));
                    context.cmd.DrawRendererList(data.rendererList);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<NormalsPassData>("Back Normals", out var passData))
            {
                builder.SetRenderAttachment(fogBackNormals, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(fogBackDepth, AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(fogBackNormals, ParallaxFogNormalsBackBufferId);
                builder.SetGlobalTextureAfterPass(fogBackDepth, ParallaxFogDepthBackBufferId);

                var rendererListParams = CreateRenderListParams(universalRenderingData, cameraData, lightData, shaderNormalsBackTagId);
                passData.rendererList = renderGraph.CreateRendererList(rendererListParams);
                builder.UseRendererList(passData.rendererList);

                builder.SetRenderFunc(static (NormalsPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(true, true, new Color(0, 0, 0));
                    context.cmd.DrawRendererList(data.rendererList);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<InternalCounterPassData>("Internal Counter", out var passData))
            {
                builder.SetRenderAttachment(internalCounter, 0, AccessFlags.Write);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.None);
                builder.SetGlobalTextureAfterPass(internalCounter, ParallaxFogInternalCounterBufferId);

                var rendererListParamsFront = CreateRenderListParams(universalRenderingData, cameraData, lightData, shaderInternalCounterFrontTagId);
                var rendererListParamsBack = CreateRenderListParams(universalRenderingData, cameraData, lightData, shaderInternalCounterBackTagId);
                passData.rendererListFront = renderGraph.CreateRendererList(rendererListParamsFront);
                passData.rendererListBack = renderGraph.CreateRendererList(rendererListParamsBack);
                builder.UseRendererList(passData.rendererListFront);
                builder.UseRendererList(passData.rendererListBack);

                builder.SetRenderFunc(static (InternalCounterPassData data, RasterGraphContext context) =>
                {
                    context.cmd.ClearRenderTarget(false, true, new Color(0, 0, 0, 0));
                    context.cmd.DrawRendererList(data.rendererListFront);
                    context.cmd.DrawRendererList(data.rendererListBack);
                });
            }

            using (var builder = renderGraph.AddRasterRenderPass<RenderPassData>("Render", out var passData))
            {
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0, AccessFlags.ReadWrite);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture, AccessFlags.ReadWrite);
                builder.UseTexture(fogFrontDepth, AccessFlags.Read);
                builder.UseTexture(fogBackDepth, AccessFlags.Read);
                builder.UseTexture(fogFrontNormals, AccessFlags.Read);
                builder.UseTexture(fogBackNormals, AccessFlags.Read);
                builder.UseTexture(internalCounter, AccessFlags.Read);
                passData.sceneDepthTexture = resourceData.activeDepthTexture;

                TextureDesc desc = resourceData.activeColorTexture.GetDescriptor(renderGraph);
                Vector2 scale = desc.scale;
                passData.cameraTexture = resourceData.activeColorTexture;
                builder.SetRenderFunc((RenderPassData data, RasterGraphContext context) =>
                {
                    blitMaterial.SetTexture(ParallaxFogSceneDepthBufferId, data.sceneDepthTexture);
                    Blitter.BlitTexture(context.cmd, data.cameraTexture, new Vector4(1, 1, 0, 0), blitMaterial, 0);
                });
            }
            renderGraph.EndProfilingSampler(sampler);
        }
    }

    RenderPass renderPass;

    [ColorUsage(true, true)]
    public Color InternalColor = Color.black;
    [ColorUsage(true, true)]
    public Color ExternalColor = Color.white;
    [ColorUsage(true, true)]
    public Color BorderColor = Color.black;
    public float fogDepth = 5;
    [Range(0, 1)]
    public float externalTransparency = 0.5f;
    [Range(0, 1)]
    public float internalTransparency = 0.5f;
    public Texture2D blueNoise;

    public override void Create()
    {
        renderPass = new RenderPass();
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Shader.SetGlobalColor("_ParallaxFogInternalColor", InternalColor);
        Shader.SetGlobalColor("_ParallaxFogExternalColor", ExternalColor);
        Shader.SetGlobalColor("_ParallaxFogBorderColor", BorderColor);
        Shader.SetGlobalFloat("_ParallaxFogDepthValue", fogDepth);
        Shader.SetGlobalFloat("_ParallaxFogExternalTransparency", externalTransparency);
        Shader.SetGlobalFloat("_ParallaxFogInternalTransparency", internalTransparency);

        Shader.SetGlobalTexture("_ParallaxFogBlueNoise", blueNoise);
        Vector2 blueNoiseSize = blueNoise != null ? new Vector2(blueNoise.width, blueNoise.height) : Vector2.one;
        Shader.SetGlobalVector("_ParallaxFogBlueNoiseSize", blueNoiseSize);

        renderer.EnqueuePass(renderPass);
    }
}
