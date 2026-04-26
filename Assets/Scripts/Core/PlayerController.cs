using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
using Game.Rhythm;

namespace Game.Core
{
    /// <summary>
    /// Camera-aligned mobile-friendly movement plus rhythm-buffered attack input.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float rotationSpeed = 720f;
        [SerializeField] private float jumpHeight = 1.2f;
        [SerializeField] private float gravity = -20f;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float cameraFollowSpeed = 5f;
        [SerializeField] private bool driveCameraDirectly;
        [SerializeField] private InputBuffer attackBuffer;

        private CharacterController controller;
#if ENABLE_INPUT_SYSTEM
        private PlayerInput playerInput;
#endif
        private Vector2 movementInput;
        private Vector3 velocity;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            attackBuffer = attackBuffer != null ? attackBuffer : GetComponent<InputBuffer>();
#if ENABLE_INPUT_SYSTEM
            playerInput = GetComponent<PlayerInput>();
#endif
        }

        private void Start()
        {
            PlayerManager.Instance?.RegisterPlayer(transform);
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void OnDestroy()
        {
            PlayerManager.Instance?.UnregisterPlayer(transform);
        }

        private void Update()
        {
            ReadInput();
            HandleMovement();
            HandleCombatInput();
            if (driveCameraDirectly)
                HandleCameraFollow();
        }

        private void ReadInput()
        {
#if ENABLE_INPUT_SYSTEM
            if (playerInput != null && playerInput.actions != null)
            {
                var moveAction = playerInput.actions.FindAction("Move", false);
                movementInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
                return;
            }
#endif
            movementInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        }

        private void HandleMovement()
        {
            if (controller.isGrounded && velocity.y < 0f)
                velocity.y = -2f;

            Vector3 direction = new Vector3(movementInput.x, 0f, movementInput.y);
            direction = Vector3.ClampMagnitude(direction, 1f);

            if (cameraTransform != null)
            {
                Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
                direction = forward * direction.z + right * direction.x;
            }

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (JumpPressedThisFrame() && controller.isGrounded)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

            velocity.y += gravity * Time.deltaTime;
            controller.Move((direction * moveSpeed + velocity) * Time.deltaTime);
        }

        private void HandleCombatInput()
        {
            if (!AttackPressedThisFrame()) return;

            if (attackBuffer != null)
                attackBuffer.RegisterPress();
            else
                Debug.Log("Player attack requested, but no InputBuffer is attached.");
        }

        private bool AttackPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (playerInput != null && playerInput.actions != null)
            {
                var attack = playerInput.actions.FindAction("Attack", false);
                if (attack != null) return attack.WasPressedThisFrame();
            }
#endif
            return Input.GetButtonDown("Fire1");
        }

        private bool JumpPressedThisFrame()
        {
#if ENABLE_INPUT_SYSTEM
            if (playerInput != null && playerInput.actions != null)
            {
                var jump = playerInput.actions.FindAction("Jump", false);
                if (jump != null) return jump.WasPressedThisFrame();
            }
#endif
            return Input.GetButtonDown("Jump");
        }

        private void HandleCameraFollow()
        {
            if (cameraTransform == null) return;

            Vector3 targetPosition = transform.position - transform.forward * 5f + Vector3.up * 2f;
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraFollowSpeed * Time.deltaTime);
            cameraTransform.LookAt(transform);
        }
    }
}
