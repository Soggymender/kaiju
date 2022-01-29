using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(CharacterController))]

public class Tricycle : MonoBehaviour
{

    public AudioSource Movesound;
    public AudioSource Jumpsfx;
    public AudioSource Impactsfx;

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
    public bool canMove = false;

    void Start() {
        //this plays the movement loop at start. 
        //it will always be playing but the player input inc/dec the volume <-- can be improved later
        if (Movesound != null)
            Movesound.Play();


        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;


    }

    void Update() {

        isGrounded = characterController.isGrounded;
        
        if (isGrounded && canMove) {
//            moveDirection.y = -0.1f;

            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            curSpeed = canMove ? speed * Input.GetAxis("Vertical") : 0;
            curTurn = canMove ? maxTurn * Input.GetAxis("Horizontal") : 0;
            moveDirection = (forward * curSpeed);// + (right * curSpeedY);
            


            if (Input.GetButton("Jump") && canMove) {
                moveDirection.y = jumpSpeed;

                //this just plays the jump sfx
                if (Jumpsfx != null)
                    Jumpsfx.Play();
            }
        }
        else {
            // this is changing decreasing the volume of the wheel sfx loop when not moving or on the ground 

            if (Movesound != null) {
                Movesound.volume = 0f;
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

        
        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }

    private void FixedUpdate() {

        // Move the controller
        //characterController.Move(moveDirection * Time.deltaTime);

        // this is changing incerasing the volume of the wheel sfx loop when moving forward   
        if (Movesound != null) {
            if (curSpeed >= .4) {
                Movesound.volume = 1f;
            }

            if (curSpeed <= -.4f) {
                Movesound.volume = 1f;
            }
        }
        
        //  characterController.Move(moveDirection * Time.deltaTime);
    }

    public void SetCanMove(bool canMove) {
        this.canMove = canMove;
    }
}
