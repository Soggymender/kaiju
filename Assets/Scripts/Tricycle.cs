using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class Tricycle : MonoBehaviour
{
    public float speed = 7.5f;
    public float maxTurn = 90.0f;
    public float jumpSpeed = 5.0f;
    public float gravity = 9.6f;
    public Transform playerCameraParent;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;
    public bool isGrounded = false;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    float curSpeed = 0.0f;
    float curTurn = 0.0f;

    [HideInInspector]
    public bool canMove = true;

    int frameCount = 0;

    void Start() {
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;
    }

    void Update() {

        if (characterController.isGrounded) {
            isGrounded = true;
        }
        else {
            isGrounded = false;
        }
        
        if (isGrounded) {

//            moveDirection.y = -0.1f;

            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            curSpeed = canMove ? speed * Input.GetAxis("Vertical") : 0;
            curTurn = canMove ? maxTurn * Input.GetAxis("Horizontal") : 0;
            moveDirection = (forward * curSpeed);// + (right * curSpeedY);

            if (Input.GetButton("Jump") && canMove) {
                moveDirection.y = jumpSpeed;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        

        // Player and Camera rotation
        if (canMove) {

            //float yVel = characterController.velocity.y;

            float effectiveSpeed = curSpeed;

            //if (yVel == 0.0f && characterController.velocity.y != 0.0f) {
            if (!isGrounded && curSpeed <= 0.01f) {
                effectiveSpeed = 1.0f;
            }

            if (effectiveSpeed != 0) {

                float turnSpeedScalar = effectiveSpeed / speed;
                
                transform.eulerAngles = new Vector2(0, transform.eulerAngles.y + (curTurn * turnSpeedScalar * Time.deltaTime));
            }
        }

       


    


        /*
        // Mouse look based turning.
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);
        playerCameraParent.localRotation = Quaternion.Euler(rotation.x, 0, 0);
        transform.eulerAngles = new Vector2(0, rotation.y);
        */
    }

    private void FixedUpdate() {

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);


    }

}
