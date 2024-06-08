using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Scanner : NetworkBehaviour
{
    public AudioSource audioSource;

    public float signal;
    public float gain = 3f;

    private void Update()
    {
        float output = 0f;

        foreach(NetworkObject go in GameManager.Singleton.pawnList)
        {
            //if (go.IsOwner) continue;

            float intensity = 1f / Vector3.Distance(transform.position, go.transform.position);
            float incidence = Mathf.Clamp01(Vector3.Dot(transform.forward, (go.transform.position - transform.position).normalized));
            output += intensity * incidence;
        }

        signal = 1 + gain * Mathf.Clamp01(output);
        audioSource.pitch = signal;
        audioSource.volume = .5f * (1f + Mathf.Clamp01(output));
    }
}
