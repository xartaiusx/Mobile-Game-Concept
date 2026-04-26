using UnityEngine;

namespace Game.Animation
{
    public class AnimationEventRelay : MonoBehaviour
    {
        [SerializeField] private CombatAnimationBridge bridge;

        private void Awake()
        {
            bridge = bridge != null ? bridge : GetComponentInParent<CombatAnimationBridge>();
        }

        public void BeginAttackActiveWindow()
        {
            bridge?.OpenHitWindow();
        }

        public void EndAttackActiveWindow()
        {
            bridge?.CloseHitWindow();
        }

        public void FinishRecovery()
        {
            bridge?.FinishAttackRecovery();
        }

        public void TriggerFootstep()
        {
            bridge?.TriggerFootstep();
        }

        public void TriggerWeaponSwing()
        {
            bridge?.TriggerWeaponSwing();
        }
    }
}
