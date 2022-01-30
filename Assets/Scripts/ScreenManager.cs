using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour
{
    public Camera kidCam;
    public Camera kaijuCam;

    public GameObject splitBorder;

    bool splitScreen = false;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void Split(bool split, Camera p1Cam, Camera p2Cam)
    {
        if (split) {

            p1Cam.enabled = true;
            p1Cam.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);

            p2Cam.enabled = true;
            p2Cam.rect = new Rect(0.5f, 0.0f, 1.0f, 1.0f);
        }
        else {

            p1Cam.enabled = true;
            p1Cam.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);

            p2Cam.enabled = false;
            p2Cam.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
        }

        splitScreen = split;
        splitBorder.SetActive(split);
    }

    public bool GetSplitScreen() {
        return splitScreen;
    }
}
