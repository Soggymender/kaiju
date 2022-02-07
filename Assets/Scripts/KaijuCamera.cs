using UnityEngine;
using System.Collections;

public class KaijuCamera : MonoBehaviour {

    public enum LeanDirection {
        NONE,
        LEFT,
        RIGHT,
    }

    public Kaiju kaiju;
    public Transform target;
    Vector3 originalTargetPosition;
    Vector3 originalTargetLocalPosition;

    // The distance in the x-z plane to the target
    public float distance = 10.0f;
    // the height we want the camera to be above the target
    public float height = 5.0f;
    // How much we 
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;
    
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    Vector3 rotation = Vector3.zero;

    float targetOffset = 0.0f;

    // lean left and right camera positions.
    float leanVelocity;
    float leftOffset = 15.0f;
    float rightOffset = -15.0f;

    bool firstUpdate = true;

    // Place the script in the Camera-Control group in the component menu
    [AddComponentMenu("Camera-Control/Smooth Follow")]

    // Place the script in the Camera-Control group in the component menu
    void Start() {

        targetOffset = 0.0f;
    
        rotation = target.transform.localRotation.eulerAngles;
    }

    private void Update() {

        // Interpolate between center, left and right to give vision assist during kaiju lean.
        float offsetX = Mathf.SmoothDamp(target.transform.localPosition.x, targetOffset, ref leanVelocity, 0.25f);

        //   offset = target.transform.localPosition;

        Vector3 offset = target.transform.localPosition;

        offset.x = offsetX;// 10.0f;

        target.transform.localPosition = offset;

    }

    void LateUpdate() {
        // Early out if we don't have a target
        if (!target)
            return;

        if (!kaiju.GetCanMove())
            return;

           
        //Vector3 newPosition = Vector3.SmoothDamp(transform.localPosition, targetPosition, ref leanVelocity, 0.25f);
        //target.transform.localPosition = newPosition;

        // Mouse look based turning.
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
        target.localRotation = Quaternion.Euler(rotation.x, rotation.y, rotation.z); 
    }

    public void SetLeanDirection(LeanDirection leanDirection) {

        switch (leanDirection) {

            case LeanDirection.NONE:
                targetOffset = 0.0f;
                break;

            case LeanDirection.LEFT:
                targetOffset = 12.5f;
                 break;

            case LeanDirection.RIGHT:
                targetOffset = -12.5f;
                 break;

            default:
                break;
        }

    }
}