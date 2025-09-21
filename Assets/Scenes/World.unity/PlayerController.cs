using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles player movement and basic input using the new Unity Input System.
/// </summary>
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public Transform cameraTransform;
    public float cameraFollowSpeed = 5f;
    private PlayerInput playerInput;
    private Vector2 movementInput;
    private Vector3 velocity;
    public float gravity = -9.81f;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        
        if (controller == null)
        {
            Debug.LogError("CharacterController component is missing on " + gameObject.name);
        }
    }

    private void Update()
    {
        HandleMovement();
        HandleCombatInput();
        HandleCameraFollow();
    }

    private void HandleMovement()
    {
        movementInput = playerInput.actions["Move"].ReadValue<Vector2>();
        Vector3 direction = new Vector3(movementInput.x, 0, movementInput.y).normalized;

        if (direction.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime; // Apply gravity
        Vector3 move = direction * moveSpeed * Time.deltaTime + velocity * Time.deltaTime;
        controller.Move(move);
    }

    private void HandleCombatInput()
    {
        if (playerInput.actions["Attack"].WasPressedThisFrame())
        {
            // Placeholder combat logic - replace with animation & attacks later
            Debug.Log("Player attacked!");
        }
    }

    private void HandleCameraFollow()
    {
        if (cameraTransform != null)
        {
            Vector3 targetPosition = transform.position - transform.forward * 5f + Vector3.up * 2f;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);
            cameraTransform.LookAt(transform);
        }
    }
}
