using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(CharacterController))]

public class Tricycle : MonoBehaviour
{

    public AudioSource as_Wheels;
    public AudioSource as_Jump;

    public PlayerControls controls;

    const float JUMP_TURN_SPEED = 4.5f;
    public float maxSpeed = 7.5f;
    public float maxTurn = 90.0f;
    public float jumpSpeed = 5.0f;
    public float gravity = 9.6f;
    public Transform playerCameraParent;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;
    public bool isGrounded = false;
    




    CharacterController characterController;
    public Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    public float curSpeed = 0.0f;
    float curTurn = 0.0f;

    [HideInInspector]
    bool canMove = false;

    void Awake()
    {
        as_Wheels = GameObject.Find("Wheels").GetComponent<AudioSource>(); 
        as_Jump = GameObject.Find("Jump").GetComponent<AudioSource>();
       
    }

    void Start() {
        //this plays the movement loop at start. 
        //it will always be playing but the player input inc/dec the volume <-- can be improved later
        if (as_Wheels != null)
            as_Wheels.Play();


        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;


    }

    void Update() {

        isGrounded = characterController.isGrounded;

        if (!canMove) {
            curSpeed = 0.0f;
            moveDirection = Vector3.zero;
        }

        if (isGrounded && canMove) {
            

            // We are grounded, so recalculate move direction based on axes
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);
            curSpeed = canMove ? maxSpeed * Input.GetAxis(controls.vertical) : 0;
            curTurn = canMove ? maxTurn * Input.GetAxis(controls.horizontal) : 0;

            moveDirection = (forward * curSpeed);// + (right * curSpeedY);
            moveDirection.y = -0.4f;


            if (Input.GetButtonDown(controls.jump) && canMove) {
                moveDirection.y = jumpSpeed;

                //this just plays the jump sfx
                if (as_Jump != null)
                    as_Jump.Play();
            }
        }
        else {
            // this is changing decreasing the volume of the wheel sfx loop when not moving or on the ground 

            if (as_Wheels != null) {
                as_Wheels.volume = 0f;
            }
        }
        
        
        moveDirection.y -= gravity * Time.deltaTime;
        
        // Player and Camera rotation
        if (canMove) {

            //float yVel = characterController.velocity.y;

            float effectiveSpeed = curSpeed;

            //if (yVel == 0.0f && characterController.velocity.y != 0.0f) {
            if (!isGrounded && curSpeed <= 0.01f) {
                effectiveSpeed = JUMP_TURN_SPEED;
            }

            if (effectiveSpeed != 0) {
                float turnSpeedScalar = effectiveSpeed / maxSpeed;
                transform.eulerAngles = new Vector2(0, transform.eulerAngles.y + (curTurn * turnSpeedScalar * Time.deltaTime));
            }
        }

        
        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }

    void FixedUpdate() {

        // Move the controller
        //characterController.Move(moveDirection * Time.deltaTime);

        // this is changing incerasing the volume of the wheel sfx loop when moving forward   
        if (as_Wheels != null) {
            as_Wheels.volume = Mathf.Abs(curSpeed) / maxSpeed;
        }
        
        //  characterController.Move(moveDirection * Time.deltaTime);
    }

    public void SetPlayerControls(bool canMove, PlayerControls newControls) {
        this.canMove = canMove;

        if (newControls != null)
            controls = newControls;
    }
}
