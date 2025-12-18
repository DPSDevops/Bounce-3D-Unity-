using UnityEngine;

public class SpeedBoost : MonoBehaviour
{
    [Header("Powerup Settings")]
    [SerializeField] private float speedIncreaseAmount = 5f;
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
                // Apply speed boost
                player.AddSpeed(speedIncreaseAmount);
                Debug.Log($"Speed Boost Collected! +{speedIncreaseAmount} Speed");
                
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
