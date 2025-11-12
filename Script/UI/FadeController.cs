using System.Collections;
using UnityEngine;
using UnityEngine.UI;


namespace CandyProject
{
    public class FadeController : Singleton<FadeController>
    {
        private CanvasGroup canvasGroup;
        private Image fadeImage;
        public float fadeDuration = 1f;
        public float DeltaTime;


        private void Start()
        {
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            fadeImage = GetComponentInChildren<Image>();
            canvasGroup.alpha = 0;
        }

        private void Update()
        {
            DeltaTime = Time.deltaTime;
        }

        public IEnumerator  FadeIn()
        {
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Max(0, 1 - (t / fadeDuration));
                yield return null;
            }
            DisableRaycast();
            
        }

        public IEnumerator FadeOut()
        {
            EnableRaycast();
            float t = 0;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = Mathf.Min(t / fadeDuration, 1);
                yield return null;
            }
            canvasGroup.alpha = 1;
        }

        private void EnableRaycast()
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            fadeImage.raycastTarget = true;

        }

        private void DisableRaycast()
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
            fadeImage.raycastTarget = false;
        }
    }
}
