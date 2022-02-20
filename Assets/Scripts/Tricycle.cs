using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(CharacterController))]

public class Tricycle : MonoBehaviour
{
    // Audio
    public AudioMixer MainMixer;
    public AudioSource as_Wheels_Rolling;
    public AudioSource as_WheelsScraping;
    public AudioSource as_Jump;
    public AudioClip ac_RollingLoop;
    public AudioClip ac_DriftingLoop;
    public float DriftingMaxVolume;

    public AudioClip[] ac_JumpClips;


    public PlayerControls controls;
    public GameObject mesh;
    public Stamina stamina;

    const float JUMP_TURN_SPEED = 4.5f;
    const float MAX_SPEED = 10.0f;
    const float SPRINT_SPEED = 13.5f;//11.5f;
    const float SPEED_BOOST_LENGTH = 10.0f;
    public float maxSpeed = MAX_SPEED;
  //  public float speedBoost = 0.0f;


    const float ACCELERATION = MAX_SPEED * 2.0f;
    const float DECCELERATION = MAX_SPEED / 2.0f;

    public float maxTurn = 90.0f;
    public float jumpSpeed = 5.0f;
    public float gravity = 9.6f;
    public bool isGrounded = false;

    //float speedBoostLength = 1.0f;
//    float speedBoostTime = 0.0f;

    float timeSinceLastDrift = 0.0f;
    float driftTime = 0.0f;
    float twist = 0.0f;
    float maxTwist = 45.0f;

    CharacterController characterController;
    public Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    bool requireSprintRepress = false;
    bool sprinting = false;

    bool drifting = false;
    Vector3 driftDirection = Vector3.zero;

    public float curSpeed = 0.0f;
    public float curTurn = 0.0f;
    bool isJumping = false;
    public float jumpDir = 0.0f;

    [HideInInspector]
    bool canMove = false;

    void Awake()
    {
        //as_Wheels_Rolling = GameObject.Find("Wheels").GetComponent<AudioSource>(); 
        //as_Jump = GameObject.Find("Jump").GetComponent<AudioSource>();
       
    }

    void Start() {

        //this plays the movement loop at start. 
        //it will always be playing but the player input inc/dec the volume <-- can be improved later
        if (as_Wheels_Rolling == null) {
            as_Wheels_Rolling = gameObject.AddComponent<AudioSource>();
            as_Wheels_Rolling.clip = ac_RollingLoop;
            as_Wheels_Rolling.outputAudioMixerGroup = MainMixer.FindMatchingGroups("SFX_Kid")[0];
            as_Wheels_Rolling.loop = true;
            as_Wheels_Rolling.Play();
        }

        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;

        if (as_WheelsScraping == null)
            as_Jump = gameObject.AddComponent<AudioSource>();
        as_WheelsScraping = gameObject.AddComponent<AudioSource>();
        as_WheelsScraping.clip = ac_DriftingLoop;
        as_WheelsScraping.Play();
        as_WheelsScraping.outputAudioMixerGroup = MainMixer.FindMatchingGroups("SFX_Kid")[0];
        as_WheelsScraping.loop = true;
        as_WheelsScraping.pitch = 1.5f;
        as_WheelsScraping.volume = 0f;
    }

