using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAiPatrol : MonoBehaviour
{
    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    [Header("Ranges")]
    public float sightRange = 25f;
    public float attackRange = 1f;

    [Header("Patrol (waypoints)")]
    public Transform[] waypoints;
    public float waypointReachDist = 1f;
    int waypointIndex = 0;

    [Header("Respawn / Hit Handling")]
    public string currentLevelId = "Level1";
    public int currentCheckpoint = 0;
    public RespawnSet[] respawnSets;
    public Transform defaultRespawnPoint;
    public GameObject hitCanvas;
    public float canvasShowSeconds = 0.8f;
    public AudioSource audioSource;
    public AudioClip hitClip;
    public float hitCooldown = 1.5f;
    public float respawnYOffset = 0.3f;
    public float respawnImmunity = 1.0f;

    [Header("Player Components")]
    public CharacterController playerController;
    public Rigidbody playerRigidbody;

    [Header("Enemy Components")]
    public Collider enemyCollider;

    GameObject player;
    NavMeshAgent agent;
    bool warnedOffNavMesh;
    enum State { None, Patrol, Chase, Attack }
    State lastState = State.None;
    Coroutine canvasRoutine;
    float lastHitTime = -999f;
    bool pausedForHit = false;
    bool hitLocked = false;
    float nextHitAllowedTime = 0f;

    [System.Serializable]
    public class RespawnSet
    {
        public string levelId;
        public Transform[] checkpoints;
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("Player");
        if (!enemyCollider) enemyCollider = GetComponent<Collider>();
        if (!enemyCollider) enemyCollider = GetComponentInChildren<Collider>();

        if (player)
        {
            int bit = 1 << player.layer;
            if ((playerLayer.value & bit) == 0) playerLayer |= bit;
            Debug.Log($"{name}: Found player '{player.name}' on layer '{LayerMask.LayerToName(player.layer)}' ({player.layer}).");
        }
        else
        {
            Debug.LogWarning($"{name}: No player found. Tag your player 'Player'.");
        }

        if (NavMesh.SamplePosition(transform.position, out var hit, 2f, NavMesh.AllAreas))
            agent.Warp(hit.position);

        if (!playerController && player) playerController = player.GetComponent<CharacterController>();
        if (!playerRigidbody && player) playerRigidbody = player.GetComponent<Rigidbody>();

        if (hitCanvas) hitCanvas.SetActive(false);
        if (audioSource) audioSource.loop = false;
    }

    void Update()
    {
        if (!agent.isOnNavMesh)
        {
            if (!warnedOffNavMesh)
            {
                Debug.LogWarning($"{name}: Agent not on NavMesh. Place on blue area or rebake.");
                warnedOffNavMesh = true;
            }
            return;
        }

        if (!player)
        {
            player = GameObject.FindWithTag("Player");
            if (!player) return;
        }

        bool playerInsight = Physics.CheckSphere(transform.position, sightRange, playerLayer);
        bool playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, playerLayer);

        State s = State.Patrol;
        if (playerInsight && !playerInAttackRange) s = State.Chase;
        if (playerInsight && playerInAttackRange) s = State.Attack;

        if (s != lastState)
        {
            Debug.Log($"{name} state: {s} | inSight={playerInsight} inAttack={playerInAttackRange} mask={playerLayer.value}");
            lastState = s;
        }

        if (s == State.Patrol) PatrolWaypoints();
        else if (s == State.Chase) Chase();
        else if (s == State.Attack) Attack(playerInAttackRange);
    }

    void Chase()
    {
        if (agent.isOnNavMesh && player)
            agent.SetDestination(player.transform.position);
    }

    void Attack(bool playerInAttackRange)
    {
        if (playerInAttackRange && !hitLocked && Time.unscaledTime >= nextHitAllowedTime && Time.unscaledTime - lastHitTime >= hitCooldown)
        {
            HandleHit("AttackRange");
            lastHitTime = Time.unscaledTime;
        }

        if (agent.isOnNavMesh && player)
            agent.SetDestination(player.transform.position);
    }

    void PatrolWaypoints()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Transform target = waypoints[waypointIndex];
        if (!target) return;

        agent.SetDestination(target.position);

        if (!agent.pathPending && agent.remainingDistance <= waypointReachDist)
            waypointIndex = (waypointIndex + 1) % waypoints.Length;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hitLocked || Time.unscaledTime < nextHitAllowedTime || Time.unscaledTime - lastHitTime < hitCooldown) return;
        HandleHit("OnTriggerEnter");
        lastHitTime = Time.unscaledTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!collision.collider.CompareTag("Player")) return;
        if (hitLocked || Time.unscaledTime < nextHitAllowedTime || Time.unscaledTime - lastHitTime < hitCooldown) return;
        HandleHit("OnCollisionEnter");
        lastHitTime = Time.unscaledTime;
    }

    void HandleHit(string source)
    {
        if (hitLocked) return;
        hitLocked = true;

        if (!player)
        {
            player = GameObject.FindWithTag("Player");
            if (!player)
            {
                hitLocked = false;
                return;
            }
        }

        if (audioSource && hitClip)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.PlayOneShot(hitClip);
        }

        if (hitCanvas)
        {
            hitCanvas.SetActive(true);
            if (canvasRoutine != null) StopCoroutine(canvasRoutine);
            canvasRoutine = StartCoroutine(HideCanvasAfter(canvasShowSeconds));
        }

        Vector3 respawnPos = GetRespawnPosition(currentLevelId, currentCheckpoint, player.transform.position);
        respawnPos.y += respawnYOffset;
        TeleportPlayer(respawnPos);

        if (agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        if (enemyCollider) enemyCollider.enabled = false;
        agent.isStopped = true;
    }

    void TeleportPlayer(Vector3 pos)
    {
        if (playerController == null && player != null)
            playerController = player.GetComponent<CharacterController>();
        if (playerRigidbody == null && player != null)
            playerRigidbody = player.GetComponent<Rigidbody>();

        if (playerController)
        {
            playerController.enabled = false;
            player.transform.position = pos;
            playerController.enabled = true;
        }
        else
        {
            player.transform.position = pos;
        }

        if (playerRigidbody)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.position = pos;
        }
    }

    IEnumerator HideCanvasAfter(float seconds)
    {
        if (!pausedForHit)
        {
            Time.timeScale = 0f;
            pausedForHit = true;
        }

        yield return new WaitForSecondsRealtime(seconds);

        if (hitCanvas) hitCanvas.SetActive(false);
        if (pausedForHit)
        {
            Time.timeScale = 1f;
            pausedForHit = false;
        }

        if (enemyCollider) enemyCollider.enabled = true;
        agent.isStopped = false;

        hitLocked = false;
        nextHitAllowedTime = Time.unscaledTime + respawnImmunity;
        lastHitTime = Time.unscaledTime;
    }

    Vector3 GetRespawnPosition(string levelId, int checkpointIndex, Vector3 fallback)
    {
        if (respawnSets != null)
        {
            for (int i = 0; i < respawnSets.Length; i++)
            {
                var set = respawnSets[i];
                if (set != null && set.levelId == levelId && set.checkpoints != null && set.checkpoints.Length > 0)
                {
                    int idx = Mathf.Clamp(checkpointIndex, 0, set.checkpoints.Length - 1);
                    var t = set.checkpoints[idx];
                    if (t != null) return t.position;
                }
            }
        }

        if (defaultRespawnPoint != null)
            return defaultRespawnPoint.position;

        return fallback;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (waypoints != null && waypoints.Length > 0)
        {
            Gizmos.color = Color.cyan;
            foreach (var t in waypoints)
                if (t) Gizmos.DrawSphere(t.position, 0.3f);
        }
    }
}