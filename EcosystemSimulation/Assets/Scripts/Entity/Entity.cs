using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{ 
    public string species;
    public float currentHealthPoints;
    public float maxHealthPoints;

    public Message sentMessage;
    public bool receivedResponse;

    [System.NonSerialized] public SimulationManager simulationManager;
    public abstract void Die();
    public abstract void Eat(Entity food);
    public abstract void Mutate();
    public abstract void Reproduce(Entity mate);

    public abstract bool ReceiveMessage(Message msg);
}
