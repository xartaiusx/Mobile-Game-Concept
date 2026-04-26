using Game.Combat;
using Game.Audio;
using Game.Core;
using Game.Rhythm;
using UnityEngine;

namespace Game.Feedback
{
    public class RhythmFeedbackController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ComboSystem comboSystem;
        [SerializeField] private AbilityController abilityController;
        [SerializeField] private DodgeController dodgeController;
        [SerializeField] private ParryController parryController;
        [SerializeField] private BossTelegraphController bossTelegraphController;
        [SerializeField] private BaseCharacter playerCharacter;
        [SerializeField] private Transform playerAnchor;

        [Header("Prefabs")]
        [SerializeField] private GameObject perfectAttackPrefab;
        [SerializeField] private GameObject goodAttackPrefab;
        [SerializeField] private GameObject missAttackPrefab;
        [SerializeField] private GameObject dodgePrefab;
        [SerializeField] private GameObject parryPrefab;
        [SerializeField] private GameObject parrySuccessPrefab;
        [SerializeField] private GameObject bossWarningPrefab;
        [SerializeField] private GameObject bossImpactPrefab;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioCuePlayer cuePlayer;
        [SerializeField] private AudioCueDefinition perfectCue;
        [SerializeField] private AudioCueDefinition goodCue;
        [SerializeField] private AudioCueDefinition missCue;
        [SerializeField] private AudioCueDefinition dodgeCue;
        [SerializeField] private AudioCueDefinition parryCue;
        [SerializeField] private AudioCueDefinition bossWarningCue;
        [SerializeField] private AudioCueDefinition bossImpactCue;
        [SerializeField] private AudioCueDefinition playerDamageCue;
        [SerializeField] private AudioCueDefinition enemyDefeatedCue;
        [SerializeField] private AudioClip perfectClip;
        [SerializeField] private AudioClip goodClip;
        [SerializeField] private AudioClip missClip;
        [SerializeField] private AudioClip defensiveClip;
        [SerializeField] private AudioClip bossClip;

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void BindPlayer(GameObject player)
        {
            if (playerCharacter != null)
                playerCharacter.OnDamaged -= HandlePlayerDamaged;
            if (comboSystem != null)
            {
                comboSystem.ComboStepResolved -= HandleComboStep;
                comboSystem.AttackWindupStarted -= HandleAttackWindup;
            }
            if (abilityController != null)
                abilityController.AbilityResolved -= HandleAbilityResolved;
            if (dodgeController != null)
                dodgeController.DodgeResolved -= HandleDodgeResolved;
            if (parryController != null)
            {
                parryController.ParryResolved -= HandleParryResolved;
                parryController.ParrySucceeded -= HandleParrySucceeded;
            }

            playerAnchor = player != null ? player.transform : null;
            playerCharacter = player != null ? player.GetComponent<BaseCharacter>() : null;
            comboSystem = player != null ? player.GetComponent<ComboSystem>() : null;
            abilityController = player != null ? player.GetComponent<AbilityController>() : null;
            dodgeController = player != null ? player.GetComponent<DodgeController>() : null;
            parryController = player != null ? player.GetComponent<ParryController>() : null;

            if (!isActiveAndEnabled) return;
            if (comboSystem != null)
            {
                comboSystem.ComboStepResolved += HandleComboStep;
                comboSystem.AttackWindupStarted += HandleAttackWindup;
            }
            if (abilityController != null)
                abilityController.AbilityResolved += HandleAbilityResolved;
            if (dodgeController != null)
                dodgeController.DodgeResolved += HandleDodgeResolved;
            if (parryController != null)
            {
                parryController.ParryResolved += HandleParryResolved;
                parryController.ParrySucceeded += HandleParrySucceeded;
            }
            if (playerCharacter != null)
                playerCharacter.OnDamaged += HandlePlayerDamaged;
        }

