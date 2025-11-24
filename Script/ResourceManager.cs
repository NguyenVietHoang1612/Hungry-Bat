using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

namespace CandyProject
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        private int currentGold = 240;
        private int currentHealth = 5;

        private int maxHealth = 5;

        [SerializeField] private GemData[] bombSlotAvailable;

        [SerializeField] private Dictionary<GemData, int> boosters = new ();

        public Dictionary<GemData, int> BoostersIV => boosters;

        private void Start()
        {
            foreach (var gem in bombSlotAvailable)
            {
                boosters.Add (gem, 0);
            }
        }

        // Gold
        public void AddGold(int amount)
        {
            currentGold += amount;
        }

        public void UseGold(int amount)
        {
            int health = currentGold - amount;
            currentGold = Mathf.Max(0, health);
        }

        //Health
        public void AddHealth(int amount = 1)
        {
            int health = currentHealth + amount;

            currentHealth = Mathf.Min(maxHealth, health);
        }

        public void UseHealth(int amount = 1)
        {
            int health = currentHealth - amount;
            currentHealth = Mathf.Max(0, health);
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
        }

        //Getter
        public int CurrentGold => currentGold; 
        public int CurrentHealth => currentHealth;
    }
}
