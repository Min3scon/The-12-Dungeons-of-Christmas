using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MusicManager : MonoBehaviour
{
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioMixerGroup musicGroup;
    public float fadeTime = 0.75f;

#if UNITY_EDITOR
    public SceneAsset[] menuScenes;
    public SceneAsset[] gameScenes;
#endif

    [SerializeField, HideInInspector] string[] menuSceneNames;
    [SerializeField, HideInInspector] string[] gameSceneNames;

    static MusicManager Instance;
    AudioSource source;
    string currentType = "";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        source = gameObject.AddComponent<AudioSource>();
        source.loop = true;
        source.outputAudioMixerGroup = musicGroup;
        source.playOnAwake = false;
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string type = GetSceneType(scene.name);
        if (type != null) PlayType(type);
    }

    string GetSceneType(string sceneName)
    {
        for (int i = 0; i < menuSceneNames.Length; i++)
            if (string.Equals(menuSceneNames[i], sceneName, System.StringComparison.OrdinalIgnoreCase))
                return "menu";

        for (int i = 0; i < gameSceneNames.Length; i++)
            if (string.Equals(gameSceneNames[i], sceneName, System.StringComparison.OrdinalIgnoreCase))
                return "game";

        return null;
    }

    public void PlayType(string type)
    {
        if (type == currentType) return;
        AudioClip target = type == "menu" ? menuMusic : gameMusic;
        if (target == null) return;

        currentType = type;
        StopAllCoroutines();
        StartCoroutine(FadeTo(target));
    }

    IEnumerator FadeTo(AudioClip target)
    {
        float startVol = source.volume;
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }

        source.clip = target;
        source.Play();

        t = 0f;
        while (t < fadeTime)
        {
            t += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, 1f, t / fadeTime);
            yield return null;
        }
        source.volume = 1f;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (menuScenes != null)
        {
            menuSceneNames = new string[menuScenes.Length];
            for (int i = 0; i < menuScenes.Length; i++)
                menuSceneNames[i] = menuScenes[i] ? menuScenes[i].name : "";
        }

        if (gameScenes != null)
        {
            gameSceneNames = new string[gameScenes.Length];
            for (int i = 0; i < gameScenes.Length; i++)
                gameSceneNames[i] = gameScenes[i] ? gameScenes[i].name : "";
        }
    }
#endif
}