using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kaiju : MonoBehaviour
{
    enum State {

        NONE,
        STAND,
        LEAN,
        SLIDE,
        LEAP,
        JUMP,

        STAND_TO_LEAN,
        LEAN_TO_STAND
    }

    public PlayerControls controls;

    bool canMove = false;

    float yVelocity = 0.0f;
    float jumpSpeed = 9.0f;
    float gravity = 9.6f;

    CoverPoint coverPoint = null;
    CoverPoint targetCoverPoint = null;
    public Transform coverOffset = null;
    public GameObject root = null;

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

    // Scale lerp stuff.
    const float JUMP_SCALE_LENGTH = 0.5f;
    const float JUMP_SCALE = 0.95f;
    float scaleTime = 0.0f;
    float scaleLength = 0.0f;
    float scaleStart = 1.0f;
    float scaleTarget = 1.0f;

    //Vector3 targetOffset;

    State state = State.STAND;

    CoverManager coverManager = null;

    // Start is called before the first frame update
    void Start()
    {
        coverManager = FindObjectOfType<CoverManager>();
        if (coverManager == null) {
            throw new System.Exception("Couldn't find CoverManager in scene.");
        }

        coverPoint = coverManager.GetRandomCoverPoint();

        WarpToCoverPoint(coverPoint);
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove) {

            // Keep track of desired direction between moves.
            // Latest direction press is the valid one.
            if (Input.GetButton(controls.horizontal)) {
                desiredDir.x = Input.GetAxis(controls.horizontal);
                desiredDir.y = 0.0f;
            } else if (Input.GetButton(controls.vertical)) {
                desiredDir.x = 0.0f;
                desiredDir.y = Input.GetAxis(controls.vertical);
            }

            // This seems nutty, but on keyboard you can let go of left, and still be holding right, etc.
            if (Input.GetButtonUp(controls.horizontal) && !Input.GetButton(controls.horizontal))
                desiredDir.x = 0.0f;

            if (Input.GetButtonUp(controls.vertical) && !Input.GetButton(controls.vertical))
                desiredDir.y = 0.0f;

            UpdateCoverPointHold();
            UpdateCoverPointRelease();
        }



        // Update Stand
        if (state == State.STAND) {

        }

        // Update Lean
        else if (state == State.LEAN) {

            // Now we are always at the lean offset - close to the building, so we don't have a special state for it.
        }

        // Update slide
        else if (state == State.SLIDE) {

            Vector3 startAngles = coverPoint.transform.rotation.eulerAngles;
            startAngles = new Vector3(startAngles.x, startAngles.y + coverPoint.headingOffset + 180, startAngles.z);

            Vector3 targetAngles = targetCoverPoint.transform.rotation.eulerAngles;
            targetAngles = new Vector3(targetAngles.x, targetAngles.y + targetCoverPoint.headingOffset + 180, targetAngles.z);
     
            Vector3 newPosition = Vector3.Lerp(coverPoint.transform.position, targetCoverPoint.transform.position, transitionTime / transitionLength);
            Quaternion newRotation = Quaternion.Slerp(Quaternion.Euler(startAngles), Quaternion.Euler(targetAngles), transitionTime / transitionLength);

            Move(newPosition, newRotation);

            if (UpdateTransitionTime(State.STAND)) {
                coverPoint = targetCoverPoint;
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

                moveDir.x = 0.0f;
            }
        }

        // Update Stand to Lean
        else if (state == State.STAND_TO_LEAN) {
            
            /* Keeping this in case we decide to step out of cover at the end of a round.
            if (transitionTime >= transitionLength * 0.5f && Input.GetMouseButtonDown(1)) {
                queuedAction = true;
            }

            root.transform.position = Vector3.Lerp(transform.position, coverOffset.position, transitionTime / transitionLength);

            UpdateTransitionTime(State.LEAN);
            */
        }

        // Update Lean to Stand
        else if (state == State.LEAN_TO_STAND) {
            
            /* Keeping this in case we decide to step out of cover at the end of a round.
            if (transitionTime >= transitionLength * 0.5f && Input.GetMouseButtonDown(1)) {
                queuedAction = true;
            }

            root.transform.position = Vector3.Lerp(coverOffset.position, transform.position, transitionTime / transitionLength);

            UpdateTransitionTime(State.STAND);
            */
        }

        MoveRoot();


        // Apply any active scale lerp.
        if (scaleLength != 0) {

            float yScale = Mathf.Lerp(scaleStart, scaleTarget, scaleTime / scaleLength);

            root.transform.localScale = new Vector3(1.0f, yScale, 1.0f);
            
        }
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

            if (state == State.JUMP) {
                state = State.STAND;
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

    void WarpToCoverPoint(CoverPoint newCoverPoint) {

        coverPoint = newCoverPoint;

        Vector3 newPosition = coverPoint.transform.position;
        Quaternion newRotation = coverPoint.transform.rotation;

        // Back to cover for now.
        Move(coverPoint.transform.position, coverPoint.transform.rotation * Quaternion.Euler(0, 180.0f, 0));
    }

    bool MoveToCoverLeft(bool far) {

        targetCoverPoint = coverManager.FindLeftCover(coverPoint, far);
        if (targetCoverPoint == null)
            return false;

        StartSlide(far);

        return true;
    }

    bool MoveToCoverRight(bool far) {
        targetCoverPoint = coverManager.FindRightCover(coverPoint, far);
        if (targetCoverPoint == null)
            return false;

        StartSlide(far);
        return true;
    }

    bool MoveToCoverBack() {

        targetCoverPoint = coverManager.FindBackCover(coverPoint);
        if (targetCoverPoint == null)
            return false;

        StartSlide(true);
        return true;
    }

    void StartJump() {

        state = State.JUMP;

        yVelocity = jumpSpeed;

        transitionTime = 0.0f;
        transitionLength = 0.0f;
    }

    void StartStandToLean() {

        state = State.STAND_TO_LEAN;

        transitionTime = 0.0f;
        transitionLength = STAND_TO_LEAN_LENGTH;
    }

    void StartLeanToStand() {

        state = State.LEAN_TO_STAND;

        transitionTime = 0.0f;
        transitionLength = LEAN_TO_STAND_LENGTH;
    }

    void StartSlide(bool far) {

        state = State.SLIDE;

        transitionTime = 0.0f;

        if (far) {
            transitionLength = JUMP_LENGTH;
        }
        else {
            transitionLength = SLIDE_LENGTH;
        }
    }

    // You can start "queing up" a cover change while transitioning cover, so process down and hold events.
    void UpdateCoverPointHold() {

        if (!canMove)
            return;

        // Press Jump.
        if (Input.GetButtonDown(controls.jump)) {
                
            // Start press/hold for clockwise / counter clockwise close cover change or far lateral change.
            jumpTimer = 0.0f;
            
            scaleTime = 0.0f;
            scaleLength = JUMP_SCALE_LENGTH;
            scaleStart = root.transform.localScale.y;
            scaleTarget = JUMP_SCALE;
        }
        
        // Hold Jump.
        if (Input.GetButton(controls.jump)) {
            jumpTimer += Time.deltaTime;

            scaleTime += Time.deltaTime;
            if (scaleTime >= scaleLength)
                scaleTime = scaleLength;
        }
    }

    // You can change cover once fully in cover so process up events.
    void UpdateCoverPointRelease() {

        if (!canMove)
            return;

        // Release jump
        if (Input.GetButtonUp(controls.jump)) {

            float oldJumpTimer = jumpTimer;

            // Jump scale is the visual feedback response for input.
            EndJumpScale();

            if (state != State.STAND) {
                return;
            }

            bool foundCover = false;

            // Transfer desired direction to move direction and take action on it.
            // moveDir can get cleared out after a move, desiredDir is maintained to indicate input based intention.
            moveDir = desiredDir;

            // If on directional input, don't change cover.
            if (Mathf.Approximately(moveDir.magnitude, 0.0f)) {
                // Jump in place. Taunt, destabalize kid?
                StartJump();
                
            }
            else {

                if (moveDir.x != 0.0f) {

                    // Going to try to move to other cover.

                    if (oldJumpTimer >= MOVE_FAR_LENGTH) {

                        if (moveDir.x < 0.0f)
                            foundCover = MoveToCoverLeft(true);

                        else if (moveDir.x > 0.0f)
                            foundCover = MoveToCoverRight(true);
                    }

                    if (foundCover) {
                        // Found far cover.
                    }
                    else {
                        // Look for near cover.
                        if (moveDir.x < 0.0f)
                            foundCover = MoveToCoverLeft(false);

                        else if (moveDir.x > 0.0f)
                            foundCover = MoveToCoverRight(false);
                    }
                }
                else if (moveDir.y < 0.0f) {

                    if (oldJumpTimer >= MOVE_FAR_LENGTH) {

                        if (MoveToCoverBack()) {
                            // Found far cover, jump.
                        }
                    }
                }
            }
            
            // We reset this now, or after we slide so we can use it to slide past corner cover.
            if (!foundCover)
                moveDir.x = 0.0f;
        }
    }

    void EndJumpScale() {

        jumpTimer = 0.0f;

        scaleLength = 0.0f;
        scaleTime = 0.0f;
        root.transform.localScale = new Vector3(1, 1, 1);
    }

    public void SetPlayerControls(bool canMove, PlayerControls newControls) {
        this.canMove = canMove;

        if (newControls != null)
            controls = newControls;
    }

    public bool GetCanMove() {
        return canMove;
    }
}
