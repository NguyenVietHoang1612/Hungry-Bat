using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace CandyProject
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        private int currentGold = 240;
        private int currentHealth = 5;

        private int maxHealth = 5;

        [SerializeField] private GemData[] bombSlotAvailable;

        [SerializeField] private Dictionary<GemData, int> boosters = new();

        public Dictionary<GemData, int> BoostersIV => boosters;

        private void Start()
        {
            GameManager.Instance.LoadResources();

            foreach (var gem in bombSlotAvailable)
            {
                if (!boosters.ContainsKey(gem))
                {
                    boosters.Add(gem, 0);
                }
            }
        }

        // Gold
        public void AddGold(int amount)
        {
            currentGold += amount;
            GameManager.Instance.SaveResources();
        }

        public void UseGold(int amount)
        {
            int health = currentGold - amount;
            currentGold = Mathf.Max(0, health);
            GameManager.Instance.SaveResources();
        }

        //Health
        public void AddHealth(int amount = 1)
        {
            int health = currentHealth + amount;

            currentHealth = Mathf.Min(maxHealth, health);
            GameManager.Instance.SaveResources();
        }

        public void UseHealth(int amount = 1)
        {
            int health = currentHealth - amount;
            currentHealth = Mathf.Max(0, health);
            GameManager.Instance.SaveResources();
        }

        //BoostersIV
        public void AddItem(GemData gem, int amount = 1)
        {
            if (!boosters.ContainsKey(gem))
            {
                Debug.Log("item not in inventory");
                return;
            }

            boosters[gem] += amount;
            GameManager.Instance.SaveResources();
        }

        public void UseItem(GemData gem)
        {
            if (!boosters.ContainsKey(gem))
            {
                Debug.Log("item not in inventory");
                return;
            }

            if (boosters[gem] <= 0)
            {
                Debug.Log($"Not enough quantity to use {gem.gemName}.");
            } 

            var quantity = boosters[gem] - 1;
            boosters[gem] = Mathf.Max(0, quantity);
            GameManager.Instance.SaveResources();
        }

        public void SetHealth(int quantity)
        {
            currentHealth = Mathf.Min(quantity, 5);
        }

        public void SetGold(int quantity)
        {
            currentGold = quantity;
        }

        public void SetBoosterIV(Dictionary<GemData, int> boos)
        {
            boosters.Clear();

            if (boos != null)
            {
                foreach (var item in boos)
                {
                    boosters.Add(item.Key, item.Value);
                }
            }
        }

        //Getter
        public int CurrentGold => currentGold; 
        public int CurrentHealth => currentHealth;
    }
}
