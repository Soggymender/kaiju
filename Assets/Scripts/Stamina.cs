using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour {

    public Image mask;
    public Image image;

    bool discharge = false;
    float depletionLength = 4.0f;
    float rechargeLength = 2.0f;

    // Hide if full for > this length of time. Inactive.
    float fullLength = 2.0f;
    float fullTime = 0.0f;

    bool forceHide = true;

    [Range(0, 1)]
    public float value = 0;

    private void Start() {

        // Fill and hide for round start.
        value = 1.0f;
        fullTime = fullLength;
    }

    void Update() {

        bool wantVisible = false;

        if (discharge) {

            wantVisible = true;

            // Deplete
            if (value > 0.0f) {

                value -= Time.deltaTime / depletionLength;
                if (value < 0.0f) {
                    value = 0.0f;
                    discharge = false;
                }
            }
        }
        else {
            // Recharge
            if (value < 1.0f) {

                wantVisible = true;

                value += Time.deltaTime / rechargeLength;
                if (value >= 1.0f) {
                    value = 1.0f;
                    fullTime = 0.0f;
                }
            }
            else {
                fullTime += Time.deltaTime;
                if (fullTime < fullLength)
                    wantVisible = true;
            }
        }

        image.fillAmount = value;
        
        mask.gameObject.SetActive(wantVisible && !forceHide);
    }

    public void SetDischarge(bool discharge) {

        this.discharge = discharge;
        
    }

    public void ForceHide(bool hide) {

        forceHide = hide;

        // Try to keep this hidden during sprint -> drift -> jump when it would normally be recharging and visible for a few seconds.
        fullTime = fullLength;
    }
    
    public float GetValue() {
        return value;
    }
}