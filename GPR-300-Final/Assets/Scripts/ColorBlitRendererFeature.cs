using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

internal class ColorBlitRendererFeature : ScriptableRendererFeature
{
    public Shader shader;
    public int kernelRadius;

    Material kMat;

    ColorBlitPass renderPass = null;

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                    ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
            renderer.EnqueuePass(renderPass);
    }

    public override void SetupRenderPasses(ScriptableRenderer renderer,
                                        in RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Game)
        {
            // Calling ConfigureInput with the ScriptableRenderPassInput.Color argument
            // ensures that the opaque texture is available to the Render Pass.
            renderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            renderPass.SetTarget(renderer.cameraColorTargetHandle, kernelRadius);
        }
    }

    public override void Create()
    {
        kMat = CoreUtils.CreateEngineMaterial(shader);
        renderPass = new ColorBlitPass(kMat);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(kMat);
    }
}