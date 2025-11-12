using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace CandyProject
{
    public class GemConditionUI : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;

        private LevelData levelData;
        private VisualTreeAsset gemRequireTemplate;
        private VisualElement gemConditionContainer;
        private readonly List<VisualElement> gemConditionItems = new();

        private Label remainingMoves;

        private void Awake()
        {
            gemRequireTemplate = Resources.Load<VisualTreeAsset>("UI/GemCondition/GemCondition");
        }

        private void Start()
        {
            levelData = levelManager.LevelData;
            var root = GetComponent<UIDocument>().rootVisualElement;
            gemConditionContainer = root.Q<VisualElement>("ConditionBackground");
            remainingMoves = root.Q<Label>("RemainingMoves");

            InitializeUI();
            levelManager.OnGemGoalsUpdated += UpdateUIConditionUI;
            levelManager.OnRestartLevel += InitializeUI;
        }

        private void InitializeUI()
        {
            gemConditionContainer.Clear();
            gemConditionItems.Clear();

            foreach (var goal in levelData.gemGoalDatas)
            {
                var gemSlot = gemRequireTemplate.CloneTree();
                var iconImage = gemSlot.Q<VisualElement>("Gem-Icon");
                var quantityLabel = gemSlot.Q<Label>("required-amount");

                iconImage.style.backgroundImage = new StyleBackground(goal.gemData.GetSprite());
                quantityLabel.text = goal.requiredAmount.ToString();

                gemConditionContainer.Add(gemSlot);
                gemConditionItems.Add(gemSlot);
            }

            remainingMoves.text = levelData.movesLimit.ToString();
        }

        private void UpdateUIConditionUI(List<GemGoal> gemGoals)
        {
            for (int i = 0; i < gemGoals.Count; i++)
            {
                var quantityLabel = gemConditionItems[i].Q<Label>("required-amount");
                quantityLabel.text = gemGoals[i].requiredAmount.ToString();
            }
            remainingMoves.text = levelManager.RemainingMoves.ToString();
        }
    }
}
