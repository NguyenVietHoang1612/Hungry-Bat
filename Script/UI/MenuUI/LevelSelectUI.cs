using UnityEngine;

namespace CandyProject
{
    public class LevelSelectUI : MonoBehaviour
    {
        [SerializeField] private Transform uiContainer;
        [SerializeField] private Transform levelContainer;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private GameObject newGameButton;

        private void Start()
        {
            ObjectPoolManager.Instance.CreatePool(levelButtonPrefab, 7);
        }

        public void GenerateLevelButtons()
        {
            ClearLevelContainer();
            EnterLevelContainer();
            var levels = GameManager.Instance.GetAllProgress();

            if (levels == null || levels.Count == 0)
            {
                Debug.LogWarning("No level progress data found.");
                return;
            }

            for (int i = 0; i < levels.Count; i++)
            {
                var buttonObj = ObjectPoolManager.Instance.Get(levelButtonPrefab);
                buttonObj.transform.SetParent(levelContainer);
                buttonObj.transform.localScale = Vector3.one;
                buttonObj.transform.localPosition = Vector3.zero;
                buttonObj.transform.localRotation = Quaternion.identity;

                var buttonUI = buttonObj.GetComponent<LevelButtonUI>();
                buttonUI.Setup(levels[i], i);
            }
            
        }

        private void ClearLevelContainer()
        {
            foreach (Transform child in levelContainer)
                ObjectPoolManager.Instance.Return(levelButtonPrefab, child.gameObject);
        }

        public void EnterLevelContainer()
        {
            
            uiContainer.gameObject.SetActive(true);
            newGameButton.SetActive(false);
        }

        public void ExitLevelContainer()
        {
            ClearLevelContainer();
            uiContainer.gameObject.SetActive(false);
            newGameButton.SetActive(true);
        }
    }
}
