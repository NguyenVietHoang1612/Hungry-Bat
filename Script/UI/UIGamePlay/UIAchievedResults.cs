using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace CandyProject
{
    public class UIAchievedResults : MonoBehaviour
    {
        [Header("UI SetUp")]
        [SerializeField] private Transform AchievedResultsContainer;
        [SerializeField] private TMP_Text textResult;
        [SerializeField] private Image loseIcon;
        [SerializeField] private Image[] Stars;
        [SerializeField] private Transform starsContainer;
        [SerializeField] private Button backToHome;
        [SerializeField] private Button nextLevel;
        [SerializeField] private Button reStartLevel;
        [SerializeField] private TMP_Text scoreAmount;
        [SerializeField] Sprite starActive;

        [Header("UI Settings")]
        [SerializeField] private LevelManager levelManager; 
        [SerializeField] private RectTransform panel;
        [SerializeField] private float duration = 0.5f;
        [SerializeField] private Vector2 endTargetPosition;
        [SerializeField] private Vector2 startTargetPosition;

        private CanvasGroup canvasGroup;

        private void Start()
        {
            canvasGroup = AchievedResultsContainer.GetComponent<CanvasGroup>();
            backToHome.onClick.AddListener(OnBackToMenuClicked);
            reStartLevel.onClick.AddListener(OnReStartLevelClicked);
            nextLevel.onClick.AddListener(OnNextLevelClicked);

            DisableAchieved();
        }

        public void UpdateAchirved()
        {
            UpdateAchievedStar();
        }

        private void UpdateAchievedStar()
        {
            EnableAchieved();

            if (levelManager.RemainingMoves == 0 && !levelManager.IsLevelComplete())
            {
                loseIcon.gameObject.SetActive(true);
                textResult.text = "Lose";
                nextLevel.gameObject.SetActive(false);
                starsContainer.gameObject.SetActive(false);
            }
            else
            {
                int starLevel = levelManager.StarLevel;
                textResult.text = "COMPLETE!";
                loseIcon.gameObject.SetActive(false);
                starsContainer.gameObject.SetActive(true);

                for (int i = 0; i < Stars.Length; i++)
                {
                    bool hasStar = (i < starLevel);
                    if (i < starLevel)
                    {
                        Stars[i].sprite = starActive;
                    }
                }

            }         
            scoreAmount.text = levelManager.CurrentScore.ToString();
        }

        public void MovePanelStart()
        {
            StartCoroutine(MovePanel(startTargetPosition));
        }

        public void MovePanelEnd()
        {
            StartCoroutine(MovePanel(endTargetPosition));
        }

        IEnumerator MovePanel(Vector2 target)
        {
            
            Vector2 start = panel.anchoredPosition;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                panel.anchoredPosition = Vector2.Lerp(start, target, t);
                yield return null;
            }
        }

        private void OnBackToMenuClicked()
        {
            GameManager.Instance.ExitLevel();
        }
        private void OnReStartLevelClicked()
        {
            levelManager.Init(levelManager.LevelData);
            MovePanelEnd();
            StartCoroutine(WaitResetLevel());
        }

        private void OnNextLevelClicked()
        {
            GameManager.Instance.NextLevel();
        }


        private IEnumerator WaitResetLevel()
        {
            yield return new WaitForSeconds(1f);
            GameManager.Instance.RestartLevel();
        }

        public void ShowAchievedResults()
        {
            canvasGroup.alpha = 1;
            MovePanelStart();
            GameManager.Instance.SetGameState(GameState.Paused);
        }

        private void EnableAchieved()
        {
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        private void DisableAchieved()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
