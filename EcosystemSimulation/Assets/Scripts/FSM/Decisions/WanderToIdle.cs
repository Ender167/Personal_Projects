using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Decision", menuName = "Agent/Decisions/WanderToIdle")]
public class WanderToIdle : Decision
{

    public override bool Decide(FSM controller)
    {
        bool decision = WanderToIdleDecision(controller);
        return decision;
    }
    private bool WanderToIdleDecision(FSM controller)
    {
        if(controller.ai != null && !controller.ai.pathPending && controller.ai.remainingDistance <= controller.ai.stoppingDistance)
        {
            controller.SetWander(false);
            return true;
        }

        return false;
    }
}
