using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plant : Entity
{
    private void Start()
    {
        simulationManager = GameObject.FindWithTag("GameController").GetComponent<SimulationManager>();
        simulationManager.AddAgent(this);
    }
    public override void Eat(Entity food)
    {

    }
    public override void Mutate()
    {

    }
    public override void Reproduce(Entity mate)
    {
        
    }
    public override void Die()
    {
        simulationManager.RemoveAgent(this);
        Destroy(gameObject);
    }
    public override bool ReceiveMessage(Message msg)
    {
        return true;
    }
    private void OnDisable()
    {
        simulationManager.RemoveAgent(this);
    }
}
