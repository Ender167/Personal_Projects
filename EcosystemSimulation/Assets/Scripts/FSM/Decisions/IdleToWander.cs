using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Decision", menuName = "Agent/Decisions/IdleToWander")]
public class IdleToWander : Decision
{
    public override bool Decide(FSM controller)
    {
        bool decision = IdleToWanderDecision(controller);
        return decision;
    }
    private bool IdleToWanderDecision(FSM controller)
    {
        if(controller.GetWander())
            return true;
        return false;
    }
}
