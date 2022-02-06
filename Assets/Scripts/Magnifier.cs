using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Magnifier : MonoBehaviour
{
    public Camera farCam;
    public Camera closeCam;

    public Transform kaijuTrans;
    public Transform kidTrans;

    public GameObject images;

    public RectTransform canvasRect;
    public Canvas canvas;

    bool show = false;
    bool kidVisible = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!show) {
            images.SetActive(false);
            return;
        }

        bool kidVisible = false;

        // Find the position of Kid on the screen.
        Vector3 screenPos = farCam.WorldToScreenPoint(kidTrans.position);

        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < farCam.pixelWidth && screenPos.y > 0 && screenPos.y < farCam.pixelHeight) {
            kidVisible = true;    
        }

        if (kidVisible) {

            // Check line of site.
            screenPos.Set(screenPos.x, screenPos.y, 2.5f);

            Vector3 startPos = farCam.ScreenToWorldPoint(screenPos);
            Vector3 endPos = kidTrans.position;

            RaycastHit hit;
            Vector3 dir = kidTrans.position - startPos;

            // Don't collide with characters.
            //LayerMask characterLM = LayerMask.NameToLayer("Character");

            if (Physics.SphereCast(startPos, 0.5f, dir, out hit, dir.magnitude + 1.0f)) {

                if (hit.transform.gameObject.GetComponent<Tricycle>())
                    kidVisible = true;
                else
                    kidVisible = false;

                // Position and orient the near camera for this viewpoint.
                dir = kidTrans.position - farCam.transform.position;
                dir.Normalize();

                closeCam.transform.position = kidTrans.position + (-dir * 1.75f);
                closeCam.transform.LookAt(kidTrans);


            }
            else {
                kidVisible = false;
            }
        }

        images.SetActive(show && kidVisible);

        screenPos.z = transform.localPosition.z;
        transform.position = screenPos;

    
    }

    public void Show(bool show) {

        this.show = show;
    }
}
