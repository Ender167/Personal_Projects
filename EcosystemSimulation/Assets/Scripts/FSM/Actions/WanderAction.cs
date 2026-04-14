using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName ="New Action", menuName ="Agent/Actions/WanderAction")]
public class WanderAction : Action
{
    public override void Act(FSM controller)
    {
        Wander(controller);
    }
    private void Wander(FSM controller)
    {
        if(controller.ai != null)
        {
            if (!controller.ai.hasPath)
            {
                controller.ai.isStopped = false;
                Vector3 wanderPosition = RandomNavmeshLocation(controller, controller.animal.maxLookupRange);
                controller.ai.SetDestination(wanderPosition);
            }
        }
       
        
    }
    public Vector3 RandomNavmeshLocation(FSM controller, float radius)
    {
        Vector3 randomDirection = Random.onUnitSphere * radius;
        randomDirection += controller.transform.position;
        NavMeshHit hit;
        Vector3 finalPosition = Vector3.zero;
        if (NavMesh.SamplePosition(randomDirection, out hit, Mathf.Infinity, NavMesh.AllAreas))
        {
            finalPosition = hit.position;
        }
        return finalPosition;
    }
    public Vector3 RandomLocation(FSM controller, float radius)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += controller.transform.position;
        randomDirection.y = Mathf.Max(randomDirection.y, 1);
        return randomDirection;
    }
}
