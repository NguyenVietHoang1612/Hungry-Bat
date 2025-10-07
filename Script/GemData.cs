using UnityEngine;

namespace CandyProject
{
    [CreateAssetMenu(fileName = "New Gem", menuName = "CandyData/Gem")]
    public class GemData : ScriptableObject
    {
        public string gemName;

        public GemType gemType;

        public Sprite gemIcon;

        public int scoreValue = 10;
    }

    public enum GemType
    {
        Red,
        Blue,
        Green,
        Yellow,
        white,
        Purple,
        Orange
    }
}

