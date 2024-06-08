using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundSource : MonoBehaviour
{
    public float lowpass;
    public AudioLowPassFilter filter;
    public AudioSource audioSource;
    public bool autoUpdate = false;

    private void Awake()
    {
        if (autoUpdate)
        {
            StartCoroutine(AutoUpdateCoroutine());
        }
    }

    public void UpdateFilter()
    {
        lowpass = SoundBank.Singleton.GetLowPassFilterValue(transform.position);
        filter.cutoffFrequency = lowpass;
    }

    IEnumerator AutoUpdateCoroutine()
    {
        

        yield return new WaitForEndOfFrame();

        UpdateFilter();

        StartCoroutine(AutoUpdateCoroutine());
    }

}
