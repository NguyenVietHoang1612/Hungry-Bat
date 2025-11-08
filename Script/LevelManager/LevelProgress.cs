using UnityEngine;

namespace CandyProject
{
    [System.Serializable]
    public class LevelProgress
    {
        public LevelData levelData;  
        public int bestScore;
        public int starLevel;        
        public bool isUnlocked;
    }
}
