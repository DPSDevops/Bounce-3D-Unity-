using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Checkpoint Settings")]
    [SerializeField] private Material activatedMaterial;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Renderer checkpointRenderer;
    [SerializeField] private ParticleSystem activationEffect;
    
    private bool isActivated = false;

    private void Start()
    {
        // Apply inactive material at start
        if (checkpointRenderer != null && inactiveMaterial != null)
        {
            checkpointRenderer.material = inactiveMaterial;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger if player enters and not already activated
        if (!isActivated && other.CompareTag("Player"))
        {
            ActivateCheckpoint();
        }
    }

    private void ActivateCheckpoint()
    {
        isActivated = true;
        
        // Notify Game Manager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCheckpoint(transform.position, transform.rotation);
        }
        
        // Visual Feedback
        if (checkpointRenderer != null && activatedMaterial != null)
        {
            checkpointRenderer.material = activatedMaterial;
        }
        
        if (activationEffect != null)
        {
            activationEffect.Play();
        }
        
        Debug.Log($"Checkpoint '{gameObject.name}' Activated!");
    }
}
