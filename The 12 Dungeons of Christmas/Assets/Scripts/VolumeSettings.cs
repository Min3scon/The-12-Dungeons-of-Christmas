using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeSettings : MonoBehaviour
{
   public Slider masterVol;
   public Slider musicVol;
   public Slider sfxVol;

   public AudioMixer audioMixer;

   void Awake()
   {
       if (audioMixer == null)
       {
           Debug.LogError("[VOLUME] AudioMixer is NULL on " + gameObject.name);
           enabled = false;
           return;
       }

       if (masterVol == null || musicVol == null || sfxVol == null)
       {
           Debug.LogError("[VOLUME] One or more Sliders are NULL");
           enabled = false;
           return;
       }

       Debug.Log("[VOLUME] VolumeSettings active on " + gameObject.name);
   }

   void Start()
   {
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
       Apply("masterVol", masterVol.value);
   }

   public void SetMusic()
   {
       Apply("musicVol", musicVol.value);
   }

   public void SetSFX()
   {
       Apply("sfxVol", sfxVol.value);
   }

   void Apply(string param, float value)
   {
       // 1️⃣ Try to read the parameter FIRST (existence check)
       if (!audioMixer.GetFloat(param, out float before))
       {
           Debug.LogError($"[VOLUME] Mixer parameter '{param}' DOES NOT EXIST or is not exposed");
           return;
       }

       // 2️⃣ Set it
       audioMixer.SetFloat(param, value);

       // 3️⃣ Read back
       audioMixer.GetFloat(param, out float after);

       // 4️⃣ Validate change
       if (Mathf.Abs(after - value) > 0.1f)
       {
           Debug.LogError($"[VOLUME] '{param}' NOT changing (set {value}, read {after})");
           return;
       }

       // 5️⃣ Success log (once per call)
       Debug.Log($"[VOLUME] '{param}' OK → {after} dB");
   }
}