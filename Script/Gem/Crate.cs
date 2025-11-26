using UnityEngine;
using UnityEngine.VFX;

namespace CandyProject
{
    public class Crate : Gem
    {
        [Header("Gears Sprites update number in descending order")]
        [SerializeField] private Sprite[] spritesGear;
        [SerializeField] GameObject crashDamageVFX;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (gemData != null && crashDamageVFX != null)
                ObjectPoolManager.Instance.CreatePool(crashDamageVFX, 3);

            Init();
        }

        public void Init()
        {
            health = spritesGear.Length;
            spriteRenderer.sprite = gemData.GetSprite();
            name = gemData.gemName;
        }   

        public void Damage(int amount)
        {
            health -= amount;
            // play destroy effect
            if (health < 0)
            {
                PlayDamageEffect();            
            }
            else
            {
                UpdateGears();
            }
        }

        private void UpdateGears()
        {
            PlayDestroyEffect();
            spriteRenderer.sprite = spritesGear[health];
        }

        public void PlayDamageEffect()
        {
            if (gemData == null || gemData.destroyEffect == null) return;

            GameObject fx = ObjectPoolManager.Instance.Get(gemData.destroyEffect);
            VisualEffect = fx.GetComponentInChildren<VisualEffect>();
            VisualEffect.Play();
            SoundManager.Instance.PlayOneShotSfx(gemData.destroySound);
            fx.transform.position = transform.position;
            fx.transform.rotation = Quaternion.identity;
            GameManager.Instance.StartCoroutine(ReturnEffectToPoolRoutine(fx, gemData.destroyEffect));
        }
    }
}
