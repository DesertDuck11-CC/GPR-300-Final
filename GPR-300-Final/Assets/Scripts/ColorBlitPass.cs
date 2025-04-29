using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class ColorBlitPass : ScriptableRenderPass
{
    ProfilingSampler sampler = new ProfilingSampler("ColorBlit");
    Material kMat;
    RTHandle camColorTarget;
    int kernelRadius;

    public ColorBlitPass(Material material)
    {
        kMat = material;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public void SetTarget(RTHandle colorHandle, int kRadius)
    {
        camColorTarget = colorHandle;
        kernelRadius = kRadius;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        ConfigureTarget(camColorTarget);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cameraData = renderingData.cameraData;
        if (cameraData.camera.cameraType != CameraType.Game)
            return;

        if (kMat == null)
            return;

        CommandBuffer cmd = CommandBufferPool.Get();
        using (new ProfilingScope(cmd, sampler))
        {
            kMat.SetInt("_KernelRadius", kernelRadius);
            Blitter.BlitCameraTexture(cmd, camColorTarget, camColorTarget, kMat, 0);
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }
}