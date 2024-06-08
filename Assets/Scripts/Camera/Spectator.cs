using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UIIS;
using UnityEngine.UI;
using Unity.Burst.Intrinsics;

public class Spectator : MonoBehaviour
{
    public Text interact_text;
    public Text soul_text;
    public Text summon_text;
    public float speed = 5f;
    float fastSpeedX;

    public GameObject overcubeController;
    public AudioSource soulAudio;


    bool isPromptInteractable;
    GameObject promptObject;

    void Update()
    {
        transform.rotation = Camera.main.transform.rotation;

        if (Keyboard.current.shiftKey.isPressed)
        {
            fastSpeedX = 2f;
        }
        else
        {
            fastSpeedX = 1f;
        }




        if (Keyboard.current.wKey.isPressed)
        {
            if (!Physics.Raycast(transform.position, transform.forward, .3f))
            {
                transform.position += fastSpeedX * speed * transform.forward * Time.deltaTime;
            }
        }
        if (Keyboard.current.sKey.isPressed)
        {
            transform.position -= fastSpeedX * speed * transform.forward * Time.deltaTime;
        }
        if (Keyboard.current.aKey.isPressed)
        {
            transform.position -= fastSpeedX * speed * transform.right * Time.deltaTime;
        }
        if (Keyboard.current.dKey.isPressed)
        {
            transform.position += fastSpeedX * speed * transform.right * Time.deltaTime;
        }

        PromptCast(Camera.main.transform.position, Camera.main.transform.forward, 3f);

        if (isPromptInteractable)
        {
            interact_text.text = "E to collect soul";

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                Destroy(promptObject);
                SpectatorManager.Singleton.capturedSoulCount++;
                soul_text.text = "souls collected: " + SpectatorManager.Singleton.capturedSoulCount.ToString();
                soulAudio.pitch = .9f + (Random.value * .4f);
                soulAudio.PlayOneShot(SpectatorManager.Singleton.captureClip);
            }
        }
        else
        {
            interact_text.text = string.Empty;
        }


        if (Keyboard.current.qKey.isPressed && SpectatorManager.Singleton.capturedSoulCount >= SpectatorManager.Singleton.summonCost)
        {
            var ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            var hitInfo = new RaycastHit();
            if (Physics.Raycast(ray, out hitInfo, 150f))
            {
                overcubeController.SetActive(true);
                overcubeController.transform.position = hitInfo.point;
                overcubeController.transform.rotation = Quaternion.identity;
            }
            else
            {
                overcubeController.SetActive(false);
            }

        }
        if (Keyboard.current.qKey.wasReleasedThisFrame && SpectatorManager.Singleton.capturedSoulCount >= SpectatorManager.Singleton.summonCost && overcubeController.activeInHierarchy)
        {
            SpectatorManager.Singleton.overCube.SetTargetRpc(overcubeController.transform.position);
            overcubeController.SetActive(false);
            SpectatorManager.Singleton.capturedSoulCount -= SpectatorManager.Singleton.summonCost;
            SpectatorManager.Singleton.summonCost++;
            soul_text.text = "souls collected: " + SpectatorManager.Singleton.capturedSoulCount.ToString();
            summon_text.text = $"hold Q to summon overcube: {SpectatorManager.Singleton.summonCost} souls";
            soulAudio.pitch = .7f + (.4f * Random.value);
            soulAudio.PlayOneShot(SpectatorManager.Singleton.summonClip);
        }
        if (Keyboard.current.xKey.wasPressedThisFrame && SpectatorManager.Singleton.capturedSoulCount >= 100)
        {
            SpectatorManager.Singleton.RespawnRequest();
            SpectatorManager.Singleton.capturedSoulCount -= 100;
            soul_text.text = "souls collected: " + SpectatorManager.Singleton.capturedSoulCount.ToString();
        }

    }

    void PromptCast(Vector3 origin, Vector3 direction, float distance)
    {
        var ray = new Ray(origin, direction);
        var hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo, distance))
        {
            var otherObject = hitInfo.collider.gameObject;

            if (otherObject == promptObject)
                return;

            if (otherObject.GetComponent<Soul>() == null)
            {
                isPromptInteractable = false;
            }
            else
            {
                isPromptInteractable = true;
            }

            promptObject = otherObject;

        }
        else
        {
            promptObject = null;
            isPromptInteractable = false;
        }
    }
}
