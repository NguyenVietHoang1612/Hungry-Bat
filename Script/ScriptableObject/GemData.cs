using UnityEngine;
using UnityEngine.U2D;

namespace CandyProject
{
    [CreateAssetMenu(fileName = "New Gem", menuName = "CandyData/Gem")]
    public class GemData : ScriptableObject
    {
        public string gemName;

        public GemType gemType;
        
        public SpriteAtlas spriteAtlas;
        public string spriteName;

        public int scoreValue = 10;

        public GameObject destroyEffect;

        public AudioClip destroySound;

        public bool IsBoom => gemType == GemType.ArrowHorizontal || gemType == GemType.ArrowVertical || gemType == GemType.BoomColor || gemType == GemType.BoomWrapped;
        public Sprite GetSprite()
        {
            if (spriteAtlas != null && !string.IsNullOrEmpty(spriteName))
            {
                return spriteAtlas.GetSprite(spriteName);
            }
            return null;
        }
    }

    

    public enum GemType
    {
        Red,
        Blue,
        Green,
        Yellow,
        white,
        Purple,
        Orange,
        ArrowHorizontal,
        ArrowVertical,
        BoomColor,
        BoomWrapped,
        BonusGem
    }
}

