using UnityEngine;

namespace Game.Feedback
{
    public class FeedbackPulse : MonoBehaviour
    {
        [SerializeField] private float lifetime = 0.35f;
        [SerializeField] private float expansion = 1.8f;
        [SerializeField] private bool flattenToRing;

        private Vector3 startScale;
        private float elapsed;
        private Renderer cachedRenderer;
        private Color startColor;
        private MaterialPropertyBlock propertyBlock;

        private void Awake()
        {
            startScale = transform.localScale;
            cachedRenderer = GetComponentInChildren<Renderer>();
            if (cachedRenderer != null && cachedRenderer.sharedMaterial != null)
                startColor = cachedRenderer.sharedMaterial.color;
            propertyBlock = new MaterialPropertyBlock();
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float t = lifetime <= 0f ? 1f : Mathf.Clamp01(elapsed / lifetime);
            Vector3 targetScale = startScale * Mathf.Lerp(1f, expansion, t);
            if (flattenToRing)
                targetScale.y = Mathf.Max(0.02f, startScale.y);
            transform.localScale = targetScale;

            if (cachedRenderer != null)
            {
                Color color = startColor;
                color.a = Mathf.Lerp(startColor.a, 0f, t);
                cachedRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor("_Color", color);
                cachedRenderer.SetPropertyBlock(propertyBlock);
            }

            if (elapsed >= lifetime)
                Destroy(gameObject);
        }
    }
}
