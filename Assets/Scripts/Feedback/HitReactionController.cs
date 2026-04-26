using System.Collections;
using Game.Combat;
using Game.Core;
using Game.Rhythm;
using UnityEngine;

namespace Game.Feedback
{
    public class HitReactionController : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private float flashSeconds = 0.08f;
        [SerializeField] private float knockbackDistance = 0.25f;
        [SerializeField] private float perfectKnockbackMultiplier = 2f;
        [SerializeField] private Color hitColor = Color.white;
        [SerializeField] private Color perfectHitColor = new Color(0.2f, 1f, 0.75f);
        [SerializeField] private Color playerDamageColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private HitPause hitPause;

        private BaseEnemy enemy;
        private BaseCharacter character;
        private Color originalColor;
        private Material runtimeMaterial;
        private Coroutine flashRoutine;

        private void Awake()
        {
            targetRenderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();
            characterController = characterController != null ? characterController : GetComponent<CharacterController>();
            enemy = GetComponent<BaseEnemy>();
            character = GetComponent<BaseCharacter>();
            hitPause = hitPause != null ? hitPause : GetComponent<HitPause>();

            if (targetRenderer != null)
            {
                runtimeMaterial = targetRenderer.material;
                originalColor = runtimeMaterial.color;
            }
        }

        private void OnEnable()
        {
            if (enemy != null)
                enemy.Damaged += HandleEnemyDamaged;
            if (character != null)
                character.OnDamaged += HandleCharacterDamaged;
        }

        private void OnDisable()
        {
            if (enemy != null)
                enemy.Damaged -= HandleEnemyDamaged;
            if (character != null)
                character.OnDamaged -= HandleCharacterDamaged;
        }

        private void HandleEnemyDamaged(BaseEnemy damagedEnemy, DamageContext context)
        {
            Color color = context.rhythmGrade == RhythmGrade.Perfect ? perfectHitColor : hitColor;
            Flash(color);
            ApplyKnockback(context.source, context.rhythmGrade == RhythmGrade.Perfect ? perfectKnockbackMultiplier : 1f);
            if (context.rhythmGrade == RhythmGrade.Perfect)
                hitPause?.TriggerPause();
        }

        private void HandleCharacterDamaged(BaseCharacter damagedCharacter)
        {
            Flash(playerDamageColor);
        }

        private void Flash(Color color)
        {
            if (runtimeMaterial == null) return;
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine(color));
        }

        private IEnumerator FlashRoutine(Color color)
        {
            runtimeMaterial.color = color;
            yield return new WaitForSeconds(flashSeconds);
            runtimeMaterial.color = originalColor;
            flashRoutine = null;
        }

        private void ApplyKnockback(GameObject source, float multiplier)
        {
            if (source == null || knockbackDistance <= 0f) return;
            Vector3 direction = transform.position - source.transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude <= 0.001f) return;
            Vector3 delta = direction.normalized * knockbackDistance * Mathf.Max(0f, multiplier);
            if (characterController != null)
                characterController.Move(delta);
            else
                transform.position += delta;
        }
    }
}
