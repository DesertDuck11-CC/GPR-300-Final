using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public struct Boid
{
    public Vector3 position;
    public Vector3 velocity;
}

public class FlockingScript : MonoBehaviour
{
    public ComputeShader cs;
    public ComputeBuffer csBuffer;
    [SerializeField] private Transform minimumBounds;
    [SerializeField] private Transform maximumBounds;
    [SerializeField] Mesh fishMesh;
    [SerializeField] Material fishMat;

    private Boid[] data;
    private int count;
    private int kernel;

    void Start()
    {
        count = 32;
        data = new Boid[count];

        for (int i = 0; i < transform.childCount; i++)
        {
            data[i].position = new Vector3(
                Random.Range(minimumBounds.position.x, maximumBounds.position.x), 
                Random.Range(minimumBounds.position.y, maximumBounds.position.y), 
                Random.Range(minimumBounds.position.z, maximumBounds.position.z)
            );
            
            while(data[i].velocity == null || data[i].velocity == Vector3.zero)
                data[i].velocity = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), Random.Range(-1, 1)).normalized;

            data[i].velocity *= 10;
        }

        csBuffer = new ComputeBuffer(count, sizeof(float) * 6);
        csBuffer.SetData(data);

        kernel = cs.FindKernel("CSMain");
        cs.SetBuffer(kernel, "boids", csBuffer);
        cs.SetInt("boidCount", count);
        cs.SetVector("lowerBounds", minimumBounds.position);
        cs.SetVector("upperBounds", maximumBounds.position);
    }

    private void FixedUpdate()
    {
        cs.SetFloat("dt", Time.fixedDeltaTime);
        int dispatchCount = Mathf.CeilToInt(count / (float)10);
        cs.Dispatch(kernel, dispatchCount, 1, 1);

        csBuffer.GetData(data);

        //might use a compute buffer to get model matrices
        //Graphics.DrawMeshInstanced(fishMesh, 0, fishMat, );//Buffer with arguments, bufferWithArgs, has to have five integer numbers at given argsOffset offset: index count per instance, instance count, start index location, base vertex location, start instance location.
    }

    private void OnDestroy()
    {
        csBuffer.Release();
    }
}
