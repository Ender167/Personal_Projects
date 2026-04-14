using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New State", menuName = "Agent/State")]
public class State : ScriptableObject
{
    public Action[] actions;
    public Transition[] transitions;

    public Color sceneGizmoColor = Color.gray;

    public bool passive = false;


    public void UpdateState(FSM controller)
    {
        DoActions(controller);
        CheckTransitions(controller);
    }
    private void DoActions(FSM controller)
    {
        for (int i = 0; i < actions.Length; i++)
        {
            actions[i].Act(controller);
        }
    }
    
    private void CheckTransitions(FSM controller)
    {
        for (int i = 0; i < transitions.Length; i++)
        {
            bool decisionSucceded = transitions[i].decision.Decide(controller);
            if (decisionSucceded)
            {
                controller.TransitionToState(transitions[i].trueState);
            }
            else
            {
                controller.TransitionToState(transitions[i].falseState);
            }
        }
        
    }

}
