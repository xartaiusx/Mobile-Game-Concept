using UnityEngine;

namespace Game.Animation
{
    public class AnimationEventRelay : MonoBehaviour
    {
        [SerializeField] private CombatAnimationBridge bridge;

        private void Awake()
        {
            ResolveBridge();
        }

        public void BeginAttackActiveWindow()
        {
            ResolveBridge()?.OpenHitWindow();
        }

        public void EndAttackActiveWindow()
        {
            ResolveBridge()?.CloseHitWindow();
        }

        public void FinishRecovery()
        {
            ResolveBridge()?.FinishAttackRecovery();
        }

        public void TriggerFootstep()
        {
            ResolveBridge()?.TriggerFootstep();
        }

        public void TriggerWeaponSwing()
        {
            ResolveBridge()?.TriggerWeaponSwing();
        }

        private CombatAnimationBridge ResolveBridge()
        {
            if (bridge == null)
                bridge = GetComponentInParent<CombatAnimationBridge>();
            return bridge;
        }
    }
}
