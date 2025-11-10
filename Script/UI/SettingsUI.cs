using UnityEngine;
using UnityEngine.UI;

namespace CandyProject
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;

        private void Start()
        {
            musicSlider.onValueChanged.AddListener(SoundManager.Instance.SetMusicVolume);
            sfxSlider.onValueChanged.AddListener(SoundManager.Instance.SetSfxVolume);
        }

        
    }
}
