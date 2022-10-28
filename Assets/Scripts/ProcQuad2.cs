using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcQuad2 : MonoBehaviour
{
    [SerializeField] private float m_Size;
    [SerializeField] private int m_Segment;
    [SerializeField] private AnimationCurve m_XPlaneCurve;
    [SerializeField] private AnimationCurve m_ZPlaneCurve;
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
        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uv = new List<Vector2>();
        List<int> indices = new List<int>();

        float segmentSize = m_Size / m_Segment;
        List<float> offsetYForXPlane = new List<float>();
        List<float> offsetYForZPlane = new List<float>();
        for (int i = 0; i <= m_Segment; i++)
        {
            float progress = i * 1f / m_Segment;
            offsetYForXPlane.Add(m_XPlaneCurve.Evaluate(progress));
            offsetYForZPlane.Add(m_ZPlaneCurve.Evaluate(progress));
        }
        for (int z = 0; z <= m_Segment; z++)
        {
            for (int x = 0; x <= m_Segment; x++)
            {
                vertices.Add(new Vector3(x * segmentSize, offsetYForXPlane[x] * offsetYForZPlane[z], z * segmentSize));
                normals.Add(Vector3.up);
                uv.Add(new Vector2(x * 1.0f / m_Segment, z * 1.0f / m_Segment));
                if (x > 0 && z > 0)
                {
                    int pointsPerRow = m_Segment + 1;
                    int p0 = vertices.Count - pointsPerRow - 2; // 0
                    int p1 = vertices.Count - 2; // 5
                    int p2 = vertices.Count - 1; // 6
                    int p3 = vertices.Count - pointsPerRow - 1; // 1
                    indices.AddRange(new List<int> { p0, p1, p2, p2, p3, p0 });
                }
            }
        }

        Mesh mesh = new Mesh();
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
