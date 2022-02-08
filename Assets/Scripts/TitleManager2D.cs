using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager2D : MonoBehaviour
{
    enum State {

        FIRST_WAIT,
        PRESS_ANY_KEY,
    }

    State state = State.FIRST_WAIT;

    float waitTime = 0.0f;
    float firstWait = 3.0f;

    public GameObject pressAnyKey;

    bool oldAnyKey = false;


    void Start()
    {
       
    }

    void Update()
    {
        if (state == State.FIRST_WAIT) {

            waitTime += Time.deltaTime;
            if (waitTime >= firstWait) {
                state = State.PRESS_ANY_KEY;
                pressAnyKey.SetActive(true);

                oldAnyKey = true;
            }
        }

        else if (state == State.PRESS_ANY_KEY) {

            if (!Input.anyKey)
                oldAnyKey = false;

            if (Input.anyKey && !oldAnyKey) {

                SceneManager.LoadScene("main", LoadSceneMode.Single);
            }
        }
    }
}
