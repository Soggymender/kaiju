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
    CoverPoint coverPoint = null;
    CoverPoint targetCoverPoint = null;
    public Transform coverOffset = null;
    public GameObject mesh = null;

    const float STAND_TO_LEAN_LENGTH = 0.5f;
    const float LEAN_TO_STAND_LENGTH = 0.5f;
    const float SLIDE_LENGTH = 1.0f;
    float transitionTime = 0.0f;
    float transitionLength = 0.0f;

    bool queuedAction = false;

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

            if (Input.GetAxis("Horizontal") < 0.0f) {
                MoveToCoverLeft();
            }
            else if (Input.GetAxis("Horizontal") > 0.0f) {
                MoveToCoverRight();
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
            startAngles = new Vector3(startAngles.x, startAngles.y + 180, startAngles.z);

            Vector3 targetAngles = targetCoverPoint.transform.rotation.eulerAngles;
            targetAngles = new Vector3(targetAngles.x, targetAngles.y + 180, targetAngles.z);
     
            transform.position = Vector3.Lerp(coverPoint.transform.position, targetCoverPoint.transform.position, transitionTime / transitionLength);
            transform.rotation = Quaternion.Slerp(Quaternion.Euler(startAngles), Quaternion.Euler(targetAngles), transitionTime / transitionLength);

            if (UpdateTransitionTime(State.STAND)) {
                coverPoint = targetCoverPoint;
                targetCoverPoint = null;

                // If this is a corner, keep going.
            }
        }

        // Update Stand to Lean
        else if (state == State.STAND_TO_LEAN) {

            if (transitionTime >= transitionLength * 0.5f && Input.GetMouseButtonDown(1)) {
                queuedAction = true;
            }

            mesh.transform.position = Vector3.Lerp(transform.position, coverOffset.position, transitionTime / transitionLength);

            UpdateTransitionTime(State.LEAN);
        }

        // Update Lean to Stand
        else if (state == State.LEAN_TO_STAND) {

            if (transitionTime >= transitionLength * 0.5f && Input.GetMouseButtonDown(1)) {
                queuedAction = true;
            }

            mesh.transform.position = Vector3.Lerp(coverOffset.position, transform.position, transitionTime / transitionLength);

            UpdateTransitionTime(State.STAND);
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

    void MoveToCoverLeft() {

        targetCoverPoint = coverManager.FindNearLeftCover(coverPoint);
        if (targetCoverPoint == null)
            return;

        StartSlide();
    }

    void MoveToCoverRight() {
        targetCoverPoint = coverManager.FindNearRightCover(coverPoint);
        if (targetCoverPoint == null)
            return;

        StartSlide();
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
}
