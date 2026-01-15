using UnityEngine;

namespace CandyProject
{
    public class ShopHUB : MonoBehaviour
    {
        [SerializeField] private GameObject itemSlotPrefab;

        [SerializeField] private Transform itemSlotContainer;

        private ShopManager shopManager;

        private CanvasGroup canvasGroup;

        private void Start()
        {
            ObjectPoolManager.Instance.CreatePool(itemSlotPrefab, 5);

            canvasGroup = GetComponent<CanvasGroup>();
            shopManager = FindFirstObjectByType<ShopManager>();

            PopulateShopUI();
        }

        public void PopulateShopUI()
        {

            var items = shopManager.GetItems();

            foreach (ShopItemData item in items)
            {
                GameObject itemSlot = ObjectPoolManager.Instance.Get(itemSlotPrefab);
                itemSlot.transform.SetParent(itemSlotContainer.transform);
                itemSlot.transform.localPosition = Vector3.zero;
                itemSlot.transform.localRotation = Quaternion.identity;
                itemSlot.transform.localScale = Vector3.one;
                ShopItemSlot shopItemSlot = itemSlot.GetComponent<ShopItemSlot>();

                if (shopItemSlot != null)
                {
                    shopItemSlot.Setup(item, shopManager);
                }
            }
        }

        public void OpenShop()
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        public void CloseShop()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
