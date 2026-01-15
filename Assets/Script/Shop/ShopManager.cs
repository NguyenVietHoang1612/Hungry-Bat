using NUnit.Framework;
using System;
using UnityEngine;

namespace CandyProject
{
    
    public class ShopManager : MonoBehaviour
    {
        [SerializeField] private ShopItemData[] items;

        public event Action OnResourceChanged;
        [SerializeField] AudioClip sfxBuy;


        public bool TryPurchaseItem(ShopItemData item)
        {
            if (ResourceManager.Instance == null) return false;

            if (ResourceManager.Instance.CurrentGold < item.price)
            {
                Debug.Log($"Not enough gold to purchase {item.bombData.gemName}. Needed: {item.price}");
                return false;
            }

            ResourceManager.Instance.UseGold(item.price);
            ResourceManager.Instance.AddItem(item.bombData);
            OnResourceChanged?.Invoke();
            SoundManager.Instance.PlayOneShotSfx(sfxBuy);
            ResourceData resourceData = new ResourceData() { gold = ResourceManager.Instance.CurrentGold, health = ResourceManager.Instance.CurrentHealth, boosters = ResourceManager.Instance.BoostersIV };
            SaveSystem.SaveResource(resourceData);
            return true; 
        }


        public ShopItemData[] GetItems()
        {
            return items;
        }
    }
}
