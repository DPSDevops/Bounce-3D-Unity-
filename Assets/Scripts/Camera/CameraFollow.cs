using UnityEngine;
using UnityEngine.InputSystem;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private string playerTag = "Player";
    
    [Header("Follow Settings")]
    [SerializeField] private float distance = 10f;
    [SerializeField] private float height = 5f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private bool lookAtPlayer = true;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float rotationSmoothSpeed = 5f;
    
    // Component references
    private Transform playerTransform;
    
    // Camera orbit angle
    private float currentAngle = 0f;
    
    /// <summary>
    /// Initialize and find player by tag
    /// </summary>
    void Start()
    {
        FindPlayer();
    }
    
    /// <summary>
    /// Handle camera following in LateUpdate for smooth following after player movement
    /// </summary>
    void LateUpdate()
    {
        if (playerTransform == null)
        {
            // Try to find player again if not found
            FindPlayer();
            return;
        }
        
        HandleCameraRotation();
        FollowPlayer();
        
        if (lookAtPlayer)
        {
            LookAtPlayer();
        }
    }
    
    /// <summary>
    /// Find the player GameObject by tag
    /// </summary>
    private void FindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
            Debug.Log($"CameraFollow: Player found with tag '{playerTag}'");
        }
        else
        {
            Debug.LogWarning($"CameraFollow: No GameObject found with tag '{playerTag}'");
        }
    }
    
    /// <summary>
    /// Handle camera rotation input
    /// </summary>
    private void HandleCameraRotation()
    {
        float rotationInput = 0f;
        
        // Get horizontal input for camera rotation
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                rotationInput = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                rotationInput = 1f;
        }
        
        // Update camera angle
        currentAngle += rotationInput * rotationSpeed * Time.deltaTime;
    }
    
    /// <summary>
    /// Follow the player with smooth interpolation using orbital position
    /// </summary>
    private void FollowPlayer()
    {
        // Calculate orbital position around player
        float radianAngle = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(
            Mathf.Sin(radianAngle) * distance,
            height,
            Mathf.Cos(radianAngle) * distance
        );
        
        // Calculate desired position
        Vector3 desiredPosition = playerTransform.position + offset;
        
        // Smoothly interpolate to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        
        // Update camera position
        transform.position = smoothedPosition;
    }
    
    /// <summary>
    /// Smoothly rotate camera to look at the player
    /// </summary>
    private void LookAtPlayer()
    {
        // Calculate direction to player
        Vector3 directionToPlayer = playerTransform.position - transform.position;
        
        // Calculate target rotation
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
        
        // Smoothly rotate towards player
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }
    

}
