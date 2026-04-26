using UnityEngine;

namespace Game.Core
{
    [CreateAssetMenu(menuName = "Game/ItemDefinition")]
    public class ItemDefinition : ScriptableObject
    {
        public string itemId;
        public string displayName;
        public Sprite icon;
        public int maxStack = 99;
        public int goldValue;
        public int healAmount;
    }
}
