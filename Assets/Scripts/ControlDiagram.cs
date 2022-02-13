using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlDiagram : MonoBehaviour
{
    public GameObject directionalUi;
    public GameObject sprintUi;
    public GameObject jumpUi;
    public GameObject lookUi;
    public GameObject goalText;

    bool goalTextDecay = false;
    float goalTextTime = 0.0f;

    void Start()
    {
        
    }

    void Update()
    {
        if (goalTextDecay) {

            goalTextTime += Time.deltaTime;
            if (goalTextTime >= 5.0f) {
                goalTextDecay = false;
                goalText.SetActive(false);
            }
        }
    }

    public void ShowKidControls() {

        directionalUi.SetActive(true);
        sprintUi.SetActive(true);
        jumpUi.SetActive(true);
        lookUi.SetActive(false);

        goalText.GetComponent<TMPro.TextMeshProUGUI>().text = "Find the Kaiju!";
    }

    public void ShowKaijuControls() {

        directionalUi.SetActive(true);
        sprintUi.SetActive(true);
        jumpUi.SetActive(true);
        lookUi.SetActive(true);

        goalText.GetComponent<TMPro.TextMeshProUGUI>().text = "Don't get caught!";
    }

    public void StartGoalTextDecay() {

        goalTextTime = 0.0f;
        goalTextDecay = true;

        goalText.SetActive(true);
    }
}
