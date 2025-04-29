using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KCamera : MonoBehaviour
{
    public Shader kShader;
    public Material kMat;

    private void Start()
    {
        Camera camera = GetComponent<Camera>();
        //kMat ??= new Material(kShader);
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, kMat);
    }
}
