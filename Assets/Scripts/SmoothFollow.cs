using UnityEngine;
using System.Collections;

public class SmoothFollow : MonoBehaviour {

    // The target we are following
    public Transform target;
    // The distance in the x-z plane to the target
    public float distance = 10.0f;
    // the height we want the camera to be above the target
    public float height = 5.0f;
    // How much we 
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;


    public float minKaijuDistance;
    public float maxKaijuDistance;
    float yOffset = 0.0f;

    float farYOffset = 0.0f;
    float farHeight = 0.0f;
    public float nearYOffset;
    public float nearHeight;

    public GameObject kaijuGo;
    float kaijuDistance;


    // Place the script in the Camera-Control group in the component menu
    [AddComponentMenu("Camera-Control/Smooth Follow")]


    // Place the script in the Camera-Control group in the component menu
    private void Start() {

        farHeight = height;
        
    }

    private void Update() {

        Vector3 direction = kaijuGo.transform.position - transform.position;
        direction.y = 0.0f;

        kaijuDistance = direction.magnitude;
    }

    void LateUpdate() {
        // Early out if we don't have a target
        if (!target)
            return;

        PanUp();
        
        // Calculate the current rotation angles
        float wantedRotationAngle = target.eulerAngles.y;
        float wantedHeight = target.position.y + yOffset + height;

        float currentRotationAngle = transform.eulerAngles.y;
        float currentHeight = transform.position.y;

        // Damp the rotation around the y-axis
        currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, rotationDamping * Time.deltaTime);

        // Damp the height
        currentHeight = Mathf.Lerp(currentHeight, wantedHeight, heightDamping * Time.deltaTime);

        // Convert the angle into a rotation
        var currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        // Set the position of the camera on the x-z plane to:
        // distance meters behind the target
        //transform.position.Set(target.position.x, target.position.y, target.position.z);// = target.position;

        Vector3 tempPos = target.position;
        tempPos.y += yOffset;

        transform.position = tempPos;
        transform.position -= currentRotation * Vector3.forward * distance;

        // Set the height of the camera
        transform.position = new Vector3(transform.position.x, currentHeight, transform.position.z);

        // Always look at the target
        transform.LookAt(target);
    }

    // Look upward when near Kaiju.
    void PanUp() {

        if (kaijuDistance > minKaijuDistance) {
            height = farHeight;
            yOffset = 0.0f;

            return;
        }

        height = Mathf.Lerp(nearHeight, farHeight, kaijuDistance / minKaijuDistance);
        yOffset = Mathf.Lerp(nearYOffset, farYOffset, kaijuDistance / minKaijuDistance);
    }
}