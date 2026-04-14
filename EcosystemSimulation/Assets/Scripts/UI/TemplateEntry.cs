using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;

public class TemplateEntry : MonoBehaviour
{
    public Text entryName;
    public Button button;
    public SimulationGraphicalInterface simulationGraphicalInterface;

    public void Init(SimulationGraphicalInterface simGrI, string name, int id)
    {
        simulationGraphicalInterface = simGrI;
        entryName.text = name;
        button.onClick.AddListener(delegate { simulationGraphicalInterface.SelectTemplate(id); }) ;
    }
}
