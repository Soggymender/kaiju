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
        if (!show)
            return;

        bool kidVisible = false;

        Vector3 screenPos = farCam.WorldToScreenPoint(kidTrans.position);

        if (screenPos.z > 0 && screenPos.x > 0 && screenPos.x < farCam.pixelWidth && screenPos.y > 0 && screenPos.y < farCam.pixelHeight) {
            kidVisible = true;    
        }

        images.SetActive(show && kidVisible);

        /*
        Vector2 result;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas., screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : farCam, out result);
        return canvas.transform.TransformPoint(result);
        */
      //  screenPos.x -= canvasRect.sizeDelta.x * 0.5f;
        //screenPos.y -= canvasRect.sizeDelta.y * 0.5f;
        screenPos.z = transform.localPosition.z;

        transform.position = screenPos;

    
    }

    public void Show(bool show) {

        this.show = show;
    }



    /*
    var hit : RaycastHit;
var rayDirection = player.position - transform.position;
if (Physics.Raycast (transform.position, rayDirection, hit)) {
if (hit.transform == player) {
// enemy can see the player!
} else {
// there is something obstructing the view
}
}
 */ 
}
