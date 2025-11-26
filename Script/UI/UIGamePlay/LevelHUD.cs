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

        private StarAnim[] starAnims;
        private bool[] starAchieved;
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
            levelManager.OnStarChanged += UpdateStarUI;

        }

        private void OnDisable()
        {
            levelManager.OnScoreChanged -= UpdateScoreUI;
            levelManager.OnMoveRemainingUpdated -= UpdateRemainingMoveUI;
        }

        private void UpdateScoreUI(int currentScore)
        {
            scoreAmount.SetText("{0}", currentScore);
            SetSlider(currentScore);
        }

        private void UpdateStarUI(int newStarLevel)
        {
            int starIndexToAnimate = newStarLevel - 1;

            if (starIndexToAnimate >= 0 && starIndexToAnimate < starAnims.Length)
            {
                if (starAnims[starIndexToAnimate] != null)
                {
                    starAnims[starIndexToAnimate].PlayAnimStar();
                }
            }
        }

        private void UpdateRemainingMoveUI()
        {
            remainingMoves.SetText("{0}", levelManager.RemainingMoves);
        }

        private void InitUI()
        {
            scoreAmount.SetText("0");
            remainingMoves.SetText("{0}", levelManager.RemainingMoves);
            scoreSlider.maxValue = levelManager.LevelData.targetScore;
            starAnims = new StarAnim[stars.Length];
            starAchieved = new bool[stars.Length];

            for (int i = 0; i < stars.Length; i++)
            {
                starAnims[i] = stars[i].GetComponent<StarAnim>();
                starAchieved[i] = false;
            }
        }

        private void SetSlider(int currentScore)
        {
            scoreSlider.value = currentScore;
        }
    }
}
