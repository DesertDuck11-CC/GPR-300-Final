using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

internal class KuwaharaPass : ScriptableRenderPass
{
    ProfilingSampler sampler = new ProfilingSampler("ColorBlit");
    Material kMat;
    RTHandle camColorTarget;
    float colorIntensity;
    [Range(1, 20)]
    public int kernelSize = 1;

    public bool animate = false;

    [Range(1, 20)]
    public int minKernelSize = 1;

    [Range(0.1f, 5.0f)]
    public float animationSpeed = 1.0f;

    [Range(0.0f, 30.0f)]
    public float noiseFrequency = 10.0f;

    public bool animateKernelOrigin = false;

    [Range(1, 4)]
    public int passes = 1;

    public KuwaharaPass(Material mat)
    {
        kMat = mat;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
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
        using (new ProfilingScope(cmd, profilingSampler))
        {
            kMat.SetInt("_KernelRadius", kernelSize);
            kMat.SetInt("_MinKernelRadius", minKernelSize);
            kMat.SetInt("_AnimateSize", animate ? 1 : 0);
            kMat.SetFloat("_SizeAnimationSpeed", animationSpeed);
            kMat.SetFloat("_NoiseFrequency", noiseFrequency);
            kMat.SetInt("_AnimateOrigin", animateKernelOrigin ? 1 : 0);
            Blitter.BlitCameraTexture(cmd, camColorTarget, camColorTarget, kMat, 0);
        }
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    public void SetTarget(RTHandle colorHandle, float intensity)
    {
        camColorTarget = colorHandle;
        colorIntensity = intensity;
    }
}
