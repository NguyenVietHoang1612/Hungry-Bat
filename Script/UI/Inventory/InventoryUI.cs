using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CandyProject
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField] private GameObject inventorySlotPrefab;
        [SerializeField] private Transform inventorySlotContainer;

        [SerializeField] private Image panelUseItem;

        private void Start()
        {
            ObjectPoolManager.Instance.CreatePool(inventorySlotPrefab, 5);

            if (ResourceManager.Instance != null)
            {
                SetUpInventory();
            }
            else
            {
                Debug.Log("ResourceManager is null");
            }
        }

        private void OnEnable()
        {
            InputManager.OnItemUsed += RefreshUI;
        }
        private void OnDisable()
        {
            InputManager.OnItemUsed -= RefreshUI;
        }

        public void SetUpInventory()
        {
            foreach(var item in ResourceManager.Instance.BoostersIV)
            {
               GameObject itemO = ObjectPoolManager.Instance.Get(inventorySlotPrefab);
               itemO.transform.SetParent(inventorySlotContainer, false);
               
               InventorySlot inventorySlot = itemO.GetComponent<InventorySlot>();

                inventorySlot.SetUp(item.Key, item.Value);
            }
        }

        private void RefreshUI()
        {

            var i = 0;
            foreach (Transform child in inventorySlotContainer)
            {
                ObjectPoolManager.Instance.Return(inventorySlotPrefab, child.gameObject);
                Debug.Log(i++);
            }

           //SetUpInventory();
        }

        private void Update()
        {
            if (InputManager.Instance.Mode == InputMode.UseInventoryGem)
            {
                PanelActive();
            }
            else
            {
                PanelDeActive();
            }
        }

        public void PanelActive()
        {
            panelUseItem.gameObject.SetActive(true);
        }

        public void PanelDeActive()
        {
            panelUseItem.gameObject.SetActive(false);
        }
    }
}
