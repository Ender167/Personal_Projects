using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Animal : Entity
{
    [Header("Animal")]
    public int generation = 1;
    public float fitness = 0;
    public FitnessWeights weights;
    public SphereCollider detectionSphere;
    public List<Animal> foundSpeciesMembers = new List<Animal>();
    public List<Entity> foundFood = new List<Entity>();

    [Header("Hunger")]
    public float maxHunger = 100;
    public float currentHunger = 0;
    public float hungerGrowthRate = 1f;
    public float hungerGrowthFrequency;
    public float hungerDangerThreshold = 80;
    public float hungerThresholdForFoodSearch = 50;

    [Header("Diet")]
    public bool isEatingPlants = true;
    public bool isEatingOtherAnimals = false;

    [Header("Reproduction")]
    public bool canReproduce = false;
    public float currentReproduction = 0f;
    public float reproductionThresholdForMateSearch = 50f;
    public float reproductionGrowthRate = 2f;
    public float reproductionGrowthFrequency = 10f;
    public float maxReproduction = 100f;
    public Animal assignedMate;

    [Header("Movement")]
    public float movementSpeed = 3.5f;
    public float maxLookupRange;
    public float wanderInterval;


    [Header("UI")]
    public Slider healthSlider;
    public Slider hungerSlider;
    public Slider reproductionSlider;

    public Entity agentPref;
    IEnumerator increaseHungerCr;
    IEnumerator decreaseHealthPointsCr;

    float cleanupTimer = 0f;

    bool isRunningDecreaseHealthPointsCr = false;
    bool isRunningIncreaseHungerCr = false;
    bool isRunningIncreaseReproductionCr = false;
    private void Start()
    {
        simulationManager = GameObject.FindWithTag("GameController").GetComponent<SimulationManager>();
        simulationManager.AddAgent(this);
        ComputeFitness();
        if (healthSlider != null && hungerSlider != null && reproductionSlider != null)
        {
            healthSlider.maxValue = maxHealthPoints;
            healthSlider.value = maxHealthPoints;

            hungerSlider.maxValue = maxHunger;
            hungerSlider.value = 0;

            reproductionSlider.maxValue = maxReproduction;
            reproductionSlider.value = 0;
        }
        detectionSphere = gameObject.AddComponent(typeof(SphereCollider)) as SphereCollider;
        detectionSphere.isTrigger = true;
        detectionSphere.radius = maxLookupRange;
        increaseHungerCr = IncreaseHunger();
        decreaseHealthPointsCr = DecreaseHealthPoints();
    }
    private void Update()
    {

        if (healthSlider != null && hungerSlider != null && reproductionSlider != null)
        {
            healthSlider.value = currentHealthPoints;
            hungerSlider.value = currentHunger;
            reproductionSlider.value = currentReproduction;
        }
        if (simulationManager.ShowAgentSliders())
        {
            healthSlider.gameObject.SetActive(true);
            hungerSlider.gameObject.SetActive(true);
            reproductionSlider.gameObject.SetActive(true);
        }
        else
        {
            healthSlider.gameObject.SetActive(false);
            hungerSlider.gameObject.SetActive(false);
            reproductionSlider.gameObject.SetActive(false);
        }

        if (currentHunger >= maxHunger)
        {
            currentHunger = maxHunger;
        }

        if(maxHunger <= 0)
        {
            maxHunger = 1;
        }
        if (maxHealthPoints <= 0)
        {
            maxHealthPoints = 1;
        }

        if (currentHealthPoints >= maxHealthPoints)
        {
            currentHealthPoints = maxHealthPoints;
        }
        if(currentReproduction >= maxReproduction)
        {
            currentReproduction = maxReproduction;
        }

        if (currentHunger >= hungerDangerThreshold && !isRunningDecreaseHealthPointsCr)
        {
            StartCoroutine(DecreaseHealthPoints());
        }
        if (currentHunger < maxHunger && !isRunningIncreaseHungerCr)
        {
            StartCoroutine(IncreaseHunger());
        }
        if(currentReproduction < maxReproduction && !isRunningIncreaseReproductionCr)
        {
            StartCoroutine(IncreaseReproduction());
        }

        if(cleanupTimer <= 1f)
        {
            cleanupTimer += Time.deltaTime;
        }
        if(cleanupTimer > 1f)
        {
            RemoveNullElements();
            RemoveUnreachableFood();
            cleanupTimer = 0f;
        }
        if (!canReproduce)
        {
            canReproduce = true;
        }

        if (DeathCondition())
        {
            Die();
        }

    }
    public bool DeathCondition()
    {
        if (currentHealthPoints <= 0)
            return true;

        return false;
    }
    public void SetSliderValues()
    {
        healthSlider.maxValue = maxHealthPoints;
        hungerSlider.maxValue = maxHunger;
        reproductionSlider.maxValue = maxReproduction;
        currentHealthPoints = maxHealthPoints;
        currentHunger = 0;
        currentReproduction = 0;
    }
    public void OnTriggerEnter(Collider col)
    {
        if(col.transform.root != transform)
        {
            Entity colEntity = col.GetComponent<Entity>();
            if (isEatingPlants)
            {
                Plant p = colEntity as Plant;
                if(p != null && !foundFood.Contains(p))
                {
                    foundFood.Add(p);
                }
            }
            if (isEatingOtherAnimals)
            {
                Animal a = colEntity as Animal;
                if(a != null && !foundFood.Contains(a) && a.species != species)
                {
                    foundFood.Add(a);
                }
            }
            Animal animal = colEntity as Animal;
            if (animal != null && animal.species == species && !foundSpeciesMembers.Contains(colEntity))
            {
                foundSpeciesMembers.Add(animal);
            }
        }
    }
    public override void Eat(Entity food)
    {
        if(food != null)
        {
        if (food.gameObject.activeSelf)
        {
            currentHunger = 0;
        }
        
        if (foundFood.Contains(food))
        {
                if (food.GetComponent<Animal>())
                {
                    food.Die();
                }
                food.gameObject.SetActive(false);
                foundFood.Remove(food); 
        }
        }
    }
    public override void Reproduce(Entity mate)
    {
        Animal animalMate = mate as Animal;
        if (animalMate != null && animalMate.assignedMate == this)
        {
            currentReproduction = 0;

            sentMessage = null;

            animalMate.currentReproduction = 0;
            animalMate.assignedMate = null;
            assignedMate = null;

            Animal child = simulationManager.Crossover(this, animalMate, transform.position);
            child = simulationManager.Mutate(child);
            child.ComputeFitness();
            child.SetSliderValues();
            child.agentPref = agentPref;
            child.generation = generation + 1;
        }
        
    }
    public override void Die()
    {
        simulationManager.RemoveAgent(this);
        Destroy(gameObject);
    }
    public override void Mutate()
    {
        simulationManager.Mutate(this);
    }
    public override bool ReceiveMessage(Message msg)
    {
        Entity sender = msg.GetSender();
        MsgType msgType = msg.GetMsgType();

        if(msgType == MsgType.Food_Request)
        {
            Animal animalSender = (Animal)sender;
            if (animalSender.isEatingOtherAnimals)
            {
                return true;
            }
        }
        if(msgType == MsgType.Crossover_Request && assignedMate == null)
        {
            Animal animalSender = (Animal)sender;
            
            float fitnessRatio = animalSender.fitness / (this.fitness + animalSender.fitness);
            float fitnessBonus = 0.3f * fitnessRatio;

            float finalChance = 0.7f + fitnessBonus;
            finalChance = Mathf.Clamp01(finalChance);

            return Random.value < finalChance;
        }
        return false;
    }
    public void ComputeFitness()
    {
        fitness = (float)(maxHunger * weights.maxHungerWeight + hungerDangerThreshold * weights.hungerDangerThresholdWeight + 
            hungerGrowthFrequency * weights.hungerGrowthFrequencyWeight + hungerGrowthRate * weights.hungerGrowthRateWeight +
            hungerThresholdForFoodSearch * weights.hungerThresholdForFoodSearchWeight + maxReproduction * weights.maxReproductionWeight +
            reproductionGrowthFrequency * weights.reproductionGrowthFrequencyWeight + reproductionGrowthRate * weights.reproductionGrowthRateWeight +
            reproductionThresholdForMateSearch * weights.reproductionThresholdForMateSearchWeight + maxHealthPoints * weights.maxHealthPointsWeight +
            wanderInterval * weights.wanderIntervalWeight + maxLookupRange * weights.maxLookupRangeWeight);
    }

    public bool isHungry()
    {
        if (currentHunger >= hungerThresholdForFoodSearch)
            return true;
        return false;
    }
    public bool hasFoodNearby()
    {
        if (foundFood.Count > 0 && isFoodAvailable())
            return true;
        return false;
    }
    public bool hasPotentialMatesNearby()
    {
        if (foundSpeciesMembers.Count > 0 && isAnySpeciesMemberAvailable())
            return true;
        return false;
    }
    public bool isAnySpeciesMemberAvailable()
    {
        foreach(Animal a in foundSpeciesMembers)
        {
            if (a != null && a.canFindMate())
                return true;
        }
        return false;
    }
    public bool isFoodAvailable()
    {
        foreach (Entity a in foundFood)
        {
            if (a != null && a.gameObject.activeInHierarchy)
                return true;
        }
        return false;
    }
    public bool canFindMate()
    {
        if(currentReproduction >= reproductionThresholdForMateSearch)
        {
            return true;
        }
        return false;
    }
    void RemoveNullElements()
    {
        int i = 0;
        while (i < foundSpeciesMembers.Count)
        {
            if (foundSpeciesMembers[i] == null)
                foundSpeciesMembers.RemoveAt(i);
            i++;
        }
    }
    void RemoveUnreachableFood()
    {
        int i = 0;
        while (i < foundFood.Count)
        {
            if (foundFood[i] == null || !foundFood[i].gameObject.activeSelf)
                foundFood.RemoveAt(i);
            i++;
        }
    }
    private void OnDestroy()
    {
        if(assignedMate != null)
        {
            assignedMate.assignedMate = null;
            assignedMate = null;
        }
    }
    IEnumerator DecreaseHealthPoints()
    {
        isRunningDecreaseHealthPointsCr = true;
        while (currentHealthPoints > 0 && currentHunger >= hungerDangerThreshold)
        {
            yield return new WaitForSeconds(hungerGrowthFrequency);
            float dmg = (float) 0.05 * maxHealthPoints;
            currentHealthPoints -= dmg;
        }
        isRunningDecreaseHealthPointsCr = false;
        yield return null;
    }
    IEnumerator IncreaseHunger()
    {
        isRunningIncreaseHungerCr = true;
        while(currentHunger < maxHunger)
        {
            yield return new WaitForSeconds(hungerGrowthFrequency);
            currentHunger += hungerGrowthRate;
        }
        isRunningIncreaseHungerCr = false;
        yield return null;
    }
    IEnumerator IncreaseReproduction()
    {
        isRunningIncreaseReproductionCr = true;
        while (currentReproduction < maxReproduction)
        {
            yield return new WaitForSeconds(reproductionGrowthRate);
            currentReproduction += reproductionGrowthRate;
        }
        isRunningIncreaseReproductionCr = false;
        yield return null;
    }
}
