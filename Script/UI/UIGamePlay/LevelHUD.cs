using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CandyProject
{
    public class LevelHUD : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;

        [Header("UI Setup")]
        [SerializeField] TMP_Text scoreAmount;
        [SerializeField] TMP_Text remainingMoves;
        [SerializeField] Slider scoreSlider;
        [SerializeField] Image[] stars;
        private void Start()
        {
            InitUI();
        }

        private void OnEnable()
        {
            if (levelManager == null)
            {
                Debug.Log("LevelManager not found");
                return;
            }

            levelManager.OnScoreChanged += UpdateScoreUI;
            levelManager.OnMoveRemainingUpdated += UpdateRemainingMoveUI;
        }

        private void OnDisable()
        {
            levelManager.OnScoreChanged -= UpdateScoreUI;
            levelManager.OnMoveRemainingUpdated -= UpdateRemainingMoveUI;
        }

        private void UpdateScoreUI(int currentScore)
        {
            scoreAmount.text = currentScore.ToString();
            
            SetSlider(currentScore);

            for (int i = 0; i < stars.Length; i++)
            {
                var hasStar = i < levelManager.StarLevel;
                
                if (hasStar)
                {
                    stars[i].color = Color.white;
                }
                else
                {
                    stars[i].color = Color.black;
                }
            }
        }

        private void UpdateRemainingMoveUI()
        {
            remainingMoves.text = levelManager.RemainingMoves.ToString();
        }

        private void InitUI()
        {
            scoreSlider.maxValue = levelManager.LevelData.targetScore;
            scoreSlider.value = 0;
            remainingMoves.text = levelManager.RemainingMoves.ToString();

            foreach (var star in stars)
            {
                star.color = Color.black;
            }

        }

        private void SetSlider(int currentScore)
        {
            scoreSlider.value = currentScore;
        }
    }
}
