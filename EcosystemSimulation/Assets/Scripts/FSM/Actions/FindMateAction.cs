using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Action", menuName = "Agent/Actions/FindMateAction")]
public class FindMateAction : Action
{

    public override void Act(FSM controller)
    {
        FindMate(controller);
    }
    void FindMate(FSM controller)
    {
        if (controller.ai != null)
        {
            if (controller.ai.hasPath && controller.animal.assignedMate == null)
            {
                controller.ai.ResetPath();
            }
            controller.ai.isStopped = false;
            Animal mate = controller.animal.assignedMate;
            if (mate == null && controller.animal.foundSpeciesMembers.Count > 0)
            {
                mate = SelectMate(controller);
            }
            if (mate != null)
            {
                if (controller.animal.sentMessage == null)
                {
                    controller.animal.sentMessage = new Message(MsgType.Crossover_Request, controller.animal);
                    controller.animal.receivedResponse = mate.ReceiveMessage(controller.animal.sentMessage);
                }

                if (controller.animal.receivedResponse)
                {
                    mate.assignedMate = controller.animal;
                    controller.animal.assignedMate = mate;

                    controller.ai.SetDestination(mate.transform.position);
                    if (Vector3.Distance(controller.transform.position, mate.transform.position) < controller.ai.stoppingDistance)
                    {
                        controller.animal.Reproduce(mate);
                    }
                }
                else
                {
                    controller.animal.sentMessage = null;
                    controller.animal.currentReproduction = 0;
                    Debug.Log("Fitness too low");
                }
            }
            else
            {
                controller.animal.sentMessage = null;
                Debug.Log("No mates available");
            }
        }
    }
    List<Animal> GenerateListOfCandidates(FSM controller)
    {
        List<Animal> result = new List<Animal>();
        foreach (Animal a in controller.animal.foundSpeciesMembers)
        {
            if (a != null)
            {
                if (a.assignedMate != null && a.assignedMate == controller.animal)
                {
                    result.Clear();
                    result.Add(a);
                    break;
                }
                if (a.assignedMate == null && a.canFindMate())
                {
                    result.Add(a);
                }
            }

        }
        return result;
    }
    Animal RouletteWheelSelection(List<Animal> candidates)
    {
        Animal finalMate = null;
        List<float> probabilities = new List<float>();
        float totalFitness = candidates.Sum(a => a.fitness);
        float previousProbability = 0;
        for(int i = 0; i < candidates.Count; i++)
        {
            probabilities.Add(previousProbability + candidates[i].fitness / totalFitness);
            previousProbability = probabilities[i];
        }
        float rand = Random.Range(0, 1);
        int selectedPos = 0;
        for(int i =0; i < probabilities.Count; i++)
        {
            if(rand < probabilities[i])
            {
                selectedPos = i;
                break;
            }
        }
        finalMate = candidates[selectedPos];
        return finalMate;
    }
    Animal FittestSelection(List<Animal> candidates)
    {
        Animal finalMate = null;
        finalMate = candidates.OrderByDescending(n => n.fitness).FirstOrDefault();
        return finalMate;
    }
    Animal SelectMate(FSM controller)
    {
        Animal finalMate = null;
        List<Animal> candidates = GenerateListOfCandidates(controller);

        if(candidates.Count > 0)
        {
            finalMate = RouletteWheelSelection(candidates);
            if (finalMate == null)
            {
                Debug.Log("error from roulette " + controller.name);
                Debug.Break();
            }
        }
        else
        {
            Debug.Log("no candidates " + controller.name);
        }
        
        return finalMate;
    }
}
