using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Networking;
using UnityEngine.UI;
using Utilities;

public class MainMenuController : MonoBehaviour
{
    public InputField usernameIF;
    public Image colorImg;
    public Image faceImage;

    private void Start()
    {
        if (NetworkController.Singleton.localPlayerData.name != null)
        {
            usernameIF.text = NetworkController.Singleton.localPlayerData.name;
            UpdateImages();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartGame()
    {
        NetworkController.Singleton.StartServer();
        
    }

    public void JoinGame()
    {
        NetworkController.Singleton.JoinServer();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OnUsernameChange(string username)
    {
        NetworkController.Singleton.localPlayerData.name = username;
        UpdateImages();
    }

    void UpdateImages()
    {
        if (NetworkController.Singleton.localPlayerData.name == null) return;

        string username = NetworkController.Singleton.localPlayerData.name;
        Random.InitState(username.GetHashCode());
        colorImg.color = Random.ColorHSV();
        Random.InitState(username.GetHashCode());
        faceImage.material.mainTextureOffset = new Vector2((float)Random.Range(0, ArtUtils.FaceAtlasCount) / ArtUtils.FaceAtlasCount, .5f);
    }

    public void OnJoinCodeChange(string code)
    {
        NetworkController.Singleton.gameJoinCode = code.ToUpper();
    }

    public void OnLobbySizeChange(string size)
    {
        NetworkController.Singleton.allocationNum = int.Parse(size);
    }

}
