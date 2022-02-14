using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricShock_ControllerExample : MonoBehaviour
{
   
    public bool IsOpenElectricShock;
    public Renderer ElectricShockObj;
    public Animator animator;
    public GameObject LightLine;
    
    float NowElectricShockPower;
     
    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOpenElectricShock)
        {
            if (NowElectricShockPower < 1) { NowElectricShockPower += Time.deltaTime * 3; }
        }
        else {
            if (NowElectricShockPower >0) { NowElectricShockPower -= Time.deltaTime * 3; }
        }
        ElectricShockObj.material.SetFloat("_IsShock", NowElectricShockPower);
        if (animator) { animator.enabled = NowElectricShockPower > 0; }
        if (LightLine) { LightLine.SetActive(NowElectricShockPower > 0); }
    }

    public void ShockNow() {
        IsOpenElectricShock = !IsOpenElectricShock;
    }
}
