using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class CheckpointDetector : MonoBehaviour
{
    [Header("Checkpoints (in order)")]
    [SerializeField] Collider[] checkpoints;

    [Header("Level Starts (in order)")]
    [SerializeField] Transform[] playerLevelStarts;
    [SerializeField] Transform[] enemyLevelStarts;
    [SerializeField] string[] levelIds;

    [Header("Enemy Reference")]
    [SerializeField] EnemyAiPatrol enemyAi;

    [Header("Audio")]
    [SerializeField] AudioClip checkpointClip;
    [SerializeField] AudioClip completedClip;

    AudioSource audioSource;
    int currentCheckpoint;
    bool wasInside;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        UpdateCheckpointVisibility();

        if (checkpoints != null && checkpoints.Length > 0)
            wasInside = checkpoints[0].bounds.Contains(transform.position);
    }

    void Update()
    {
        if (currentCheckpoint >= checkpoints.Length) return;

        Collider nextCheckpoint = checkpoints[currentCheckpoint];
        bool inside = nextCheckpoint.bounds.Contains(transform.position);

        if (inside && !wasInside)
        {
            currentCheckpoint++;
            PlayCheckpointSound();

            int targetIndex = currentCheckpoint;
            bool finished = currentCheckpoint >= checkpoints.Length;
            TeleportToLevelStart(targetIndex, finished);

            UpdateCheckpointVisibility();

            if (finished)
            {
                PlayCompletedSound();
                wasInside = false;
                return;
            }
        }

        wasInside = inside;
    }

    void TeleportToLevelStart(int levelIndex, bool finished)
    {
        int idx = Mathf.Clamp(levelIndex, 0, Mathf.Max(playerLevelStarts.Length - 1, 0));

        Transform playerStart = (playerLevelStarts != null && playerLevelStarts.Length > idx) ? playerLevelStarts[idx] : null;
        if (playerStart)
            TeleportPlayer(playerStart.position);

        if (enemyAi)
        {
            Transform enemyStart = (enemyLevelStarts != null && enemyLevelStarts.Length > idx) ? enemyLevelStarts[idx] : null;
            if (enemyStart)
                TeleportEnemy(enemyStart.position);

            string id = ResolveLevelId(idx);
            enemyAi.currentLevelId = id;
            enemyAi.currentCheckpoint = 0;
        }
    }

    string ResolveLevelId(int idx)
    {
        if (levelIds != null && levelIds.Length > idx && !string.IsNullOrEmpty(levelIds[idx]))
            return levelIds[idx];
        return "Level" + (idx + 1);
    }

    void TeleportPlayer(Vector3 pos)
    {
        var cc = GetComponent<CharacterController>();
        var rb = GetComponent<Rigidbody>();

        if (cc)
        {
            cc.enabled = false;
            transform.position = pos;
            cc.enabled = true;
        }
        else
        {
            transform.position = pos;
        }

        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = pos;
        }
    }

    void TeleportEnemy(Vector3 pos)
    {
        var enemyTransform = enemyAi.transform;
        var agent = enemyAi.GetComponent<NavMeshAgent>();

        if (agent && agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(pos, out hit, 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                agent.Warp(pos);
        }
        else
        {
            enemyTransform.position = pos;
        }
    }

    void PlayCheckpointSound()
    {
        if (audioSource && checkpointClip)
            audioSource.PlayOneShot(checkpointClip);
    }

    void PlayCompletedSound()
    {
        if (audioSource && completedClip)
            audioSource.PlayOneShot(completedClip);
    }

    void UpdateCheckpointVisibility()
    {
        for (int i = 0; i < checkpoints.Length; i++)
            checkpoints[i].gameObject.SetActive(i == currentCheckpoint);
        wasInside = false;
    }
}