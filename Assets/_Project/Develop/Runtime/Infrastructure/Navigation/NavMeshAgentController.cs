using UnityEngine;
using UnityEngine.AI;

namespace Infrastructure.Navigation
{
    public class NavMeshAgentController : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent agent;

        private NavMeshPath path;

        private bool isPathReseted;


        public void Initialize()
        {
            path = new NavMeshPath();
        }


        public void Move(Vector3 direction, in float speed, in float deltaTime)
        {
            if (!IsAgentReady())
            {
                return;
            }

            agent.Move(direction * speed * deltaTime);
        }


        public void SetAvoidancePriority(int value)
        {
            agent.avoidancePriority = value;
        }


        public void SetDestination(in Vector3 destinationPoint, in float speed, bool ignorePartialPath = false)
        {
            if (!IsAgentReady())
            {
                return;
            }

            agent.speed = speed;

            if (agent.destination == destinationPoint)
            {
                return;
            }

            if (agent.pathStatus != NavMeshPathStatus.PathPartial
                || ignorePartialPath
                || agent.remainingDistance == 0)
            {
                agent.SetDestination(destinationPoint);
            }

            isPathReseted = false;

        }


        public bool HasPath(in Vector3 from, in Vector3 to)
        {
            return CalculatePath(in from, in to) == NavMeshPathStatus.PathComplete;
        }


        public bool Raycast(in Vector3 from, in Vector3 to)
        {
            return NavMesh.Raycast(from, to, out _, NavMesh.AllAreas);
        }


        public NavMeshPathStatus CalculatePath(in Vector3 from, in Vector3 to)
        {
            if (!IsAgentReady())
            {
                return NavMeshPathStatus.PathInvalid;
            }

            NavMesh.CalculatePath(from, to, NavMesh.AllAreas, path);
            return path.status;
        }


        public void ResetPath()
        {
            if (isPathReseted)
            {
                return;
            }

            if (!IsAgentReady())
            {
                return;
            }

            agent.ResetPath();
            isPathReseted = true;
        }


        public virtual void Warp(Vector3 point)
        {
            if (!agent.enabled)
            {
                return;
            }

            agent.Warp(point);
        }


        public bool HasPath()
        {
            if (!IsAgentReady())
            {
                return false;
            }

            return agent.hasPath || agent.pathPending;
        }


        public Vector3 GetDestinationPoint()
        {
            return !IsAgentReady() ? transform.position : agent.destination;

        }


        private bool IsAgentReady()
        {
            return agent.enabled && agent.isOnNavMesh;
        }
    }
}