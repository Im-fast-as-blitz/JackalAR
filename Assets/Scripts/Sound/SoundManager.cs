using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public bool canChange = false;

    private bool wasChange = false;
    
    [SerializeField] private Slider _Slider;
    [SerializeField] private AudioSource _AudioSurce;
    [SerializeField] private AudioClip[] _Music;

    private float _Volume = 0.5f;
    public int currMusic = 0;
    void Start()
    {
        _AudioSurce.volume = _Volume;
        if (_Slider != null)
        {
            _Slider.value = _Volume;
        }
        DontDestroyOnLoad(gameObject);
    }
    
    void Update()
    {
        if (canChange)
        {
            if (!wasChange)
            {
                GameObject slider = GameObject.Find("Slider");
                if (slider)
                {
                    _Slider = slider.GetComponent<Slider>();
                    _Slider.value = _Volume;
                    wasChange = true;
                }
            }
        }
        else
        {
            wasChange = false;
            _Slider = null;
        }
        
        
        if (_Slider)
        {
            _Volume = _Slider.value;
            _AudioSurce.volume = _Volume;
        }
    }

    public bool NextSound()
    {
        ++currMusic;
        _AudioSurce.clip = _Music[currMusic];
        _AudioSurce.Play();
        return currMusic != (_Music.Length - 1);
    }

    public bool PrevSound()
    {
        --currMusic;
        _AudioSurce.clip = _Music[currMusic];
        _AudioSurce.Play();
        return currMusic != 0;
    }

    public int GetSize()
    {
        return _Music.Length;
    }
}