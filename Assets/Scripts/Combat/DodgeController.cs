using System.Collections;
using System;
using Game.Core;
using Game.Rhythm;
using UnityEngine;

namespace Game.Combat
{
    public class DodgeController : MonoBehaviour, IDamageResponder
    {
        [SerializeField] private InputBuffer dodgeBuffer;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float distance = 3f;
        [SerializeField] private float duration = 0.22f;
        [SerializeField] private float cooldown = 0.8f;
        [SerializeField] private float perfectInvulnerability = 0.35f;
        [SerializeField] private float goodInvulnerability = 0.22f;
        [SerializeField] private float missInvulnerability;
        [SerializeField] private bool readKeyboardInput = true;
        [SerializeField] private KeyCode dodgeKey = KeyCode.LeftShift;

        private Vector3 requestedDirection;
        private float cooldownRemaining;
        private float invulnerableUntil;
        private Coroutine dodgeRoutine;

        public event Action<RhythmGrade, float, float> DodgeResolved;
        public event Action<bool> DodgeStateChanged;

        public bool IsDodging { get; private set; }
        public bool IsInvulnerable => Time.time < invulnerableUntil;
        public float CooldownRemaining => cooldownRemaining;

        private void Awake()
        {
            dodgeBuffer = dodgeBuffer != null ? dodgeBuffer : GetComponent<InputBuffer>();
            characterController = characterController != null ? characterController : GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        private void OnEnable()
        {
            if (dodgeBuffer != null)
                dodgeBuffer.OnResolved += ResolveDodge;
        }

        private void OnDisable()
        {
            if (dodgeBuffer != null)
                dodgeBuffer.OnResolved -= ResolveDodge;
        }

        private void Update()
        {
            if (cooldownRemaining > 0f)
                cooldownRemaining -= Time.deltaTime;

            if (readKeyboardInput && Input.GetKeyDown(dodgeKey))
                RequestDodge(ReadMoveDirection());
        }

        public bool RequestDodge(Vector3 direction)
        {
            if (cooldownRemaining > 0f) return false;

            requestedDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
            if (dodgeBuffer != null)
                dodgeBuffer.RegisterPress();
            else
                ResolveDodge(RhythmGrade.Miss);

            return true;
        }

        public void ResolveDodgeForTests(RhythmGrade grade, Vector3 direction)
        {
            requestedDirection = direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
            ResolveDodge(grade);
        }

        public bool TryModifyDamage(ref DamageContext context)
        {
            if (!IsInvulnerable) return false;
            context.amount = 0;
            return true;
        }

        public void GrantInvulnerability(float seconds)
        {
            if (seconds <= 0f) return;
            invulnerableUntil = Mathf.Max(invulnerableUntil, Time.time + seconds);
        }

        private void ResolveDodge(RhythmGrade grade)
        {
            if (cooldownRemaining > 0f) return;

            float gradeDistance = grade == RhythmGrade.Miss ? distance * 0.6f : distance;
            float invulnerability = grade == RhythmGrade.Perfect ? perfectInvulnerability : grade == RhythmGrade.Good ? goodInvulnerability : missInvulnerability;
            float recoveryMultiplier = grade == RhythmGrade.Perfect ? 0.75f : grade == RhythmGrade.Miss ? 1.2f : 1f;

            invulnerableUntil = Time.time + Mathf.Max(0f, invulnerability);
            cooldownRemaining = Mathf.Max(0f, cooldown * recoveryMultiplier);
            DodgeResolved?.Invoke(grade, cooldownRemaining, invulnerability);

            if (dodgeRoutine != null)
                StopCoroutine(dodgeRoutine);
            dodgeRoutine = StartCoroutine(DodgeRoutine(requestedDirection, gradeDistance, duration * recoveryMultiplier));
        }

        private IEnumerator DodgeRoutine(Vector3 direction, float dodgeDistance, float dodgeDuration)
        {
            IsDodging = true;
            DodgeStateChanged?.Invoke(true);
            float elapsed = 0f;
            while (elapsed < dodgeDuration)
            {
                float step = dodgeDistance * (Time.deltaTime / Mathf.Max(0.01f, dodgeDuration));
                if (characterController != null)
                    characterController.Move(direction * step);
                else
                    transform.position += direction * step;

                elapsed += Time.deltaTime;
                yield return null;
            }
            IsDodging = false;
            DodgeStateChanged?.Invoke(false);
            dodgeRoutine = null;
        }

        private Vector3 ReadMoveDirection()
        {
            Vector3 direction = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            direction = Vector3.ClampMagnitude(direction, 1f);

            if (cameraTransform != null && direction.sqrMagnitude > 0.001f)
            {
                Vector3 forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                Vector3 right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
                direction = forward * direction.z + right * direction.x;
            }

            return direction.sqrMagnitude > 0.001f ? direction.normalized : transform.forward;
        }
    }
}
