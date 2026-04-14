using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Decision", menuName = "Agent/Decisions/FindMateToIdle")]
public class FindMateToIdle : Decision
{

    public override bool Decide(FSM controller)
    {
        return FindMateToIdleDecision(controller);
    }
    bool FindMateToIdleDecision(FSM controller)
    {
        if(!controller.animal.canFindMate() || !controller.animal.hasPotentialMatesNearby())
        {
            return true;
        }
        return false;
    }
}
