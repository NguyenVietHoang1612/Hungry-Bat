using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CandyProject
{
    public class ScoreBarUI : MonoBehaviour
    {
        private VisualElement fillScore;
        private Label scoreValue;
        private List<VisualElement> star;
        private Button settingsButton;

        [SerializeField] private LevelManager levelManager;

        [SerializeField] private Transform settingPanel;

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            fillScore = root.Q<VisualElement>("FillScore");
            scoreValue = root.Q<Label>("ScoreValue");
            star = root.Query<VisualElement>("StarLevel").ToList();
            settingsButton = root.Q<Button>("SettingButton");

            Debug.Log(star.Count);
            levelManager.OnScoreChanged += UpdateUI;
            settingsButton.clicked += OpenSettings;
        }

        private void UpdateUI(int currentScore, float percent)
        {
            fillScore.style.width = new Length(percent * 100, LengthUnit.Percent);
            scoreValue.text = currentScore.ToString();

            fillScore.style.backgroundColor = Color.Lerp(Color.red, Color.green, percent);

            for(int i = 0; i < star.Count; i++)
            {
                var hasStar = i < levelManager.StarLevel;
                
                if (hasStar)
                {
                    star[i].style.unityBackgroundImageTintColor = Color.white;
                }
                else
                {
                    star[i].style.unityBackgroundImageTintColor = Color.black;
                }
            }
        }

        public void OpenSettings()
        {
             settingPanel.gameObject.SetActive(true);
            GameManager.Instance.HandleWaitingGameState();
        }

        public void CloseSetting()         {
            settingPanel.gameObject.SetActive(false);
            GameManager.Instance.HandleCanMoveGameState();
        }
    }
}