    void Update() {

        isGrounded = characterController.isGrounded;

        //        if (!canMove) {
        //          curSpeed = 0.0f;
        //        moveDirection = Vector3.zero;
        //  }

        UpdateSpeed();

        curTurn = canMove ? maxTurn * Input.GetAxis(controls.horizontal) : 0;

        UpdateTwist();
        
        if (isGrounded && canMove) {

            if (isJumping) {
                isJumping = false;

                // If hard turn while landing, power slide.
                if (curSpeed == maxSpeed && Mathf.Abs(curTurn) >= maxTurn) {

                    // If jump was a left turn, drift has to be a left turn.
                    if ((curTurn < 0.0f && jumpDir < 0) || curTurn > 0.0f && jumpDir > 0) {

                        StartDrifting();
                    }
                }
            }

            if (drifting) {
                UpdateDrifting();
                as_WheelsScraping.volume = DriftingMaxVolume;
                
            }

            else {
                as_WheelsScraping.volume = 0f;
                timeSinceLastDrift += Time.deltaTime;

                Vector3 forward = transform.TransformDirection(Vector3.forward);

                moveDirection = forward * curSpeed;

             //   if (speedBoostTime > 0.0f) {
             //
//                    speedBoost = 0.25f;
               //     moveDirection += forward * curSpeed * speedBoost;
                //    speedBoostTime -= Time.deltaTime;
               // }
               // else
               //     speedBoost = 0.0f;
            }

            // This is a stupid gravity hack to keep isGrounded true.
            moveDirection.y = -0.4f;

            UpdateStartJump();
            UpdateSprint();
        }
        else {
            // this is changing decreasing the volume of the wheel sfx loop when not moving or on the ground 

            if (as_Wheels_Rolling != null) {
                as_Wheels_Rolling.volume = 0f;
            }
                as_WheelsScraping.volume = 0f;
        }
        
        
        moveDirection.y -= gravity * Time.deltaTime;
        
        // Player and Camera rotation
        //if (canMove) {

            // Choose effective speed.
            float effectiveSpeed = curSpeed;
            if (!isGrounded && curSpeed <= 0.01f) {
                effectiveSpeed = JUMP_TURN_SPEED;
            }

            // Choose effective turn.
            float effectiveTurn = curTurn;
            if (drifting) {
                effectiveTurn = curTurn * 0.33f;// 0.25f;
            }

            if (effectiveSpeed != 0) {
                float turnSpeedScalar = effectiveSpeed / maxSpeed;
                transform.eulerAngles = new Vector2(0, transform.eulerAngles.y + (effectiveTurn * turnSpeedScalar * Time.deltaTime));
            }
        //}

        // Twist the mesh for visual effect.
        mesh.transform.localRotation = Quaternion.Euler(0.0f, twist, 0.0f);

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }

    void UpdateSpeed() {

        float targetSpeed = 0.0f;
        float acceleration = 0.0f;

        if (isJumping) {

            targetSpeed = curSpeed;
       
        } else if (!canMove || (!Input.GetButton(controls.vertical) && !sprinting)) {

            targetSpeed = 0.0f;
            acceleration = DECCELERATION;

            // As soon as you let off forward, you lose any speed boost.
          //  speedBoostTime = 0.0f;
        }
        else {
            if (canMove) {

                // Accelerate toward target speed.
                if (sprinting)
                    targetSpeed = maxSpeed;
                else 
                    targetSpeed = maxSpeed * Input.GetAxis(controls.vertical);

                acceleration = ACCELERATION;
            }
        }

        curSpeed = Accelerate(curSpeed, acceleration, targetSpeed);
    }

    void UpdateTwist() {

        
        // Apply some funky mesh twisting to make it feel more cooler.
        float dir = curTurn < 0 ? -1.0f : 1.0f;

        if (drifting)
            twist = Accelerate(twist, maxTwist * 2.0f, dir * maxTwist);
        else
            twist = Accelerate(twist, maxTwist * 8.0f, 0.0f);
    }

    float Accelerate(float curValue, float acceleration, float targetValue) {

        float newValue = curValue;

        // Accelerate toward target speed.
        if (targetValue > curValue) {
            newValue += acceleration * Time.deltaTime;
            if (newValue > targetValue)
                newValue = targetValue;
        }
        else if (targetValue < newValue) {
            newValue -= acceleration * Time.deltaTime;
            if (newValue < targetValue)
                newValue = targetValue;
        }

        return newValue;
    }
    
    void UpdateStartJump() {

        if (Input.GetButtonDown(controls.jump) && canMove) {
            moveDirection.y = jumpSpeed;

            isJumping = true;
           // StopDrifting(false);

            jumpDir = curTurn; // Track this so we don't drift in the opposite direction of the jump. Feels bad.

            //this just plays the jump sfx
            if (as_Jump != null)
                playRandomJump(ac_JumpClips, MainMixer, as_Jump);
        }
    }

