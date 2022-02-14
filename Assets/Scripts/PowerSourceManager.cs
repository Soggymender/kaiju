using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerSourceManager : MonoBehaviour
{
   
    PowerSource[] powerSources = null;


    void Start() {

    }

    void Update() {

    }

    void CollectPowerSources() {

        powerSources = FindObjectsOfType<PowerSource>();

        // Iterate these and assign IDs.
        for (int i = 0; i < powerSources.Length; i++) {

            powerSources[i].id = i;
        }
    }

    public PowerSource FindNearbyPowerSource(Vector3 position) {

        if (powerSources == null) {
            CollectPowerSources();
        }

        for (int i = 0; i < powerSources.Length; i++) {

            Vector3 dir = powerSources[i].transform.position - position;
            dir.y = 0.0f;

            if (dir.magnitude < 25.0f) {
                return powerSources[i];
            }
        }

        return null;
    }
}
