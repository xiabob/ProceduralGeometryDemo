using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcCylinderCream : MonoBehaviour
{
    [SerializeField] private MeshFilter m_MeshFilter;
    [SerializeField] private bool m_SupportLight;
    [SerializeField] private int m_RadialSegmentCount = 20;
    [SerializeField] private float m_Radius = 1;
    [SerializeField] private float m_CreamConcaveValue = 0.3f;
    [SerializeField] private AnimationCurve m_CreamConcaveValueCurve;
    [SerializeField] private AnimationCurve m_RadiusCurve;
    [SerializeField] private float m_Height = 6;
    [SerializeField] private int m_HeightSegmentCountPerHeight = 7;
    [SerializeField] private int m_CirclePerHeight = 6;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int> indices = new List<int>();
    private Mesh mesh;
    private int m_HeightSegmentCount = 40;

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

        m_HeightSegmentCount = (int)(m_HeightSegmentCountPerHeight * m_Height);
        float circle = Mathf.PI * 2 * m_Height / m_CirclePerHeight;
        float ringVertexOffset = circle / m_HeightSegmentCount;

        for (int i = 0; i <= m_HeightSegmentCount; i++)
        {
            float progress = i * 1.0f / m_HeightSegmentCount;
            var centerPos = Vector3.left * progress * m_Height;
            var radius = m_RadiusCurve.Evaluate(progress) * m_Radius;
            var offset = ringVertexOffset * i;
            var concaveValue = m_CreamConcaveValueCurve.Evaluate(progress) * m_CreamConcaveValue;
            BuildRing(m_RadialSegmentCount, centerPos, radius, progress, i > 0, offset, concaveValue);

            if (i == 0)
            {
                BuildCap(m_RadialSegmentCount, centerPos, radius, true);
            }
            else if (i == m_HeightSegmentCount)
            {
                BuildCap(m_RadialSegmentCount, centerPos, radius, false);
            }
        }


        RenderMesh();
    }


    private void BuildRing(int segemnt, Vector3 center, float radius, float v, bool makeTriangles, float offset, float concaveValue)
    {
        float anglePerSegment = Mathf.PI * 2f / segemnt;
        for (int i = 0; i <= segemnt; i++)
        {
            float realRadius = radius;
            if (i % 2 == 1)
            {
                realRadius += concaveValue;
            }

            float angle = anglePerSegment * i + offset;
            Vector3 unitPosition = Vector3.zero;
            unitPosition.z = Mathf.Cos(angle);
            unitPosition.y = Mathf.Sin(angle);

            vertices.Add(unitPosition * realRadius + center);
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
                indices.AddRange(new List<int> { p0, p2, p1, p0, p3, p2 });
            }
        }
    }

    private void BuildCap(int segemnt, Vector3 center, float radius, bool isRight)
    {
        Vector3 normal = isRight ? Vector3.left : Vector3.right;

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
            unitPosition.z = Mathf.Cos(angle);
            unitPosition.y = Mathf.Sin(angle);

            vertices.Add(unitPosition * radius + center);
            normals.Add(unitPosition);
            uv.Add(new Vector2(unitPosition.z + 1.0f, unitPosition.y + 1.0f) * 0.5f);

            if (i > 0)
            {
                int baseIndex = vertices.Count - 1;
                if (isRight)
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
        mesh.RecalculateNormals();

        m_MeshFilter.sharedMesh = mesh;
    }
}
