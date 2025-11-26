using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyProject
{
    public class HintManager : MonoBehaviour
    {
        [Header("Hint Settings")]
        [SerializeField] private float hintDelay = 10f;
        [SerializeField] private GameObject hintEffect;

        private float hintTimer;
        private Gem currentHint;
        private GameObject currentHintObj;
        private Coroutine hintCoroutine;
        public bool HintActive { get; private set; } = true;

        [SerializeField] LevelManager levelManager;


        private void Start()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.RegisterHintManager(this);
            }

            if (currentHint != null) 
            {
                ClearHintMark();
            }

            ObjectPoolManager.Instance.CreatePool(hintEffect, 1);
            hintTimer = hintDelay;
        }

        private void Update()
        {
            hintTimer -= Time.deltaTime;
            if (hintTimer <= 0 && currentHint == null)
            {
                HintMark();
            }
        }

        private Gem PickOneRandomHintMatches()
        {
            List<Gem> possibleMoves = GameManager.Instance.Board.ListMatchesSwap();
            if (possibleMoves.Count > 0)
            {
                int randomIndex = Random.Range(0, possibleMoves.Count);
                return possibleMoves[randomIndex];
            }
            Debug.Log("No possible moves found for hint.");
            return null;
        }


        public void HintMark()
        {
            if ((levelManager.IsLevelComplete() && HintActive && GameManager.Instance.HandleWaitingGameState()) || (!levelManager.IsLevelComplete() && levelManager.RemainingMoves <= 0))
                return;

            currentHint = PickOneRandomHintMatches();
            if (currentHint != null)
            {
                currentHintObj = ObjectPoolManager.Instance.Get(hintEffect);
                currentHintObj.transform.position = currentHint.transform.position;
                currentHintObj.transform.rotation = Quaternion.identity;

                if (hintCoroutine != null)
                    StopCoroutine(hintCoroutine);

                hintCoroutine = StartCoroutine(DisableHintAfterDelay(currentHintObj));
            }
        }

        private IEnumerator DisableHintAfterDelay(GameObject hintObj)
        {
            yield return GameManager.Instance.twoSecondDelay;
            ObjectPoolManager.Instance.Return(hintEffect, hintObj);
            currentHint = null;
            hintTimer = hintDelay;

        }

        public void ClearHintMark()
        {
            if (hintCoroutine != null)
                StopCoroutine(hintCoroutine);

            if (currentHintObj != null)
            {
                ObjectPoolManager.Instance.Return(hintEffect, currentHintObj);
                currentHintObj = null;
            }

            currentHint = null;
            hintTimer = hintDelay;
            hintCoroutine = null;
        }

        public void SetHint(bool value)
        {
            HintActive = value;
        }

        public GameObject HintPrefab => hintEffect;
    }
}
