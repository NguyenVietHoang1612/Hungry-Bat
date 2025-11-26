using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CandyProject
{
    public class SettingsUI : MonoBehaviour
    {
        [SerializeField] Slider musicSlider;
        [SerializeField] Slider sfxSlider;
        [SerializeField] private Transform settingPanel;
        [SerializeField] private Button homeButon;
        [SerializeField] private Button reStartButton;

        [SerializeField] LevelManager levelManager;
        private void Start()
        {
            musicSlider.onValueChanged.AddListener(value =>
            {
                SoundManager.Instance.SetMusicVolume(value);
                SaveSystem.SaveSettings(new SettingsData
                {
                    volumeMusic = value,
                    volumeSFX = SoundManager.Instance.VolumnSFX
                });
            });

            sfxSlider.onValueChanged.AddListener(value =>
            {
                SoundManager.Instance.SetSFXVolume(value);
                SaveSystem.SaveSettings(new SettingsData
                {
                    volumeMusic = SoundManager.Instance.VolumnMusic,
                    volumeSFX = value
                });
            });

            if (homeButon != null)
            {
                homeButon.onClick.AddListener(OnBackToMenuClicked);
            }

            if (reStartButton != null)
            {
                reStartButton.onClick.AddListener(OnReStartLevelClicked);
            }
            
            

            musicSlider.value = SoundManager.Instance.VolumnMusic;
            sfxSlider.value = SoundManager.Instance.VolumnSFX;
        }
        public void OpenSettings()
        {
            settingPanel.gameObject.SetActive(true);
            GameManager.Instance.TogglePause();
        }

        public void CloseSetting()
        {
            settingPanel.gameObject.SetActive(false);
            GameManager.Instance.TogglePause();
        }

        private void OnBackToMenuClicked()
        {
            GameManager.Instance.ExitLevel();
            ResourceManager.Instance.UseHealth(1);
            GameManager.Instance.AutoOpenLevelSelect = true;
        }
        private void OnReStartLevelClicked()
        {
            levelManager.Init(levelManager.LevelData);
            CloseSetting();
            GameManager.Instance.StartCoroutine(WaitResetLevel());
        }

        private IEnumerator WaitResetLevel()
        {
            yield return GameManager.Instance.oneSecondDelay;
            GameManager.Instance.RestartLevel();
        }
    }
}
