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
               itemO.transform.SetParent(inventorySlotContainer);
               itemO.transform.localPosition = Vector3.zero;
               itemO.transform.localRotation = Quaternion.identity;
               itemO.transform.localScale = Vector3.one;
               
               InventorySlot inventorySlot = itemO.GetComponent<InventorySlot>();

                inventorySlot.SetUp(item.Key, item.Value);
            }
        }

        private void RefreshUI()
        {
            var i = 0;

            Transform[] children = new Transform[inventorySlotContainer.childCount];
            for (int j = 0; j < inventorySlotContainer.childCount; j++)
            {
                children[j] = inventorySlotContainer.GetChild(j);
            }

            foreach (Transform child in children)
            {
                ObjectPoolManager.Instance.Return(inventorySlotPrefab, child.gameObject);
                Debug.Log(i++);
            }

            SetUpInventory();
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
