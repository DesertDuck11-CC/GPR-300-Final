using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static UnityEngine.XR.XRDisplaySubsystem;

internal class KuwaharaRenderer : ScriptableRendererFeature
{

    public Shader kShader;

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


    Material kMat;

    KuwaharaPass kPass = null;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(kPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {

        //kPass.ConfigureInput(ScriptableRenderPassInput.Color);
        var cameraColorTargetID = renderer.cameraColorTarget;
        kPass.Setup(cameraColorTargetID);
        

    }

    public override void Create()
    {
        kMat = CoreUtils.CreateEngineMaterial(kShader);
        kPass = new KuwaharaPass("Kuwahara Pass");
        name = "Kuwahara Pass";
        kPass.SetValues(kernelSize, animate, minKernelSize, animationSpeed, noiseFrequency, animateKernelOrigin);
        kPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(kMat);
    }
}
