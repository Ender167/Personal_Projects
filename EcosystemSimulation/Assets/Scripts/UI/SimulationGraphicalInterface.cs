using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class SimulationGraphicalInterface : MonoBehaviour
{
    public CameraMovement mainCam;
    public bool isCameraLocked = false;
    public LayerMask layerMask;

    [Header("Templates")]
    public Animal groundedHerbivore;
    public Animal groundedCarnivore;
    public Animal flyingHerbivore;
    public Plant plant;

    [Header("UI")]

    public Toggle agentsSlidersToggle;
    public Slider simulationSpeedSlider;
    public Text currentTime;
    public Text currentDayState;

    [Header("Other")]
    public GameObject agentPanel;
    public GameObject simulationPanel;
    public GameObject agentTemplatesPanel;
    public GameObject selectedAgentTemplatePanel;
    public GameObject createAgentTemplatePanel;
    public GameObject weatherConditionsTemplatePanel;
    public GameObject checkWeatherConditionsTemplatePanel;
    public Entity selectedAgent;

    SimulationManager simManager;

    void Start()
    {
        simManager = GameObject.FindWithTag("GameController").GetComponent<SimulationManager>();
    }

    void Update()
    {
        Cursor.lockState = isCameraLocked == true ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isCameraLocked == true ? true : false;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            isCameraLocked = !isCameraLocked;
        }

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Agent")))
            {
                selectedAgent = hit.collider.gameObject.GetComponent<Entity>();
                OpenFormForAgent(selectedAgent);
            }
        }
    }
    public void UpdateAgent()
    {
        if(selectedAgent != null)
        {
            Animal a = selectedAgent as Animal;
            if(a != null)
            {
                a.ComputeFitness();
            }
        }
    }
    public void DeleteAgent()
    {
        if (selectedAgent != null)
        {
            selectedAgent.Die();
        }
    }
    public void StartSimulation()
    {
        CloseAllPanels();
        simManager.StartSimulation();
        simulationSpeedSlider.value = 1;
    }
    public void CloseAllPanels()
    {
        agentPanel.SetActive(false);
        simulationPanel.SetActive(false);
        agentTemplatesPanel.SetActive(false);
        selectedAgentTemplatePanel.SetActive(false);
        createAgentTemplatePanel.SetActive(false);
        weatherConditionsTemplatePanel.SetActive(false);
        checkWeatherConditionsTemplatePanel.SetActive(false);
    }
    public void EnablePanel(GameObject obj)
    {
        CloseAllPanels();
        if (obj != null)
            obj.SetActive(true);
    }
    public void SelectTemplate(string s)
    {
        int selectedAgentId = 0;
        if (s == "plant")
        {
            selectedAgentId = 0;
        }
        if (s== "groundedHerbivore")
        {
            selectedAgentId = 1;
        }
        if (s == "groundedCarnivore")
        {
            selectedAgentId = 2;
        }
        EnablePanel(selectedAgentTemplatePanel);
        selectedAgentTemplatePanel.GetComponent<FormManager>().FillForm(simManager, selectedAgentId);
    }
    public void SelectTemplate(int s)
    {
        int selectedAgentId = s;
        EnablePanel(selectedAgentTemplatePanel);
        selectedAgentTemplatePanel.GetComponent<FormManager>().FillForm(simManager, selectedAgentId);
    }
    public void CreateCustomTemplate()
    {
        EnablePanel(createAgentTemplatePanel);
        createAgentTemplatePanel.GetComponent<FormManager>().OpenCreateForm(simManager, agentTemplatesPanel);
    }
    public void OpenFormForAgent(Entity agent)
    {
        EnablePanel(agentPanel);
        agentPanel.GetComponent<FormManager>().FillFormWithSelectedAgent(simManager, agent);
    }
    public void OpenWeatherConditionsPanel(int s)
    {
        EnablePanel(checkWeatherConditionsTemplatePanel);
        agentPanel.GetComponent<FormManager>().FillFormWeather(simManager, s);
    }
}
