using System;
using TMPro;
using UnityEngine;

namespace CandyProject
{
    public class ResourceUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text goldAmount;
        [SerializeField] private TMP_Text healthQuantity;
        [SerializeField] private TMP_Text timerText;

        private ShopManager shopManager;

        public int minutesPerGain = 5;
        public int maxHealth = 5;

        private DateTime nextGainTime;

        private void Awake()
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }

        private void Start()
        {
            UpdateResource();

            if (PlayerPrefs.HasKey("nextGainTime"))
            {
                long binary = Convert.ToInt64(PlayerPrefs.GetString("nextGainTime"));
                nextGainTime = DateTime.FromBinary(binary);
            }
            else
            {
                nextGainTime = DateTime.Now.AddMinutes(minutesPerGain);
                PlayerPrefs.SetString("nextGainTime", nextGainTime.ToBinary().ToString());
            }
        }

        private void OnEnable()
        {
            if (shopManager != null)
                shopManager.OnResourceChanged += OnResourceChange;
        }

        private void OnDisable()
        {
            if (shopManager != null)
                shopManager.OnResourceChanged -= OnResourceChange;
        }

        private void OnResourceChange()
        {
            UpdateResource();

            // Nếu health giảm → reset timer từ đầu
            if (ResourceManager.Instance.CurrentHealth < maxHealth)
            {
                nextGainTime = DateTime.Now.AddMinutes(minutesPerGain);
                PlayerPrefs.SetString("nextGainTime", nextGainTime.ToBinary().ToString());
            }
        }

        public void UpdateResource()
        {
            goldAmount.text = ResourceManager.Instance.CurrentGold.ToString();
            healthQuantity.text = ResourceManager.Instance.CurrentHealth.ToString();
        }

        private void Update()
        {
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            int current = ResourceManager.Instance.CurrentHealth;

            if (current >= maxHealth)
            {
                timerText.text = "";
                return;
            }

            // Tính thời gian còn lại
            TimeSpan remain = nextGainTime - DateTime.Now;

            if (remain.TotalSeconds <= 0)
            {
                ResourceManager.Instance.AddHealth(1);
                UpdateResource();

                // Nếu sau khi hồi máu mà FULL thì reset và tắt timer
                if (ResourceManager.Instance.CurrentHealth >= maxHealth)
                {
                    timerText.text = "";
                    return;
                }

                nextGainTime = DateTime.Now.AddMinutes(minutesPerGain);
                PlayerPrefs.SetString("nextGainTime", nextGainTime.ToBinary().ToString());

                remain = nextGainTime - DateTime.Now;
            }

            timerText.text = string.Format("{0:D2}:{1:D2}", remain.Minutes, remain.Seconds);
        }
    }
}
