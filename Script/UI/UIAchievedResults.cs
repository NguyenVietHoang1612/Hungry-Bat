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
        [SerializeField] private Transform AchievedResultsContainer;
        [SerializeField] private Sprite[] AchievedIcon;
        [SerializeField] private Image AchievedImage;
        [SerializeField] private TMP_Text levelMap;
        [SerializeField] private Image[] Stars;
        [SerializeField] private Button backToHome;
        [SerializeField] private Button nextLevel;

        [SerializeField] private Button reStartLevel;


        private CanvasGroup canvasGroup;

        [SerializeField] private LevelManager levelManager;
        public Animator Animator { get; private set; }

        private void Start()
        {
            Animator = GetComponentInChildren<Animator>();
            canvasGroup = AchievedResultsContainer.GetComponent<CanvasGroup>();
            levelManager.OnAchievedChanged += UpdateAchirved;
            backToHome.onClick.AddListener(OnBackToMenuClicked);
            reStartLevel.onClick.AddListener(OnReStartLevelClicked);

            canvasGroup.alpha = 0;
            levelMap.text = levelManager.LevelData.levelName;
        }

        public void UpdateAchirved()
        {
            UpdateAchievedStar();
        }

        public void UpdateAchievedStar()
        {
            int starLevel = levelManager.StarLevel;

            switch(starLevel)
            {
                case 1:
                    AchievedImage.sprite = AchievedIcon[0];
                    break;
                case 2:
                    AchievedImage.sprite = AchievedIcon[1];
                    break;
                case 3:
                    AchievedImage.sprite = AchievedIcon[2];
                    break;
                default:
                    Debug.Log("Lose");
                    break;
            }


            for (int i = 0; i < Stars.Length; i++)
            {
                bool hasStar = (i < starLevel);
                if (i < starLevel)
                {
                    Stars[i].color = Color.white;
                }
                else
                {
                    Stars[i].color = Color.black;
                }
            }
        }

        private void OnBackToMenuClicked()
        {
            GameManager.Instance.ExitLevel();
        }
        private void OnReStartLevelClicked()
        {
            levelManager.Init(levelManager.LevelData);
            Animator.Play("HideAchievedResults");
            StartCoroutine(WaitResetLevel());
        }


        private IEnumerator WaitResetLevel()
        {
            yield return new WaitForSeconds(1f);
            GameManager.Instance.RestartLevel();
        }
    }
}
