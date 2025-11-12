using UnityEngine;
using UnityEngine.VFX;

namespace CandyProject
{
    public class PlayEffectController : MonoBehaviour
    {
        [SerializeField] private VisualEffect WinVFX;
        [SerializeField] private VisualEffect LoseVFX;

        private void Awake()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterVFXController(this);
            }
        }
        public void PlayWinEffect()
        {
 
            WinVFX.gameObject.SetActive(true);
        }

        public void StopWinEffect()
        {
            WinVFX.gameObject.SetActive(false);
        }

        public void PlayLoseEffect()
        {
            LoseVFX.gameObject.SetActive(true);
        }

    }
}
