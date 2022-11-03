using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;

public class ProcCylinderCream3 : MonoBehaviour
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
    [SerializeField] private float m_HeightIncreaseValueForAnimation = 1f / 80;
    [SerializeField] private float m_RingCircleDegree = 30;
    [SerializeField] private MeshCollider m_Collider;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int> indices = new List<int>();
    private Mesh mesh;
    private float m_HeightSegmentCount = 40;
    private float m_OffsetStartPoint;
    private float m_CenterOffset;
    private float m_Speed;
    private float m_OriginRadius, m_OriginCreamConcaveValue, m_OriginHeight;
    private bool m_DidPrepare;

    // Start is called before the first frame update
    void Start()
    {
        m_OriginRadius = m_Radius;
        m_OriginCreamConcaveValue = m_CreamConcaveValue;
        m_OriginHeight = m_Height;
        m_OffsetStartPoint = m_Height;

        m_Radius *= 0.1f;
        m_CreamConcaveValue *= 0.1f;
        m_Height *= 0.1f;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_Speed = m_HeightIncreaseValueForAnimation;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            m_Speed = 0;
        }

        if (!m_DidPrepare && m_Speed > 0)
        {
            float delta = Time.fixedDeltaTime * 1f / 10;
            m_Radius += delta * m_OriginRadius;
            m_CreamConcaveValue += delta * m_OriginCreamConcaveValue;
            m_Height += delta * m_OriginHeight;

            if (m_Radius >= m_OriginRadius)
            {
                m_Radius = m_OriginRadius;
                m_DidPrepare = true;
            }

            BuildCylinder();
        }

        if (m_DidPrepare && m_Speed > 0)
        {
            m_CenterOffset += m_Speed;
            m_Height += m_Speed;

            BuildCylinder();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var position = other.ClosestPointOnBounds(transform.position);
        Debug.LogError("xxxx " + position.ToString());
    }

    private void BuildCylinder()
    {
        vertices.Clear();
        normals.Clear();
        uv.Clear();
        indices.Clear();

        m_HeightSegmentCount = m_HeightSegmentCountPerHeight * m_Height;
        float ringCircleDegree = m_RingCircleDegree * Mathf.Deg2Rad;

        for (int i = 0; i <= Mathf.CeilToInt(m_HeightSegmentCount); i++)
        {
            float progress = i * 1.0f / m_HeightSegmentCount;

            var centerPos = Vector3.down * progress * m_Height;
            var rotation = Quaternion.identity;

            var radius = m_RadiusCurve.Evaluate(progress) * m_Radius;
            var offset = ringCircleDegree * i;
            var concaveValue = m_CreamConcaveValueCurve.Evaluate(progress) * m_CreamConcaveValue;
            BuildRing(m_RadialSegmentCount, centerPos, radius, progress, i > 0, offset, concaveValue, rotation);

            // if (i == 0)
            // {
            //     BuildCap(m_RadialSegmentCount, centerPos, radius, true, offset, concaveValue, rotation);
            // }
            // else if (i >= m_HeightSegmentCount)
            // {
            //     BuildCap(m_RadialSegmentCount, centerPos, radius, false, offset, concaveValue, rotation);
            // }
        }


        RenderMesh();
    }


    private void BuildRing(int segemnt, Vector3 center, float radius, float v, bool makeTriangles, float offset, float concaveValue, Quaternion rotation)
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
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.z = Mathf.Sin(angle);

            unitPosition = rotation * unitPosition;

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

    private void BuildCap(int segemnt, Vector3 center, float radius, bool isTopCap, float offset, float concaveValue, Quaternion rotation)
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
            float realRadius = radius;
            if (i % 2 == 1)
            {
                realRadius += concaveValue;
            }

            float angle = anglePerSegment * i + offset;
            Vector3 unitPosition = Vector3.zero;
            unitPosition.x = Mathf.Cos(angle);
            unitPosition.z = Mathf.Sin(angle);

            unitPosition = rotation * unitPosition;

            vertices.Add(unitPosition * realRadius + center);
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
            mesh.RecalculateNormals();
        }

        m_MeshFilter.sharedMesh = mesh;
        m_Collider.sharedMesh = mesh;
    }
}
