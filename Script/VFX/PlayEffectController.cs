using UnityEngine;
using UnityEngine.VFX;

namespace CandyProject
{
    public class PlayEffectController : MonoBehaviour
    {
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private VisualEffect WinVFX;
        [SerializeField] private VisualEffect LoseVFX;

        private void PlayWinEffect()
        {
            GameManager.Instance.HandleWaitingGameState();
            WinVFX.gameObject.SetActive(true);
        }

        private void PlayLoseEffect()
        {
            GameManager.Instance.HandleWaitingGameState();
            LoseVFX.gameObject.SetActive(true);
        }

    }
}
