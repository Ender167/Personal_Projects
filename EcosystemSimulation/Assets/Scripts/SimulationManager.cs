
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SimulationManager : MonoBehaviour
{
    public SimulationGraphicalInterface simulationUI;
    public MeshFilter groundMesh;

    public bool canSpawnAgents = true;
    public List<SimulationAgent> agentPrefs = new List<SimulationAgent>();
    public List<Entity> agents = new List<Entity>();
    public List<Entity> vegetables = new List<Entity>();
    public int nrOfFoodToRespawn = 10;
    
    public Entity food;
    public float respawnFoodRateInSeconds = 600;
    public float currentRespawnFoodRate = 0;

    string currentDayState;
    float respawnFoodCounter = 0;

    void Awake()
    {
        currentRespawnFoodRate = respawnFoodRateInSeconds;
        StartSimulation();
    }

    public void StartSimulation()
    {
        CleanupAll();
        if (canSpawnAgents)
            CreateAgents();
    }
    private void Update()
    {
        Time.timeScale = simulationUI.simulationSpeedSlider.value;
        if (respawnFoodCounter < currentRespawnFoodRate)
        {
            respawnFoodCounter += Time.unscaledDeltaTime * Time.timeScale;
        }
        if(respawnFoodCounter >= currentRespawnFoodRate)
        {
            respawnFoodCounter = 0;
            SpawnFood(nrOfFoodToRespawn);
        }
    }

    void CreateAgents()
    {
        for(int i = 0; i < agentPrefs.Count; i++)
        {
            CreateAgentInArea(agentPrefs[i]);
        }
    }
    void CreateAgentInArea(SimulationAgent simAgent)
    {
        List<Vector3> spawnPoints = simAgent.spawnOnWholeMap == true ? GenerateRandomPointsOnMesh(simAgent.nrOfAgents) : GenerateRandomPointsInsideCircle(simAgent.spawnCenter, simAgent.radius, simAgent.nrOfAgents);
        Entity agent = simAgent.agentPref;
        bool isAnimal = simAgent.agentPref.GetComponent<Animal>() ? true : false;
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (isAnimal)
            {
                Animal a = Instantiate(agent, spawnPoints[i], Quaternion.identity) as Animal;
                a.agentPref = agent;
                RandomizeInitialStats(a, simAgent.initialStats);
                a.SetSliderValues();
                a.ComputeFitness();
            }
            else
            {
                Instantiate(agent, spawnPoints[i], Quaternion.identity);
            }
        }
    }
    void SpawnFood(int nrFood)
    {   
        List<Vector3> spawnPoints = GenerateRandomPointsOnMesh(nrFood);
        for(int i = 0; i < spawnPoints.Count; i++)
        {
            Instantiate(food, spawnPoints[i], Quaternion.identity);
        }
    }
    List<Vector3> GenerateRandomPointsOnMesh(int nrOfSpawnPoints)
    {
        List<Vector3> result = new List<Vector3>();
        Vector3 minPoint = groundMesh.GetComponent<MeshCollider>().bounds.min;
        Vector3 maxPoint = groundMesh.GetComponent<MeshCollider>().bounds.max;

        int nrOfPoints = nrOfSpawnPoints;

        while(result.Count < nrOfPoints)
        {
            Vector3 randomPoint = new Vector3(Random.Range(minPoint.x, maxPoint.x), 0, Random.Range(minPoint.z, maxPoint.z)) + 
                new Vector3(0, 0.1f, 0);
            if (!result.Contains(randomPoint))
            {
                result.Add(randomPoint);
            }
        }
        return result;

    }
    List<Vector3> GenerateRandomPointsInsideCircle(Vector3 center, float maxRadius, int nrOfSpawnPoints)
    {
        List<Vector3> result = new List<Vector3>();
        
        while (result.Count < nrOfSpawnPoints)
        {
            Vector3 pos = Vector3.zero ;
            float angle = Random.value * 360;
            pos.x = center.x + Random.Range(0, maxRadius) * Mathf.Sin(angle * Mathf.Deg2Rad);
            pos.y = center.y + 0.1f;
            pos.z = center.z + Random.Range(0, maxRadius) * Mathf.Cos(angle * Mathf.Deg2Rad);

            bool found = false;
            for(int i = 0; i < result.Count; i++)
            {
                if (Mathf.Approximately(result[i].x,pos.x) && Mathf.Approximately(result[i].y, pos.y) && Mathf.Approximately(result[i].z, pos.z))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                result.Add(pos);
            }
        }
        return result;
    }
    public void AddAgent(Entity agent)
    {
        if (!IsInAgentList(agent))
        {
            if(agent.GetComponent<Animal>())
                agents.Add(agent);
            if (agent.GetComponent<Plant>())
                vegetables.Add(agent);
        }
        
    }
    public void RemoveAgent(Entity agent)
    {
        if(agent.GetComponent<Animal>())
            agents.Remove(agent);
        if (agent.GetComponent<Plant>())
            vegetables.Remove(agent);
    }

    public Animal Crossover(Animal a, Animal b, Vector3 spawnPos)
    {
        Animal result = Instantiate(a.agentPref, spawnPos, Quaternion.identity) as Animal;

        int n = 2;
        int rand = Random.Range(0, n);
        result.maxHunger = rand == 0 ? a.maxHunger : b.maxHunger;
        rand = Random.Range(0, n);
        result.hungerGrowthRate = rand == 0 ? a.hungerGrowthRate : b.hungerGrowthRate;
        rand = Random.Range(0, n);
        result.hungerGrowthFrequency = rand == 0 ? a.hungerGrowthFrequency : b.hungerGrowthFrequency;
        rand = Random.Range(0, n);
        result.hungerDangerThreshold = rand == 0 ? a.hungerDangerThreshold : b.hungerDangerThreshold;
        rand = Random.Range(0, n);
        result.hungerThresholdForFoodSearch = rand == 0 ? a.hungerThresholdForFoodSearch : b.hungerThresholdForFoodSearch;
        rand = Random.Range(0, n);
        result.maxReproduction = rand == 0 ? a.maxReproduction : b.maxReproduction;
        rand = Random.Range(0, n);
        result.reproductionThresholdForMateSearch = rand == 0 ? a.reproductionThresholdForMateSearch : b.reproductionThresholdForMateSearch;
        rand = Random.Range(0, n);
        result.reproductionGrowthRate = rand == 0 ? a.reproductionGrowthRate : b.reproductionGrowthRate;
        rand = Random.Range(0, n);
        result.reproductionGrowthFrequency = rand == 0 ? a.reproductionGrowthFrequency : b.reproductionGrowthFrequency;
        rand = Random.Range(0, n);
        result.movementSpeed = rand == 0 ? a.movementSpeed : b.movementSpeed;
        rand = Random.Range(0, n);
        result.maxLookupRange = rand == 0 ? a.maxLookupRange : b.maxLookupRange;
        rand = Random.Range(0, n);
        result.wanderInterval = rand == 0 ? a.wanderInterval : b.wanderInterval;
        rand = Random.Range(0, n);
        result.maxHealthPoints = rand == 0 ? a.maxHealthPoints : b.maxHealthPoints;


        result.reproductionThresholdForMateSearch = result.reproductionThresholdForMateSearch > result.maxReproduction || result.reproductionThresholdForMateSearch < 0 ? result.maxReproduction / 2 : result.reproductionThresholdForMateSearch;
        result.hungerDangerThreshold = result.hungerDangerThreshold > result.maxHunger || result.hungerDangerThreshold < 0 ? result.maxHunger / 2 : result.hungerDangerThreshold;
        result.hungerThresholdForFoodSearch = result.hungerThresholdForFoodSearch > result.maxHunger || result.hungerThresholdForFoodSearch < 0 ? result.maxHunger / 2 : result.hungerThresholdForFoodSearch;

        return result;
    }

    public Animal Mutate(Animal a)
    {
        Animal result = a;
        int n = 2;
        float mutValPercentage = 0.25f;
        int rand = Random.Range(0, n);
        result.maxHunger = rand == 0 ? a.maxHunger - mutValPercentage * a.maxHunger : a.maxHunger + mutValPercentage * a.maxHunger;

        rand = Random.Range(0, n);
        result.hungerGrowthRate = rand == 0 ? a.hungerGrowthRate - mutValPercentage * a.hungerGrowthRate : a.hungerGrowthRate + mutValPercentage * a.hungerGrowthRate;
        result.hungerGrowthRate = result.hungerGrowthRate <= 0 ? a.hungerGrowthRate : result.hungerGrowthRate;

        rand = Random.Range(0, n);
        result.hungerGrowthFrequency = rand == 0 ? a.hungerGrowthFrequency - mutValPercentage * a.hungerGrowthFrequency : a.hungerGrowthFrequency + mutValPercentage * a.hungerGrowthFrequency;
        result.hungerGrowthFrequency = result.hungerGrowthFrequency <= 0 ? a.hungerGrowthFrequency : result.hungerGrowthFrequency;

        rand = Random.Range(0, n);
        result.hungerDangerThreshold = rand == 0 ? a.hungerDangerThreshold - mutValPercentage * a.hungerDangerThreshold : a.hungerDangerThreshold + mutValPercentage * a.hungerDangerThreshold;
        result.hungerDangerThreshold = result.hungerDangerThreshold <= 0 ? a.hungerDangerThreshold : result.hungerDangerThreshold;

        rand = Random.Range(0, n);
        result.hungerThresholdForFoodSearch = rand == 0 ? a.hungerThresholdForFoodSearch - mutValPercentage * a.hungerThresholdForFoodSearch : a.hungerThresholdForFoodSearch + mutValPercentage * a.hungerThresholdForFoodSearch;
        result.hungerThresholdForFoodSearch = result.hungerThresholdForFoodSearch <= 0 ? a.hungerThresholdForFoodSearch : result.hungerThresholdForFoodSearch;

        rand = Random.Range(0, n);
        result.maxReproduction = rand == 0 ? a.maxReproduction - mutValPercentage * a.maxReproduction : a.maxReproduction + mutValPercentage * a.maxReproduction;

        rand = Random.Range(0, n);
        result.reproductionThresholdForMateSearch = rand == 0 ? a.reproductionThresholdForMateSearch - mutValPercentage * a.reproductionThresholdForMateSearch : a.reproductionThresholdForMateSearch + mutValPercentage * a.reproductionThresholdForMateSearch;
        result.reproductionThresholdForMateSearch = result.reproductionThresholdForMateSearch <= 0 ? a.reproductionThresholdForMateSearch : result.reproductionThresholdForMateSearch;

        rand = Random.Range(0, n);
        result.reproductionGrowthRate = rand == 0 ? a.reproductionGrowthRate - mutValPercentage * a.reproductionGrowthRate: a.reproductionGrowthRate + mutValPercentage * a.reproductionGrowthRate;
        result.reproductionGrowthRate = result.reproductionGrowthRate <= 0 ? a.reproductionGrowthRate : result.reproductionGrowthRate;

        rand = Random.Range(0, n);
        result.reproductionGrowthFrequency = rand == 0 ? a.reproductionGrowthFrequency - mutValPercentage * a.reproductionGrowthFrequency : a.reproductionGrowthFrequency + mutValPercentage * a.reproductionGrowthFrequency;
        result.reproductionGrowthFrequency = result.reproductionGrowthFrequency <= 0 ? a.reproductionGrowthFrequency : result.reproductionGrowthFrequency;


        rand = Random.Range(0, n);
        result.movementSpeed = rand == 0 ? a.movementSpeed - mutValPercentage * a.movementSpeed : a.movementSpeed + mutValPercentage * a.movementSpeed;
        result.movementSpeed = result.movementSpeed <= 0 ? a.movementSpeed : result.movementSpeed;

        rand = Random.Range(0, n);
        result.maxLookupRange = rand == 0 ? a.maxLookupRange - mutValPercentage * a.maxLookupRange : a.maxLookupRange + mutValPercentage * a.maxLookupRange;
        result.maxLookupRange = result.maxLookupRange <= 0 ? a.maxLookupRange : result.maxLookupRange;

        rand = Random.Range(0, n);
        result.wanderInterval = rand == 0 ? a.wanderInterval - mutValPercentage * a.wanderInterval : a.wanderInterval + mutValPercentage * a.wanderInterval;
        result.wanderInterval = result.wanderInterval <= 0 ? a.wanderInterval : result.wanderInterval;

        rand = Random.Range(0, n);
        result.maxHealthPoints = rand == 0 ? a.maxHealthPoints - mutValPercentage * a.maxHealthPoints : a.maxHealthPoints + mutValPercentage * a.maxHealthPoints;


        result.reproductionThresholdForMateSearch = result.reproductionThresholdForMateSearch > result.maxReproduction || result.reproductionThresholdForMateSearch < 0 ? result.maxReproduction / 2 : result.reproductionThresholdForMateSearch;
        result.hungerDangerThreshold = result.hungerDangerThreshold > result.maxHunger || result.hungerDangerThreshold < 0 ? result.maxHunger / 2 : result.hungerDangerThreshold;
        result.hungerThresholdForFoodSearch = result.hungerThresholdForFoodSearch > result.maxHunger || result.hungerThresholdForFoodSearch < 0 ? result.maxHunger / 2 : result.hungerThresholdForFoodSearch;

        return result;
    }

    public void RandomizeInitialStats(Animal agent, RandomStats randomStats)
    {
        agent.maxHunger = Random.Range(randomStats.minHunger, randomStats.maxHunger);
        agent.hungerGrowthRate = agent.maxHunger / 6;
        agent.hungerGrowthFrequency = Random.Range(randomStats.minHungerGrowthFrequency, randomStats.maxHungerGrowthFrequency);
        agent.hungerDangerThreshold = agent.maxHunger - Random.Range(0, agent.maxHunger / 2);
        agent.hungerThresholdForFoodSearch = agent.hungerDangerThreshold;

        agent.maxReproduction = Random.Range(randomStats.minReproduction, randomStats.maxReproduction);
        agent.reproductionGrowthRate = agent.maxReproduction / 5;
        agent.reproductionGrowthFrequency = Random.Range(randomStats.minReproductionGrowthFrequency, randomStats.maxReproductionGrowthFrequency);
        agent.reproductionThresholdForMateSearch = agent.maxReproduction / 2;

        agent.maxHealthPoints = Random.Range(randomStats.minHealth, randomStats.maxHealth);
        agent.maxLookupRange = Random.Range(randomStats.minLookup, randomStats.maxLookup);
        agent.wanderInterval = Random.Range(randomStats.minWander, randomStats.maxWander);

    }

    void CleanupAll()
    {
        CleanupRemainingVegetables();
        CleanupRemainingAgents();
    }
    void CleanupRemainingVegetables()
    {
        while (vegetables.Count > 0)
        {
            vegetables[0].Die();
        }
    }
    void CleanupRemainingAgents()
    {
        while(agents.Count > 0)
        {
            agents[0].Die();
        }
    }
    public bool ShowAgentSliders()
    {
        return simulationUI.agentsSlidersToggle.isOn;
    }
    public bool IsInAgentList(Entity agent)
    {
        return agents.Contains(agent);
    }
}
[System.Serializable]
public class SimulationAgent
{
    public Entity agentPref;
    public bool spawnOnWholeMap = false;
    public Vector3 spawnCenter;
    public float radius;
    public int nrOfAgents;
    public RandomStats initialStats;
}
[System.Serializable]
public class RandomStats
{
    public float minHealth = 10;
    public float maxHealth = 100;
    public float minHunger = 10;
    public float maxHunger = 100;
    public float minReproduction = 10;
    public float maxReproduction = 100;
    public float minHungerGrowthFrequency = 900;
    public float maxHungerGrowthFrequency = 1800;
    public float minReproductionGrowthFrequency = 900;
    public float maxReproductionGrowthFrequency = 1800;
    public float minWander = 2;
    public float maxWander = 3;
    public float minLookup = 5;
    public float maxLookup = 10;
}