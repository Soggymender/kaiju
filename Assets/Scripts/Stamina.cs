using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour {

    public Image image;

    bool discharge = false;
    float depletionLength = 4.0f;
    float rechargeLength = 2.0f;

    [Range(0, 1)]
    public float value = 0;

    void Update() {

        if (discharge) {
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

                value += Time.deltaTime / rechargeLength;
                if (value >= 1.0f) {
                    value = 1.0f;
                }
            }
        }

        image.fillAmount = value;
    }

    public void Show(bool show) {
        // Leave THIS object active for recharging but hide the image.
        image.gameObject.SetActive(show);
    }

    public void SetDischarge(bool discharge) {

        this.discharge = discharge;
    }

    public float GetValue() {
        return value;
    }
}