    void UpdateSprint() {

        if (requireSprintRepress && Input.GetButtonDown(controls.sprint))
            requireSprintRepress = false;

        // Holding sprint, and accelerating forward.
        if (!drifting && Input.GetButton(controls.sprint) && !requireSprintRepress) {// && Input.GetButton(controls.vertical) && Input.GetAxis(controls.vertical) > 0.0f) {

            float staminaValue = stamina.GetValue();

            if (!sprinting && staminaValue > 0.25f) {

                StartSprinting();
            }
            else {

                if (staminaValue == 0.0f) {
                    StopSprinting();
                    requireSprintRepress = true;
                }
            }
        }
        else {

            if (sprinting) {

                StopSprinting();
            }
        }
    }

    void StartSprinting() {

        stamina.SetDischarge(true);
        maxSpeed = SPRINT_SPEED;

        sprinting = true;
    }

    void StopSprinting() {

        stamina.SetDischarge(false);
        maxSpeed = MAX_SPEED;

        sprinting = false;
    }

    void UpdateDrifting() {

        if (!drifting)
            return;

        //timeSinceLastDrift = 0.0f;

        // If stop turning.
        if (Mathf.Abs(curTurn) < maxTurn) {
            StopDrifting(true);
            return;
        }

        // If stop going.
        if (!Input.GetButton(controls.vertical)) {//curSpeed <= 0.0f) {
            StopDrifting(false);
            return;
        }

        driftTime += Time.deltaTime;

        // Actually calculate the drift velocity.
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        if (curTurn < 0)
            moveDirection = Vector3.Slerp(forward, right, 0.5f) * curSpeed;
        else
            moveDirection = Vector3.Slerp(forward, -right, 0.5f) * curSpeed;
    }

    void StartDrifting() {

        drifting = true;
        driftTime = 0.0f;

        timeSinceLastDrift = 0.0f;

        //stamina.ForceHide(true);
        stamina.rechargeLength = 4.0f;
    }

    void StopDrifting(bool speedBoost) {

        drifting = false;
        timeSinceLastDrift = 0.0f;

/*        if (speedBoost) {
            // Grant speed boost.
            speedBoostTime = driftTime;
            if (speedBoostTime > SPEED_BOOST_LENGTH)
                speedBoostTime = SPEED_BOOST_LENGTH;
        }
        else {
            speedBoostTime = 0.0f;
        }
  */      
        // If they've been holding sprint, ignore it.
        requireSprintRepress = true;
        //stamina.ForceHide(false);

        stamina.rechargeLength = 8.0f;
    }

    void FixedUpdate() {

        // Move the controller
        //characterController.Move(moveDirection * Time.deltaTime);

        // this is changing incerasing the volume of the wheel sfx loop when moving forward   
        if (as_Wheels_Rolling != null) {
            as_Wheels_Rolling.volume = Mathf.Abs(curSpeed) / maxSpeed;
        }
        
        //  characterController.Move(moveDirection * Time.deltaTime);
    }

    public void SetPlayerControls(bool canMove, PlayerControls newControls) {
        this.canMove = canMove;

        if (!canMove) {
            StopSprinting();
            StopDrifting(false);
        }

        if (newControls != null)
            controls = newControls;

        if (canMove) {
            stamina.ForceHide(false);
        }
    }
        void playRandomJump(AudioClip[] clips, AudioMixer mix, AudioSource as_JumpSource)
    {
        int rand = Random.Range(0,clips.Length);
        as_JumpSource.PlayOneShot(clips[rand], 1); 
        as_JumpSource.outputAudioMixerGroup = mix.FindMatchingGroups("SFX_Echo")[0];
        
    }

    void OnControllerColliderHit(ControllerColliderHit hit) {
        
        if (hit.collider.tag == "obstacle") {
            drifting = false;
        }
    }
    
}
