using Game.Combat;
using Game.Rhythm;
using UnityEngine;

namespace Game.Core
{
    public enum PlayerSimulationState
    {
        ApproachEnemy,
        MaintainDistance,
        AttackOnBeat,
        DodgeThreat,
        ParryTelegraph,
        UseAbility
    }

    [RequireComponent(typeof(CharacterController))]
    public class PlayerSimulationController : MonoBehaviour
    {
        [SerializeField] private bool simulationEnabled;
        [SerializeField] private float preferredRange = 2.2f;
        [SerializeField] private float rangedPreferredRange = 6f;
        [SerializeField] private float moveSpeed = 4.5f;
        [SerializeField] private float beatPressLead = 0.035f;
        [SerializeField] private float mistimedPressChance = 0.22f;
        [SerializeField] private float decisionInterval = 0.12f;
        [SerializeField] private float dodgeThreatRange = 2.1f;

        private CharacterController characterController;
        private ComboSystem comboSystem;
        private AbilityController abilityController;
        private DodgeController dodgeController;
        private ParryController parryController;
        private BossTelegraphController bossTelegraph;
        private float nextDecisionTime;
        private int lastActionBeat = -1;

        public PlayerSimulationState State { get; private set; } = PlayerSimulationState.ApproachEnemy;
        public int PerfectCount { get; private set; }
        public int GoodCount { get; private set; }
        public int MissCount { get; private set; }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            comboSystem = GetComponent<ComboSystem>();
            abilityController = GetComponent<AbilityController>();
            dodgeController = GetComponent<DodgeController>();
            parryController = GetComponent<ParryController>();
        }

        private void Update()
        {
            if (!simulationEnabled) return;
            if (Time.time < nextDecisionTime) return;
            nextDecisionTime = Time.time + decisionInterval;

            bossTelegraph = bossTelegraph != null ? bossTelegraph : FindAnyObjectByType<BossTelegraphController>();
            BaseEnemy target = FindNearestEnemy();
            if (target == null) return;

            float distance = Vector3.Distance(transform.position, target.transform.position);
            Face(target.transform.position);

            if (bossTelegraph != null && bossTelegraph.IsTelegraphing && parryController != null && parryController.CooldownRemaining <= 0f)
            {
                State = PlayerSimulationState.ParryTelegraph;
                TryBeatAction(() => parryController.RequestParry(), true);
                return;
            }

            if (distance <= dodgeThreatRange && dodgeController != null && dodgeController.CooldownRemaining <= 0f)
            {
                State = PlayerSimulationState.DodgeThreat;
                Vector3 away = (transform.position - target.transform.position).normalized;
                dodgeController.RequestDodge(away);
                return;
            }

            float desiredRange = abilityController != null && abilityController.Abilities != null && abilityController.Abilities.Length > 0 && abilityController.Abilities[0] != null && abilityController.Abilities[0].executionStyle == AbilityExecutionStyle.ProjectileLike
                ? rangedPreferredRange
                : preferredRange;

            if (abilityController != null && abilityController.CanRequest(0) && distance <= abilityController.Abilities[0].range)
            {
                State = PlayerSimulationState.UseAbility;
                TryBeatAction(() => abilityController.RequestAbility(0), false);
                return;
            }

            if (distance > desiredRange)
            {
                State = PlayerSimulationState.ApproachEnemy;
                MoveToward(target.transform.position);
                return;
            }

            if (distance < Mathf.Max(1.5f, desiredRange * 0.65f))
            {
                State = PlayerSimulationState.MaintainDistance;
                MoveAway(target.transform.position);
                return;
            }

            State = PlayerSimulationState.AttackOnBeat;
            TryBeatAction(() =>
            {
                var buffer = GetComponent<InputBuffer>();
                buffer?.RegisterPress();
                return true;
            }, false);
        }

        private void OnEnable()
        {
            if (comboSystem != null)
                comboSystem.ComboStepResolved += HandleGrade;
            if (abilityController != null)
                abilityController.AbilityResolved += HandleAbilityGrade;
            if (dodgeController != null)
                dodgeController.DodgeResolved += HandleDodgeGrade;
        }

        private void OnDisable()
        {
            if (comboSystem != null)
                comboSystem.ComboStepResolved -= HandleGrade;
            if (abilityController != null)
                abilityController.AbilityResolved -= HandleAbilityGrade;
            if (dodgeController != null)
                dodgeController.DodgeResolved -= HandleDodgeGrade;
        }

        public void SetSimulationEnabled(bool enabled)
        {
            simulationEnabled = enabled;
        }

        private void TryBeatAction(System.Func<bool> action, bool preferPerfect)
        {
            BeatClock clock = BeatClock.Instance;
            if (clock == null)
            {
                action();
                return;
            }

            int beat = clock.CurrentBeatIndex;
            if (beat == lastActionBeat) return;

            double phase = clock.CurrentPhase;
            bool intentionalMistime = !preferPerfect && Random.value < mistimedPressChance;
            bool intentionalGood = !preferPerfect && !intentionalMistime && Random.value < 0.28f;
            bool onBeatWindow = phase >= 1.0 - beatPressLead || phase <= beatPressLead;
            bool goodWindow = phase > 0.09 && phase < 0.18;
            bool mistimeWindow = phase > 0.28 && phase < 0.55;
            if ((intentionalMistime && mistimeWindow) || (intentionalGood && goodWindow) || (!intentionalMistime && !intentionalGood && onBeatWindow))
            {
                if (action())
                    lastActionBeat = beat;
            }
        }

        private void HandleGrade(int step, RhythmGrade grade, int damage)
        {
            CountGrade(grade);
        }

        private void HandleAbilityGrade(AbilityDefinition ability, RhythmGrade grade, float cooldown)
        {
            CountGrade(grade);
        }

        private void HandleDodgeGrade(RhythmGrade grade, float cooldown, float invulnerability)
        {
            CountGrade(grade);
        }

        private void CountGrade(RhythmGrade grade)
        {
            switch (grade)
            {
                case RhythmGrade.Perfect:
                    PerfectCount++;
                    break;
                case RhythmGrade.Good:
                    GoodCount++;
                    break;
                default:
                    MissCount++;
                    break;
            }
        }

        private BaseEnemy FindNearestEnemy()
        {
            BaseEnemy[] enemies = FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude);
            BaseEnemy best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < enemies.Length; i++)
            {
                BaseEnemy enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive) continue;
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = enemy;
                }
            }
            return best;
        }

        private void Face(Vector3 point)
        {
            Vector3 flat = point - transform.position;
            flat.y = 0f;
            if (flat.sqrMagnitude <= 0.001f) return;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(flat.normalized), 720f * Time.deltaTime);
        }

        private void MoveToward(Vector3 point)
        {
            Move((point - transform.position).normalized);
        }

        private void MoveAway(Vector3 point)
        {
            Move((transform.position - point).normalized);
        }

        private void Move(Vector3 direction)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f) return;
            characterController.Move(direction.normalized * moveSpeed * Time.deltaTime);
        }
    }
}
