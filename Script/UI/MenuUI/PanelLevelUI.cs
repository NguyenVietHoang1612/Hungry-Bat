using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CandyProject
{
    public class PanelLevelUI : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private TMP_Text currentLevelText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button playButton;
        [SerializeField] private Transform levelGoalContainer;
        [SerializeField] private Transform boosterContainer;
        [SerializeField] private CanvasGroup canvasGroup;

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            CloseUI();

            if (ResourceManager.Instance != null)
            {
                if (ResourceManager.Instance.CurrentHealth <= 0)
                {
                    playButton.interactable = false;
                }
                else
                {
                    playButton.interactable = true;
                    playButton.onClick.RemoveAllListeners();
                    playButton.onClick.AddListener(OnPlayButtonClicked);
                }
            }
        }

        private void OnEnable()
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseUI); 
        }

        public void Init(LevelData levelData)
        {
            currentLevelText.text = levelData.levelName;
            
            List<GemData> boostersAvailable = ResourceManager.Instance.BoostersIV.Keys.ToList();

            // Setup Booster
            for (int i = 0; i < boosterContainer.childCount; i++)
            {
                InventorySlot slot =  boosterContainer.GetChild(i).GetComponent<InventorySlot>();
                if (slot != null)
                {
                    slot.SetUp(boostersAvailable[i], ResourceManager.Instance.BoostersIV[boostersAvailable[i]]);
                }
            }

            //Setup GemGoal
            for (int i = 0;i < levelData.gemGoalDatas.Count; i++)
            {
                Image gemGoalIg = levelGoalContainer.GetChild(i).GetComponent<Image>();
                TMP_Text quantityRequire = gemGoalIg.GetComponentInChildren<TMP_Text>();

                gemGoalIg.sprite = levelData.gemGoalDatas[i].gemData.GetSprite();
                quantityRequire.text = levelData.gemGoalDatas[i].requiredAmount.ToString();
                gemGoalIg.gameObject.SetActive(true);
            }
        }

        public void OnPlayButtonClicked()
        {
            GameManager.Instance.LoadScene();
        }

        public void CloseUI()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            for (int i = 0; i < levelGoalContainer.childCount; i++)
            {
                levelGoalContainer.GetChild(i).gameObject.SetActive(false);
            }
        }

        public void OpenUI()
        {
            if (GameManager.Instance != null)
            {
                Init(GameManager.Instance.GetCurrentLevelData());
            }
            
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }
}
