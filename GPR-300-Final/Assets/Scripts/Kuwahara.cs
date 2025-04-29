using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kuwahara : MonoBehaviour
{
    public Shader shader;

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

    private Material kMat;

    void OnEnable()
    {
        kMat = new Material(shader);
        kMat.hideFlags = HideFlags.HideAndDontSave;
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        kMat.SetInt("_KernelRadius", kernelSize);
        kMat.SetInt("_MinKernelSize", minKernelSize);
        kMat.SetInt("_AnimateSize", animate ? 1 : 0);
        kMat.SetFloat("_SizeAnimationSpeed", animationSpeed);
        kMat.SetFloat("_NoiseFrequency", noiseFrequency);
        kMat.SetInt("_AnimateOrigin", animateKernelOrigin ? 1 : 0);

        RenderTexture[] kuwaharaPasses = new RenderTexture[passes];

        for (int i = 0; i < passes; ++i)
        {
            kuwaharaPasses[i] = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
        }

        Graphics.Blit(source, kuwaharaPasses[0], kMat);

        for (int i = 1; i < passes; ++i)
        {
            Graphics.Blit(kuwaharaPasses[i - 1], kuwaharaPasses[i], kMat);
        }

        Graphics.Blit(kuwaharaPasses[passes - 1], destination);
        for (int i = 0; i < passes; ++i)
        {
            RenderTexture.ReleaseTemporary(kuwaharaPasses[i]);
        }
    }

    void OnDisable()
    {
        kMat = null;
    }
}
