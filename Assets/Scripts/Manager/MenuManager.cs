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
    private GameManagerScr _gameManager;

    public int playersNumb = 0;

    public void Start()
    {
        _gameManager = GetComponent<GameManagerScr>();
    }
    
    public void CreateRoom()
    {
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
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined to room");
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("Game");
        }
        // SceneManager.LoadScene("GameAR");
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
    }
}