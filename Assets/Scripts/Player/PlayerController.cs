using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    // Stats are now controlled via GameManager
    private float groundMovementForce;
    private float airMovementForce;
    private float maxSpeed;
    private float jumpForce;
    
    [Header("Ground Control")]
    [SerializeField] private float groundDrag = 6f;
    [SerializeField] private float airDrag = 1f;
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayer;
    
    // Component references
    private Transform cameraTransform;
    private Rigidbody rb;
    
    // Input variables
    private Vector2 inputVector;
    private bool jumpInput;
    private bool isGrounded;
    
    /// <summary>
    /// Initialize component references
    /// </summary>
    void Start()
    {
        InitializeComponents();
        FindCamera();
        SyncStats(); // Get initial stats from GameManager
    }
    
    /// <summary>
    /// Handle input capture every frame
    /// </summary>
    void Update()
    {
        CaptureInput();
        CheckGroundStatus();
        HandleJump();
        CheckFallStatus();
    }
    
    /// <summary>
    /// Handle physics-based movement
    /// </summary>
    void FixedUpdate()
    {
        ApplyMovement(inputVector);
        ApplyDrag();
        LimitSpeed();
    }
    
    /// <summary>
    /// Initialize and cache component references
    /// </summary>
    private void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb == null)
        {
            Debug.LogError("PlayerController: Rigidbody component is required!");
        }
        
        // Configure Rigidbody for better sphere rolling
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }
    
    /// <summary>
    /// Find and cache the main camera transform
    /// </summary>
    private void FindCamera()
    {
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogWarning("PlayerController: Main camera not found!");
        }
    }
    
    /// <summary>
    /// Capture and process player input
    /// </summary>
    private void CaptureInput()
    {
        float horizontal = 0f;
        float vertical = 0f;
        
        // Check if keyboard is available
        if (Keyboard.current != null)
        {
            // Horizontal input (A/D or Left/Right arrows)
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                horizontal = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                horizontal = 1f;
            
            // Vertical input (W/S or Up/Down arrows)
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                vertical = -1f;
            else if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                vertical = 1f;
            
            // Jump input (Space key)
            jumpInput = Keyboard.current.spaceKey.wasPressedThisFrame;
        }
        
        inputVector = new Vector2(horizontal, vertical).normalized;
    }
    
    /// <summary>
    /// Check if the player is on the ground
    /// </summary>
    private void CheckGroundStatus()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }
    
    /// <summary>
    /// Check if player has fallen below threshold
    /// </summary>
    private void CheckFallStatus()
    {
        if (GameManager.Instance != null)
        {
            if (transform.position.y < GameManager.Instance.fallRespawnThreshold)
            {
                GameManager.Instance.SpawnPlayer();
            }
        }
    }
    
    /// <summary>
    /// Handle jump input and execution
    /// </summary>
    private void HandleJump()
    {
        if (jumpInput && isGrounded)
        {
            Jump();
        }
        
        // Reset jump input
        jumpInput = false;
    }
    
    /// <summary>
    /// Execute the jump by applying upward force
    /// </summary>
    private void Jump()
    {
        if (rb == null) return;
        
        // Apply upward force
        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }
    
    /// <summary>
    /// Apply movement force to the player based on input
    /// </summary>
    /// <param name="input">Normalized input vector from player</param>
    private void ApplyMovement(Vector2 input)
    {
        if (rb == null) return;
        
        // Calculate camera-relative movement direction
        Vector3 moveDirection = GetCameraRelativeMovement(input);
        
        // Select force based on state
        float currentForce = isGrounded ? groundMovementForce : airMovementForce;
        
        // Apply force to move the sphere
        rb.AddForce(moveDirection * currentForce, ForceMode.Acceleration);
    }
    
    /// <summary>
    /// Calculate movement direction relative to camera orientation
    /// </summary>
    /// <param name="input">Normalized input vector from player</param>
    /// <returns>Camera-relative movement direction</returns>
    private Vector3 GetCameraRelativeMovement(Vector2 input)
    {
        // If no camera found, use world-space movement
        if (cameraTransform == null)
        {
            return new Vector3(input.x, 0f, input.y);
        }
        
        // Get camera's forward and right vectors (projected onto horizontal plane)
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        
        // Project onto horizontal plane (ignore Y axis)
        cameraForward.y = 0f;
        cameraRight.y = 0f;
        
        // Normalize the vectors
        cameraForward.Normalize();
        cameraRight.Normalize();
        
        // Calculate movement direction relative to camera
        Vector3 moveDirection = (cameraRight * input.x) + (cameraForward * input.y);
        
        return moveDirection;
    }
    
    /// <summary>
    /// Apply drag based on ground status for better control
    /// </summary>
    private void ApplyDrag()
    {
        if (rb == null) return;
        
        rb.linearDamping = isGrounded ? groundDrag : airDrag;
    }
    
    /// <summary>
    /// Sync stats from GameManager
    /// </summary>
    public void SyncStats()
    {
        if (GameManager.Instance != null)
        {
            groundMovementForce = GameManager.Instance.currentGroundSpeed;
            airMovementForce = GameManager.Instance.currentAirSpeed;
            maxSpeed = GameManager.Instance.currentMaxSpeed;
            jumpForce = GameManager.Instance.currentJumpForce;
        }
    }
    
    /// <summary>
    /// Increase jump force by a specific amount
    /// </summary>
    /// <param name="amount">Amount to increase jump force by</param>
    public void AddJumpForce(float amount)
    {
        // Delegate to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncreaseJump(amount);
        }
    }
    
    /// <summary>
    /// Increase movement speed and max speed by a specific amount
    /// </summary>
    /// <param name="amount">Amount to increase speed by</param>
    public void AddSpeed(float amount)
    {
        // Delegate to GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.IncreaseSpeed(amount);
        }
    }
    
    /// <summary>
    /// Limit the maximum speed of the player
    /// </summary>
    private void LimitSpeed()
    {
        if (rb == null) return;
        
        Vector3 flatVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        // Limit velocity if exceeding max speed
        if (flatVelocity.magnitude > maxSpeed)
        {
            Vector3 limitedVelocity = flatVelocity.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVelocity.x, rb.linearVelocity.y, limitedVelocity.z);
        }
    }
    
    /// <summary>
    /// Draw debug visualization in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Visualize ground check distance
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
