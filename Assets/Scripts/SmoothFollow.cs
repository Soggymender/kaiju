using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour {

    public Transform target;

    public float followDistance = 10.0f;
    public float follwoHeight = 5.0f;

    public float heightDamping = 2.0f;
    public float headingDamping = 3.0f;

    // KAIJU DISTANCE PITCH UP
    public float minKaijuDistance;
    public float maxKaijuDistance;
    float yOffset = 0.0f;

    float farYOffset = 0.0f;
    float farHeight = 0.0f;
    public float nearYOffset;
    public float nearHeight;

    public GameObject kaijuGo;
    float kaijuDistance;

    private void Start() {

        farHeight = follwoHeight;        
    }

    private void Update() {

        Vector3 direction = kaijuGo.transform.position - transform.position;
        direction.y = 0.0f;

        kaijuDistance = direction.magnitude;
    }

    void LateUpdate() {
        
        if (!target)
            return;

        PanUp();
        
        // Calculate the current rotation angles
        float targetHeading = target.eulerAngles.y;
        float currentHeading = transform.eulerAngles.y;

        currentHeading = Mathf.LerpAngle(currentHeading, targetHeading, headingDamping * Time.deltaTime);

        float targetHeight = target.position.y + yOffset + follwoHeight;       
        float currentHeight = transform.position.y;

        currentHeight = Mathf.Lerp(currentHeight, targetHeight, heightDamping * Time.deltaTime);



        // Convert the angle into a rotation
        var currentRotation = Quaternion.Euler(0, currentHeading, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        // transform.position = Vector3.Lerp(transform.position, target.position, 20 * Time.deltaTime);
       // transform.position.Set(target.position.x, target.position.y, target.position.z);// = target.position;

        Vector3 tempPos = target.position;
        tempPos.y += yOffset;

        transform.position = tempPos;
        transform.position -= currentRotation * Vector3.forward * followDistance;

        // Set the height of the camera
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        // Always look at the target
        transform.LookAt(target);
    }

    // Look upward when near Kaiju.
    void PanUp() {

        if (kaijuDistance > minKaijuDistance) {
            follwoHeight = farHeight;
            yOffset = 0.0f;

            return;
        }

        follwoHeight = Mathf.Lerp(nearHeight, farHeight, kaijuDistance / minKaijuDistance);
        yOffset = Mathf.Lerp(nearYOffset, farYOffset, kaijuDistance / minKaijuDistance);
    }
}