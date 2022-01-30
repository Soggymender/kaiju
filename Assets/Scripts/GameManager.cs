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

    public bool player1IsKid = true;

    public ScreenManager screenManager;
    public AudioListener audioListener;

    GameState gameState = GameState.NONE;

    bool oldSwap = false;

    void Start() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        
    }

    void Update() {

        UpdateGameState();

        // Spacebar to release the cursor - for debugging.
        if (Input.GetKey(KeyCode.Space))
            Cursor.lockState = CursorLockMode.None;

        // Quit.
        if (Input.GetKeyDown("escape")) {
            Application.Quit();
        }

        // Merge screens.
        if (Input.GetKeyDown("1")) {
            if (screenManager.GetSplitScreen()) {
                if (player1IsKid) {
                    screenManager.Split(false, kidCam, kaijuCam);
                }
                else {
                    screenManager.Split(false, kaijuCam, kidCam);
                }
            }
        }

        // Split screens, or swap sides.
        if (Input.GetKeyDown("2")) {
            if (screenManager.GetSplitScreen()) {
                // Swap.
                if (player1IsKid) {
                    player1IsKid = false;
                    screenManager.Split(true, kaijuCam, kidCam);
                }
                else {
                    player1IsKid = true;
                    screenManager.Split(true, kidCam, kaijuCam);
                }
            }
            else {
                // Split
                if (player1IsKid) {
                    screenManager.Split(true, kidCam, kaijuCam);
                }
                else {
                    screenManager.Split(true, kaijuCam, kidCam);
                }
            }
        }
        
        if (gameState == GameState.PLAY) {

            if (!screenManager.GetSplitScreen()) {

                // Swap players - for debugging.
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
    }

    void PlayAsKid() {

        if (!screenManager.GetSplitScreen()) {
            kidCam.enabled = true;
            kaijuCam.enabled = false;
        }

        kid.SetCanMove(true);
        kaiju.SetCanMove(false);

        ParentAudioListener(kidCam.transform);
    }

    void PlayAsKaiju() {

        if (!screenManager.GetSplitScreen()) {
            kidCam.enabled = false;
            kaijuCam.enabled = true;
        }

        kid.SetCanMove(false);
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
