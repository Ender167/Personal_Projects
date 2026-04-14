using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Decision", menuName = "Agent/Decisions/IdleToFindMate")]
public class IdleToFindMate : Decision
{

    public override bool Decide(FSM controller)
    {
        return IdleToFindMateDecision(controller);
    }
    bool IdleToFindMateDecision(FSM controller)
    {
        if (controller.CanFindMate())
            return true;
        return false;
    }
}
