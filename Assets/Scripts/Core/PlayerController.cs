using UnityEngine;

/// <summary>
/// Handles player movement and basic input.
/// </summary>
public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f;
    public Camera playerCamera;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMovement();
        HandleCombatInput();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontal, 0, vertical).normalized;

        if (direction.magnitude > 0)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        controller.Move(direction * moveSpeed * Time.deltaTime);
    }

    private void HandleCombatInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Placeholder combat logic - replace with animation & attacks later
            Debug.Log("Player attacked!");
        }
    }
}
