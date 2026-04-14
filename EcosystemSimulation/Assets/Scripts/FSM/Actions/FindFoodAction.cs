using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Action", menuName = "Agent/Actions/FindFoodAction")]
public class FindFoodAction : Action
{

    public override void Act(FSM controller)
    {
        FindFood(controller);
    }
    public void FindFood(FSM controller)
    {
        if (controller.ai != null)
        {
            controller.ai.isStopped = false;
            if (controller.animal.foundFood.Count > 0)
            {
                Entity prey = null;
                for (int i = 0; i < controller.animal.foundFood.Count; i++)
                {
                    if (controller.animal.foundFood[i] != null && controller.animal.foundFood[i].gameObject.activeSelf &&
                    controller.animal.foundFood[i].gameObject.transform.position.y < 1)
                    {
                        prey = controller.animal.foundFood[i];
                        break;
                    }
                }
                Message msg = new Message(MsgType.Food_Request, controller.animal);
                if (prey != null && prey.ReceiveMessage(msg))
                {
                    controller.ai.SetDestination(prey.transform.position);
                    if (Vector3.Distance(controller.transform.position, prey.transform.position) < controller.ai.stoppingDistance)
                    {
                        controller.animal.Eat(prey);
                    }
                }

            }
        }

        
    }

}
