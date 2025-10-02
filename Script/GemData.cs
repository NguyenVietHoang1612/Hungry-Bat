using UnityEngine;

namespace CandyProject
{
    [CreateAssetMenu(fileName = "New Gem", menuName = "CandyData/Gem")]
    public class GemData : ScriptableObject
    {
        public string gemName;
        public Sprite gemIcon;

        public int scoreValue = 10;
    }
}

