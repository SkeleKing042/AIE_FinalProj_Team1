using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace ILOVEYOU
{
    namespace Player
    {
        [RequireComponent(typeof(NavMeshAgent))]
        public class PointerArrow : MonoBehaviour
        {
            [SerializeField] private bool m_isDebugging;
            private NavMeshPath m_path;
            private NavMeshAgent m_agent;
            private Transform m_targetPosition;
            [SerializeField, Range(0.0f, 1.0f)] private float m_turnSpeed;

            public void Awake()
            {
                m_path = new();
                m_agent = GetComponent<NavMeshAgent>();
            }
            public void Update()
            {
                if(!m_targetPosition.parent.gameObject.activeSelf)
                {
                    gameObject.SetActive(false);
                    return;
                }

                for (int i = m_path.corners.Length - 1; i >= 1; i--)
                {
                    //get the distance of this corner
                    Vector3 dist = _getCorner(i) - transform.parent.position;
                    RaycastHit hit;
                    bool isBlocked = Physics.Raycast(transform.parent.position, dist.normalized, out hit, dist.magnitude);
                    if (isBlocked && m_isDebugging)
                        Debug.Log($"Ray blocked by {hit.transform.name}.");
                    else
                    {
                        RegeneratePath();
                        break;
                    }
                }

                transform.localPosition = new Vector3(0, 1.5f, 0);
                Quaternion wantedRot = Quaternion.LookRotation((_getCorner(1) - transform.position).normalized);
                transform.rotation = Quaternion.Lerp(transform.rotation, wantedRot, m_turnSpeed);
            }
            private Vector3 _getCorner(int index)
            {
                return new Vector3(m_path.corners[index].x, m_path.corners[index].y + 1, m_path.corners[index].z);
            }
            public void GeneratePath(Transform pos)
            {
                gameObject.SetActive(true);
                m_targetPosition = pos;
                m_agent.CalculatePath(m_targetPosition.position, m_path);
            }
            public void RegeneratePath()
            {
                m_agent.CalculatePath(m_targetPosition.position, m_path);
            }
            /*private void OnEnable()
            {
                RegeneratePath();
            }*/
            private void OnDrawGizmos()
            {
                try
                {

                    Gizmos.color = Color.blue;
                    Gizmos.DrawSphere(transform.parent.position, 0.25f);
                    if (Application.isPlaying)
                    {

                        Gizmos.color = Color.yellow;
                        Gizmos.DrawSphere(_getCorner(m_path.corners.Length - 1), 0.5f);
                        for (int i = m_path.corners.Length - 2; i > 1; i--)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawSphere(_getCorner(i), 0.25f);
                        }
                        Gizmos.color = Color.green;
                        Gizmos.DrawSphere(_getCorner(1), 0.5f);


                    }
                }
                catch
                {

                }
            }

        }
    }
}