using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public struct VerletNode
{
    public Vector3 Position;
    public Vector3 PrevoiusPosition;
    public Vector3 Accelertaion;

    public void AddAcceleration(Vector3 accelaration)
    {
        Accelertaion += accelaration;
    }

    public void ZeroAcceleration()
    {
        Accelertaion = Vector3.zero;
    }
}

public class VerletRope : MonoBehaviour
{
    private VerletNode[] m_VerletNodes;
    [SerializeField] private float m_RopeLength;
    [SerializeField] private int m_NumberOfNodesPerLength;
    [SerializeField] private float m_ConstraintIterationCount;
    [SerializeField] private Vector3 m_Gravity;

    private float m_DistanceBetweenNodes;

    [SerializeField] private float m_RopeRadius;

    private void Awake()
    {
        m_VerletNodes = new VerletNode[(int)(m_NumberOfNodesPerLength * m_RopeLength)];
        Debug.Log("Initializing Verlet rope with " + m_VerletNodes.Length + " nodes.");
        m_DistanceBetweenNodes = 1f / m_NumberOfNodesPerLength;

        for (int i = 0; i < m_VerletNodes.Length; i++)
        {
            m_VerletNodes[i].Position = transform.position - new Vector3(0f, (m_DistanceBetweenNodes * i), 0f);
            m_VerletNodes[i].PrevoiusPosition = m_VerletNodes[i].Position;
        }
    }

    private void FixedUpdate()
    {
        CalculateNewPositions();
        ApplyCollision();


        for (int i = 0; i < m_ConstraintIterationCount; i++)
        {
            FixNodeDistances();
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;


        for (int i = 0; i < m_VerletNodes.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(m_VerletNodes[i].Position, m_RopeRadius);

            if (i != m_VerletNodes.Length - 1)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(m_VerletNodes[i].Position, m_VerletNodes[i + 1].Position);
            }
        }
    }

    private void CalculateNewPositions()
    {
        for (int i = 0; i < m_VerletNodes.Length; i++)
        {
            var currNode = m_VerletNodes[i];
            var newPreviousPosition = currNode.Position;

            m_VerletNodes[i].Position = (2 * currNode.Position) - currNode.PrevoiusPosition +
                                        ((m_Gravity) * (float)Math.Pow(Time.fixedDeltaTime, 2));
            m_VerletNodes[i].PrevoiusPosition = newPreviousPosition;
        }
    }

    private void FixNodeDistances()
    {
        m_VerletNodes[0].Position = transform.position;

        for (int i = 0; i < m_VerletNodes.Length - 1; i++)
        {
            var n1 = m_VerletNodes[i];
            var n2 = m_VerletNodes[i + 1];

            var d1 = n1.Position - n2.Position;
            var d2 = d1.magnitude;
            var d3 = (d2 - m_DistanceBetweenNodes) / d2;

            m_VerletNodes[i].Position -= (d1 * (0.5f * d3));
            m_VerletNodes[i + 1].Position += (d1 * (0.5f * d3));
        }
    }

    private void ApplyCollision()
    {
        // Dictionary<VerletNode, Collider[]> collisionsOfEachNode = new Dictionary<VerletNode, Collider[]>();

        for(int i = 0; i < m_VerletNodes.Length; i++)
        {
            // Collider[] colliders = new Collider[1];
            var colliders = Physics.OverlapSphere(m_VerletNodes[i].Position, m_RopeRadius);

            if (colliders.Length == 0)
            {
                m_VerletNodes[i].ZeroAcceleration();
                continue;
            }

            foreach (var col in colliders)
            {
                var closestPoint = col.ClosestPoint(m_VerletNodes[i].Position);
                var isInside = closestPoint.Equals(m_VerletNodes[i].Position);
                
                if (isInside)
                {
                    m_VerletNodes[i].Position += (closestPoint - m_VerletNodes[i].Position);
                    m_VerletNodes[i].Position += ((-closestPoint + m_VerletNodes[i].Position).normalized * m_RopeRadius);
                }
                else
                {
                    Debug.Log("node: " + m_VerletNodes[i].Position + "point: " + closestPoint);
                    m_VerletNodes[i].Position += (-closestPoint + m_VerletNodes[i].Position).normalized * m_RopeRadius;
                }
            }
        }
    }
}