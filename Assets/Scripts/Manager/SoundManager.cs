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

    private float _Volume = 0.5f;
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
}
