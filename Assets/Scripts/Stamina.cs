using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Stamina : MonoBehaviour {

    public Camera camera;
    public Transform worldAnchor;
    public Vector3 anchorOffset;

    public Image mask;
    public Image image;
    
    bool discharge = false;
    public float depletionLength = 4.0f;
    public float rechargeLength = 2.0f;
    float overrideRechargeLength = 0.0f;

    // Hide if full for > this length of time. Inactive.
    float fullLength = 2.0f;
    float fullTime = 0.0f;

    bool forceHide = true;
    bool forceShow = false;

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

                if (overrideRechargeLength > 0.0f)
                    value += Time.deltaTime / overrideRechargeLength;
                else
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
        
        mask.gameObject.SetActive(forceShow || (wantVisible && !forceHide));
    }

    void LateUpdate() {

        // Find the position of Kid on the screen.
        Vector3 screenPos = camera.WorldToScreenPoint(worldAnchor.position + anchorOffset);

        screenPos.z = transform.localPosition.z;
        transform.position = screenPos;
    }

    public void SetDischarge(bool discharge) {

        this.discharge = discharge;
        
    }

    public void ForceHide(bool hide) {

        forceHide = hide;

        // Try to keep this hidden during sprint -> drift -> jump when it would normally be recharging and visible for a few seconds.
        fullTime = fullLength;
    }

    public void ForceShow(bool show) {

        forceHide = false;
        forceShow = true;

        fullTime = 0.0f;
    }

    public float GetValue() {
        return value;
    }
    
    public bool Use(float amount) {

        if (value >= amount) {

            value -= amount;
            fullTime = 0.0f;
            return true;
        }

        return false;
    }

    public void OverrideRechargeLength(float length) {
        overrideRechargeLength = length;
    }

    public void ClearOverride() {
        overrideRechargeLength = 0.0f;
    }
}