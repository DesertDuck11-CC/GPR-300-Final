using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using static UnityEngine.XR.XRDisplaySubsystem;

internal class KuwaharaRenderer : ScriptableRendererFeature
{

    public Shader kShader;
    public float intensity;

    Material kMat;

    KuwaharaPass kPass = null;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(kPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            kPass.ConfigureInput(ScriptableRenderPassInput.Color);
            
        }
    }

    public override void Create()
    {
        throw new System.NotImplementedException();
    }
}
