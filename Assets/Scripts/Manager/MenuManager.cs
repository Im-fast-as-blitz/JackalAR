using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;
using UnityEngine.UI;

public class MenuManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update

    public InputField createInput;
    public InputField playersNumbInput;
    public InputField joinInput;
    public GameObject pausePanel;
    public GameObject pauseBtn;
    public Toggle isARCreate;
    public Toggle isARJoin;
    
    private GameManagerScr _gameManager;

    private int playersNumb = 0;
    public bool isAR = false;

    public void Start()
    {
        _gameManager = GetComponent<GameManagerScr>();
        PhotonNetwork.AutomaticallySyncScene = false;
    }
    
    public void CreateRoom()
    {
        isAR = isARCreate.isOn;
        Debug.Log(isAR);
        Debug.Log(isARCreate.isOn);
        if (createInput.text.Length == 0)
        {
            return;
        }

        try
        {
            playersNumb = int.Parse(playersNumbInput.text);
            if (playersNumb < 1 || playersNumb > 4)
            {
                throw new Exception();
            }
        }
        catch (Exception)
        {
            return;
        }
        
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = (byte)playersNumb;
        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        isAR = isARJoin.isOn;
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        //PhotonNetwork.IsMasterClient
        if (isAR)
        {
            Debug.Log("Joined to AR room");
            PhotonNetwork.LoadLevel("GameAR");
        }
        else
        {
            Debug.Log("Joined to room");
            PhotonNetwork.LoadLevel("Game");
        }
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

    public void OpenPausePanel()
    {
        pauseBtn.SetActive(false);
        pausePanel.SetActive(true);
        var soundSystem = GameObject.Find("SoundSystem");
        if (soundSystem)
        {
            soundSystem.GetComponent<SoundManager>().canChange = true;
        }
        
        _gameManager.endGameTitle.SetActive(false);
        _gameManager.currTeamTitle.SetActive(false);
    }

    public void ClosePausePanel()
    {
        var soundSystem = GameObject.Find("SoundSystem");
        if (soundSystem)
        {
            soundSystem.GetComponent<SoundManager>().canChange = false;
        }
        pauseBtn.SetActive(true);
        pausePanel.SetActive(false);
        
        _gameManager.currTeamTitle.SetActive(true);
        if (_gameManager.CurrentGame.IsGameEnded)
        {
            _gameManager.endGameTitle.SetActive(true);
        }
    }

    public void ExitGame()
    {
        Debug.Log("Exit");
        Application.Quit();
    }
}