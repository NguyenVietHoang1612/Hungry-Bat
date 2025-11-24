using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CandyProject
{
    public class LevelButtonUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private Image[] stars;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private Button playButton;
        [SerializeField] private Sprite starActive;

        [SerializeField] private PanelLevelUI panelLevelUI;

        private int levelIndex;
        private bool isUnlocked;

        private void Start()
        {
            panelLevelUI = FindFirstObjectByType<PanelLevelUI>();

            if (panelLevelUI == null )
            {
                Debug.Log("panelLevelUI not Found");
            }
        }

        public void Setup(LevelProgress progress, int index)
        {
            levelIndex = index;
            isUnlocked = progress.isUnlocked;

            UpdateVisual(progress, index + 1);
        }

        private void UpdateVisual(LevelProgress progress, int index)
        {
            if (progress == null)
            {
                Debug.LogWarning("LevelProgress data is null.");
            }

            levelText.text = index.ToString();

            for (int i = 0; i < stars.Length; i++)
            {
                bool star = (i < progress.starLevel);

                if (star)
                {
                    stars[i].sprite = starActive;
                }
                    
            }

            lockIcon.SetActive(!isUnlocked);
            playButton.gameObject.SetActive(isUnlocked);
            playButton.interactable = isUnlocked;
        }

        public void OnPlayButtonClicked()
        {
            if (!isUnlocked) return;

            GameManager.Instance.SetCurrentIndex(levelIndex);
            panelLevelUI.OpenUI();
        }
    }
}
