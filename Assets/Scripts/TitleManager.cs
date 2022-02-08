using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public Kaiju kaiju;
    public Animator kaijuAnimator;

    // Start is called before the first frame update
    void Start()
    {
        kaijuAnimator.SetBool("Lean Right", true);
    }

    // Update is called once per frame
    void Update()
    {
    }
}
