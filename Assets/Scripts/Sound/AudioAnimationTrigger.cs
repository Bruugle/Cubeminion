using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAnimationTrigger : MonoBehaviour
{
    public AudioSource audioSource;

    public bool play;

    private void Update()
    {
        if (play)
        {
            PlayClip();
        }
    }
    void PlayClip()
    {
        audioSource.Play();
        play = false;
    }
}
