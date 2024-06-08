using Networking;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameMenuController : MonoBehaviour
{

    public GameObject canvas;
    public AudioMixer masterMixer;
    public Text framerate;

    public void ToggleMenu()
    {
        canvas.SetActive(!canvas.activeInHierarchy);

        if (canvas.activeInHierarchy)
        {
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
            Camera.main.GetComponentInParent<MouseAim>().enabled = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Camera.main.GetComponentInParent<MouseAim>().enabled = true;
        }
        
    }

    private void Start()
    {
        StartCoroutine(FrameRateCounter());
    }

    public void SetMasterVolume(float volume)
    {
        masterMixer.SetFloat("MasterVolume", volume);
    }

    public void SetFOV(float value)
    {
        Camera.main.fieldOfView = value;
    }
    public void SetSensitivity(float value)
    {
        Camera.main.GetComponentInParent<MouseAim>().mouseSensitivity = value;
    }

    private void Update()
    {

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            ToggleMenu();
        }
    }

    public void LeaveGame()
    {
        NetworkController.Singleton.LeaveServer();
    }

    public void Resart()
    {
        NetworkController.Singleton.ReloadLobby();
    }

    IEnumerator FrameRateCounter()
    {

        int lastFrameCount = Time.frameCount;

        yield return new WaitForSeconds(1f);

        framerate.text = "Fps: " + (Time.frameCount - lastFrameCount).ToString();

        StartCoroutine(FrameRateCounter());
    }

}
