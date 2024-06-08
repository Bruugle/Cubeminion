using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class SmokeStopper : MonoBehaviour
{
    public VisualEffect smoke;

    public float stopAfter;

    private void Awake()
    {
        StartCoroutine(StopAfterTime(stopAfter));  
    }

    IEnumerator StopAfterTime(float t)
    {
        yield return new WaitForSeconds(t);
        smoke.Stop();
    }
}
