using Game.Audio;
using Game.Combat;
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

        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int AttackStateHash = Animator.StringToHash("AttackState");
        private static readonly int RhythmGradeHash = Animator.StringToHash("RhythmGrade");
        private static readonly int IsDodgingHash = Animator.StringToHash("IsDodging");
        private static readonly int IsParryingHash = Animator.StringToHash("IsParrying");
        private RhythmGrade lastGrade = RhythmGrade.Miss;

        private void Awake()
        {
            animator = animator != null ? animator : GetComponentInChildren<Animator>();
            comboSystem = comboSystem != null ? comboSystem : GetComponent<ComboSystem>();
            dodgeController = dodgeController != null ? dodgeController : GetComponent<DodgeController>();
            parryController = parryController != null ? parryController : GetComponent<ParryController>();
            audioCuePlayer = audioCuePlayer != null ? audioCuePlayer : GetComponent<AudioCuePlayer>();
        }

        private void OnEnable()
        {
            if (comboSystem != null)
            {
                comboSystem.AttackWindupStarted += HandleAttackGrade;
                comboSystem.ComboStepResolved += HandleComboResolved;
            }
        }

        private void OnDisable()
        {
            if (comboSystem != null)
            {
                comboSystem.AttackWindupStarted -= HandleAttackGrade;
                comboSystem.ComboStepResolved -= HandleComboResolved;
            }
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
        }

        public void BeginAttackActiveWindow()
        {
            comboSystem?.BeginAttackActiveWindow();
        }

        public void EndAttackActiveWindow()
        {
            comboSystem?.EndAttackActiveWindow();
        }

        public void FinishRecovery()
        {
            comboSystem?.FinishRecovery();
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
    }
}
