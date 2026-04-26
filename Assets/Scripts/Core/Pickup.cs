using UnityEngine;

namespace Game.Core
{
    [RequireComponent(typeof(Collider))]
    public class Pickup : MonoBehaviour
    {
        [SerializeField] private ItemDefinition item;
        [SerializeField] private int amount = 1;
        [SerializeField] private bool autoUseHealthPotionWhenLow = true;
        [SerializeField, Range(0f, 1f)] private float autoUseHealthThreshold = 0.45f;
        [SerializeField] private float bobHeight = 0.15f;
        [SerializeField] private float bobSpeed = 2f;

        private Vector3 startPosition;

        public ItemDefinition Item => item;
        public int Amount => amount;

        private void Awake()
        {
            Collider trigger = GetComponent<Collider>();
            trigger.isTrigger = true;
            startPosition = transform.position;
        }

        private void Update()
        {
            if (bobHeight <= 0f) return;
            transform.position = startPosition + Vector3.up * (Mathf.Sin(Time.time * bobSpeed) * bobHeight);
        }

        public void Configure(ItemDefinition definition, int count)
        {
            item = definition;
            amount = Mathf.Max(1, count);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (item == null) return;

            BaseCharacter character = other.GetComponentInParent<BaseCharacter>();
            if (character == null) return;

            InventorySystem inventory = other.GetComponentInParent<InventorySystem>() ?? FindAnyObjectByType<InventorySystem>();
            bool collected = inventory == null || inventory.AddItem(item, amount);
            if (!collected) return;

            if (autoUseHealthPotionWhenLow && item.healAmount > 0 && character.CurrentHealth <= Mathf.RoundToInt(character.MaxHealth * autoUseHealthThreshold))
            {
                if (inventory != null && inventory.RemoveItem(item, 1))
                    character.Heal(item.healAmount);
            }

            Destroy(gameObject);
        }
    }
}
