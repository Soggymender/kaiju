using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(CharacterController))]

public class Tricycle : MonoBehaviour
{
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

    const float JUMP_TURN_SPEED = 4.5f;
    public float maxSpeed = 7.5f;
    public float maxTurn = 90.0f;
    public float jumpSpeed = 5.0f;
    public float gravity = 9.6f;
    public bool isGrounded = false;

    //float speedBoostLength = 1.0f;
    float speedBoostTime = 0.0f;

    float moveTime = 0.0f;
    float driftTime = 0.0f;
    float driftHeadingOffset = 0.0f;
    float maxDriftHeadingOffset = 45.0f;


    CharacterController characterController;
    public Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    bool drifting = false;
    Vector3 driftDirection = Vector3.zero;

    public float curSpeed = 0.0f;
    float curTurn = 0.0f;
    bool isJumping = false;

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
        if (as_Wheels_Rolling == null)
<<<<<<< Updated upstream
            as_Wheels_Rolling = gameObject.AddComponent<AudioSource>();
            as_Wheels_Rolling.clip = ac_RollingLoop;
            as_Wheels_Rolling.outputAudioMixerGroup = MainMixer.FindMatchingGroups("SFX_Kid")[0];
            as_Wheels_Rolling.loop = true;
            as_Wheels_Rolling.Play();
=======
        as_Wheels_Rolling = gameObject.AddComponent<AudioSource>();
        as_Wheels_Rolling.clip = ac_RollingLoop;
        as_Wheels_Rolling.outputAudioMixerGroup = MainMixer.FindMatchingGroups("SFX_Kid")[0];
        as_Wheels_Rolling.loop = true;
        as_Wheels_Rolling.Play();
>>>>>>> Stashed changes



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

        if (!canMove) {
            curSpeed = 0.0f;
            moveDirection = Vector3.zero;
        }

        if (isGrounded && canMove) {

            if (isJumping) {
                isJumping = false;

                // If hard turn while landing, power slide.
                if (curSpeed == maxSpeed && Mathf.Abs(curTurn) >= maxTurn * 0.66f) {

                    drifting = true;
                    driftTime = 0.0f;

                    moveTime = 0.0f;
                }
            }

            // We are grounded, so recalculate move direction based on axes
            curSpeed = canMove ? maxSpeed * Input.GetAxis(controls.vertical) : 0;
            curTurn = canMove ? maxTurn * Input.GetAxis(controls.horizontal) : 0;

            if (drifting) {
                UpdateDrifting();
                as_WheelsScraping.volume = DriftingMaxVolume;
                
            }

            else {
                as_WheelsScraping.volume = 0f;
                moveTime += Time.deltaTime;

                Vector3 forward = transform.TransformDirection(Vector3.forward);

                moveDirection = forward * curSpeed;

                if (speedBoostTime > 0.0f) {

                    moveDirection += forward * curSpeed * 0.25f;
                    speedBoostTime -= Time.deltaTime;
                }

                // Apply some funky mesh twisting to make it feel more cooler.
                if (!Mathf.Approximately(driftHeadingOffset, 0.0f)) {
                    
                    driftHeadingOffset = Mathf.Lerp(driftHeadingOffset, 0.0f, moveTime / 2.0f);
                }
            }

            // This is a stupid gravity hack to keep isGrounded true.
            moveDirection.y = -0.4f;

            if (Input.GetButtonDown(controls.jump) && canMove) {
                moveDirection.y = jumpSpeed;

                isJumping = true;
                drifting = false;

                //this just plays the jump sfx
                if (as_Jump != null)
                    playRandomJump(ac_JumpClips, MainMixer, as_Jump);
            }
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
        if (canMove) {

            // Choose effective speed.
            float effectiveSpeed = curSpeed;
            if (!isGrounded && curSpeed <= 0.01f) {
                effectiveSpeed = JUMP_TURN_SPEED;
            }

            // Choose effective turn.
            float effectiveTurn = curTurn;
            if (drifting) {
                effectiveTurn = curTurn * 0.25f;
            }

            if (effectiveSpeed != 0) {
                float turnSpeedScalar = effectiveSpeed / maxSpeed;
                transform.eulerAngles = new Vector2(0, transform.eulerAngles.y + (effectiveTurn * turnSpeedScalar * Time.deltaTime));
            }
        }

        // Twist the mesh for visual effect.
        mesh.transform.localRotation = Quaternion.Euler(0.0f, driftHeadingOffset, 0.0f);

        // Move the controller
        characterController.Move(moveDirection * Time.deltaTime);
    }

    void UpdateDrifting() {

        if (!drifting)
            return;

        moveTime = 0.0f;

        // If stop turning.
        if (Mathf.Abs(curTurn) <= 5.0f) {
            drifting = false;

            // Grant speed boost.
            speedBoostTime = driftTime;
            if (speedBoostTime > 3.0f)
                speedBoostTime = 3.0f;

            return;
        }

        // If stop going.
        if (curSpeed <= 0.0f) {
            drifting = false;
            speedBoostTime = 0.0f;

            return;
        }

        driftTime += Time.deltaTime;

        // Apply some funky mesh twisting to make it feel more cooler.
        float dir = curTurn < 0 ? -1.0f : 1.0f;

        // Lets cheese the interpolation function to get a nice lazy ease-in.
        driftHeadingOffset = Mathf.Lerp(driftHeadingOffset * 0.5f, maxDriftHeadingOffset * dir, driftTime / 1.0f);

        // Actually calculate the drift velocity.
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        if (curTurn < 0)
            moveDirection = Vector3.Slerp(forward, right, 0.5f) * curSpeed;
        else
            moveDirection = Vector3.Slerp(forward, -right, 0.5f) * curSpeed;
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

        if (newControls != null)
            controls = newControls;
    }
        void playRandomJump(AudioClip[] clips, AudioMixer mix, AudioSource as_JumpSource)
    {
        int rand = Random.Range(0,clips.Length);
        as_JumpSource.PlayOneShot(clips[rand], 1); 
        as_JumpSource.outputAudioMixerGroup = mix.FindMatchingGroups("SFX_Echo")[0];
        
    }
}
