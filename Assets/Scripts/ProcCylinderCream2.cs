using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierSolution;

public class ProcCylinderCream2 : MonoBehaviour
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
    [SerializeField] private BezierSpline m_BezierSpline;
    [SerializeField] private float m_SplineMaxHeight = 2;

    List<Vector3> vertices = new List<Vector3>();
    List<Vector3> normals = new List<Vector3>();
    List<Vector2> uv = new List<Vector2>();
    List<int> indices = new List<int>();
    private Mesh mesh;
    private float m_HeightSegmentCount = 40;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void Update()
    {
        BuildCylinder();
        m_Height += m_HeightIncreaseValueForAnimation;
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

            var centerPos = Vector3.left * progress * m_Height;
            var rotation = Quaternion.identity;
            // var splineValue = m_BezierSpline.GetPoint(1) - m_BezierSpline.GetPoint(0);
            // var splineOffset = m_BezierSpline.GetPoint(progress) - m_BezierSpline.GetPoint(0);
            // var splineLength = m_BezierSpline.GetLengthApproximately(0, 1);
            // Vector3 centerPos = Vector3.zero;
            // centerPos.x = splineOffset.x / splineValue.x * m_Height;
            // centerPos.y = splineOffset.y / splineValue.y * (splineValue.y / splineLength * m_SplineMaxHeight);
            // var currentNormal = m_BezierSpline.GetNormal(progress);
            // var radians = Mathf.Atan2(currentNormal.x, currentNormal.y);
            // float zAngle = radians * Mathf.Rad2Deg;
            // var rotation = Quaternion.Euler(0, 0, zAngle);



            var radius = m_RadiusCurve.Evaluate(progress) * m_Radius;
            var offset = ringCircleDegree * i;
            var concaveValue = m_CreamConcaveValueCurve.Evaluate(progress) * m_CreamConcaveValue;
            BuildRing(m_RadialSegmentCount, centerPos, radius, progress, i > 0, offset, concaveValue, rotation);

            if (i == 0)
            {
                BuildCap(m_RadialSegmentCount, centerPos, radius, true);
            }
            else if (i >= m_HeightSegmentCount)
            {
                BuildCap(m_RadialSegmentCount, centerPos, radius, false);
            }
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
            unitPosition.z = Mathf.Cos(angle);
            unitPosition.y = Mathf.Sin(angle);

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
            mesh.RecalculateNormals();
        }

        m_MeshFilter.sharedMesh = mesh;
    }

}
