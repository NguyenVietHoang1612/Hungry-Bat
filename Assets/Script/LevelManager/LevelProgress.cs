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

        public LevelProgress() { }

        public LevelProgress(LevelProgress other)
        {
            this.bestScore = other.bestScore;
            this.starLevel = other.starLevel;
            this.isUnlocked = other.isUnlocked;

            this.levelData = other.levelData;
        }
    }
}
