using Game.Audio;
using Game.Combat;
using Game.Core;
using Game.Rhythm;
using UnityEngine;

namespace Game.Animation
{
    public class CombatAnimationBridge : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private ComboSystem comboSystem;
        [SerializeField] private DodgeController dodgeController;
        [SerializeField] private ParryController parryController;
        [SerializeField] private AudioCuePlayer audioCuePlayer;
        [SerializeField] private AudioCueDefinition footstepCue;
        [SerializeField] private AudioCueDefinition weaponSwingCue;

        public const string IsMovingParameter = "IsMoving";
        public const string AttackStateParameter = "AttackState";
        public const string RhythmGradeParameter = "RhythmGrade";
        public const string IsDodgingParameter = "IsDodging";
        public const string IsParryingParameter = "IsParrying";
        public const string IsHitParameter = "IsHit";
        public const string IsDeadParameter = "IsDead";

        private static readonly int IsMovingHash = Animator.StringToHash(IsMovingParameter);
        private static readonly int AttackStateHash = Animator.StringToHash(AttackStateParameter);
        private static readonly int RhythmGradeHash = Animator.StringToHash(RhythmGradeParameter);
        private static readonly int IsDodgingHash = Animator.StringToHash(IsDodgingParameter);
        private static readonly int IsParryingHash = Animator.StringToHash(IsParryingParameter);
        private static readonly int IsHitHash = Animator.StringToHash(IsHitParameter);
        private static readonly int IsDeadHash = Animator.StringToHash(IsDeadParameter);
        private RhythmGrade lastGrade = RhythmGrade.Miss;
        private BaseCharacter character;
        private float hitUntil;

        private void Awake()
        {
            animator = animator != null ? animator : GetComponentInChildren<Animator>();
            comboSystem = comboSystem != null ? comboSystem : GetComponent<ComboSystem>();
            dodgeController = dodgeController != null ? dodgeController : GetComponent<DodgeController>();
            parryController = parryController != null ? parryController : GetComponent<ParryController>();
            audioCuePlayer = audioCuePlayer != null ? audioCuePlayer : GetComponent<AudioCuePlayer>();
            character = GetComponent<BaseCharacter>();
        }

        private void OnEnable()
        {
            if (comboSystem != null)
            {
                comboSystem.AttackWindupStarted += HandleAttackGrade;
                comboSystem.ComboStepResolved += HandleComboResolved;
            }
            if (character != null)
                character.OnDamaged += HandleDamaged;
        }

        private void OnDisable()
        {
            if (comboSystem != null)
            {
                comboSystem.AttackWindupStarted -= HandleAttackGrade;
                comboSystem.ComboStepResolved -= HandleComboResolved;
            }
            if (character != null)
                character.OnDamaged -= HandleDamaged;
        }

        private void Update()
        {
            if (animator == null) return;

            Vector2 move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            animator.SetBool(IsMovingHash, move.sqrMagnitude > 0.01f);
            animator.SetInteger(AttackStateHash, comboSystem != null ? (int)comboSystem.TimingState : 0);
            animator.SetInteger(RhythmGradeHash, (int)lastGrade);
            animator.SetBool(IsDodgingHash, dodgeController != null && dodgeController.IsDodging);
            animator.SetBool(IsParryingHash, parryController != null && parryController.IsParrying);
            animator.SetBool(IsHitHash, Time.time < hitUntil);
            animator.SetBool(IsDeadHash, character != null && !character.IsAlive);
        }

        public void BeginAttackActiveWindow()
        {
            OpenHitWindow();
        }

        public void EndAttackActiveWindow()
        {
            CloseHitWindow();
        }

        public void FinishRecovery()
        {
            FinishAttackRecovery();
        }

        public void OpenHitWindow()
        {
            comboSystem?.OpenHitWindow();
        }

        public void CloseHitWindow()
        {
            comboSystem?.CloseHitWindow();
        }

        public void FinishAttackRecovery()
        {
            comboSystem?.FinishAttackRecovery();
        }

        public void TriggerFootstep()
        {
            audioCuePlayer?.Play(footstepCue);
        }

        public void TriggerWeaponSwing()
        {
            audioCuePlayer?.Play(weaponSwingCue);
        }

        private void HandleAttackGrade(int step, RhythmGrade grade)
        {
            lastGrade = grade;
        }

        private void HandleComboResolved(int step, RhythmGrade grade, int damage)
        {
            lastGrade = grade;
        }

        private void HandleDamaged(BaseCharacter damaged)
        {
            hitUntil = Time.time + 0.16f;
        }
    }
}
