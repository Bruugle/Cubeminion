using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;

public class PlayerPawnHealth : NetworkBehaviour, IHealthComponent
{
    public float blood = 100f;
    public bool isBleeding = false;
    public VisualEffect bloodEffect;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (!isBleeding && blood < 100f) blood += .2f * Time.deltaTime;
    }

    void PlayBleedEffect()
    {
        bloodEffect.SetBool("overrideColor", true);
        bloodEffect.SetVector4("color", GetComponent<Pawn>().color);
        bloodEffect.Play();
        BloodSplatterManager.Singleton.SpawnBloodPuddle(transform.position, 200f, GetComponent<Pawn>().color);
    }

    [Rpc(SendTo.Everyone)]
    void PlayBleedEffectRpc()
    {
        PlayBleedEffect();
    }

    public void AddWound(float BleedRate)
    {
        AddWoundRpc(BleedRate);
    }

    public void InstantKill(Vector3 killForce)
    {
        InstantKillRpc(killForce);
    }

    [Rpc(SendTo.Owner)]
    void AddWoundRpc(float BleedRate)
    {
        isBleeding = true;
        blood -= BleedRate;

        PlayBleedEffectRpc();

        if (blood < 0)
        {
            InstantKill(Vector3.zero);
            return;
        }
        StartCoroutine(BleedCoroutine(BleedRate));
    }

    [Rpc(SendTo.Owner)]
    void InstantKillRpc(Vector3 killForce)
    {
        GetComponent<DeathComponent>().flingForce = killForce;
        GetComponent<DeathComponent>().TriggerDeath(GetComponent<Pawn>().color);
    }

    IEnumerator BleedCoroutine(float bleedRate)
    {


        yield return new WaitForSeconds(10f * Random.value);

        if (isBleeding && isActiveAndEnabled)
        {
            PlayBleedEffectRpc();

            blood -= bleedRate;
            if (blood < 0)
            {
                InstantKill(Vector3.zero);
            }
            else
            {
                StartCoroutine(BleedCoroutine(bleedRate));
            }

            
        }
    }
}
