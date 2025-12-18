using UnityEngine;

public class LevelFinish : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string playerTag = "Player";

    /// <summary>
    /// Detect physical collision with the player
    /// </summary>
    /// <param name="collision">Collision data</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(playerTag))
        {
            Debug.Log("Level Done");
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
            Debug.Log("Level Done");
        }
    }
}
