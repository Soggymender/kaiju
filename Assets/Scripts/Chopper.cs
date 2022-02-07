using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chopper : MonoBehaviour {

    enum State {

        NONE,
        STANDBY,
        FACE_POI,
    };

    State state = State.NONE;
    Vector3 initialTargetDir;

    Transform poi;


    Vector3 targetDir;

    Vector3 poiVelocity;

    // Start is called before the first frame update
    void Start()
    {
        initialTargetDir = transform.forward;
        poi.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == State.STANDBY || state == State.FACE_POI) {

            UpdateFacePoi();
        }
    }

    void StartStandby() {

        state = State.STANDBY;

        targetDir = initialTargetDir;
    }
    
    void StartFacePoi() {

        state = State.FACE_POI;

        targetDir = poi.position - transform.position;
    }

    void UpdateFacePoi() {

        Vector3 dir = targetDir;

        dir = Vector3.SmoothDamp(transform.forward, dir, ref poiVelocity, 15.0f); 

        Quaternion target = Quaternion.LookRotation(dir);

        transform.rotation = target;
    }

    public void SetPointOfInterest(Transform poi) {
        
        this.poi = poi;

        if (poi.position != Vector3.zero) {
            StartFacePoi();
        }
        else {
            state = State.NONE;
        }
    }
}
