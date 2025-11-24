using UnityEngine;

namespace CandyProject
{
    [CreateAssetMenu(fileName = "New ShopItem", menuName = "CandyData/ShopData")]
    public class ShopItemData : ScriptableObject
    {
        public GemData bombData;
        [TextArea(3, 5)] 
        public string description;
        public int price;
    }

}
