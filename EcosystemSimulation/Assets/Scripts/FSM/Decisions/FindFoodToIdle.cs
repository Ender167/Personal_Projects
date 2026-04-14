using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Decision", menuName = "Agent/Decisions/FindFoodToIdle")]
public class FindFoodToIdle : Decision
{
    public override bool Decide(FSM controller)
    {
        bool decision = FindFoodToIdleDecision(controller);
        return decision;
    }
    private bool FindFoodToIdleDecision(FSM controller)
    {
        if (!controller.CanFindFood() || !controller.animal.isHungry())
        {
            Debug.Log("Done eating!");
            return true;
        }
            
        return false;
    }
}
