using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


namespace CandyProject
{
    public class ShopItemSlot : MonoBehaviour
    {
        private ShopManager shopManager;
        [SerializeField] private ShopItemData shopItemData;

        [Header("ShopItemSlot Setup")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text price;
        [SerializeField] private Button buyButton;

        private void Start()
        {
            shopManager = FindFirstObjectByType<ShopManager>();
        }

        public void Setup(ShopItemData item, ShopManager manager)
        {
            shopItemData = item;
            shopManager = manager;

            itemIcon.sprite = shopItemData.bombData.GetSprite();
            price.SetText("{0}", shopItemData.price);

            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyButtonClicked);
        }

        private void OnBuyButtonClicked()
        {
            if (shopManager.TryPurchaseItem(shopItemData)) {
                GameManager.Instance.SaveResources();
            }
        }
    }
}
