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
    //Compute shaders to handle flocking
    public ComputeShader csFlocking;
    public ComputeBuffer cbFlocking;

    //Compute shaders to change from position and forward to model matrix
    public ComputeShader csMatrix;
    public ComputeBuffer cbMatrixMatrices;

    private Vector3 minimumBounds = Vector3.zero;
    private Vector3 maximumBounds = new Vector3(250, 50, 250);

    //Passed into DrawMeshInstanced
    [SerializeField] Mesh fishMesh;
    [SerializeField] Material fishMat;

    private Boid[] data; //buffer of boids
    private int count; //number of boids
    private int flockingKernel;

    private Matrix4x4[] modelMatrices;
    private int matrixKernel;

    void Awake()
    {
        count = 1024 * 8;
        data = new Boid[count];
        modelMatrices = new Matrix4x4[count];

        //region contains random generating might move to a compute shader
        #region
        for (int i = 0; i < count; i++)
        {
            data[i].position = new Vector3(
                Random.Range(minimumBounds.x, maximumBounds.x), 
                Random.Range(minimumBounds.y, maximumBounds.y), 
                Random.Range(minimumBounds.z, maximumBounds.z)
            );
            
            while(data[i].velocity == null || data[i].velocity == Vector3.zero || data[i].velocity == Vector3.up || data[i].velocity == Vector3.down)
                data[i].velocity = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

            data[i].velocity *= 10;
        }
        #endregion

        // setting up flocking compute shader
        cbFlocking = new ComputeBuffer(count, sizeof(float) * 6);
        cbFlocking.SetData(data);

        flockingKernel = csFlocking.FindKernel("CSMain");
        csFlocking.SetBuffer(flockingKernel, "boids", cbFlocking);
        csFlocking.SetInt("boidCount", count);
        csFlocking.SetVector("lowerBounds", minimumBounds);
        csFlocking.SetVector("upperBounds", maximumBounds);

        //setting up model compute shader
        cbMatrixMatrices = new ComputeBuffer(count, sizeof(float) * 4 * 4);
        cbMatrixMatrices.SetData(modelMatrices);

        matrixKernel = csMatrix.FindKernel("CSMain");
        csMatrix.SetBuffer(matrixKernel, "boids", cbFlocking);
        csMatrix.SetBuffer(matrixKernel, "matrices", cbMatrixMatrices);
        csMatrix.SetInt("boidCount", count);
        csMatrix.SetVector("offset", transform.position);
    }

    private void Update()
    {
        csFlocking.SetFloat("dt", Time.deltaTime);
        int dispatchCount = (count + 9) / 10;
        csFlocking.Dispatch(flockingKernel, dispatchCount, 1, 1);
    }

    private void LateUpdate()
    {
        int dispatchCount = (count + 9) / 10;
        csMatrix.Dispatch(matrixKernel, dispatchCount, 1, 1);

        cbMatrixMatrices.GetData(modelMatrices);

        
        Graphics.DrawMeshInstanced(fishMesh, 0, fishMat, modelMatrices);
    }

    private void OnDestroy()
    {
        cbFlocking.Release();
        cbMatrixMatrices.Release();
    }
}
