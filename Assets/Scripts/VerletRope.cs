using System;
using System.Collections.Generic;
using UnityEngine;

public struct VerletNode
{
    public Vector3 Position;
    public Vector3 PrevoiusPosition;
    public Vector3 Accelertaion;

    public void AddAcceleration(Vector3 accelaration)
    {
        Accelertaion += accelaration;
    }
}

public class VerletRope : MonoBehaviour
{
    private VerletNode[] m_VerletNodes;
    [SerializeField] private float m_RopeLength;
    [SerializeField] private int m_NumberOfNodesPerLength;
    [SerializeField] private int m_ConstraintIterationCount;
    [SerializeField] private Vector3 m_Gravity;


    private float m_DistanceBetweenNodes;

    [SerializeField] private float m_RopeRadius;
    private Vector3[] m_lookaheadSimulationResults;
    private List<int> m_simulationIgnoreIndex;
    [SerializeField] [Range(0f, 1f)] private float m_LowPassFilterCutoff;

    private void Awake()
    {
        m_VerletNodes = new VerletNode[(int)(m_NumberOfNodesPerLength * m_RopeLength)];
        m_DistanceBetweenNodes = 1f / m_NumberOfNodesPerLength;
        m_lookaheadSimulationResults = new Vector3[m_VerletNodes.Length];
        m_simulationIgnoreIndex = new List<int>();

        for (int i = 0; i < m_VerletNodes.Length; i++)
        {
            m_VerletNodes[i].Position = transform.position - new Vector3(0f, (m_DistanceBetweenNodes * i), 0f);
            m_VerletNodes[i].PrevoiusPosition = m_VerletNodes[i].Position;
        }
    }

    private void FixedUpdate()
    {
        CalculateNewPositions();

        for (int i = 0; i < m_ConstraintIterationCount; i++)
        {
            FixNodeDistances();

            if (i % 2 == 0)
                ApplyCollision();
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

            if (m_simulationIgnoreIndex.Contains(i))
                Gizmos.color = Color.white;
            else
                Gizmos.color = Color.blue;
            Gizmos.DrawLine(m_VerletNodes[i].Position, m_VerletNodes[i].PrevoiusPosition);
        }
    }

    private void CalculateNewPositions()
    {
        for (int i = 0; i < m_VerletNodes.Length; i++)
        {
            var currNode = m_VerletNodes[i];
            var newPreviousPosition = currNode.Position;

            var newPosition = (2 * currNode.Position) - currNode.PrevoiusPosition +
                              ((m_Gravity) * (float)Math.Pow(Time.fixedDeltaTime, 2));

            var ray = new Ray(m_VerletNodes[i].Position, newPosition - m_VerletNodes[i].Position);
            RaycastHit rhit;

            if (Physics.Raycast(ray, out rhit, Vector3.Distance(m_VerletNodes[i].Position, newPosition) * 2))
            {
                Debug.Log($"IT'S IMSIDE!!!!!{i}, {Vector3.Distance(m_VerletNodes[i].Position, newPosition)}");

                Debug.Log($"POS: {m_VerletNodes[i].Position}, ADJ POS: {rhit.point + rhit.normal * m_RopeRadius}");
                m_VerletNodes[i].Position = Vector3.Lerp(m_VerletNodes[i].PrevoiusPosition,
                    rhit.point + rhit.normal * m_RopeRadius, m_LowPassFilterCutoff);
            }

            else
            {
                m_VerletNodes[i].Position = newPosition;
            }

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
        for (int i = 0; i < m_VerletNodes.Length; i++)
        {
            var colliders = Physics.OverlapSphere(m_VerletNodes[i].Position, m_RopeRadius);

            foreach (var col in colliders)
            {
                var closestPoint = col.ClosestPoint(m_VerletNodes[i].Position);
                
                RaycastHit rhit;

                Ray ray = new Ray(m_VerletNodes[i].Position, closestPoint - m_VerletNodes[i].Position);
                Physics.Raycast(ray, out rhit, Vector3.Distance(closestPoint, m_VerletNodes[i].Position));

                var hitNormal = rhit.normal;
                m_VerletNodes[i].Position = closestPoint + hitNormal * m_RopeRadius;
            }
        }
    }
}