using UnityEngine;

namespace Game.Visuals
{
    public class VisualAttachmentRoot : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Animator animator;
        [SerializeField] private Renderer[] renderers = new Renderer[0];

        public Transform VisualRoot => visualRoot;
        public Animator Animator => animator;
        public Renderer[] Renderers => renderers;

        private void Awake()
        {
            EnsureVisualRoot();
            RefreshReferences();
        }

        public void SetModel(GameObject modelPrefab)
        {
            EnsureVisualRoot();
            ClearModel();
            if (modelPrefab == null || visualRoot == null)
            {
                RefreshReferences();
                return;
            }

            GameObject instance = Instantiate(modelPrefab, visualRoot);
            instance.name = modelPrefab.name;
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            RefreshReferences();
        }

        public void ClearModel()
        {
            if (visualRoot == null)
                return;

            for (int i = visualRoot.childCount - 1; i >= 0; i--)
            {
                GameObject child = visualRoot.GetChild(i).gameObject;
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }

            RefreshReferences();
        }

        public void PlayState(string stateName)
        {
            if (animator == null || string.IsNullOrEmpty(stateName))
                return;

            animator.Play(stateName);
        }

        public void SetTrigger(string triggerName)
        {
            if (animator == null || string.IsNullOrEmpty(triggerName))
                return;

            animator.SetTrigger(triggerName);
        }

        public void RefreshReferences()
        {
            EnsureVisualRoot();
            if (visualRoot == null)
            {
                animator = null;
                renderers = new Renderer[0];
                return;
            }

            animator = visualRoot.GetComponentInChildren<Animator>(true);
            renderers = visualRoot.GetComponentsInChildren<Renderer>(true);
        }

        public void Configure(Transform root, Animator visualAnimator, Renderer[] visualRenderers)
        {
            visualRoot = root;
            animator = visualAnimator;
            renderers = visualRenderers ?? new Renderer[0];
        }

        private void EnsureVisualRoot()
        {
            if (visualRoot != null)
                return;

            Transform existing = transform.Find("VisualRoot");
            if (existing != null)
            {
                visualRoot = existing;
                return;
            }

            GameObject root = new GameObject("VisualRoot");
            root.transform.SetParent(transform, false);
            visualRoot = root.transform;
        }
    }
}
