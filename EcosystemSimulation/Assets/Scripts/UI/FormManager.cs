using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FormManager : MonoBehaviour
{
    public int agentTemplateId = -1;
    public SimulationManager simManager;

    public GameObject templateEntry;
    public RectTransform templateEntrySpawnPoint;

    [Header("Agent UI")]
    public GameObject speciesParent;
    public Text speciesText;
    public InputField speciesInput;

    public GameObject numberOfAgentsParent;
    public Text numberOfAgentsText;
    public InputField numberOfAgentsInput;

    public GameObject healthParent;
    public Text healthText;
    public InputField minHealthInput;
    public InputField maxHealthInput;

    public GameObject hungerParent;
    public Text hungerText;
    public InputField minHungerInput;
    public InputField maxHungerInput;

    public GameObject reproductionParent;
    public Text reproductionText;
    public InputField minReproductionInput;
    public InputField maxReproductionInput;

    public GameObject hungerGrowthParent;
    public Text hungerGrowthFrequencyText;
    public InputField minHungerGrowthFrequencyInput;
    public InputField maxHungerGrowthFrequencyInput;

    public GameObject reproductionGrowthParent;
    public Text reproductionGrowthFrequencyText;
    public InputField minReproductionGrowthFrequencyInput;
    public InputField maxReproductionGrowthFrequencyInput;

    public GameObject wanderParent;
    public Text wanderText;
    public InputField minWanderInput;
    public InputField maxWanderInput;

    public GameObject lookupParent;
    public Text lookupText;
    public InputField minLookupInput;
    public InputField maxLookupInput;

    public GameObject movementParent;
    public Text movementText;
    public InputField movementInput;

    public Toggle spawnOnWholeMap;
    public Dropdown typeDropdown;
    public Dropdown foodDropdown;

    [Header("Agent UI SpawnLocation")]

    public GameObject locationParent;
    public InputField locationXInput;
    public InputField locationYInput;
    public InputField locationZInput;

    public GameObject locationRadiusParent;
    public InputField locationRadiusInput;

    GameObject previousPanel;

    void Start()
    {

    }
    public void CreateTemplateUi()
    {
        for(int i = 0; i < simManager.agentPrefs.Count; i++)
        {
            SimulationAgent simAgent = simManager.agentPrefs[i];
            GameObject clone = Instantiate(templateEntry, Vector3.zero, Quaternion.identity);
            clone.transform.SetParent(templateEntrySpawnPoint);
            clone.transform.localScale = Vector3.one;
            clone.GetComponent<TemplateEntry>().Init(simManager.simulationUI, simAgent.agentPref.species, i);
            simManager.simulationUI.EnablePanel(previousPanel);
        }
    }
    public void OpenCreateForm(SimulationManager manager, GameObject prevPanel)
    {
        simManager = manager;
        previousPanel = prevPanel;
    }
    public void CreateAgentTemplate()
    {      
        SimulationAgent simAgent = new SimulationAgent();
        simAgent.initialStats = new RandomStats();

        simAgent.spawnOnWholeMap = spawnOnWholeMap.isOn;

        int.TryParse(numberOfAgentsInput.text, out simAgent.nrOfAgents);

        float.TryParse(minHealthInput.text, out simAgent.initialStats.minHealth);
        float.TryParse(maxHealthInput.text, out simAgent.initialStats.maxHealth);

        if(typeDropdown.value != 2)
        {
            float.TryParse(minHungerInput.text, out simAgent.initialStats.minHunger);
            float.TryParse(maxHungerInput.text, out simAgent.initialStats.maxHunger);

            float.TryParse(minReproductionInput.text, out simAgent.initialStats.minReproduction);
            float.TryParse(maxReproductionInput.text, out simAgent.initialStats.maxReproduction);

            float.TryParse(minHungerGrowthFrequencyInput.text, out simAgent.initialStats.minHungerGrowthFrequency);
            float.TryParse(maxHungerGrowthFrequencyInput.text, out simAgent.initialStats.maxHungerGrowthFrequency);

            float.TryParse(minReproductionGrowthFrequencyInput.text, out simAgent.initialStats.minReproductionGrowthFrequency);
            float.TryParse(maxReproductionGrowthFrequencyInput.text, out simAgent.initialStats.maxReproductionGrowthFrequency);

            float.TryParse(minWanderInput.text, out simAgent.initialStats.minWander);
            float.TryParse(maxWanderInput.text, out simAgent.initialStats.maxWander);

            float.TryParse(minLookupInput.text, out simAgent.initialStats.minLookup);
            float.TryParse(maxLookupInput.text, out simAgent.initialStats.maxLookup);
        }
        

        if(typeDropdown.value == 0)
        {
            if(foodDropdown.value == 0)
            {
                simAgent.agentPref = simManager.agentPrefs[1].agentPref;
            }
            if (foodDropdown.value == 1)
            {
                simAgent.agentPref = simManager.agentPrefs[2].agentPref;
            }
        }
        if (typeDropdown.value == 1)
        {
            if (foodDropdown.value == 0)
            {
                simAgent.agentPref = simManager.agentPrefs[3].agentPref;
            }
            if (foodDropdown.value == 1)
            {
                simAgent.agentPref = simManager.agentPrefs[3].agentPref;
            }
        }
        if(typeDropdown.value == 2)
        {
            simAgent.agentPref = simManager.agentPrefs[0].agentPref;
        }

        if (spawnOnWholeMap.isOn)
        {
            simAgent.spawnOnWholeMap = true;
        }
        else
        {
            simAgent.spawnOnWholeMap = false;
            float.TryParse(locationXInput.text, out simAgent.spawnCenter.x);
            float.TryParse(locationYInput.text, out simAgent.spawnCenter.y);
            float.TryParse(locationZInput.text, out simAgent.spawnCenter.z);
            float.TryParse(locationRadiusInput.text, out simAgent.radius);
        }
        simManager.agentPrefs.Add(simAgent);
        GameObject clone = Instantiate(templateEntry, Vector3.zero, Quaternion.identity);
        clone.transform.SetParent(templateEntrySpawnPoint);
        clone.transform.localScale = Vector3.one;
        clone.GetComponent<TemplateEntry>().Init(simManager.simulationUI, speciesInput.text, simManager.agentPrefs.Count - 1);
        simManager.simulationUI.EnablePanel(previousPanel);
    }
    public void FillFormWithSelectedAgent(SimulationManager manager, Entity entity)
    {
        simManager = manager;

        Animal entityAnimal = entity as Animal;
        Plant entityPlant = entity as Plant;

        //healthText.text = "Max health";
        minHealthInput.text = entity.currentHealthPoints.ToString();
        if(entityAnimal != null)
        {
            DisplayUiElements(true);
            minHungerInput.text = entityAnimal.currentHunger.ToString();
            minReproductionInput.text = entityAnimal.currentReproduction.ToString();
            minHungerGrowthFrequencyInput.text = entityAnimal.hungerGrowthFrequency.ToString();
            minReproductionGrowthFrequencyInput.text = entityAnimal.reproductionGrowthFrequency.ToString();
            minWanderInput.text = entityAnimal.wanderInterval.ToString();
            minLookupInput.text = entityAnimal.maxLookupRange.ToString();
            //ageInput.text = entityAnimal.currentAge.ToString();
            movementInput.text = entityAnimal.movementSpeed.ToString();
        }
        if(entityPlant != null)
        {
            DisplayUiElements(false);
        }

        speciesText.text = entity.species;
    }
    public void FillFormWeather(SimulationManager manager, int id)
    {
        simManager = manager;
        
    }
    public void FillForm(SimulationManager manager, int id)
    {
        agentTemplateId = id;
        simManager = manager;

        SimulationAgent simAgent = manager.agentPrefs[id];
        Entity entity = simAgent.agentPref;

        Animal entityAnimal = entity as Animal;
        Plant entityPlant = entity as Plant;

        healthText.text = "Max health range";
        minHealthInput.text = simAgent.initialStats.minHealth.ToString();
        maxHealthInput.text = simAgent.initialStats.maxHealth.ToString();

        speciesText.text = simAgent.agentPref.species;

        numberOfAgentsText.text = "Number of agents";
        numberOfAgentsInput.text = simAgent.nrOfAgents.ToString();

        spawnOnWholeMap.isOn = simAgent.spawnOnWholeMap;

        if (entityAnimal != null)
        {
            DisplayUiElements(true);
            hungerText.text = "Max hunger range";
            minHungerInput.text = simAgent.initialStats.minHunger.ToString();
            maxHungerInput.text = simAgent.initialStats.maxHunger.ToString();

            reproductionText.text = "Max reproduction range";
            minReproductionInput.text = simAgent.initialStats.minReproduction.ToString();
            maxReproductionInput.text = simAgent.initialStats.maxReproduction.ToString();

            hungerGrowthFrequencyText.text = "Hunger growth frequency range";
            minHungerGrowthFrequencyInput.text = simAgent.initialStats.minHungerGrowthFrequency.ToString();
            maxHungerGrowthFrequencyInput.text = simAgent.initialStats.maxHungerGrowthFrequency.ToString();

            reproductionGrowthFrequencyText.text = "Reproduction growth frequency range";
            minReproductionGrowthFrequencyInput.text = simAgent.initialStats.minReproductionGrowthFrequency.ToString();
            maxReproductionGrowthFrequencyInput.text = simAgent.initialStats.maxReproductionGrowthFrequency.ToString();

            wanderText.text = "Wander interval";
            minWanderInput.text = simAgent.initialStats.minWander.ToString();
            maxWanderInput.text = simAgent.initialStats.maxWander.ToString();

            lookupText.text = "Max lookup range";
            minLookupInput.text = simAgent.initialStats.minLookup.ToString();
            maxLookupInput.text = simAgent.initialStats.maxLookup.ToString();
        }
        if(entityPlant != null)
        {
            DisplayUiElements(false);
        }
    }
    public void UpdateAgentTemplate()
    {
        SimulationAgent simAgent = simManager.agentPrefs[agentTemplateId];

        simAgent.spawnOnWholeMap = spawnOnWholeMap.isOn;

        int.TryParse(numberOfAgentsInput.text, out simAgent.nrOfAgents);

        float.TryParse(minHealthInput.text, out simAgent.initialStats.minHealth);
        float.TryParse(maxHealthInput.text, out simAgent.initialStats.maxHealth);

        float.TryParse(minHungerInput.text, out simAgent.initialStats.minHunger);
        float.TryParse(maxHungerInput.text, out simAgent.initialStats.maxHunger);

        float.TryParse(minReproductionInput.text, out simAgent.initialStats.minReproduction);
        float.TryParse(maxReproductionInput.text, out simAgent.initialStats.maxReproduction);

        float.TryParse(minHungerGrowthFrequencyInput.text, out simAgent.initialStats.minHungerGrowthFrequency);
        float.TryParse(maxHungerGrowthFrequencyInput.text, out simAgent.initialStats.maxHungerGrowthFrequency);

        float.TryParse(minReproductionGrowthFrequencyInput.text, out simAgent.initialStats.minReproductionGrowthFrequency);
        float.TryParse(maxReproductionGrowthFrequencyInput.text, out simAgent.initialStats.maxReproductionGrowthFrequency);

        float.TryParse(minWanderInput.text, out simAgent.initialStats.minWander);
        float.TryParse(maxWanderInput.text, out simAgent.initialStats.maxWander);

        float.TryParse(minLookupInput.text, out simAgent.initialStats.minLookup);
        float.TryParse(maxLookupInput.text, out simAgent.initialStats.maxLookup);

        if (spawnOnWholeMap.isOn)
        {
            simAgent.spawnOnWholeMap = true;
        }
        else
        {
            simAgent.spawnOnWholeMap = false;
            float.TryParse(locationXInput.text, out simAgent.spawnCenter.x);
            float.TryParse(locationYInput.text, out simAgent.spawnCenter.y);
            float.TryParse(locationZInput.text, out simAgent.spawnCenter.z);
            float.TryParse(locationRadiusInput.text, out simAgent.radius);
        }

    }
    void DisplayUiElements(bool state)
    {
        hungerParent.SetActive(state);
        reproductionParent.SetActive(state);
        reproductionGrowthParent.SetActive(state);
        hungerGrowthParent.SetActive(state);
        wanderParent.SetActive(state);
        lookupParent.SetActive(state);
        movementParent.SetActive(state);
    }
    public void OnTypeDropdownChange(Dropdown typeDropdown)
    {
        if (typeDropdown.value == 1 || typeDropdown.value == 0)
        {
            DisplayUiElements(true);
        }
        if (typeDropdown.value == 2)
        {
            DisplayUiElements(false);
        }
    }
    public void OnSpawnLocationToggleChange(Toggle toggle)
    {
        if (toggle.isOn)
        {
            locationParent.SetActive(false);
            locationRadiusParent.SetActive(false);
        }
        else
        {
            locationParent.SetActive(true);
            locationRadiusParent.SetActive(true);
        }
    }
}
