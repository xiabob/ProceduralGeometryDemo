using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcCube : MonoBehaviour
{

    [SerializeField] private float m_Width;
    [SerializeField] private float m_Height;
    [SerializeField] private float m_Length;
    [SerializeField] private MeshFilter m_MeshFilter;
    [SerializeField] private bool m_SupportLight;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int> indices = new List<int>();
    Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
    }

    // Update is called once per frame
    void Update()
    {
        BuildCube();
    }

    private void BuildCube()
    {
        vertices.Clear();
        normals.Clear();
        uv.Clear();
        indices.Clear();

        Vector3 upDir = Vector3.up * m_Height;
        Vector3 rightDir = Vector3.right * m_Width;
        Vector3 forwardDir = Vector3.forward * m_Length;

        Vector3 nearCorner = Vector3.zero;
        Vector3 farCorner = upDir + rightDir + forwardDir;

        BuildQuad(nearCorner, forwardDir, rightDir);
        BuildQuad(nearCorner, rightDir, upDir);
        BuildQuad(nearCorner, upDir, forwardDir);

        BuildQuad(farCorner, -rightDir, -forwardDir);
        BuildQuad(farCorner, -upDir, -rightDir);
        BuildQuad(farCorner, -forwardDir, -upDir);

        mesh.vertices = vertices.ToArray();
        mesh.uv = uv.ToArray();
        mesh.triangles = indices.ToArray();
        if (m_SupportLight)
        {
            mesh.normals = normals.ToArray();
        }
        mesh.RecalculateBounds();

        m_MeshFilter.sharedMesh = mesh;
    }

    private void BuildQuad(Vector3 offset, Vector3 widthDir, Vector3 lengthDir)
    {
        var normal = Vector3.Cross(lengthDir, widthDir).normalized;

        vertices.Add(offset);
        uv.Add(new Vector2(0f, 0f));
        normals.Add(normal);

        vertices.Add(offset + lengthDir);
        uv.Add(new Vector2(0f, 1f));
        normals.Add(normal);

        vertices.Add(offset + lengthDir + widthDir);
        uv.Add(new Vector2(1f, 1f));
        normals.Add(normal);

        vertices.Add(offset + widthDir);
        uv.Add(new Vector2(1f, 0f));
        normals.Add(normal);

        int baseIndex = vertices.Count - 4;
        int p0 = baseIndex;
        int p1 = baseIndex + 1;
        int p2 = baseIndex + 2;
        int p3 = baseIndex + 3;
        indices.AddRange(new List<int> { p0, p1, p2, p2, p3, p0 });
    }
}
