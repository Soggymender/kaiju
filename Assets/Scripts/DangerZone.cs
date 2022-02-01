using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class DangerZone : MonoBehaviour
{
    public AudioMixer mixer;
    AudioSource as_RevealWarning;
    public AudioClip ac_WarningSound;
    

    public float revealDistance;
    public float winDistance;

    public Tricycle tricycle;
    public GameObject mesh;

    bool win = false;



    void Start()
    {
        ac_WarningSound = Resources.Load("sfx_gameplay_warning_lvl1_loop 1") as AudioClip;
        
    }

    void Update()
    {
        // If the round is over, terminate this behavior.
        if (win)
            return;

        float dist = (transform.position - tricycle.transform.position).magnitude;

        if (mesh.activeSelf) {
            if (dist > revealDistance) {
                mesh.SetActive(false);
                StopWarningSound();
            }

            else if (dist <= winDistance) {
                //mesh.SetActive(false);
                win = true;
                StopWarningSound();     
            }
        }
        else {
            if (dist <= revealDistance) {

                mesh.SetActive(true);
                PlayWarningSound(ac_WarningSound, mixer);
            }
        }
    }

    public bool GetWinStatus() {
        return win;
    }


    void PlayWarningSound(AudioClip clip, AudioMixer mix)
    {
        if(as_RevealWarning == null)
        {
            
            as_RevealWarning = gameObject.AddComponent<AudioSource>();
            as_RevealWarning.clip = clip;
            
            as_RevealWarning.loop = true;
            as_RevealWarning.Play();
            as_RevealWarning.outputAudioMixerGroup = mix.FindMatchingGroups("SFX_Gameplay")[0];  
        }
    }
    
    void StopWarningSound()
    {
        if(as_RevealWarning != null)
        {
            as_RevealWarning.Stop(); 
            Destroy(as_RevealWarning);
        }
    }

}
