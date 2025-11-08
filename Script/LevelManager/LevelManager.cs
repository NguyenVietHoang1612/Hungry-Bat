using System;
using System.Collections.Generic;
using UnityEngine;

namespace CandyProject
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelData levelData;

        public int CurrentScore { get; private set; }
        public int RemainingMoves { get; private set; }
        public int StarLevel { get; private set; }
        public LevelData LevelData => levelData;
        public List<GemGoal> GemGoals { get; private set; }
        public UIAchievedResults uiAchievedResults;

        public event Action<int, float> OnScoreChanged;
        public event Action<int> OnStarChanged;
        public event Action<List<GemGoal>> OnGemGoalsUpdated;
        public event Action OnAchievedChanged;
        public event Action OnRestartLevel;

        private void Awake()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterLevelManager(this);
                GameManager.Instance.StartLevel();
            }
        }

        public void Init(LevelData data)
        {
            levelData = data;
            InitializeLevel();
        }

        private void InitializeLevel()
        {
            GemGoals = new List<GemGoal>();

            foreach (var goal in levelData.gemGoalDatas)
            {
                GemGoals.Add(new GemGoal
                {
                    gemData = goal.gemData,
                    requiredAmount = goal.requiredAmount
                });
            }

            CurrentScore = 0;
            RemainingMoves = levelData.movesLimit;
            StarLevel = 0;
            OnStarChanged?.Invoke(StarLevel);
            OnRestartLevel?.Invoke();
            OnScoreChanged?.Invoke(CurrentScore, 0f);
        }

        public void AddScore(int score)
        {
            CurrentScore += score;
            float percent = Mathf.Clamp01((float)CurrentScore / levelData.targetScore);

            OnScoreChanged?.Invoke(CurrentScore, percent);

            int newStar = CalculateStarLevel();
            if (newStar != StarLevel)
            {
                StarLevel = newStar;
                OnStarChanged?.Invoke(StarLevel);
                OnAchievedChanged?.Invoke();
            }
        }

        public void UseMove() => RemainingMoves = Mathf.Max(0, RemainingMoves - 1);

        public void ReduceGemGoal(Gem gem, int amount = 1)
        {
            foreach (var goal in GemGoals)
            {
                if (goal.gemData.gemType == gem.TypeOfGem)
                {
                    goal.requiredAmount = Mathf.Max(0, goal.requiredAmount - amount);
                    OnGemGoalsUpdated?.Invoke(GemGoals);
                    break;
                }
            }
        }

        private int CalculateStarLevel()
        {
            int score = CurrentScore;
            if (score >= levelData.threeStarScore) return 3;
            if (score >= levelData.twoStarScore) return 2;
            if (score >= levelData.oneStarScore) return 1;
            return 0;
        }

        public bool IsLevelComplete() 
        {   
            foreach (var g in GemGoals) 
            { 
                if (g.requiredAmount > 0) 
                    return false; 
            } 
            return true; 
        }
    }
}
