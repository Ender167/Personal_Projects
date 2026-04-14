using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class FSM : MonoBehaviour
{
    public State currentState;
    public State remainState;

    [System.NonSerialized] public NavMeshAgent ai;
    [System.NonSerialized] public Animal animal;
    public Transform target;

    public float wanderCounter = 0f;
    public bool canWander = false;
    private void Start()
    {
        ai = GetComponent<NavMeshAgent>();
        animal = GetComponent<Animal>();
        if(ai != null)
        {
            ai.speed = animal.movementSpeed;
        }
        
        
    }
    void Update()
    {
        currentState.UpdateState(this);

        if (ai != null)
        {
            ai.speed = animal.movementSpeed;
        }


        if (wanderCounter < animal.wanderInterval && !canWander)
        {
            wanderCounter += Time.deltaTime;
        }
        if (wanderCounter >= animal.wanderInterval && !canWander)
        {
            canWander = true;
            wanderCounter = 0f;
        }
        

    }
    public void TransitionToState(State nextState)
    {
        if (nextState != remainState)
        {
            currentState = nextState;
        }
    }
    public void SetWander(bool newState)
    {
        //wanderCounter = 0;
        canWander = newState;
    }
    public bool GetWander()
    {
        return canWander;
    }
    public bool CanFindFood()
    {
        if (animal != null && animal.isHungry() && animal.hasFoodNearby())
            return true;
        return false;
    }
    public bool CanFindMate()
    {
        if(animal != null && animal.canFindMate() && animal.hasPotentialMatesNearby() && animal.assignedMate == null)
        {
            return true;
        }
        return false;
    }
    void OnDrawGizmosSelected()
    {
        if(animal != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, animal.maxLookupRange);
        }

    }

}
