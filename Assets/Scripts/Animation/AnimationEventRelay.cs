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
            bridge?.BeginAttackActiveWindow();
        }

        public void EndAttackActiveWindow()
        {
            bridge?.EndAttackActiveWindow();
        }

        public void FinishRecovery()
        {
            bridge?.FinishRecovery();
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
