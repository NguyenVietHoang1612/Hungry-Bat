using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace CandyProject
{
    public class NotificationLabel : MonoBehaviour
    {
        [SerializeField] private Transform fadePanel;
        [SerializeField] private Transform GemRequireContainer;

        [SerializeField] private RectTransform notificationRect;
        
        [SerializeField] GameObject gemRequirePrefab;

        [SerializeField] LevelManager levelManager;

        [SerializeField] private float duration = 0.5f;
        [SerializeField] private float durationMove = 2f;
        private Vector2 startPoint;
        [SerializeField] private Vector2 endPoint;

        private void Start()
        {
            ObjectPoolManager.Instance.CreatePool(gemRequirePrefab, 6);
            InitialNotificationConditionLevel();
            MovePanelToEnd();
        }


        public void InitialNotificationConditionLevel()
        {
            var gemRequire = levelManager.LevelData.gemGoalDatas;
            startPoint = notificationRect.anchoredPosition;

            for (int i = 0; i < gemRequire.Count; i++)
            {
                var gemRequieObj = ObjectPoolManager.Instance.Get(gemRequirePrefab);
                gemRequieObj.transform.SetParent(GemRequireContainer);
                gemRequieObj.transform.localPosition = Vector3.zero;
                gemRequieObj.transform.localRotation = Quaternion.identity;
                gemRequieObj.transform.localScale = Vector3.one;
                Image gemRequireImage = gemRequieObj.GetComponent<Image>();
                gemRequireImage.sprite = gemRequire[i].gemData.GetSprite();
                TMP_Text amountRequire = gemRequireImage.GetComponentInChildren<TMP_Text>();
                amountRequire.text = gemRequire[i].requiredAmount.ToString();
            }

            SetActiveFade();
        }

        

        IEnumerator MovePanelCondition(Vector2 targetPos)
        {
            GameManager.Instance.HandleWaitingGameState();
            var startRect = notificationRect.anchoredPosition;
            float t = 0f;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                notificationRect.anchoredPosition = Vector2.Lerp(startRect, targetPos, t);
                yield return null;
            }
        }

        
        public void MovePanelToStart()
        {
            StartCoroutine(MovePanelCondition(startPoint));
            GameManager.Instance.HandleCanMoveGameState();
            SetInactiveFade();
        }

        public void MovePanelToEnd()
        {
            StartCoroutine(MovePanelCondition(endPoint));
            GameManager.Instance.HandleWaitingGameState();
            StartCoroutine(WaitForSecondNotification());
        }

        private IEnumerator WaitForSecondNotification()
        {
            yield return new WaitForSeconds(durationMove);
            MovePanelToStart();
        }

        public void SetActiveFade()
        {
            fadePanel.gameObject.SetActive(true);
        }

        public void SetInactiveFade()
        {
            fadePanel.gameObject.SetActive(false);
        }

    }
}
