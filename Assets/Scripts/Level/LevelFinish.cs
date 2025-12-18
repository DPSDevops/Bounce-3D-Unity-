using UnityEngine;

public class LevelFinish : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("Victory Launch")]
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private bool disablePlayerControl = true;

    /// <summary>
    /// Detect physical collision with the player
    /// </summary>
    /// <param name="collision">Collision data</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            TriggerLevelComplete(collision.gameObject);
        }
    }

    /// <summary>
    /// Detect trigger overlap with the player (if Collider is set to IsTrigger)
    /// </summary>
    /// <param name="other">The other collider</param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            TriggerLevelComplete(other.gameObject);
        }
    }
    
    /// <summary>
    /// Handle level completion effects
    /// </summary>
    private void TriggerLevelComplete(GameObject player)
    {
        Debug.Log("Level Done");
        
        // Spawn celebration particles
        FinishCelebrationEffect.SpawnCelebration(transform.position);
        
        // Launch player upward
        Rigidbody rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Reset velocity and apply massive upward force
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.up * launchForce, ForceMode.VelocityChange);
            
            Debug.Log($"Victory Launch! Applied {launchForce} upward force");
        }
        
        // Optionally disable player control
        if (disablePlayerControl)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = false;
            }
        }
    }
}

