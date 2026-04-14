using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Decision", menuName = "Agent/Decisions/IdleToFindFood")]
public class IdleToFindFood : Decision
{
    public override bool Decide(FSM controller)
    {
        bool decision = IdleToFindFoodDecision(controller);
        return decision;
    }
    private bool IdleToFindFoodDecision(FSM controller)
    {
        if (controller.CanFindFood())
            return true;
        return false;
    }
}
