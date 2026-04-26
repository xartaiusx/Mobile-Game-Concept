using Game.Combat;
using Game.Core;
using Game.AI.Enemies;
using Game.Rhythm;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public class VerticalSliceHud : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ComboSystem comboSystem;
        [SerializeField] private AbilityController abilityController;
        [SerializeField] private DodgeController dodgeController;
        [SerializeField] private ParryController parryController;
        [SerializeField] private BossTelegraphController bossTelegraphController;
        [SerializeField] private BossPhaseController bossPhaseController;
        [SerializeField] private BaseCharacter playerCharacter;
        [SerializeField] private ArenaController arenaController;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private InventorySystem inventorySystem;

        [Header("Text")]
        [SerializeField] private Text feedbackText;
        [SerializeField] private Text comboText;
        [SerializeField] private Text abilityText;
        [SerializeField] private Text dodgeText;
        [SerializeField] private Text parryText;
        [SerializeField] private Text attackStateText;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text inventoryText;
        [SerializeField] private Text bossText;
        [SerializeField] private Text arenaText;
        [SerializeField] private Text debugText;
        [SerializeField] private bool showDebugOverlay = true;

        private RhythmGrade lastGrade = RhythmGrade.Miss;
        private string lastFeedback = "Ready";

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
            RefreshStaticText();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (comboText != null)
                comboText.text = "Combo: " + (comboSystem != null ? comboSystem.CurrentComboCount.ToString() : "0");

            if (abilityText != null)
                abilityText.text = FormatAbilityText();

            if (dodgeText != null)
                dodgeText.text = "Dodge: " + FormatCooldown(dodgeController != null ? dodgeController.CooldownRemaining : 0f) + (dodgeController != null && dodgeController.IsInvulnerable ? "  INV" : string.Empty);

            if (parryText != null)
                parryText.text = "Parry: " + FormatCooldown(parryController != null ? parryController.CooldownRemaining : 0f) + (parryController != null && parryController.IsParrying ? "  ACTIVE" : string.Empty);

            if (attackStateText != null)
                attackStateText.text = "Attack: " + (comboSystem != null ? comboSystem.TimingState.ToString() : "Ready");

            if (scoreText != null)
                scoreText.text = "Score: " + (scoreSystem != null ? scoreSystem.Score.ToString() : "0");

            if (inventoryText != null)
                inventoryText.text = FormatInventoryText();

            if (bossText != null && bossTelegraphController != null && bossTelegraphController.IsTelegraphing)
                bossText.text = "Boss: " + bossTelegraphController.ActiveTelegraph.displayName + " in " + bossTelegraphController.RemainingBeats + " beats";
            else if (bossText != null)
                bossText.text = "Boss: watching";

            if (arenaText != null && arenaController != null)
                arenaText.text = "Arena: " + arenaController.State + "  " + arenaController.ActiveEnemyCount;

            if (debugText != null)
            {
                debugText.gameObject.SetActive(showDebugOverlay);
                if (showDebugOverlay)
                    debugText.text = FormatDebugText();
            }
        }

        public void BindPlayer(GameObject player)
        {
            if (player == null) return;
            comboSystem = player.GetComponent<ComboSystem>();
            abilityController = player.GetComponent<AbilityController>();
            dodgeController = player.GetComponent<DodgeController>();
            parryController = player.GetComponent<ParryController>();
            playerCharacter = player.GetComponent<BaseCharacter>();
        }

        private void ResolveReferences()
        {
            if (comboSystem == null || abilityController == null || dodgeController == null || parryController == null || playerCharacter == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    BindPlayer(player);
            }

            if (bossTelegraphController == null)
                bossTelegraphController = FindAnyObjectByType<BossTelegraphController>();
            if (bossPhaseController == null)
                bossPhaseController = FindAnyObjectByType<BossPhaseController>();
            if (arenaController == null)
                arenaController = FindAnyObjectByType<ArenaController>();
            if (scoreSystem == null)
                scoreSystem = FindAnyObjectByType<ScoreSystem>();
            if (inventorySystem == null)
                inventorySystem = FindAnyObjectByType<InventorySystem>();
        }

        private void Subscribe()
        {
            if (comboSystem != null)
            {
                comboSystem.ComboStepResolved += HandleComboStep;
                comboSystem.DamageDealt += HandleDamageDealt;
                comboSystem.ComboReset += HandleComboReset;
            }

            if (abilityController != null)
            {
                abilityController.AbilityResolved += HandleAbilityResolved;
                abilityController.AbilityEffectApplied += HandleAbilityEffectApplied;
            }

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

            if (bossPhaseController != null)
                bossPhaseController.PhaseChanged += HandleBossPhaseChanged;

            if (playerCharacter != null)
                playerCharacter.OnDamaged += HandlePlayerDamaged;

            if (arenaController != null)
                arenaController.StateChanged += HandleArenaStateChanged;

            if (scoreSystem != null)
                scoreSystem.ScoreChanged += HandleScoreChanged;

            if (inventorySystem != null)
                inventorySystem.OnInventoryChanged += HandleInventoryChanged;
        }

        private void Unsubscribe()
        {
            if (comboSystem != null)
            {
                comboSystem.ComboStepResolved -= HandleComboStep;
                comboSystem.DamageDealt -= HandleDamageDealt;
                comboSystem.ComboReset -= HandleComboReset;
            }

            if (abilityController != null)
            {
                abilityController.AbilityResolved -= HandleAbilityResolved;
                abilityController.AbilityEffectApplied -= HandleAbilityEffectApplied;
            }

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

            if (bossPhaseController != null)
                bossPhaseController.PhaseChanged -= HandleBossPhaseChanged;

            if (playerCharacter != null)
                playerCharacter.OnDamaged -= HandlePlayerDamaged;

            if (arenaController != null)
                arenaController.StateChanged -= HandleArenaStateChanged;

            if (scoreSystem != null)
                scoreSystem.ScoreChanged -= HandleScoreChanged;

            if (inventorySystem != null)
                inventorySystem.OnInventoryChanged -= HandleInventoryChanged;
        }

        private void RefreshStaticText()
        {
            if (feedbackText != null)
                feedbackText.text = lastFeedback;
        }

        private void HandleComboStep(int step, RhythmGrade grade, int damage)
        {
            lastGrade = grade;
            lastFeedback = grade + " attack  " + damage;
            if (feedbackText != null)
                feedbackText.text = lastFeedback;
        }

        private void HandleDamageDealt(int step, RhythmGrade grade, int damage, BaseEnemy enemy)
        {
            lastGrade = grade;
            lastFeedback = grade + " hit  " + damage;
            if (feedbackText != null)
                feedbackText.text = lastFeedback;
        }

        private void HandleComboReset()
        {
            if (feedbackText != null)
                feedbackText.text = "Combo reset";
        }

        private void HandleAbilityResolved(AbilityDefinition ability, RhythmGrade grade, float cooldown)
        {
            lastGrade = grade;
            if (feedbackText != null)
                feedbackText.text = grade + " " + ability.displayName;
        }

        private void HandleAbilityEffectApplied(AbilityDefinition ability, RhythmGrade grade, int amount)
        {
            lastGrade = grade;
            if (feedbackText != null)
                feedbackText.text = grade + " " + ability.displayName + "  " + amount;
        }

        private void HandleDodgeResolved(RhythmGrade grade, float cooldown, float invulnerability)
        {
            lastGrade = grade;
            if (feedbackText != null)
                feedbackText.text = grade + " Dodge" + (invulnerability > 0f ? " INV" : string.Empty);
        }

        private void HandleParryResolved(RhythmGrade grade, float cooldown, float activeWindow)
        {
            lastGrade = grade;
            if (feedbackText != null)
                feedbackText.text = grade + " Parry";
        }

        private void HandleParrySucceeded(DamageContext context)
        {
            if (feedbackText != null)
                feedbackText.text = context.amount <= 0 ? "Parry Success" : "Parry Reduced";
        }

        private void HandleTelegraphStarted(BossTelegraphData data, int beats, Vector3 point)
        {
            if (bossText != null)
                bossText.text = "Boss: " + data.displayName + " in " + beats + " beats";
            if (feedbackText != null)
                feedbackText.text = "Boss Warning";
        }

        private void HandleTelegraphBeat(BossTelegraphData data, int beats)
        {
            if (bossText != null)
                bossText.text = "Boss: " + data.displayName + " in " + Mathf.Max(0, beats) + " beats";
        }

        private void HandleTelegraphImpact(BossTelegraphData data, Vector3 point)
        {
            if (bossText != null)
                bossText.text = "Boss: impact";
            if (feedbackText != null)
                feedbackText.text = "Boss Impact";
        }

        private void HandleBossPhaseChanged(BossPhaseData phase, int phaseIndex)
        {
            string label = phase != null && !string.IsNullOrEmpty(phase.displayName) ? phase.displayName : "Phase " + phaseIndex;
            if (bossText != null)
                bossText.text = "Boss: " + label;
            if (feedbackText != null)
                feedbackText.text = "Boss " + label;
        }

        private void HandlePlayerDamaged(BaseCharacter character)
        {
            if (feedbackText != null)
                feedbackText.text = "Player Hit";
        }

        private void HandleScoreChanged(int total, int added, RhythmGrade grade)
        {
            if (scoreText != null)
                scoreText.text = "Score: " + total;
            if (added > 0 && feedbackText != null)
                feedbackText.text = grade + " +" + added;
        }

        private void HandleInventoryChanged()
        {
            if (inventoryText != null)
                inventoryText.text = FormatInventoryText();
        }

        private void HandleArenaStateChanged(ArenaState state, string message)
        {
            if (arenaText != null)
                arenaText.text = "Arena: " + message;
            if ((state == ArenaState.Victory || state == ArenaState.Failure) && feedbackText != null)
                feedbackText.text = message;
        }

        private string FormatAbilityText()
        {
            if (abilityController == null || abilityController.Abilities == null || abilityController.Abilities.Length == 0 || abilityController.Abilities[0] == null)
                return "Ability: none";

            float remaining = abilityController.GetCooldownRemaining(0);
            string status = remaining <= 0f ? "Ready" : remaining.ToString("0.0") + "s";
            return "Ability: " + abilityController.Abilities[0].displayName + "  " + status;
        }

        private string FormatInventoryText()
        {
            if (inventorySystem == null) return "Gold: 0  Potions: 0";

            int gold = 0;
            int potions = 0;
            var items = inventorySystem.Items;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                string id = item.definition != null ? item.definition.itemId : item.itemName;
                if (id == "gold" || id == "Gold")
                    gold += item.count;
                else if (id == "health_potion" || id == "Health Potion")
                    potions += item.count;
            }

            return "Gold: " + gold + "  Potions: " + potions;
        }

        private static string FormatCooldown(float remaining)
        {
            return remaining <= 0f ? "Ready" : remaining.ToString("0.0") + "s";
        }

        private string FormatDebugText()
        {
            BeatClock clock = BeatClock.Instance;
            int enemyCount = FindObjectsByType<BaseEnemy>(FindObjectsInactive.Exclude).Length;
            string beat = clock != null ? clock.CurrentBeatIndex + " / " + clock.CurrentPhase.ToString("0.00") : "none";
            string hp = playerCharacter != null ? playerCharacter.CurrentHealth + "/" + playerCharacter.MaxHealth : "n/a";
            string attack = comboSystem != null ? comboSystem.TimingState.ToString() : "Ready";
            string bpm = clock != null ? clock.GetEffectiveBpm().ToString("0") : "n/a";
            return "BPM " + bpm + "\nBeat " + beat + "\nLast " + lastGrade + "\nAttack " + attack + "\nHP " + hp + "\nEnemies " + enemyCount;
        }
    }
}
