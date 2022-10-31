using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcCylinderBlendTaper : MonoBehaviour
{
    [SerializeField] private MeshFilter m_MeshFilter;
    [SerializeField] private bool m_SupportLight;
    [SerializeField] private int m_RadialSegmentCount = 20;
    [SerializeField] private int m_HeightSegmentCount = 2;
    [SerializeField] private float m_RadiusStart = 4;
    [SerializeField] private float m_RadiusEnd = 0;
    [SerializeField] private float m_Height = 3;
    [SerializeField] private float m_BlendAngle = 180;

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

        float blendRadians = m_BlendAngle * Mathf.Deg2Rad;
        float blendRadius = m_Height / blendRadians;
        float blendRadiansPerSegment = blendRadians / m_HeightSegmentCount;
        Vector3 startOffset = blendRadius * Vector3.right;

        for (int i = 0; i <= m_HeightSegmentCount; i++)
        {
            Vector3 centerPos = Vector3.zero;
            centerPos.x = Mathf.Cos(blendRadiansPerSegment * i);
            centerPos.y = Mathf.Sin(blendRadiansPerSegment * i);
            centerPos *= blendRadius;
            centerPos -= startOffset;

            float zAngle = blendRadiansPerSegment * i * Mathf.Rad2Deg;
            var rotation = Quaternion.Euler(0, 0, zAngle);

            float progress = i * 1.0f / m_HeightSegmentCount;
            var radius = Mathf.Lerp(m_RadiusStart, m_RadiusEnd, progress);
            BuildRing(m_RadialSegmentCount, centerPos, radius, progress, i > 0, rotation);

            if (i == 0)
            {
                BuildCap(m_RadialSegmentCount, centerPos, radius, false, rotation);
            }
            else if (i == m_HeightSegmentCount)
            {
                BuildCap(m_RadialSegmentCount, centerPos, radius, true, rotation);
            }
        }

        RenderMesh();
    }


    private void BuildRing(int segemnt, Vector3 center, float radius, float v, bool makeTriangles, Quaternion rotation)
    {
        float anglePerSegment = Mathf.PI * 2f / segemnt;
        for (int i = 0; i <= segemnt; i++)
        {
            float angle = anglePerSegment * i;
            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.z = Mathf.Sin(angle);

            unitPosition = rotation * unitPosition;

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

    private void BuildCap(int segemnt, Vector3 center, float radius, bool isTopCap, Quaternion rotation)
    {
        var rotationRadians = rotation.z * Mathf.Deg2Rad;
        Vector3 normal = new Vector3(Mathf.Sin(rotationRadians), Mathf.Cos(rotationRadians), 0);

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

            unitPosition = rotation * unitPosition;

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
