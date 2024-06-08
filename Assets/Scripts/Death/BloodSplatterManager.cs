using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class BloodSplatterManager : MonoBehaviour
{

    public GameObject bloodPuddle;

    public static BloodSplatterManager Singleton { get; private set; }

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

    }

    public void SpawnBloodPuddle(Vector3 position)
    {
        var go = Instantiate(bloodPuddle,transform);
        go.transform.position = position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        go.transform.rotation = Quaternion.Euler(90f + Random.Range(-20f, 20f), Random.Range(0f, 360f), 0f);
        go.transform.localScale = new Vector3(Random.Range(.5f, 3f), 1f, Random.Range(.5f, 3f));
    }
    public void SpawnBloodPuddle(Vector3 position, float time)
    {
        var go = Instantiate(bloodPuddle, transform);
        go.transform.position = position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        go.transform.rotation = Quaternion.Euler(90f + Random.Range(-20f, 20f), Random.Range(0f, 360f), 0f);
        go.transform.localScale = new Vector3(Random.Range(.5f, 3f), 1f, Random.Range(.5f, 3f));

        Destroy(go, time);
    }
    public void SpawnBloodPuddle(Vector3 position, Color color)
    {
        var go = Instantiate(bloodPuddle, transform);
        var projector = go.GetComponent<DecalProjector>();
        projector.material = new Material(projector.material);
        projector.material.color = color;
        
        go.transform.position = position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        go.transform.rotation = Quaternion.Euler(90f + Random.Range(-20f, 20f), Random.Range(0f, 360f), 0f);
        go.transform.localScale = new Vector3(Random.Range(.5f, 3f), 1f, Random.Range(.5f, 3f));
    }
    public void SpawnBloodPuddle(Vector3 position, float time, Color color)
    {
        var go = Instantiate(bloodPuddle, transform);
        var projector = go.GetComponent<DecalProjector>();
        projector.material = new Material(projector.material);
        projector.material.color = color;

        go.transform.position = position + new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
        go.transform.rotation = Quaternion.Euler(90f + Random.Range(-20f, 20f), Random.Range(0f, 360f), 0f);
        go.transform.localScale = new Vector3(Random.Range(.5f, 3f), 1f, Random.Range(.5f, 3f));

        Destroy(go, time);
    }



}
