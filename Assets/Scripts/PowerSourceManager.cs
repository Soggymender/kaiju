using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSourceManager : MonoBehaviour
{
   
    PowerSource[] powerSources = null;


    void Start() {

        CollectPowerSources();
    }

    void Update() {

    }

    void CollectPowerSources() {

        powerSources = FindObjectsOfType<PowerSource>();

        // Iterate these and assign IDs.
        for (int i = 0; i < powerSources.Length; i++) {
            powerSources[i].id = i;
            powerSources[i].transform.parent.gameObject.SetActive(false);
        }

        int numTurnedOn = 0;
        while (numTurnedOn < 3) {

            int idx = Random.Range(0, powerSources.Length);
            if (powerSources[idx].transform.parent.gameObject.activeSelf) {
                continue;
            }

            powerSources[idx].transform.parent.gameObject.SetActive(true);
            numTurnedOn++;
        }
    }

    public PowerSource FindNearbyPowerSource(Vector3 position) {

        if (powerSources == null) {
            CollectPowerSources();
        }

        for (int i = 0; i < powerSources.Length; i++) {

            if (!powerSources[i].transform.parent.gameObject.activeSelf) {
                continue;
            }

            if (powerSources[i].discharged) {
                continue;
            }

            Vector3 dir = powerSources[i].transform.position - position;
            dir.y = 0.0f;

            if (dir.magnitude < 25.0f) {
                return powerSources[i];
            }
        }

        return null;
    }
}
