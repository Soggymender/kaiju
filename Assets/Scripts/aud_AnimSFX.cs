using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class aud_AnimSFX : StateMachineBehaviour
{
    public AudioMixer Mixer;
    //public GameObject kaiju;
    public AudioSource Source;
    public bool PlayEnterSFX;
    public bool PlayExitSFX;
    public AudioClip[] ac_EnterClips;
    public AudioClip[] ac_ExitClips;
    public int rand;
    public float volume;
    float Pitch;
    
    void Awake()
    {
    }

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    { 
            if(PlayEnterSFX)
            {
            Source = animator.GetComponent<AudioSource>();
            rand = Random.Range(0,ac_EnterClips.Length);
            Source.PlayOneShot(ac_EnterClips[rand], 1);
            Pitch = Random.Range(.75f, 1.15f);
            Source.pitch = Pitch;
            Source.spatialBlend = .5f;
            Source.outputAudioMixerGroup = Mixer.FindMatchingGroups("SFX_Kaiju")[0];                
            }


    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(PlayExitSFX)
        {
            Source = animator.GetComponent<AudioSource>();
            rand = Random.Range(0,ac_ExitClips.Length);
            Source.PlayOneShot(ac_ExitClips[rand], 1);
            Source.outputAudioMixerGroup = Mixer.FindMatchingGroups("SFX_Kaiju")[0];
        }
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
