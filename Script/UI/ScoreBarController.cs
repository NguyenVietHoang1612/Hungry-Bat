using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

namespace CandyProject
{
    public class ScoreBarController : MonoBehaviour
    {
        private VisualElement fillScore;

        private Label scoreValue;

        [SerializeField] private int maxScore = 100;
        [SerializeField] private int  currentScore;

        [SerializeField] private VisualEffect WinVFX;
        [SerializeField] private VisualEffect LoseVFX;

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            fillScore = root.Q<VisualElement>("FillScore");
            scoreValue = root.Q<Label>("ScoreValue");

            currentScore = 0;
            UpdateScoreBar();
        }

        private void Update()
        {
            PlayerWinVFX();
        }

        private void PlayerWinVFX()
        {
            if (IsValidScore())
            {
                GameManager.Instance.HandleWaitingGameState();
                WinVFX.gameObject.SetActive(true);
            }
        }

        public void TakeScore(int score)
        {
            currentScore = Mathf.Min(maxScore, currentScore + score);
            UpdateScoreBar();
        }

        private void UpdateScoreBar()
        {
            float fillPercentage = (float)currentScore / maxScore;
            fillScore.style.width = new Length(fillPercentage * 100, LengthUnit.Percent);
            scoreValue.text = currentScore.ToString();
            Color full = Color.green;
            Color low = Color.red;
            fillScore.style.backgroundColor = Color.Lerp(low, full, fillPercentage);
        }

        public bool IsValidScore()
        {
            return currentScore >= maxScore;
        }

    }
}
