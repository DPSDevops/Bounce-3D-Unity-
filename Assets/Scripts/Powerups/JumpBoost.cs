using UnityEngine;

public class JumpBoost : MonoBehaviour
{
    [Header("Powerup Settings")]
    [SerializeField] private float jumpIncreaseAmount = 2f;
    [SerializeField] private bool destroyOnPickup = true;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            
            if (player != null)
            {
                // Apply jump boost
                player.AddJumpForce(jumpIncreaseAmount);
                Debug.Log($"Jump Boost Collected! +{jumpIncreaseAmount} Jump Force");
                
                // Spawn effect if assigned
                if (pickupEffect != null)
                {
                    Instantiate(pickupEffect, transform.position, Quaternion.identity);
                }
                
                // Destroy the powerup object
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
