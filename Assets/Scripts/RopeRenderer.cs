using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(VerletRope))]
public class RopeRenderer : MonoBehaviour
{
    [Min(3)] [SerializeField] private int m_RopeSegmentSides;

    private MeshFilter m_MeshFilter;
    private MeshRenderer m_MeshRenderer;
    private Mesh m_RopeMesh;
    private VerletRope m_Rope;
    private Vector3[] m_Vertices;
    private int[] m_Triangles;

    private float m_Angle;
    private int m_NodeCount;
    private bool m_IsInitialized;

    private void Awake()
    {
        m_MeshFilter = GetComponent<MeshFilter>();
        m_MeshRenderer = GetComponent<MeshRenderer>();

        m_RopeMesh = new Mesh();
        m_Angle = ((m_RopeSegmentSides - 2) * 180) / m_RopeSegmentSides;
        m_IsInitialized = false;
    }

    private void Start()
    {
        m_Rope = GetComponent<VerletRope>();
        m_Vertices = new Vector3[m_Rope.GetNodeCount() * m_RopeSegmentSides];
        m_Triangles = new int[m_RopeSegmentSides * (m_Rope.GetNodeCount() - 1) * 6];
    }

    // private void OnDrawGizmos()
    // {
    //     if (!Application.isPlaying)
    //         return;
    //
    //     if (m_Vertices is null || m_Triangles is null)
    //         return;
    //
    //
    //     foreach (var vert in m_Vertices)
    //     {
    //         Gizmos.color = Color.white;
    //         Gizmos.DrawSphere(vert, 0.01f);
    //     }
    //
    //     for (int i = 0; i < m_Triangles.Length - 3; i += 3)
    //     {
    //         Gizmos.DrawLine(m_Vertices[m_Triangles[i]], m_Vertices[m_Triangles[i + 1]]);
    //         Gizmos.DrawLine(m_Vertices[m_Triangles[i + 1]], m_Vertices[m_Triangles[i + 2]]);
    //         Gizmos.DrawLine(m_Vertices[m_Triangles[i + 2]], m_Vertices[m_Triangles[i]]);
    //     }
    // }

    public void RenderRope(VerletNode[] nodes, float radius)
    {
        if (m_Vertices is null || m_Triangles is null)
            return;

        ComputeVertices(nodes, radius);
        
        if(!m_IsInitialized){
            ComputeTriangles();
            m_IsInitialized = true;
        }
        
        SetupMeshFilter();
    }

    private void ComputeVertices(VerletNode[] nodes, float radius)
    {
        var angle = (360f / m_RopeSegmentSides) * Mathf.Deg2Rad;

        for (int i = 0; i < m_Vertices.Length; i++)
        {
            var nodeindex = i / m_RopeSegmentSides;
            var sign = nodeindex == nodes.Length - 1 ? -1 : 1;
            Debug.Log($"Node Index: {nodeindex}, Vert Index: {i} , {m_Vertices[i]}");
            
            var currNodePosition = nodes[nodeindex].Position;
            var normalOfPlane =
                (sign * nodes[nodeindex].Position + -sign * nodes[nodeindex + (nodeindex == nodes.Length - 1 ? -1 : 1)].Position)
                .normalized;

            var u = Vector3.Cross(normalOfPlane, Vector3.forward).normalized;
            var v = Vector3.Cross(u, normalOfPlane).normalized;

            m_Vertices[i] = currNodePosition + radius * (float)Math.Cos(angle * (i % m_RopeSegmentSides)) * u +
                            radius * (float)Math.Sin(angle * (i % m_RopeSegmentSides)) * v;
        }
    }

    private void ComputeTriangles()
    {
        var tn = 0;

        for (int i = 0; i < m_Vertices.Length - m_RopeSegmentSides; i++)
        {
            var nexti = (i + 1) % m_RopeSegmentSides == 0 ? i - m_RopeSegmentSides + 1 : i + 1;

            m_Triangles[tn] = i;
            m_Triangles[tn + 1] = i + m_RopeSegmentSides;
            m_Triangles[tn + 2] = nexti + m_RopeSegmentSides;

            m_Triangles[tn + 3] = i;
            m_Triangles[tn + 4] = nexti + m_RopeSegmentSides;
            m_Triangles[tn + 5] = nexti;

            tn += 6;
        }
    }

    private void SetupMeshFilter()
    {
        for (int i = 0; i < m_Vertices.Length; i++)
        {
            m_Vertices[i] -= transform.position;
        }
        
        m_RopeMesh.Clear();
        m_RopeMesh.vertices = m_Vertices;
        m_RopeMesh.triangles = m_Triangles;

        m_MeshFilter.mesh = m_RopeMesh;
        m_MeshFilter.mesh.RecalculateBounds();
        m_MeshFilter.mesh.RecalculateNormals();
    }
}