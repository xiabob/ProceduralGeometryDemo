using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcCylinder : MonoBehaviour
{
    [SerializeField] private MeshFilter m_MeshFilter;
    [SerializeField] private bool m_SupportLight;
    [SerializeField] private int m_RadialSegmentCount = 20;
    [SerializeField] private int m_HeightSegmentCount = 2;
    [SerializeField] private float m_Radius;
    [SerializeField] private float m_Height;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int> indices = new List<int>();
    private Mesh mesh;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        BuildCylinder();
    }

    private void BuildCylinder()
    {
        vertices.Clear();
        normals.Clear();
        uv.Clear();
        indices.Clear();

        BuildRings();

        BuildCap(m_RadialSegmentCount, Vector3.zero, m_Radius, false);
        BuildCap(m_RadialSegmentCount, Vector3.up * m_Height, m_Radius, true);

        RenderMesh();
    }

    private void BuildRings()
    {
        for (int i = 0; i <= m_HeightSegmentCount; i++)
        {
            float part = i * 1.0f / m_HeightSegmentCount;
            BuildRing(m_RadialSegmentCount, Vector3.up * part * m_Height, m_Radius, part, i > 0);
        }
        // BuildRing(m_RadialSegmentCount, Vector3.zero, m_Radius, 0, false);
        // BuildRing(m_RadialSegmentCount, Vector3.up * m_Height, m_Radius, 1, true);
    }

    private void BuildRing(int segemnt, Vector3 center, float radius, float v, bool makeTriangles)
    {
        float anglePerSegment = Mathf.PI * 2f / segemnt;
        for (int i = 0; i <= segemnt; i++)
        {
            float angle = anglePerSegment * i;
            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.z = Mathf.Sin(angle);

            vertices.Add(unitPosition * radius + center);
            normals.Add(unitPosition);
            uv.Add(new Vector2(i * 1.0f / segemnt, v));

            if (i > 0 && makeTriangles)
            {
                int baseIndex = vertices.Count - 1;
                int vericesPerRow = segemnt + 1;

                int p0 = baseIndex - 1 - vericesPerRow;
                int p1 = baseIndex - 1;
                int p2 = baseIndex;
                int p3 = p0 + 1;
                indices.AddRange(new List<int> { p0, p1, p2, p2, p3, p0 });
            }
        }
    }

    private void BuildCap(int segemnt, Vector3 center, float radius, bool isTopCap)
    {
        Vector3 normal = isTopCap ? Vector3.up : Vector3.down;

        // add one vertex in the center
        vertices.Add(center);
        normals.Add(normal);
        uv.Add(Vector2.one * 0.5f);

        int centerVertexIndex = vertices.Count - 1;

        // for simple logic we add some duplicate vertex
        float anglePerSegment = Mathf.PI * 2f / segemnt;
        for (int i = 0; i <= segemnt; i++)
        {
            float angle = anglePerSegment * i;
            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.z = Mathf.Sin(angle);

            vertices.Add(unitPosition * radius + center);
            normals.Add(unitPosition);
            uv.Add(new Vector2(unitPosition.x + 1.0f, unitPosition.z + 1.0f) * 0.5f);

            if (i > 0)
            {
                int baseIndex = vertices.Count - 1;
                if (isTopCap)
                {
                    indices.AddRange(new List<int> { baseIndex, baseIndex - 1, centerVertexIndex });
                }
                else
                {
                    indices.AddRange(new List<int> { baseIndex, centerVertexIndex, baseIndex - 1 });
                }
            }
        }

    }

    private void RenderMesh()
    {
        if (mesh == null)
        {
            mesh = new Mesh();
        }
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

}
