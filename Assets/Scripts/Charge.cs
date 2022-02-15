using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Charge : MonoBehaviour
{
    public Kaiju kaiju;
    public Material kaijuMaterial;
    public Stamina kaijuStamina;


    public Transform kaijuAnchor;
    public Vector3 anchorOffset;
    PowerSource powerSource;
    public ElectricShock_LightningLineDemo spark;

    bool charging = false;

    void Start()
    {
        spark.gameObject.SetActive(false);
    }

    void Update()
    {
        
    }

    public bool StartCharge(PowerSource mast) {

        if (kaijuStamina.GetValue() == 1.0f) {
            return false;
        }

        this.powerSource = mast;

        spark.StartPosition = mast.transform;
        spark.EndPostion = kaijuAnchor;
        spark.anchorOffset = anchorOffset;
        spark.gameObject.SetActive(true);

        // Turn on the effect.
        kaijuMaterial.SetFloat("_IsShock", 1.0f);

        // Show Kaiju's stamina guage and set it to fill.
        kaijuStamina.ForceShow(true);
        kaijuStamina.OverrideRechargeLength(5.0f);

        charging = true;

        return true;
    }

    public void StopCharge() {

        // Turn off the effect.
        kaijuMaterial.SetFloat("_IsShock", 0.0f);

        spark.gameObject.SetActive(false);

        kaijuStamina.ForceShow(false);
        kaijuStamina.ClearOverride();

        powerSource.discharged = true;

        charging = false;
    }

    public bool Complete() {

        if (!charging)
            return true;

        if (kaijuStamina.GetValue() == 1.0f)
            return true;

        return false;
    }

    private void OnDestroy() {
        // Smash this back down so it's not annoying during scene editing.
        kaijuMaterial.SetFloat("_IsShock", 0.0f);
        //spark.gameObject.SetActive(false);
    }

}
