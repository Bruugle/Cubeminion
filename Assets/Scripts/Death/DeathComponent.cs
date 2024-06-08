using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathComponent : MonoBehaviour
{
    public GameObject corpsePrefab;
    public Vector3 flingForce;
    public MeshRenderer face;

    public void TriggerDeath()
    {

        IKillable[] killables = GetComponentsInChildren<IKillable>();
        foreach(IKillable k in killables)
        {
            k.OnDeath();
        }

        var corpse = Instantiate(corpsePrefab, transform.position, transform.rotation);


        if (corpse.TryGetComponent<Corpse>(out Corpse crps))
        {
            crps.rb.AddForceAtPosition(flingForce, transform.position + .05f * Random.insideUnitSphere);
            crps.face.material.mainTextureOffset = new Vector2(face.material.mainTextureOffset.x, 0f);
        }
    }
    public void TriggerDeath(Color color)
    {

        IKillable[] killables = GetComponentsInChildren<IKillable>();
        foreach (IKillable k in killables)
        {
            k.OnDeath();
        }

        var corpse = Instantiate(corpsePrefab, transform.position, transform.rotation);


        if (corpse.TryGetComponent<Corpse>(out Corpse crps))
        {
            crps.rb.AddForceAtPosition(flingForce, transform.position + .01f * Random.insideUnitSphere);
            crps.face.material.mainTextureOffset = new Vector2(face.material.mainTextureOffset.x, 0f);
            crps.isColorOverride = true;
            crps.colorOverride = color;
            crps.bloodEffect.SetBool("overrideColor", true);
            crps.bloodEffect.SetVector4("color", color);
        }
    }


}
