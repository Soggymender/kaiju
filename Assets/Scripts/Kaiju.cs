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

        STAND_TO_LEAN,
        LEAN_TO_STAND
    }
    
    bool canMove = false;

    float yVelocity = 0.0f;
    float jumpSpeed = 6.5f;
    float gravity = 9.6f;

    CoverPoint coverPoint = null;
    CoverPoint targetCoverPoint = null;
    public Transform coverOffset = null;
    public GameObject root = null;

    const float STAND_TO_LEAN_LENGTH = 0.5f;
    const float LEAN_TO_STAND_LENGTH = 0.5f;
    const float SLIDE_LENGTH = 1.0f;
    float transitionTime = 0.0f;
    float transitionLength = 0.0f;

    bool queuedAction = false;

    Vector2 moveDir;
    float moveVal = 0.0f;
    float moveTimer = 0.0f;
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
        // Update Stand
        if (state == State.STAND) {

            if (Input.GetMouseButtonDown(1) || queuedAction) {
                StartStandToLean();
                queuedAction = false;
            }

            if (canMove) {

                UpdateCoverPointHold();
                UpdateCoverPointRelease();
            }
        }

        // Update Lean
        else if (state == State.LEAN) {

            if (Input.GetMouseButtonDown(1) || queuedAction) {
                StartLeanToStand();
                queuedAction = false;
            }

        }

        // Update slide
        else if (state == State.SLIDE) {

            Vector3 startAngles = coverPoint.transform.rotation.eulerAngles;
            startAngles = new Vector3(startAngles.x, startAngles.y + coverPoint.headingOffset + 180, startAngles.z);

            Vector3 targetAngles = targetCoverPoint.transform.rotation.eulerAngles;
            targetAngles = new Vector3(targetAngles.x, targetAngles.y + targetCoverPoint.headingOffset + 180, targetAngles.z);
     
            transform.position = Vector3.Lerp(coverPoint.transform.position, targetCoverPoint.transform.position, transitionTime / transitionLength);
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(startAngles), Quaternion.Euler(targetAngles), transitionTime / transitionLength);

            UpdateCoverPointHold();

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

            if (transitionTime >= transitionLength * 0.5f && Input.GetMouseButtonDown(1)) {
                queuedAction = true;
            }

            root.transform.position = Vector3.Lerp(transform.position, coverOffset.position, transitionTime / transitionLength);

            UpdateTransitionTime(State.LEAN);
        }

        // Update Lean to Stand
        else if (state == State.LEAN_TO_STAND) {

            if (transitionTime >= transitionLength * 0.5f && Input.GetMouseButtonDown(1)) {
                queuedAction = true;
            }

            root.transform.position = Vector3.Lerp(coverOffset.position, transform.position, transitionTime / transitionLength);

            UpdateTransitionTime(State.STAND);
        }

        // Fake jumping and gravity. Applies to root, not full object. No collision. Just visual flare / feedback absent animations.
        Vector3 curPos = root.transform.position;

        curPos.y += yVelocity * Time.deltaTime;
        if (curPos.y < transform.position.y && yVelocity < 0.0f) {
            curPos.y = transform.position.y;
            yVelocity = 0.0f;
        }

        root.transform.position = curPos;

        yVelocity -= gravity * Time.deltaTime;



        // Apply any active scale lerp.
        if (scaleLength != 0) {

            float yScale = Mathf.Lerp(scaleStart, scaleTarget, scaleTime / scaleLength);

            root.transform.localScale = new Vector3(1.0f, yScale, 1.0f);//.Set(1.0f, 2.0f, 1.0f);

  //          float yScale = scaleTarget * scaleTime / scaleLength;
    //        root.transform.localScale.Set(1.0f, yScale, 1.0f);
            
        }
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

    public void SetCanMove(bool canMove) {
        this.canMove = canMove;
    }

    void WarpToCoverPoint(CoverPoint newCoverPoint) {

        coverPoint = newCoverPoint;

        transform.position = coverPoint.transform.position;
        transform.rotation = coverPoint.transform.rotation;

        // Back to cover for now.
        transform.Rotate(new Vector3(0, 180.0f, 0));
    }

    bool MoveToCoverLeft(bool far) {

        targetCoverPoint = coverManager.FindLeftCover(coverPoint, far);
        if (targetCoverPoint == null)
            return false;

        StartSlide();

        return true;
    }

    bool MoveToCoverRight(bool far) {
        targetCoverPoint = coverManager.FindRightCover(coverPoint, far);
        if (targetCoverPoint == null)
            return false;

        StartSlide();
        return true;
    }

    bool MoveToCoverBack() {

        targetCoverPoint = coverManager.FindBackCover(coverPoint);
        if (targetCoverPoint == null)
            return false;

        StartSlide();
        return true;
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

    void StartSlide() {

        state = State.SLIDE;

        transitionTime = 0.0f;
        transitionLength = SLIDE_LENGTH;
    }

    // You can start "queing up" a cover change while transitioning cover, so process down and hold events.
    void UpdateCoverPointHold() {

        // If move is pressed, start a timer.
        if (moveDir.magnitude == 0.0f) {
            if (Input.GetButtonDown("Horizontal")) {
                
                // Start press/hold for clockwise / counter clockwise close cover change or far lateral change.
                moveTimer = 0.0f;
                moveDir.x = Input.GetAxis("Horizontal");
                moveDir.y = 0.0f;

                scaleTime = 0.0f;
                scaleLength = JUMP_SCALE_LENGTH;
                scaleStart = root.transform.localScale.y;
                scaleTarget = JUMP_SCALE;
            }
            else if (Input.GetButtonDown("Vertical")) {

                // Start press/hold for far forward cover change.
                moveTimer = 0.0f;
                moveDir.x = 0.0f;
                moveDir.y = Input.GetAxis("Vertical");

                scaleTime = 0.0f;
                scaleLength = JUMP_SCALE_LENGTH;
                scaleStart = root.transform.localScale.y;
                scaleTarget = JUMP_SCALE;
            }
        }

        if (Input.GetButton("Horizontal") || Input.GetButton("Vertical")) {
            moveTimer += Time.deltaTime;

            scaleTime += Time.deltaTime;
            if (scaleTime >= scaleLength)
                scaleTime = scaleLength;
        }
        else {
          //  if (scaleLength > 0) {

                /*
                scaleTime -= Time.deltaTime;
                if (scaleTime < 0) {
                    scaleTime = 0.0f;
                    scaleLength = 0.0f;
                    scaleStart = 1.0f;
                    scaleTarget = 1.0f;
                    root.transform.localScale = new Vector3(1, 1, 1);
                }
                */
            //}
        }
    }

    // You can change cover once fully in cover so process up events.
    void UpdateCoverPointRelease() {
        
        // Released horizontal AND we were tracking horizontal (user may have press vertical then horizontal).
        if (Input.GetButtonUp("Horizontal") && moveDir.x != 0.0f) {

            bool foundCover = false;
            if (moveTimer >= MOVE_FAR_LENGTH) {

                if (moveDir.x < 0.0f)
                    foundCover = MoveToCoverLeft(true);
                
                else if (moveDir.x > 0.0f)
                    foundCover = MoveToCoverRight(true);
            }

            if (!foundCover) {

                if (moveDir.x < 0.0f)
                    foundCover = MoveToCoverLeft(false);

                else if (moveDir.x > 0.0f)
                    foundCover = MoveToCoverRight(false);
            }

            moveTimer = 0.0f;

            // scaleTimer = 0.0f; don't reset, we want to reverse.

            scaleLength = 0.0f;// scaleTime * 0.5f;
            scaleTime = 0.0f;
            //scaleStart = root.transform.localScale.y;
            //scaleTarget = 1.0f;
            root.transform.localScale = new Vector3(1, 1, 1);

            yVelocity = jumpSpeed;

            // We reset this now, or after we slide so we can use it to slide past corner cover.
            if (!foundCover)
                moveDir.x = 0.0f;
        }

        if (Input.GetButtonUp("Vertical") && moveDir.y != 0.0f) {

            // There is no close forward cover, only far, so we only need to check far without fall back to close as with horizontal above.

            if (moveTimer >= MOVE_FAR_LENGTH) {
                
                if (moveDir.y < 0.0f)
                    MoveToCoverBack();
            }

            moveTimer = 0.0f;
            moveDir.y = 0.0f;

            // scaleTimer = 0.0f; don't reset, we want to reverse.
            scaleLength = 0.0f;// scaleTime * 0.5f;
            scaleTime = 0.0f;
            //scaleStart = root.transform.localScale.y;
            //scaleTarget = 1.0f;
            root.transform.localScale = new Vector3(1, 1, 1);
        }
    }
}
