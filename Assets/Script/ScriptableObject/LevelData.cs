using System.Collections.Generic;
using UnityEngine;

namespace CandyProject
{
    [CreateAssetMenu(fileName ="New level data", menuName = "CandyData/LevelData")]
    public class LevelData : ScriptableObject
    {
        [Header("Level Info")]
        public int levelIndex;
        public string levelName;

        [Header("Board Settings")]
        public int width;
        public int height;

        [Header("TileKind")]
        public TileKind[] tileKinds;

        [Header("Goals / Conditions")]
        public int targetScore;
        public int movesLimit;
        public bool hasTimeLimit;
        public float timeLimit;

        [Header("Candy Goals")]
        public List<GemGoal> gemGoalDatas; 

        [Header("Star Settings")]
        public int oneStarScore;
        public int twoStarScore;
        public int threeStarScore;

        [Header("Rewards")]
        public int gold;
    }

    [System.Serializable]
    public class GemGoal
    {
        public GemData gemData;
        public int requiredAmount;

        public GemGoal() { }

        public GemGoal(GemGoal other)
        {
            this.requiredAmount = other.requiredAmount;
            this.gemData = other.gemData;
        }
    }
}
