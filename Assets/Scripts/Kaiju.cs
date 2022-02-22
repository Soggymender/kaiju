using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Kaiju : MonoBehaviour {
    enum State {

        NONE,
        STAND,
        CHARGE,
        LEAN,
        SLIDE_NEAR,
        SLIDE_LEFT,
        SLIDE_RIGHT,
        SLIDE_BACK,
        LEAP,
        TAUNT,
    }

    public PlayerControls controls;
    public ControlDiagram controlDiagram;
    public CoverArrows coverArrows;
    public Charge charge;

    public Transform headTransform;
    public Transform leftEyeTransform;
    public Transform rightEyeTransform;

    bool canMove = false;

    float yVelocity = 0.0f;
    float jumpSpeed = 4.0f;
    float gravity = 9.6f;

    CoverPoint oldCoverPoint = null;
    CoverPoint coverPoint = null;
    CoverPoint targetCoverPoint = null;

    Vector3 slideVelocity;

    public Camera kidCamera = null;
    public CameraShake kidCameraShake = null;
    public KaijuCamera camera = null;
    public Magnifier magnifier = null;

    public Stamina stamina;
    public bool PlayChargeSound = false;

    public Chopper chopper = null;

    // LEAN LEFT / RIGHT
    bool leftLeanPositionValid = false;
    Vector3 leftLeanPosition;

    bool rightLeanPositionValid = false;
    Vector3 rightLeanPosition;

    Vector3 leanVelocity;


    // Lean OR Slide target position. May be a cover corner, may be left / center / right lean position.
    // Allows full control while switching cover.
    Vector3 targetPosition;
    Quaternion targetRotation;



    // COVER OFFSET (toward building)

    public Transform coverOffset = null;
    public GameObject root = null;

    Vector3 lateralOffset;

    const float NEAR_SLIDE_STAMINA = 1.0f / 8.0f;
    const float FAR_SLIDE_STAMINA = 1.0f / 8.0f;

    float targetLeanLength = 0.0f;
    float targetSlideLength = 0.0f;

    const float LEAN_LENGTH = 0.15f;
    const float NEAR_SLIDE_LENGTH = 0.15f;
    const float FAR_SLIDE_LENGTH = 0.15f;

    const float STAND_TO_LEAN_LENGTH = 0.5f;
    const float LEAN_TO_STAND_LENGTH = 0.5f;
    const float JUMP_LENGTH = 1.85f;
    const float SLIDE_LENGTH = 0.75f; // .5 is nice because a slide is always two back to back passing through a corner.
    float transitionTime = 0.0f;
    float transitionLength = 0.0f;

    public Vector2 desiredDir;
    public Vector2 moveDir;
    float jumpTimer = 0.0f;
    const float MOVE_FAR_LENGTH = 0.5f;

    //Vector3 targetOffset;

    State state = State.STAND;

    CoverManager coverManager = null;
    PowerSourceManager powerSourceManager = null;

    bool monsterVision = true;

    Animator animator = null;

    public AudioSource as_Kaiju;
    public AudioMixer Mixer;

    //Arrays of sound clips for calling randomly
    public AudioClip[] ac_SlideChargeClips;


    // Start is called before the first frame update
    void Start() {

        as_Kaiju = gameObject.AddComponent<AudioSource>();


        coverManager = FindObjectOfType<CoverManager>();
        if (coverManager == null) {
            throw new System.Exception("Couldn't find CoverManager in scene.");
        }

        powerSourceManager = FindObjectOfType<PowerSourceManager>();
        if (powerSourceManager == null) {
            throw new System.Exception("Couldn't find PowerSourceManager in scene.");
        }

        animator = GetComponentInChildren<Animator>();
        if (coverManager == null) {
            throw new System.Exception("Couldn't find CoverManager in scene.");
        }

        WarpToRandomCoverPoint();
    }

    // Update is called once per frame
    void Update() {
        UpdateMonsterVision();

        // Update desired direction and jump hold / release.
        if (canMove) {

            // Keep track of desired direction between moves.
            // Latest direction press is the valid one.
            if (Input.GetButton(controls.horizontal)) {
                desiredDir.x = Input.GetAxis(controls.horizontal);
                //desiredDir.y = 0.0f;
            }

            if (Input.GetButton(controls.vertical)) {
                //desiredDir.x = 0.0f;
                desiredDir.y = Input.GetAxis(controls.vertical);
            }

            // This seems nutty, but on keyboard you can let go of left, and still be holding right, etc.
            if (Input.GetButtonUp(controls.horizontal) && !Input.GetButton(controls.horizontal))
                desiredDir.x = 0.0f;

            if (Input.GetButtonUp(controls.vertical) && !Input.GetButton(controls.vertical))
                desiredDir.y = 0.0f;

            UpdateCharge();
            UpdateCoverChange();

            if (state == State.STAND && targetLeanLength == 0.0f) {
                if (Input.GetButtonDown(controls.sprint)) {
                    
                    // If on directional input, don't change cover.
                    if (Mathf.Approximately(moveDir.magnitude, 0.0f)) {
                        
                        // Jump in place. Taunt, destabalize kid?
                        StartTaunt();
                    }
                }
            }
        }

        UpdateCoverArrows();
        
        // Update Stand
        if (state == State.STAND) {
            // While standing the transform is always some interpolation between the current cover point and the one to the left or right, depending on input.
            // This creates a lean-out-of-cover effect.

            UpdateLean();

            // Stand angle with back to wall.
            Vector3 standAngle = coverPoint.transform.rotation.eulerAngles;
            standAngle = new Vector3(standAngle.x, standAngle.y + coverPoint.headingOffset + 180, standAngle.z);

            targetRotation = Quaternion.Euler(standAngle);
        }

        // Update Lean
        else if (state == State.LEAN) {

        }

        // Update slide
        else if (state == State.SLIDE_NEAR || state == State.SLIDE_LEFT || state == State.SLIDE_RIGHT || state == State.SLIDE_BACK) {

            UpdateLean();
            UpdateSlide();

            if (UpdateTransitionTime(State.STAND)) {
                //coverPoint = targetCoverPoint;

                targetCoverPoint = null;

                // If this is a corner, keep going.
                if (coverPoint.coverType == CoverPoint.CoverType.CORNER) {
                    if (moveDir.x < 0) {
                        MoveToCoverLeft(false);
                    }
                    else if (moveDir.x > 0) {
                        MoveToCoverRight(false);
                    }
                }
                else {
                    animator.SetBool("Stand", true);
                }

                moveDir.x = 0.0f;
                moveDir.y = 0.0f;
            }
        }
        
        // ALWAYS interpolating between current position and possible target position. This covers slide and lean.
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref leanVelocity, targetLeanLength + targetSlideLength);

        // Transform.
        Move(newPosition, targetRotation);

        // Mesh.
        MoveRoot();

        UpdateAnimations();
    }

    void UpdateCoverArrows() {

        if (state != State.STAND) {
            coverArrows.HideArrows();
            return;
        }


        if (desiredDir.x == 0.0f) {

            if (desiredDir.y < 0.0f)
                coverArrows.ShowBackArrow();
            else
                coverArrows.HideArrows();
        }

        else if (desiredDir.x > 0.0f) {

            if (desiredDir.y > 0.0f)
                coverArrows.ShowRightForwardArrow();
            else
                coverArrows.ShowRightArrow();
        }

        else if (desiredDir.x < 0.0f) {
            if (desiredDir.y > 0.0f)
                coverArrows.ShowLeftForwardArrow();
            else
                coverArrows.ShowLeftArrow();
        }
    }

    void UpdateMonsterVision() {

        // Can't toggle off right now because when kid is not in view, there's no indicator that monster vision is on. Need an open / closed eye icon.
        //if (Input.GetButtonDown(controls.sprint))
        //    monsterVision = !monsterVision;

        magnifier.Show(canMove && monsterVision);
    }

    void UpdateLeanAnim() {

        if (coverPoint.coverType == CoverPoint.CoverType.CORNER) {
            return;
        }

        animator.SetBool("Lean Left", false);
        animator.SetBool("Lean Right", false);

        // Lean left.
        if (desiredDir.x < 0.0f) {
            if (state != State.SLIDE_LEFT && state != State.SLIDE_RIGHT)
                animator.SetBool("Lean Right", true);

        }

        // Lean right.
        else if (desiredDir.x > 0.0f) {
            if (state != State.SLIDE_LEFT && state != State.SLIDE_RIGHT)
                animator.SetBool("Lean Left", true);


        }
    }

    // Leaning left / right away from an "anchor" cover point.
    void UpdateLean() {

        UpdateLeanAnim();

        // Assume we want to be at the cover point.
        targetPosition = coverPoint.transform.position;

        // There is no lean control at corners.
        if (coverPoint.coverType == CoverPoint.CoverType.CORNER) {
            camera.SetLeanDirection(KaijuCamera.LeanDirection.NONE);
            targetLeanLength = 0.0f;
            return;
        }

        // Lean left.
        if (desiredDir.x < 0.0f) {
            targetPosition = leftLeanPosition;
            targetLeanLength = LEAN_LENGTH;
            camera.SetLeanDirection(KaijuCamera.LeanDirection.LEFT);
        }

        // Lean right.
        else if (desiredDir.x > 0.0f) {
            targetPosition = rightLeanPosition;
            targetLeanLength = LEAN_LENGTH;
            camera.SetLeanDirection(KaijuCamera.LeanDirection.RIGHT);
        }

        else {
            camera.SetLeanDirection(KaijuCamera.LeanDirection.NONE);
            targetLeanLength = 0.0f;
        }
    }

    // Sliding left / right away from the current transform toward a corner cover point, or "lean" target (from UpdateLean).
    void UpdateSlide() {

        // Position interpolation.
        Vector3 newPosition = Vector3.SmoothDamp(transform.position, targetPosition, ref slideVelocity, transitionTime);

        // Angle interpolation.
        Vector3 startAngles = oldCoverPoint.transform.rotation.eulerAngles;
        startAngles = new Vector3(startAngles.x, startAngles.y + oldCoverPoint.headingOffset + 180, startAngles.z);

        Vector3 targetAngles = coverPoint.transform.rotation.eulerAngles;
        targetAngles = new Vector3(targetAngles.x, targetAngles.y + coverPoint.headingOffset + 180, targetAngles.z);

        targetRotation = Quaternion.Slerp(Quaternion.Euler(startAngles), Quaternion.Euler(targetAngles), transitionTime / transitionLength);
    }

    // Move the top level game object.
    void Move(Vector3 newPosition, Quaternion newRotation) {

        transform.position = newPosition;
        transform.rotation = newRotation;
    }

    // Moving the root is just mesh manipulation to give the appearance of motion.
    void MoveRoot() {

        float y = root.transform.position.y;

        root.transform.position = Vector3.Lerp(transform.position, coverOffset.position, 1.0f);

        // Fake jumping and gravity. Applies to root, not full object. No collision. Just visual flare / feedback absent animations.
        Vector3 curPos = root.transform.position;
        curPos.y = y;

        curPos.y += yVelocity * Time.deltaTime;
        if (curPos.y < transform.position.y && yVelocity < 0.0f) {

            if (state == State.TAUNT) {

                StopTaunt();
            }

            curPos.y = transform.position.y;
            yVelocity = 0.0f;
        }

        root.transform.position = curPos;

        yVelocity -= gravity * Time.deltaTime;
    }

    // Update the transition time and change state to nextState if complete then return true.
    bool UpdateTransitionTime(State nextState) {

        transitionTime += Time.deltaTime;
        transitionTime = Mathf.Clamp(transitionTime, 0.0f, transitionLength);

        if (transitionTime == transitionLength) {
            state = nextState;
            return true;
        }

        return false;
    }

    void CalculateLeanPoints() {

        // Look up the left and right cover points relative to this one for leaning.
        CoverPoint leftCoverPoint = coverManager.FindLeftCover(coverPoint, false);
        leftLeanPositionValid = true;

        // Calculate left lean position relative to left cover point.
        Vector3 toLeft = leftCoverPoint.transform.position - coverPoint.transform.position;

        leftLeanPosition = coverPoint.transform.position + (toLeft * 0.1f);
        rightLeanPositionValid = true;

        // Calculate right lean position relative to right cover point.
        CoverPoint rightCoverPoint = coverManager.FindRightCover(coverPoint, false);
        Vector3 toRight = rightCoverPoint.transform.position - coverPoint.transform.position;

        rightLeanPosition = coverPoint.transform.position + (toRight * 0.1f);

        //camera.SetLeanPositions(leftCoverPoint.transform.position - coverPoint.transform.position, rightCoverPoint.transform.position - coverPoint.transform.position);
        
        coverArrows.SetRightArrowTransforms(rightCoverPoint.transform.position, coverPoint.transform.rotation * Quaternion.Euler(0, 180.0f, 0));
        coverArrows.SetLeftArrowTransforms(leftCoverPoint.transform.position, coverPoint.transform.rotation * Quaternion.Euler(0, 180.0f, 0));
        coverArrows.SetBackArrowTransforms(coverPoint.transform.position, coverPoint.transform.rotation * Quaternion.Euler(0, -90.0f, 0));

    }

    void WarpToRandomCoverPoint() {

        CoverPoint searchResult;

        // Search for random cover points until we find one that is not visible from the tricycle start location.
        while (true) {

            searchResult = coverManager.GetRandomCoverPoint();

            if (searchResult.hiddenFromStart)
                break;

            //Vector3 startPos = /*kidCamera.transform.position*/ new Vector3(72.0f, 10.0f, 28.0f);// + (Vector3.up * 10.0f);
            //Vector3 endPos   = searchResult.transform.position + (Vector3.up * 10.0f);

            //RaycastHit hit;
            //Vector3 dirTmp = endPos - startPos;
            
            // Don't collide with characters.
           // LayerMask characterLM = LayerMask.NameToLayer("Character");

            //if (Physics.Raycast(startPos, dirTmp, out hit, dirTmp.magnitude)) {//, ~(1 << characterLM))) {
            //    break;
            //}
        }

        coverPoint = searchResult;
        oldCoverPoint = coverPoint;

        WarpToCoverPoint(coverPoint);
    }

    void WarpToCoverPoint(CoverPoint newCoverPoint) {

        oldCoverPoint = coverPoint;
        coverPoint = newCoverPoint;
      
        Vector3 newPosition = coverPoint.transform.position;
        Quaternion newRotation = coverPoint.transform.rotation;

        // Back to cover for now.
        Move(coverPoint.transform.position, coverPoint.transform.rotation * Quaternion.Euler(0, 180.0f, 0));

        CalculateLeanPoints();
    }

    bool MoveToCoverLeft(bool far) {

        CoverPoint candidateCoverPoint = coverManager.FindLeftCover(coverPoint, far);
        if (candidateCoverPoint == null)
            return false;

        oldCoverPoint = coverPoint;
        coverPoint = candidateCoverPoint;
        CalculateLeanPoints();
        
        StartSlide(far, far ? State.SLIDE_LEFT : State.SLIDE_NEAR);

        return true;
    }

    bool MoveToCoverRight(bool far) {

        CoverPoint candidateCoverPoint = coverManager.FindRightCover(coverPoint, far);
        if (candidateCoverPoint == null)
            return false;

        oldCoverPoint = coverPoint;
        coverPoint = candidateCoverPoint;
        CalculateLeanPoints();
        
        StartSlide(far, far ? State.SLIDE_RIGHT : State.SLIDE_NEAR);
        return true;
    }

    bool MoveToCoverBack() {

        CoverPoint candidateCoverPoint = coverManager.FindBackCover(coverPoint);
        if (candidateCoverPoint == null)
            return false;

        oldCoverPoint = coverPoint;
        coverPoint = candidateCoverPoint;
        CalculateLeanPoints();
        
        StartSlide(true, State.SLIDE_BACK);
        return true;
    }

    void StartTaunt() {

        state = State.TAUNT;

        // Height is built in to this animation.
        yVelocity = 2.75f;

        animator.SetTrigger("Taunt");
        animator.SetBool("Stand", true);

        transitionTime = 0.0f;
        transitionLength = 0.0f;
    }

    void StopTaunt() {

        state = State.STAND;

        // Tell the helicopter.
        chopper.SetPointOfInterest(coverPoint.transform);

        kidCameraShake.shakeDuration = 1.0f;
        camera.GetComponent<CameraShake>().shakeDuration = 1.0f;
    }
    
    void StartSlide(bool far, State newState) {

        state = newState;

        transitionTime = 0.0f;

        if (far) {
            transitionLength = SLIDE_LENGTH;// JUMP_LENGTH;
        }
        else {
            transitionLength = SLIDE_LENGTH;
        }

        animator.SetBool("Lean Left", false);
        animator.SetBool("Lean Right", false);
        animator.SetBool("Stand", false);

        if (far) {
            // Only use the slide animations for far slides.
            // Leans will work for near slides.
            if (state == State.SLIDE_LEFT) {
                animator.SetTrigger("Slide Right");
            }
            else if (state == State.SLIDE_RIGHT) {
                animator.SetTrigger("Slide Left");
            }
            else if (state == State.SLIDE_BACK) {
                yVelocity = 2.75f;// jumpSpeed;
                animator.SetTrigger("Jump");
            }
        }
    }

    void UpdateCharge() {

        if (state == State.STAND && desiredDir.x == 0.0f && desiredDir.y > 0.0f) {

            // Start?
            if (Input.GetButtonDown(controls.vertical)) {
                PowerSource powerSource = powerSourceManager.FindNearbyPowerSource(transform.position);

                if (powerSource == null)
                    return;

                if (charge.StartCharge(powerSource))
                    state = State.CHARGE;
            }
        }
        else if (state == State.CHARGE) {

            if (charge.Complete()) {
                charge.StopCharge();
                state = State.STAND;
            }
        }
    }

    // You can change cover once fully in cover so process up events.
    void UpdateCoverChange() {

        if (!canMove) {
            return;
        }

        // Press forward means we want near cover, otherwise it's far.
        bool wantFarCover = desiredDir.y <= 0.0f;
        
        // Press jump to change cover
        if (Input.GetButtonDown(controls.jump)) {
            
            //checks if it should stop sfx posted by the player holding down space bar
            if(!PlayChargeSound) {

                as_Kaiju.Stop();
                PlayChargeSound = true;              
            }

            if (state != State.STAND) {
                return;
            }

            bool foundCover = false;

            // Transfer desired direction to move direction and take action on it.
            // moveDir can get cleared out after a move, desiredDir is maintained to indicate input based intention.
            moveDir = desiredDir;

            // If there's not enough stamina.
            float staminaRemaining = stamina.GetValue();
            if (wantFarCover && staminaRemaining < FAR_SLIDE_STAMINA) {
                return;
            }
            else if (!wantFarCover && staminaRemaining < NEAR_SLIDE_STAMINA) {
                return;
            }

            if (moveDir.x != 0.0f) {

                // Going to try to move to other cover.

                if (wantFarCover) {

                    if (moveDir.x < 0.0f)
                        foundCover = MoveToCoverLeft(true);

                    else if (moveDir.x > 0.0f)
                        foundCover = MoveToCoverRight(true);
                }

                if (foundCover) {
                    // Found far cover.
                    stamina.Use(FAR_SLIDE_STAMINA);
                    targetSlideLength = FAR_SLIDE_LENGTH;
                }
                else {
                    // Look for near cover.
                    if (moveDir.x < 0.0f)
                        foundCover = MoveToCoverLeft(false);

                    else if (moveDir.x > 0.0f)
                        foundCover = MoveToCoverRight(false);

                    if (foundCover) {

                        stamina.Use(NEAR_SLIDE_STAMINA);
                        targetSlideLength = NEAR_SLIDE_LENGTH;
                    }
                }
            }
            else if (moveDir.y < 0.0f) {

                if (wantFarCover) {

                    if (MoveToCoverBack()) {
                        // Found far cover, jump.
                        stamina.Use(FAR_SLIDE_STAMINA);
                        targetSlideLength = FAR_SLIDE_LENGTH;
                    }
                }
            }
            
            // We reset this now, or after we slide so we can use it to slide past corner cover.
            if (!foundCover)
                moveDir.x = 0.0f;
        }
    }

    public void SetPlayerControls(bool canMove, PlayerControls newControls) {
        this.canMove = canMove;

        stamina.ForceHide(false);

        if (newControls != null)
            controls = newControls;
    }

    public bool GetCanMove() {
        return canMove;
    }

    void UpdateAnimations() {

    }

    void playRandomSound(AudioClip[] clips, AudioMixer mix, AudioSource as_Source, float volume)
    {
        int rand = Random.Range(0,clips.Length);
        as_Source.PlayOneShot(clips[rand], volume); 
        as_Source.outputAudioMixerGroup = mix.FindMatchingGroups("SFX_Kaiju")[0];
        
    }
}
