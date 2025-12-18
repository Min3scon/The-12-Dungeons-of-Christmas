using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{
    public Slider masterVol;
    public Slider musicVol;
    public Slider sfxVol;
    public AudioMixer audioMixer;

    private void Start()
    {
        masterVol.value = PlayerPrefs.GetFloat("MasterVol", 1f);
        musicVol.value = PlayerPrefs.GetFloat("MusicVol", 1f);
        sfxVol.value = PlayerPrefs.GetFloat("SFXVol", 1f);

        masterVol.onValueChanged.AddListener(delegate { SetMaster(); });
        musicVol.onValueChanged.AddListener(delegate { SetMusic(); });
        sfxVol.onValueChanged.AddListener(delegate { SetSFX(); });

        ApplyAll();
    }

    void ApplyAll()
    {
        SetMaster();
        SetMusic();
        SetSFX();
    }

    public void SetMaster()
    {
        SetVolume("MasterVol", masterVol.value);
        PlayerPrefs.SetFloat("MasterVol", masterVol.value);
    }

    public void SetMusic()
    {
        SetVolume("MusicVol", musicVol.value);
        PlayerPrefs.SetFloat("MusicVol", musicVol.value);
    }

    public void SetSFX()
    {
        SetVolume("SFXVol", sfxVol.value);
        PlayerPrefs.SetFloat("SFXVol", sfxVol.value);
    }

    void SetVolume(string parameter, float sliderValue)
    {
        float dB = sliderValue <= 0f ? -80f : Mathf.Lerp(-80f, 0f, sliderValue);
        if (!audioMixer.SetFloat(parameter, dB))
        {
            Debug.LogWarning("AudioMixer parameter missing: " + parameter);
        }
    }
}