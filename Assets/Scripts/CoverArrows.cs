using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoverArrows : MonoBehaviour {
    public GameObject rightArrow;
    public GameObject rightForwardArrow;
    public GameObject leftArrow;
    public GameObject leftForwardArrow;
    public GameObject backArrow;

    void Start() {

    }

    void Update() {

    }

    public void SetRightArrowTransforms(Vector3 position, Quaternion rotation) {

        rightArrow.transform.position = position;
        rightArrow.transform.rotation = rotation;

        rightForwardArrow.transform.position = position;
        rightForwardArrow.transform.rotation = rotation;
    }

    public void SetLeftArrowTransforms(Vector3 position, Quaternion rotation) {

        leftArrow.transform.position = position;
        leftArrow.transform.rotation = rotation;

        leftForwardArrow.transform.position = position;
        leftForwardArrow.transform.rotation = rotation;
    }

    public void SetBackArrowTransforms(Vector3 position, Quaternion rotation) {

        backArrow.transform.position = position;
        backArrow.transform.rotation = rotation;
    }

    public void HideArrows() {

        rightArrow.SetActive(false);
        rightForwardArrow.SetActive(false);
        leftArrow.SetActive(false);
        leftForwardArrow.SetActive(false);
        backArrow.SetActive(false);
    }

    public void ShowRightForwardArrow() {

        HideArrows();
        rightForwardArrow.SetActive(true);
    }

    public void ShowLeftForwardArrow() {

        HideArrows();
        leftForwardArrow.SetActive(true);
    }

    public void ShowRightArrow() {

        HideArrows();
        rightArrow.SetActive(true);

    }

    public void ShowLeftArrow() {

        HideArrows();
        leftArrow.SetActive(true);
    }

    public void ShowBackArrow() {

        HideArrows();
        backArrow.SetActive(true);
    }
}
