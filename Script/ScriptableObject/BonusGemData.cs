using UnityEngine;
using UnityEngine.U2D;

namespace CandyProject
{
    [CreateAssetMenu(fileName = "new Bonus Gem Data", menuName = "CandyData/BonusGemData")]
    public class BonusGemData : ScriptableObject
    {
        public string gemName;

        public Sprite[] SpritesGear;

        public int scoreValue = 10;

        public GameObject destroyEffect;

        public AudioClip destroySound;
    }
}
