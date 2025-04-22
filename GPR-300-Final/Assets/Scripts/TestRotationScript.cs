using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestRotationScript : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    [SerializeField] Material mat;

    [SerializeField] Vector3 forward;

    Matrix4x4 matrix;


    void Update()
    {
        Vector3 forw = Vector3.Normalize(forward);
        float theta = Mathf.Asin(forw.y);
        float phi = -Mathf.Atan2(forw.x, forw.z);

        Matrix4x4 yaw = new Matrix4x4
        (
            new Vector4(Mathf.Cos(phi), 0, Mathf.Sin(phi), 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(-Mathf.Sin(phi), 0, Mathf.Cos(phi), 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 pitch = new Matrix4x4
        (
            new Vector4(1, 0, 0, 0),
            new Vector4(0, Mathf.Cos(theta), -Mathf.Sin(theta), 0),
            new Vector4(0, Mathf.Sin(theta), Mathf.Cos(theta), 0),
            new Vector4(0, 0, 0, 1)
        );

        Matrix4x4 translation = new Matrix4x4
        (
            new Vector4(1, 0, 0, 0),
            new Vector4(0, 1, 0, 0),
            new Vector4(0, 0, 1, 0),
            new Vector4(transform.position.x, transform.position.y, transform.position.z, 1)
        );

        Matrix4x4 rotation = yaw * pitch;

        matrix = Matrix4x4.identity;
        matrix = matrix * translation * rotation;
        //matrix = matrix * translation;

        Matrix4x4[] matrixList = new Matrix4x4[] { matrix };
        Graphics.DrawMeshInstanced(mesh, 0, mat, matrixList);
        Debug.DrawLine(Vector3.zero, forward * 10f, Color.red, Time.deltaTime);
    }
}
