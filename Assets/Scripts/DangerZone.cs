using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DangerZone : MonoBehaviour
{
    public float revealDistance;
    public float winDistance;

    public Tricycle tricycle;
    public GameObject mesh;

    bool win = false;

    void Start()
    {
        
    }

    void Update()
    {
        // If the round is over, terminate this behavior.
        if (win)
            return;

        float dist = (transform.position - tricycle.transform.position).magnitude;

        if (mesh.activeSelf) {
            if (dist > revealDistance) {
                mesh.SetActive(false);
            }

            else if (dist <= winDistance) {
                //mesh.SetActive(false);
                win = true;
            }
        }
        else {
            if (dist <= revealDistance) {

                mesh.SetActive(true);
            }
        }
    }

    public bool GetWinStatus() {
        return win;
    }
}
