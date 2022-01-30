using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    enum GameState {

        NONE,
        WAIT,
        READY,
        GO,
        PLAY,
        ROUND
    }

    const float WAIT_LENGTH = 1.0f;
    const float READY_LENGTH = 3.0f;
    const float GO_LENGTH = 1.0f;
    const float ROUND_LENGTH = 3.0f;

    public GameObject readyUI;
    public GameObject goUI;
    public GameObject roundUI;

    public DangerZone dangerZone;

    float stateTime = 0.0f;
    float stateLength = 0.0f;

    public Camera kidCam;
    public Tricycle kid;

    public Camera kaijuCam;
    public Kaiju kaiju;

    public AudioListener audioListener;

    GameState gameState = GameState.NONE;

    bool oldSwap = false;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        
    }

    void Update() {

        UpdateGameState();

        //Press the space bar to apply no locking to the Cursor
        if (Input.GetKey(KeyCode.Space))
            Cursor.lockState = CursorLockMode.None;

        if (Input.GetKey("escape")) {
            Application.Quit();
        }

        if (gameState == GameState.PLAY) {
            if (Input.GetAxis("Fire1") != 0.0f) {

                if (oldSwap == false)
                    DebugSwapAvatars();

                oldSwap = true;
            }
            else {
                oldSwap = false;
            }
        }
    }

    void PlayAsKid() {

        kidCam.enabled = true;
        kid.SetCanMove(true);

        kaijuCam.enabled = false;
        kaiju.SetCanMove(false);

        ParentAudioListener(kidCam.transform);
    }

    void PlayAsKaiju() {

        kidCam.enabled = false;
        kid.SetCanMove(false);

        kaijuCam.enabled = true;
        kaiju.SetCanMove(true);

        ParentAudioListener(kaijuCam.transform);
    }

    void ParentAudioListener(Transform parent) {

        // attach audio listner to camera
        audioListener.gameObject.transform.parent = parent;

        // align audio listener to camera (do after parent)
        audioListener.gameObject.transform.localPosition = Vector3.zero;
        audioListener.gameObject.transform.localRotation = Quaternion.identity;
    }

    // Debug swap avatars for testing both quickly in the same session.
    void DebugSwapAvatars() {

        if (kidCam.enabled) {
            PlayAsKaiju();
        }
        else if (kaijuCam.enabled) {
            PlayAsKid();
        }
    }

    // Game state sequencer.
    void UpdateGameState() {

        if (gameState == GameState.NONE) {
            StartWait();
        }

        else if (gameState == GameState.WAIT) {
            if (UpdateStateTime()) {
                EndWait();
                StartReady();
            }
        }

        else if (gameState == GameState.READY) {
            if (UpdateStateTime()) {
                EndReady();
                StartGo();
            }
        }

        else if (gameState == GameState.GO) {
            if (UpdateStateTime()) {
                EndGo();
                StartPlay();
            }
        }

        else if (gameState == GameState.PLAY) {

            // Watch the DangerZone state on Kaiju.
            if (dangerZone.GetWinStatus()) {
                EndPlay();
                StartRound();
            }
        }

        else if (gameState == GameState.ROUND) {
            if (UpdateStateTime()) {
                EndRound();
                SceneManager.LoadScene("Main", LoadSceneMode.Single);
            }
        }
    }

    bool UpdateStateTime() {

        stateTime += Time.deltaTime;
        if (stateTime >= stateLength) {
            stateTime = stateLength;
            return true;
        }

        return false;
    }

    void StartWait() {

        gameState = GameState.WAIT;
        stateTime = 0.0f;
        stateLength = WAIT_LENGTH;
    }

    void StartReady() {

        gameState = GameState.READY;
        stateTime = 0.0f;
        stateLength = READY_LENGTH;

        readyUI.SetActive(true);
    }

    void StartGo() {

        gameState = GameState.GO;
        stateTime = 0.0f;
        stateLength = GO_LENGTH;
        goUI.SetActive(true);
    }

    void StartPlay() {

        gameState = GameState.PLAY;
        stateTime = 0.0f;
        stateLength = 0.0f;

        // TODO: Play as selected avatar.
        PlayAsKid();
    }

    void StartRound() {

        gameState = GameState.ROUND;
        stateTime = 0.0f;
        stateLength = ROUND_LENGTH;

        roundUI.SetActive(true);
    }

    void EndWait() {

    }

    void EndReady() {
        readyUI.SetActive(false);
    }

    void EndGo() {
        goUI.SetActive(false);
    }

    void EndPlay() {

        kid.SetCanMove(false);
        kaiju.SetCanMove(false);
    }

    void EndRound() {
        roundUI.SetActive(false);
    }

}
