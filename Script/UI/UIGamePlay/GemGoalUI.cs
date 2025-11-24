using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CandyProject
{
    public class GemGoalUI : MonoBehaviour
    {
        [SerializeField] private GameObject gemGoalPrefab;
        [SerializeField] private LevelManager levelManager;
        private LevelData levelData;

        [Header("UI Setup")]
        [SerializeField] private Transform gemGoalListTransform;
        [SerializeField] private List<TMP_Text> gemGoalsText;

        private void Start()
        {
            levelData = levelManager.LevelData;

            InitializeUI();    
        }

        private void OnEnable()
        {
            if (levelManager == null)
            {
                Debug.Log("LevelManager not found");
                return;
            }

            levelManager.OnGemGoalsUpdated += UpdateUIConditionUI;
            levelManager.OnRestartLevel += InitializeUI;
        }

        private void OnDisable()
        {
            levelManager.OnGemGoalsUpdated -= UpdateUIConditionUI;
            levelManager.OnRestartLevel -= InitializeUI;
        }

        private void InitializeUI()
        {

            gemGoalsText.Clear();
            foreach (var goal in levelData.gemGoalDatas)
            {
                if (goal == null || goal.gemData == null) continue;

                GameObject gemGoalGO = Instantiate(gemGoalPrefab);
                gemGoalGO.transform.SetParent(gemGoalListTransform);
                gemGoalGO.transform.localRotation = Quaternion.identity;
                gemGoalGO.transform.localScale = Vector3.one;
                var imageGemGoal = gemGoalGO.GetComponent<Image>();
                var requireAmount = gemGoalGO.GetComponentInChildren<TMP_Text>();
                
                imageGemGoal.sprite = goal.gemData.GetSprite();
                requireAmount.text = goal.requiredAmount.ToString();
                gemGoalsText.Add(requireAmount);

            }
        }

        private void UpdateUIConditionUI(List<GemGoal> gemGoals, Gem gem)
        {
            for (int i = 0; i < gemGoals.Count; i++)
            {
                var quantityLabel = gemGoalsText[i];
                var rect = quantityLabel.rectTransform;
                Vector3 worlPos = Camera.main.ScreenToWorldPoint(rect.position);

                if (gem != null)
                {
                    gem.MoveGemtoCondition(worlPos);
                }
               
                quantityLabel.text = gemGoals[i].requiredAmount.ToString();
            }
        }

    }
}
