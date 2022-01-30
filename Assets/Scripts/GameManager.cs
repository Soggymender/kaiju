using System;
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

    public PlayerControls leftPlayerControls;
    public PlayerControls rightPlayerControls;

    bool player1IsKid = true;

    public ScreenManager screenManager;
    public AudioListener audioListener;

    GameState gameState = GameState.NONE;
    GameState oldGameState = GameState.NONE;

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

        UpdatePlayerControlsAndDisplay();
    }

    void UpdatePlayerControlsAndDisplay()
    {
        bool change = false;

        bool kidHasControl = false;
        bool kaijuHasControl = false;

        bool splitScreen = screenManager.GetSplitScreen();
        
        // Swap avatars or merge split screens.
        if (Input.GetKeyDown("1")) {

            if (screenManager.GetSplitScreen()) {

                change = true;
                splitScreen = false;
            }
            else {

                change = true;
                splitScreen = false;

                player1IsKid = !player1IsKid;
            }

            if (player1IsKid) {
                kidHasControl = true;
                kaijuHasControl = false;
            }
            else {
                kidHasControl = false;
                kaijuHasControl = true;
            }
        }

        // Split screens, or swap sides.
        if (Input.GetKeyDown("2")) {
            if (screenManager.GetSplitScreen()) {

                // Swap
                change = true;
                splitScreen = true;

                player1IsKid = !player1IsKid;
            }
            else {
                // Split
                change = true;
                splitScreen = true;
            }

            kidHasControl = true;
            kaijuHasControl = true;
        }

        // Game state changes.
        if (gameState != oldGameState) {

            change = true;

            if (gameState == GameState.PLAY) {

                if (splitScreen) {
                    kidHasControl = true;
                    kaijuHasControl = true;
                }
                else {
                    if (player1IsKid)
                        kidHasControl = true;
                    else
                        kaijuHasControl = true;
                }
            }
            else {
                kidHasControl = false;
                kaijuHasControl = false;

            }
        }
        
        if (change) {

            // Figure out left and right camera.
            Camera lCamera;
            Camera rCamera;

            if (player1IsKid) {
                lCamera = kidCam;
                rCamera = kaijuCam;
            }
            else {
                lCamera = kaijuCam;
                rCamera = kidCam;
            }

            screenManager.Split(splitScreen, lCamera, rCamera);

            if (player1IsKid) {
                kid.SetPlayerControls(kidHasControl, leftPlayerControls);
                kaiju.SetPlayerControls(kaijuHasControl, rightPlayerControls);

                // Audio Listener always goes to player 1. Works in single or split screen.
                ParentAudioListener(kidCam.transform);
            } else {
                kaiju.SetPlayerControls(kaijuHasControl, leftPlayerControls);
                kid.SetPlayerControls(kidHasControl, rightPlayerControls);

                // Audio Listener always goes to player 1. Works in single or split screen.
                ParentAudioListener(kaijuCam.transform);
            }
        }



        /*
        // Merge screens, or swap avatars.
        if (Input.GetKeyDown("1")) {
            
            if (screenManager.GetSplitScreen()) {

                // Merge.
                if (player1IsKid) {
                    screenManager.Split(false, kidCam, kaijuCam);
                    kid.SetPlayerControls(kidHasControl, leftPlayerControls);
                    kaiju.SetPlayerControls(kaijuHasControl, rightPlayerControls);

                    ParentAudioListener(kidCam.transform);
                }
                else {
                    screenManager.Split(false, kaijuCam, kidCam);
                    kaiju.SetPlayerControls(kaijuHasControl, leftPlayerControls);
                    kid.SetPlayerControls(kidHasControl, rightPlayerControls);

                    ParentAudioListener(kaijuCam.transform);
                }
            }
            else {

                // Swap.
                if (player1IsKid) {
                    screenManager.Split(false, kaijuCam, kidCam);
                    kaiju.SetPlayerControls(kaijuHasControl, leftPlayerControls);
                    kid.SetPlayerControls(kidHasControl, leftPlayerControls);

                    ParentAudioListener(kaijuCam.transform);
                }
                else {
                    screenManager.Split(false, kidCam, kaijuCam);
                    kid.SetPlayerControls(kidHasControl, leftPlayerControls);
                    kaiju.SetPlayerControls(kaijuHasControl, rightPlayerControls);

                    ParentAudioListener(kidCam.transform);
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
                    kaiju.SetPlayerControls(kaijuHasControl, leftPlayerControls);
                    kid.SetPlayerControls(kidHasControl, rightPlayerControls);
                }
                else {
                    player1IsKid = true;
                    screenManager.Split(true, kidCam, kaijuCam);
                    kid.SetPlayerControls(kidHasControl, leftPlayerControls);
                    kaiju.SetPlayerControls(kaijuHasControl, rightPlayerControls);
                }
            }
            else {
                // Split

                if (player1IsKid) {
                    screenManager.Split(true, kidCam, kaijuCam);
                    kid.SetPlayerControls(kidHasControl, leftPlayerControls);
                    kaiju.SetPlayerControls(kaijuHasControl, rightPlayerControls);
                }
                else {
                    screenManager.Split(true, kaijuCam, kidCam);
                    kaiju.SetPlayerControls(kaijuHasControl, leftPlayerControls);
                    kid.SetPlayerControls(kidHasControl, rightPlayerControls);
                }
            }
        }
        */
    }

    void ParentAudioListener(Transform parent) {

        // attach audio listner to camera
        audioListener.gameObject.transform.parent = parent;

        // align audio listener to camera (do after parent)
        audioListener.gameObject.transform.localPosition = Vector3.zero;
        audioListener.gameObject.transform.localRotation = Quaternion.identity;
    }

    // Game state sequencer.
    void UpdateGameState() {

        oldGameState = gameState;

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

    }

    void EndRound() {
        roundUI.SetActive(false);
    }

}