        private void ResolveReferences()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            if (cuePlayer == null)
                cuePlayer = GetComponent<AudioCuePlayer>();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerAnchor = playerAnchor != null ? playerAnchor : player.transform;
                playerCharacter = playerCharacter != null ? playerCharacter : player.GetComponent<BaseCharacter>();
                comboSystem = comboSystem != null ? comboSystem : player.GetComponent<ComboSystem>();
                abilityController = abilityController != null ? abilityController : player.GetComponent<AbilityController>();
                dodgeController = dodgeController != null ? dodgeController : player.GetComponent<DodgeController>();
                parryController = parryController != null ? parryController : player.GetComponent<ParryController>();
            }

            if (bossTelegraphController == null)
                bossTelegraphController = FindAnyObjectByType<BossTelegraphController>();
        }

        private void Subscribe()
        {
            if (comboSystem != null)
            {
                comboSystem.ComboStepResolved += HandleComboStep;
                comboSystem.AttackWindupStarted += HandleAttackWindup;
            }

            if (abilityController != null)
                abilityController.AbilityResolved += HandleAbilityResolved;

            if (dodgeController != null)
                dodgeController.DodgeResolved += HandleDodgeResolved;

            if (parryController != null)
            {
                parryController.ParryResolved += HandleParryResolved;
                parryController.ParrySucceeded += HandleParrySucceeded;
            }

            if (bossTelegraphController != null)
            {
                bossTelegraphController.TelegraphStarted += HandleTelegraphStarted;
                bossTelegraphController.TelegraphBeat += HandleTelegraphBeat;
                bossTelegraphController.TelegraphImpacted += HandleTelegraphImpact;
            }

            if (playerCharacter != null)
                playerCharacter.OnDamaged += HandlePlayerDamaged;

            BaseEnemy.EnemyDefeatedGlobal += HandleEnemyDefeated;
        }

        private void Unsubscribe()
        {
            if (comboSystem != null)
            {
                comboSystem.ComboStepResolved -= HandleComboStep;
                comboSystem.AttackWindupStarted -= HandleAttackWindup;
            }

            if (abilityController != null)
                abilityController.AbilityResolved -= HandleAbilityResolved;

            if (dodgeController != null)
                dodgeController.DodgeResolved -= HandleDodgeResolved;

            if (parryController != null)
            {
                parryController.ParryResolved -= HandleParryResolved;
                parryController.ParrySucceeded -= HandleParrySucceeded;
            }

            if (bossTelegraphController != null)
            {
                bossTelegraphController.TelegraphStarted -= HandleTelegraphStarted;
                bossTelegraphController.TelegraphBeat -= HandleTelegraphBeat;
                bossTelegraphController.TelegraphImpacted -= HandleTelegraphImpact;
            }

            if (playerCharacter != null)
                playerCharacter.OnDamaged -= HandlePlayerDamaged;

            BaseEnemy.EnemyDefeatedGlobal -= HandleEnemyDefeated;
        }

        private void HandleAttackWindup(int step, RhythmGrade grade)
        {
            SpawnAtPlayer(PrefabForGrade(grade), 0.6f);
        }

        private void HandleComboStep(int step, RhythmGrade grade, int damage)
        {
            SpawnAtPlayer(PrefabForGrade(grade), 0.8f);
            PlayGrade(grade);
        }

        private void HandleAbilityResolved(AbilityDefinition ability, RhythmGrade grade, float cooldown)
        {
            SpawnAtPlayer(PrefabForGrade(grade), 1f);
            PlayGrade(grade);
        }

        private void HandleDodgeResolved(RhythmGrade grade, float cooldown, float invulnerability)
        {
            SpawnAtPlayer(dodgePrefab != null ? dodgePrefab : PrefabForGrade(grade), 0.35f);
            cuePlayer?.Play(dodgeCue);
            PlayOneShot(defensiveClip);
        }

        private void HandleParryResolved(RhythmGrade grade, float cooldown, float activeWindow)
        {
            SpawnAtPlayer(parryPrefab != null ? parryPrefab : PrefabForGrade(grade), 0.45f);
            cuePlayer?.Play(parryCue);
            PlayOneShot(defensiveClip);
        }

        private void HandleParrySucceeded(Game.Core.DamageContext context)
        {
            SpawnAtPlayer(parrySuccessPrefab != null ? parrySuccessPrefab : parryPrefab != null ? parryPrefab : perfectAttackPrefab, 0.75f);
            cuePlayer?.Play(parryCue);
            PlayOneShot(defensiveClip);
        }

        private void HandleTelegraphStarted(BossTelegraphData data, int beats, Vector3 point)
        {
            SpawnAtPoint(bossWarningPrefab, point, Mathf.Max(0.75f, data.radius));
            cuePlayer?.Play(bossWarningCue);
            PlayOneShot(bossClip);
        }

        private void HandleTelegraphBeat(BossTelegraphData data, int beats)
        {
            SpawnAtPoint(bossWarningPrefab, bossTelegraphController != null ? bossTelegraphController.ImpactPoint : transform.position, Mathf.Max(0.75f, data.radius));
            cuePlayer?.Play(bossWarningCue);
        }

        private void HandleTelegraphImpact(BossTelegraphData data, Vector3 point)
        {
            SpawnAtPoint(bossImpactPrefab, point, Mathf.Max(0.75f, data.radius));
            cuePlayer?.Play(bossImpactCue);
            PlayOneShot(bossClip);
        }

        private void HandlePlayerDamaged(BaseCharacter character)
        {
            cuePlayer?.Play(playerDamageCue);
        }

        private void HandleEnemyDefeated(BaseEnemy enemy)
        {
            cuePlayer?.Play(enemyDefeatedCue);
        }

        private GameObject PrefabForGrade(RhythmGrade grade)
        {
            switch (grade)
            {
                case RhythmGrade.Perfect: return perfectAttackPrefab;
                case RhythmGrade.Good: return goodAttackPrefab;
                default: return missAttackPrefab;
            }
        }

        private void SpawnAtPlayer(GameObject prefab, float scale)
        {
            if (playerAnchor == null) return;
            SpawnAtPoint(prefab, playerAnchor.position + Vector3.up * 1.1f, scale);
        }

        private void SpawnAtPoint(GameObject prefab, Vector3 point, float scale)
        {
            if (prefab == null) return;
            GameObject instance = Instantiate(prefab, point, Quaternion.identity);
            instance.transform.localScale = Vector3.one * Mathf.Max(0.1f, scale);
        }

        private void PlayGrade(RhythmGrade grade)
        {
            switch (grade)
            {
                case RhythmGrade.Perfect:
                    cuePlayer?.Play(perfectCue);
                    PlayOneShot(perfectClip);
                    break;
                case RhythmGrade.Good:
                    cuePlayer?.Play(goodCue);
                    PlayOneShot(goodClip);
                    break;
                default:
                    cuePlayer?.Play(missCue);
                    PlayOneShot(missClip);
                    break;
            }
        }

        private void PlayOneShot(AudioClip clip)
        {
            if (audioSource != null && clip != null)
                audioSource.PlayOneShot(clip);
        }
    }
}
