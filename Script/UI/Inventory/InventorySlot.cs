using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CandyProject
{
    public class InventorySlot : MonoBehaviour
    {

        [SerializeField] private GemData gemData;

        [Header("ShopItemSlot Setup")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text quantityText;
        [SerializeField] private Button useSlot;

        public void SetUp(GemData data, int quantity)
        {
            gemData = data;
            itemIcon.sprite = gemData.GetSprite();
            quantityText.SetText("{0}", quantity);
            useSlot.onClick.AddListener(() =>
            {
                InputManager.Instance.EnterUseItemMode(gemData);
            });
        }

       

    }
}
