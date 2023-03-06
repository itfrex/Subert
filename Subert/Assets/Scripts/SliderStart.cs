using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SliderStart : MonoBehaviour
{

    public AudioMixer audioMixer;

    // Start is called before the first frame update
    void Start()
    {
        bool exists = false;
        float volume = 0;
        exists = audioMixer.GetFloat("volume", out volume);
        if (exists)
        {
            volume = volume / 20;
            volume = Mathf.Pow(10, volume);
            GetComponent<Slider>().value = volume;
        }

    }

}
