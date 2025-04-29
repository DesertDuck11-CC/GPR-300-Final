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

    private RenderTargetIdentifier source;
    RenderTargetHandle tempTexture;
    private string profilerTag;

    public KuwaharaPass(string pTag)
    {
        this.profilerTag = pTag;
    }

    public void Setup(RenderTargetIdentifier source)
    {
        this.source = source;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cmd.GetTemporaryRT(tempTexture.id, cameraTextureDescriptor);
        ConfigureTarget(tempTexture.Identifier());
        ConfigureClear(ClearFlag.All, Color.black);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        cmd.Clear();
        if (kMat == null)
            return;

        kMat.SetInt("_KernelRadius", kernelSize);
        kMat.SetInt("_MinKernelRadius", minKernelSize);
        kMat.SetInt("_AnimateSize", animate ? 1 : 0);
        kMat.SetFloat("_SizeAnimationSpeed", animationSpeed);
        kMat.SetFloat("_NoiseFrequency", noiseFrequency);
        kMat.SetInt("_AnimateOrigin", animateKernelOrigin ? 1 : 0);
        cmd.Blit(source, tempTexture.Identifier());
        cmd.Blit(tempTexture.Identifier(), source, kMat, 0);


        context.ExecuteCommandBuffer(cmd);
        
        cmd.Clear();

        CommandBufferPool.Release(cmd);
    }

    public void SetValues(int kSize, bool anim, int minKSize, float animSpeed, float noiseFreq, bool animKOrigin)
    {
        kernelSize = kSize;
        animationSpeed = animSpeed;
        noiseFrequency = noiseFreq;
        animate = anim;
        minKernelSize = minKSize;
        animateKernelOrigin = animKOrigin;
    }
}
