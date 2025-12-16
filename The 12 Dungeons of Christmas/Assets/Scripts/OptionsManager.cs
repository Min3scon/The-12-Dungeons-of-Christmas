using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class OptionsManager : MonoBehaviour
{
    public Slider masterVol, musicVol, sfxVol;
    public AudioMixer newAudioMixer;
    
    public void ChangeMasterVolume()
    {
        newAudioMixer.SetFloat("MasterVol", masterVol.value);
    }
    public void ChangeMusicVolume()
    {
        newAudioMixer.SetFloat("MusicVol", musicVol.value);
    }
    public void ChangeSFXVolume()
    {
        newAudioMixer.SetFloat("SFXVol", sfxVol.value);
    }
}
