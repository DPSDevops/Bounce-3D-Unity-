using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    public float defaultGroundSpeed = 10f;
    public float defaultAirSpeed = 2f;
    public float defaultMaxSpeed = 5f;
    public float defaultJumpForce = 5f;

    [Header("Current Stats")]
    public float currentGroundSpeed;
    public float currentAirSpeed;
    public float currentMaxSpeed;
    public float currentJumpForce;
    
    [Header("Spawn Settings")]
    [SerializeField] private string spawnTag = "Spawn";
    public float fallRespawnThreshold = -10f; // Height to trigger respawn
    
    // Checkpoint system
    private Vector3 currentRespawnPosition;
    private Quaternion currentRespawnRotation;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeSpawnPoint();
        SpawnPlayer();
    }
    
    /// <summary>
    /// Find default spawn point and set initial respawn coordinates
    /// </summary>
    private void InitializeSpawnPoint()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag(spawnTag);
        if (spawnPoint != null)
        {
            currentRespawnPosition = spawnPoint.transform.position;
            currentRespawnRotation = spawnPoint.transform.rotation;
        }
        else
        {
            Debug.LogError($"GameManager: No object found with tag '{spawnTag}' for initial spawn!");
            currentRespawnPosition = Vector3.zero;
            currentRespawnRotation = Quaternion.identity;
        }
    }
    
    /// <summary>
    /// Update the current respawn point (Checkpoint)
    /// </summary>
    /// <param name="position">New spawn position</param>
    /// <param name="rotation">New spawn rotation</param>
    public void SetCheckpoint(Vector3 position, Quaternion rotation)
    {
        currentRespawnPosition = position;
        currentRespawnRotation = rotation;
        Debug.Log($"GameManager: Checkpoint reached! New respawn point set to {position}");
    }
    
    /// <summary>
    /// Reset stats to default values
    /// </summary>
    public void InitializeStats()
    {
        currentGroundSpeed = defaultGroundSpeed;
        currentAirSpeed = defaultAirSpeed;
        currentMaxSpeed = defaultMaxSpeed;
        currentJumpForce = defaultJumpForce;
    }
    
    /// <summary>
    /// Move player to the current respawn position
    /// </summary>
    public void SpawnPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            Debug.Log($"GameManager: Respawning Player at {currentRespawnPosition}");
            
            // 1. Reset Transform
            player.transform.position = currentRespawnPosition;
            player.transform.rotation = currentRespawnRotation;
            
            // 2. Reset Physics (Crucial for Rigidbody)
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                // Force position update for physics engine
                rb.position = currentRespawnPosition;
                rb.rotation = currentRespawnRotation;
            }
        }
        else
        {
            Debug.LogError("GameManager: Player object not found! Make sure your player is tagged 'Player'");
        }
    }
    
    // --- Stat Modification Methods ---
    
    public void IncreaseSpeed(float amount)
    {
        currentGroundSpeed += amount;
        currentMaxSpeed += amount / 2f;
        
        // Update player if exists
        UpdatePlayerStats();
    }
    
    public void IncreaseJump(float amount)
    {
        currentJumpForce += amount;
        
        // Update player if exists
        UpdatePlayerStats();
    }
    
    private void UpdatePlayerStats()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.SyncStats();
            }
        }
    }
}
