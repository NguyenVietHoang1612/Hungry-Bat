using UnityEngine;

namespace CandyProject
{
    public class BonusGem : MonoBehaviour
    {
        [SerializeField] private int health = 1;
        public BonusGemData bonusGemData;
        [Header("Gears Sprites update number in descending order")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Init(BonusGemData data)
        {
            bonusGemData = data;
            health = bonusGemData.SpritesGear.Length - 1;
            spriteRenderer.sprite = bonusGemData.SpritesGear[health];
            name = data.gemName;
        }   

        public void Damage(int amount)
        {
            health -= amount;
            // play destroy effect
            if (health < 0)
            {
                // Return to pool
                gameObject.SetActive(false);

            }
            else
            {
                UpdateGears();
            }
        }

        private void UpdateGears()
        {
            spriteRenderer.sprite = bonusGemData.SpritesGear[health];
        }


    }
}
