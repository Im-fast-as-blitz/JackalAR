using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceMusic : MonoBehaviour
{
    private SoundManager _soundSystem;
    private bool _prevActive = true;
    private bool _nextActive = true;
    private void Start()
    {
        _soundSystem = GameObject.FindGameObjectWithTag("Sound System").GetComponent<SoundManager>();

        if (_soundSystem.currMusic == 0)
        {
            ChangeChildColor(0, 0.25f);
            _prevActive = false;
        } else if (_soundSystem.currMusic == _soundSystem.GetSize() - 1)
        {
            ChangeChildColor(2, 0.25f);
            _nextActive = false;
        }

        ChangeMusicName();
    }

    private void ChangeMusicName()
    {
        transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Sound " + (_soundSystem.currMusic + 1).ToString();
    }

    private void ChangeChildColor(int child, float color)
    {
        Color currColor = transform.GetChild(child).GetComponent<Image>().color;
        transform.GetChild(child).GetComponent<Image>().color = new Color(currColor.r, currColor.g, currColor.b, color);
    }

    public void PrevSound()
    {
        if (!_prevActive)
        {
            return;
        }
        
        if (!_nextActive)
        {
            _nextActive = true;
            ChangeChildColor(2, 1);
        }
        if (!_soundSystem.PrevSound())
        {
            _prevActive = false;
            ChangeChildColor(0, 0.25f);
        }
        ChangeMusicName();
    }
    
    public void NextSound()
    {
        if (!_nextActive)
        {
            return;
        }
        
        if (!_prevActive)
        {
            _prevActive = true;
            ChangeChildColor(0, 1);
        }
        if (!_soundSystem.NextSound())
        {
            _nextActive = false;
            ChangeChildColor(2, 0.25f);
        }
        ChangeMusicName();
    }
}
