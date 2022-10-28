using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcQuad : MonoBehaviour
{
    [SerializeField] private float m_Width;
    [SerializeField] private float m_Height;
    [SerializeField] private MeshFilter m_MeshFilter;
    [SerializeField] private bool m_SupportLight;

    // Start is called before the first frame update
    void Start()
    {
        BuildMesh();
    }

    private void Update()
    {
        BuildMesh();
    }

    private void BuildMesh()
    {
        Vector3[] vertices = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Vector2[] uv = new Vector2[4];

        vertices[0] = new Vector3(0, 0, 0);
        normals[0] = Vector3.up;
        uv[0] = new Vector2(0, 0);

        vertices[1] = new Vector3(0, 0, m_Height);
        normals[1] = Vector3.up;
        uv[1] = new Vector2(0, 1);

        vertices[2] = new Vector3(m_Width, 0, m_Height);
        normals[2] = Vector3.up;
        uv[2] = new Vector2(1, 1);

        vertices[3] = new Vector3(m_Width, 0, 0);
        normals[3] = Vector3.up;
        uv[3] = new Vector2(1, 0);

        int[] indices = new int[6] { 0, 1, 2, 2, 3, 0 }; // a triangle containes 3 vertices 

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = indices;
        if (m_SupportLight)
        {
            mesh.normals = normals;
        }
        mesh.RecalculateBounds();

        m_MeshFilter.sharedMesh = mesh;
    }
}
