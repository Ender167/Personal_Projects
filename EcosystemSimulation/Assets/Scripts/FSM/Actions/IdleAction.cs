using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Agent/Actions/IdleAction")]
public class IdleAction : Action
{
    public override void Act(FSM controller)
    {
        Idle(controller);
    }
    private void Idle(FSM controller)
    {
        if(controller.ai != null)
        {
            controller.ai.isStopped = true;
            controller.ai.ResetPath();
        }
        
    }
}
