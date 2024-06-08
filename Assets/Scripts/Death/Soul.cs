using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soul : MonoBehaviour
{

    private void Awake()
    {
        StartCoroutine(RiseUp());  
    }
    private void OnDestroy()
    {
        SpectatorManager.Singleton.SpectatorAdded -= OnSpectating;
        SpectatorManager.Singleton.SpectatorRemoved -= OnEndSpectating;
    }

    public void StartListening()
    {
        SpectatorManager.Singleton.SpectatorAdded += OnSpectating;
        SpectatorManager.Singleton.SpectatorRemoved += OnEndSpectating;
    }

    void OnSpectating()
    {
        gameObject.SetActive(true);
        
    }

    private void OnEndSpectating()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        Vector3 eulers = 10f * Time.deltaTime * new Vector3(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
        transform.Rotate(eulers);
    }

    IEnumerator RiseUp()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position + Vector3.up * (1f + UnityEngine.Random.value);

        float t = 0;

        while (t < 5f)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, t/3f);
            yield return null;
        }
    }
}
