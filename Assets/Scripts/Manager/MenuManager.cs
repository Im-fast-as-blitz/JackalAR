using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MenuManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update

    public InputField createInput;
    public InputField joinInput;

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 4;
        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined to room");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }
        //SceneManager.LoadScene("GameAR");
        // SceneManager.LoadScene("Game");
    }

    public void BackToMainMenu()
    {
        var soundSystem = GameObject.Find("SoundSystem");
        if (soundSystem)
        {
            soundSystem.GetComponent<SoundManager>().canChange = false;
        }
        SceneManager.LoadScene("Menu");
    }

    public void ToSettingsPanel()
    {
        SceneManager.LoadScene("Settings");
        var soundSystem = GameObject.Find("SoundSystem");
        if (soundSystem)
        {
            soundSystem.GetComponent<SoundManager>().canChange = true;
        }
    }

    public void ToRulesPanel()
    {
        SceneManager.LoadScene("Rules");
    }
}