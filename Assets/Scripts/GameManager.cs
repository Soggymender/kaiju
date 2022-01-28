using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Camera kidCam;
    public Tricycle kid;

    public Camera kaijuCam;
    public Kaiju kaiju;

    public AudioListener audioListener;

    bool oldSwap = false;

    void Update() {
        //Press the space bar to apply no locking to the Cursor
        if (Input.GetKey(KeyCode.Space))
            Cursor.lockState = CursorLockMode.None;

        if (Input.GetAxis("Fire1") != 0.0f) {

            if (oldSwap == false)
                DebugSwapAvatars();

            oldSwap = true;
        }
        else {
            oldSwap = false;
        }
    }

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Debug swap avatars for testing both quickly in the same session.
    void DebugSwapAvatars() {

        if (kidCam.enabled) {

            // Switch to Kaiju control.
            kidCam.enabled = false;
            kid.SetCanMove(false);

            kaijuCam.enabled = true;
            kaiju.SetCanMove(true);

            ParentAudioListener(kaijuCam.transform);
        }
        else if (kaijuCam.enabled) {
            // Switch to Kid control.
            kidCam.enabled = true;
            kid.SetCanMove(true);

            kaijuCam.enabled = false;
            kaiju.SetCanMove(false);

            ParentAudioListener(kidCam.transform);
        }
    }

    void ParentAudioListener(Transform parent) {

        // attach audio listner to camera
        audioListener.gameObject.transform.parent = parent;

        // align audio listener to camera (do after parent)
        audioListener.gameObject.transform.localPosition = Vector3.zero;
        audioListener.gameObject.transform.localRotation = Quaternion.identity;
    }
}
