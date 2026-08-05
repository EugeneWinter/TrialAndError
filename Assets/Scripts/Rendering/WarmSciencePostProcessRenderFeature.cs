using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class WarmSciencePostProcessRenderFeature : ScriptableRendererFeature
{
    class WarmSciencePass : ScriptableRenderPass
    {
        Material compositeMat;
        RTHandle tempHandle;

        public WarmSciencePass(Material composite)
        {
            compositeMat = composite;
        }

        public void SetupHandles(RenderTextureDescriptor cameraDescriptor)
        {
            var descriptor = cameraDescriptor;
            descriptor.depthBufferBits = 0;
            descriptor.msaaSamples = 1;

            RenderingUtils.ReAllocateIfNeeded(ref tempHandle, descriptor, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_WarmScienceTemp");
        }

        public void ReleaseHandles()
        {
            tempHandle?.Release();
            tempHandle = null;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var volume = VolumeManager.instance.stack.GetComponent<WarmSciencePostProcessVolume>();
            if (volume == null || !volume.IsActive())
                return;

            if (compositeMat == null)
                return;

            var renderer = renderingData.cameraData.renderer;
            var source = renderer.cameraColorTargetHandle;
            if (source == null || source.rt == null)
                return;

            SetupHandles(renderingData.cameraData.cameraTargetDescriptor);

            CommandBuffer cmd = CommandBufferPool.Get("Warm Science PostProcess");

            Blitter.BlitCameraTexture(cmd, source, tempHandle, compositeMat, 0);
            Blitter.BlitCameraTexture(cmd, tempHandle, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    [System.Serializable]
    public class Settings
    {
        public Shader compositeShader;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public Settings settings = new Settings();

    WarmSciencePass pass;
    Material compositeMat;

    public override void Create()
    {
        if (settings.compositeShader == null)
            settings.compositeShader = Shader.Find("Hidden/WarmScience/Composite");

        if (settings.compositeShader != null)
            compositeMat = CoreUtils.CreateEngineMaterial(settings.compositeShader);

        pass = new WarmSciencePass(compositeMat);
        pass.renderPassEvent = settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass == null) return;
        if (!renderingData.cameraData.postProcessEnabled) return;

        var cameraData = renderingData.cameraData;
        var camera = cameraData.camera;

        if (camera.cameraType != CameraType.Game && camera.cameraType != CameraType.SceneView)
            return;

        if (cameraData.renderType == CameraRenderType.Base)
        {
            var camData = camera.GetUniversalAdditionalCameraData();
            if (camData != null && camData.cameraStack != null && camData.cameraStack.Count > 0)
            {
                foreach (var overlayCam in camData.cameraStack)
                {
                    if (overlayCam != null && overlayCam.isActiveAndEnabled)
                        return;
                }
            }
        }

        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        pass?.ReleaseHandles();
        CoreUtils.Destroy(compositeMat);
    }
}