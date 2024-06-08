using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public struct VerletNode
{
    public Vector3 Position;
    public Vector3 PrevoiusPosition;
    public Vector3 Velocity;
    public float NodeMass;
}


public class VerletRope : MonoBehaviour
{
    private VerletNode[] m_VerletNodes;
    [SerializeField] private float m_RopeMass;
    [SerializeField] private float m_RopeLength;
    [SerializeField] private int m_NumberOfNodesPerLength;
    [SerializeField] private float m_ConstraintIterationCount;
    [SerializeField] private Vector3 m_Gravity;

    private float m_DistanceBetweenNodes;

    [SerializeField] private float m_VerletNodeGizmoRadius;

    private void Awake()
    {
        m_VerletNodes = new VerletNode[(int)(m_NumberOfNodesPerLength * m_RopeLength)];
        Debug.Log("Initializing Verlet rope with " + m_VerletNodes.Length + " nodes.");
        m_DistanceBetweenNodes = 1f / m_NumberOfNodesPerLength;

        for (int i = 0; i < m_VerletNodes.Length; i++)
        {
            m_VerletNodes[i].Position = transform.position - new Vector3(0f, (m_DistanceBetweenNodes * i), 0f);
            m_VerletNodes[i].PrevoiusPosition = m_VerletNodes[i].Position;
            m_VerletNodes[i].NodeMass = m_RopeMass / m_VerletNodes.Length;
        }
    }

    private void FixedUpdate()
    {
        CalculateNewPositions();

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
            Gizmos.DrawSphere(m_VerletNodes[i].Position, m_VerletNodeGizmoRadius);

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
                                        (m_Gravity * (float)Math.Pow(Time.fixedDeltaTime, 2));
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

            Debug.Log("d1: " + d1 + ", d2: " + d2 + ", d3: " + d3);
            m_VerletNodes[i].Position -= (d1 * (0.5f * d3));
            m_VerletNodes[i + 1].Position += (d1 * (0.5f * d3));
        }
    }
}
