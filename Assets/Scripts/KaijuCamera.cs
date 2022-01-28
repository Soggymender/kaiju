using UnityEngine;
using System.Collections;

public class KaijuCamera : MonoBehaviour {

    // The target we are following
    public Transform target;
    // The distance in the x-z plane to the target
    public float distance = 10.0f;
    // the height we want the camera to be above the target
    public float height = 5.0f;
    // How much we 
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;


    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    Vector2 rotation = Vector2.zero;

    bool firstUpdate = true;

    // Place the script in the Camera-Control group in the component menu
    [AddComponentMenu("Camera-Control/Smooth Follow")]

    void LateUpdate() {
        // Early out if we don't have a target
        if (!target)
            return;


        if (firstUpdate) {
            

        }

        // Mouse look based turning.
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
        target.localRotation = Quaternion.Euler(rotation.x, rotation.y, 0); 
    }
}