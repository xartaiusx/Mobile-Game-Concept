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

        public void TriggerPause()
        {
            if (!running) StartCoroutine(CoPause());
        }

        private IEnumerator CoPause()
        {
            running = true;
            float original = Time.timeScale;
            Time.timeScale = pauseScale;
            yield return new WaitForSecondsRealtime(pauseDuration);
            Time.timeScale = original;
            running = false;
        }
    }
}
