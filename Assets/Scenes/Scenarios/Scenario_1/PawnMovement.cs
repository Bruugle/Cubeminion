using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UIIS;
using UnityEngine.VFX;

public class PawnMovement : NetworkBehaviour
{

    Vector3 movementDirection;
    float fastSpeedX;
    float runSpeed = 3f;
    public VisualEffect spit;

    void Update()
    {
        if (!IsOwner) return;

        movementDirection = Vector3.zero;

        if (Keyboard.current.shiftKey.isPressed)
        {
            fastSpeedX = runSpeed;
        }
        else
        {
            fastSpeedX = 1f;
        }

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            GetComponent<InteractiveRaycast>().TryInteract();
        }
        if (Keyboard.current.fKey.wasPressedThisFrame)
        {
            SpitRpc();
        }
        
        if (Keyboard.current.wKey.isPressed)
        {
            movementDirection += Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        }
        if (Keyboard.current.sKey.isPressed)
        {
            movementDirection -= Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            movementDirection -= Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            movementDirection += Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
        }

        if (movementDirection == Vector3.zero) return;


        movementDirection.Normalize();
        transform.position += fastSpeedX * movementDirection * Time.deltaTime;

        float rotateTime = .007f * Vector3.Angle(transform.forward, movementDirection);
        Quaternion lookRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, Time.deltaTime / rotateTime);

    }

    [Rpc(SendTo.Everyone)]
    void SpitRpc()
    {
        spit.Play();
        spit.GetComponent<AudioSource>().Play();
    }

    public void IncreaseRunspeedForSeconds(float add, float seconds)
    {

        StartCoroutine(DoubleRunspeedForSecondsRountine(Mathf.Max(add, 1f), seconds));
    }

    IEnumerator DoubleRunspeedForSecondsRountine(float add, float seconds)
    {
        runSpeed += add;
        yield return new WaitForSeconds(seconds);
        runSpeed -= add;
    }
    
   
}
