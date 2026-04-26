using System.Collections;
using Game.Core;
using UnityEngine;

namespace Game.Feedback
{
    [RequireComponent(typeof(BaseEnemy))]
    public class EnemyAttackFlash : MonoBehaviour
    {
        [SerializeField] private Renderer targetRenderer;
        [SerializeField] private GameObject warningPrefab;
        [SerializeField] private Color flashColor = new Color(1f, 0.9f, 0.2f, 1f);
        [SerializeField] private float flashSeconds = 0.18f;
        [SerializeField] private Vector3 warningOffset = new Vector3(0f, 1.2f, 0f);
        [SerializeField] private float warningScale = 0.75f;

        private BaseEnemy enemy;
        private MaterialPropertyBlock propertyBlock;
        private Color baseColor = Color.white;
        private Coroutine flashRoutine;

        private void Awake()
        {
            enemy = GetComponent<BaseEnemy>();
            targetRenderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();
            propertyBlock = new MaterialPropertyBlock();
            if (targetRenderer != null && targetRenderer.sharedMaterial != null)
                baseColor = targetRenderer.sharedMaterial.color;
        }

        private void OnEnable()
        {
            if (enemy != null)
                enemy.AttackStarted += HandleAttackStarted;
        }

        private void OnDisable()
        {
            if (enemy != null)
                enemy.AttackStarted -= HandleAttackStarted;
        }

        private void HandleAttackStarted(BaseEnemy source)
        {
            if (targetRenderer == null) return;
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashRoutine());

            if (warningPrefab != null)
            {
                GameObject warning = Instantiate(warningPrefab, transform.position + warningOffset, Quaternion.identity);
                warning.transform.localScale = Vector3.one * Mathf.Max(0.1f, warningScale);
            }
        }

        private IEnumerator FlashRoutine()
        {
            SetColor(flashColor);
            yield return new WaitForSeconds(flashSeconds);
            SetColor(baseColor);
            flashRoutine = null;
        }

        private void SetColor(Color color)
        {
            targetRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor("_Color", color);
            targetRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
