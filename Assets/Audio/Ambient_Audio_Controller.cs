using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Ambient_Audio_Controller : MonoBehaviour
{
    public AudioMixer MixerMaster;
    public AudioSource as_ambienceSource;
    public AudioClip[] AmbLoops;
    public int ambLoop;

    void Awake()
    {
     
    }

    // Start is called before the first frame update
    void Start()
    {   if(as_ambienceSource == null)
    {
        as_ambienceSource = gameObject.AddComponent<AudioSource>();
    }
        playRandomAmbience(AmbLoops, MixerMaster, as_ambienceSource);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void playRandomAmbience(AudioClip[] clips, AudioMixer mix, AudioSource as_ambience)
    {
        int rand = Random.Range(0,clips.Length);
        as_ambience.clip = clips[rand];
        as_ambience.loop = true;
        as_ambience.Play();
        as_ambience.outputAudioMixerGroup = mix.FindMatchingGroups("Ambience")[0];
        
    }
}
