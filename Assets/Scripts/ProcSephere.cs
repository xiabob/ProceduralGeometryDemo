using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcSephere : MonoBehaviour
{
    [SerializeField] private MeshFilter m_MeshFilter;
    [SerializeField] private bool m_SupportLight = true;
    [SerializeField] private int m_RadialSegmentCount = 20;
    [SerializeField] private float m_Radius;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int> indices = new List<int>();
    private Mesh mesh;
    private int heightSegmentCount = 2;

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

        heightSegmentCount = 20;
        float angleInc = Mathf.PI / heightSegmentCount;
        for (int i = 0; i <= heightSegmentCount; i++)
        {
            Vector3 centerPos = Vector3.zero;
            centerPos.y = -Mathf.Cos(angleInc * i) * m_Radius;
            var radius = Mathf.Sin(angleInc * i) * m_Radius;
            float v = i * 1.0f / heightSegmentCount;
            BuildRing(m_RadialSegmentCount, centerPos, radius, v, i > 0);
        }

        RenderMesh();
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
