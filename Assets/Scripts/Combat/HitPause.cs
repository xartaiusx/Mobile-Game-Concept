using UnityEngine;
using System.Collections;

namespace Game.Combat
{
    /// <summary>
    /// Simple hit pause effect that briefly reduces time scale.
    /// Call TriggerPause for tactile impact.
    /// </summary>
    public class HitPause : MonoBehaviour
    {
        [SerializeField] private float pauseScale = 0.1f;
        [SerializeField] private float pauseDuration = 0.06f;

        private bool running;
        private float originalTimeScale = 1f;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void ResetTimeScaleBeforeSceneLoad()
        {
            Time.timeScale = 1f;
        }

        public void TriggerPause()
        {
            if (!running) StartCoroutine(CoPause());
        }

        private void OnDisable()
        {
            RestoreTimeScale();
        }

        private void OnDestroy()
        {
            RestoreTimeScale();
        }

        private void RestoreTimeScale()
        {
            if (running)
                Time.timeScale = originalTimeScale;
            running = false;
        }

        private IEnumerator CoPause()
        {
            running = true;
            originalTimeScale = Time.timeScale;
            Time.timeScale = pauseScale;
            yield return new WaitForSecondsRealtime(pauseDuration);
            Time.timeScale = originalTimeScale;
            running = false;
        }
    }
}
