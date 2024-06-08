using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundBank : MonoBehaviour
{
    public AudioClip[] idleNpcClips;
    public AudioClip[] scaredNpcClips;
    public AudioClip[] deathClips;
    public AudioListener listener;
    public float decayRate;


    public static SoundBank Singleton { get; private set; }

    private void Awake()
    {
        if (Singleton == null)
        {
            Singleton = this;
            Init();
        }
        else if (Singleton != this)
        {
            Destroy(gameObject);
        }
    }

    void Init()
    {
        listener = FindObjectOfType<AudioListener>();
    }

    public AudioClip GetRandomIdleClip()
    {
        return idleNpcClips[Random.Range(0, idleNpcClips.Length)];
    }
    public AudioClip GetRandomScaredeClip()
    {
        return scaredNpcClips[Random.Range(0, scaredNpcClips.Length)];
    }
    public AudioClip GetRandomDeathClip()
    {
        return deathClips[Random.Range(0, deathClips.Length)];
    }

    public float GetLowPassFilterValue(Vector3 position)
    {

        float d = Vector3.Distance(listener.transform.position, position);
        return Mathf.Clamp( 22000f * Mathf.Pow(decayRate, d), 1f, 22000f);
    }
}
