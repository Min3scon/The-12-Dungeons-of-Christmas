using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused;

    [SerializeField] GameObject pauseMenuUI;
    [SerializeField] MonoBehaviour[] disableWhilePaused;

    struct BehaviourState
    {
        public MonoBehaviour behaviour;
        public bool wasEnabled;
    }

    BehaviourState[] cachedStates;

    void Start()
    {
        CacheStates();
        SetPaused(false);
    }

    void CacheStates()
    {
        cachedStates = new BehaviourState[disableWhilePaused.Length];
        for (int i = 0; i < disableWhilePaused.Length; i++)
        {
            cachedStates[i] = new BehaviourState
            {
                behaviour = disableWhilePaused[i],
                wasEnabled = disableWhilePaused[i] != null && disableWhilePaused[i].enabled
            };
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetPaused(!GameIsPaused);
        }
    }

    void SetPaused(bool paused)
    {
        GameIsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (pauseMenuUI) pauseMenuUI.SetActive(paused);
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        if (paused)
        {
            for (int i = 0; i < disableWhilePaused.Length; i++)
            {
                if (disableWhilePaused[i])
                {
                    cachedStates[i].wasEnabled = disableWhilePaused[i].enabled;
                    disableWhilePaused[i].enabled = false;
                }
            }
        }
        else
        {
            for (int i = 0; i < cachedStates.Length; i++)
            {
                if (cachedStates[i].behaviour)
                {
                    cachedStates[i].behaviour.enabled = cachedStates[i].wasEnabled;
                }
            }
        }
    }

    public void Resume() => SetPaused(false);
    public void Pause() => SetPaused(true);
}