using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Corpse : MonoBehaviour
{

    public Transform colliderTransform;
    public Rigidbody rb;
    public VisualEffect bloodEffect;
    public GameObject bloodPuddle;
    public Color colorOverride;
    public bool isColorOverride = false;
    public MeshRenderer face;
    bool isGrounded = false;
    public SoundSource sound;

    private void Awake()
    {
        colliderTransform.rotation = Random.rotation;
        rb = GetComponent<Rigidbody>();
        rb.ResetInertiaTensor();
        bloodEffect.Play();
        FearTrigger.Singleton.TriggerFearEvent(transform.position, 10f, 50f);
        sound.UpdateFilter();
        sound.audioSource.clip = SoundBank.Singleton.GetRandomDeathClip();
        sound.audioSource.Play();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isGrounded) return;

        StartCoroutine(QueueSleep());
        for (int i = 0; i < Random.Range(1,5); i++)
        {
            StartCoroutine(SpawnBloodAfter(i));
        }

        isGrounded = true;
    }

    IEnumerator QueueSleep()
    {
        yield return new WaitForSeconds(5f);

        rb.isKinematic = true;
        colliderTransform.gameObject.SetActive(false);
        bloodEffect.Stop();
        SpectatorManager.Singleton.SpawnSoul(transform.position);
    }
    IEnumerator SpawnBloodAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (isColorOverride)
        {
            BloodSplatterManager.Singleton.SpawnBloodPuddle(transform.position, colorOverride);
        }
        else
        {
            BloodSplatterManager.Singleton.SpawnBloodPuddle(transform.position);
        }
        

    }

}
