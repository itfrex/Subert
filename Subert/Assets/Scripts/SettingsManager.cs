using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class SettingsManager : MonoBehaviour
{

    public Slider volSlider;
    public AudioMixer audioMixer;
    public void SetVolume()
    {
        float volume = volSlider.value;
        audioMixer.SetFloat("volume", Mathf.Log10(volume) * 20);
    }

}
