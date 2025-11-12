using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

namespace CandyProject
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameFinish,
        CanMove,
        Waiting
    }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameState currentState;

        [SerializeField] private List<LevelData> allLevelData;
        private List<LevelProgress> levelProgressList = new List<LevelProgress>();

        [SerializeField] private LevelManager levelManager;
        private int currentLevelIndex;
        public LevelData CurrentLevelData { get; private set; }
        public GameState CurrentState => currentState;

        public BoardManager Board;

        [SerializeField] private PlayEffectController playEffectController;

        [SerializeField] AudioClip musicMenu;
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
            LoadSettings();
        }

        #region Level Progress

        public void LoadProgress()
        {
            InitializeProgressList();
            levelProgressList = SaveSystem.LoadProgress(levelProgressList);
        }

        void GetReferences()
        {
            Board = FindFirstObjectByType<BoardManager>();
        }

        private void InitializeProgressList()
        {
            levelProgressList.Clear();

            foreach (var data in allLevelData)
            {
                levelProgressList.Add(new LevelProgress
                {
                    levelData = data,
                    isUnlocked = data.levelIndex == 1,
                    bestScore = 0,
                    starLevel = 0
                });
            }
        }

        public List<LevelProgress> GetAllProgress() => levelProgressList;

        #endregion

        #region Level Control

        public void SetCurrentIndex(int index)
        {
            currentLevelIndex = Mathf.Clamp(index, 0, allLevelData.Count - 1);
        }

        public void SaveSettings()
        {
            SettingsData settingsData = new SettingsData();
            settingsData.volumeMusic = SoundManager.Instance.VolumnMusic;
            settingsData.volumeSFX = SoundManager.Instance.VolumnSFX;
            SaveSystem.SaveSettings(settingsData);
        }

        public void LoadSettings()
        {
            var data = SaveSystem.LoadSetings();
            if (data == null) return;

            SoundManager.Instance.SetMusicVolume(data.volumeMusic);
            SoundManager.Instance.SetSFXVolume(data.volumeSFX);
        }

        public void LoadScene()
        {
            
            StartCoroutine(LoadSceneWaitForSecond("SceneGame"));
        }

        private IEnumerator LoadSceneWaitForSecond(string sceneName)
        {
            StartCoroutine(FadeController.Instance.FadeOut());
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene(sceneName);
            StartCoroutine(FadeController.Instance.FadeIn());
        }

        public void ExitLevel()
        {
            currentLevelIndex = 0;
            levelManager = null;
            StartCoroutine(FadeController.Instance.FadeOut());
            StartCoroutine(LoadSceneWaitForSecond("SceneMenu"));
            SoundManager.Instance.PlayMusic(musicMenu);
        }

        public void RestartLevel()
        {
            playEffectController.StopWinEffect();

            if (Board != null)
                Board.ClearBoard();

            StartLevel();
            StartCoroutine(LoadSceneWaitForSecond("SceneGame"));
            HandleCanMoveGameState();
        }

        public void StartLevel()
        {
            GetReferences();
            if (currentLevelIndex < 0 || currentLevelIndex >= allLevelData.Count)
            {
                Debug.LogError("Invalid level index");
                return;
            }

            CurrentLevelData = allLevelData[currentLevelIndex];

            if (levelManager == null)
            {
                Debug.LogError("LevelManager not registered!");
                return;
            }

            levelManager.Init(CurrentLevelData);
            SoundManager.Instance.StopMusic();
        }

        public void NextLevel()
        {
            if (Board != null)
            {
                Board.ClearBoard();
                Destroy(Board.gameObject);
                Board = null;
            }

            playEffectController?.StopWinEffect();

            if (allLevelData == null || allLevelData.Count == 0)
            {
                Debug.LogError("No level data found!");
                return;
            }

            int nextIndex = currentLevelIndex + 1;

            if (nextIndex < allLevelData.Count)
            {
                currentLevelIndex = nextIndex;

                if (nextIndex < levelProgressList.Count)
                    levelProgressList[nextIndex].isUnlocked = true;

                SaveSystem.SaveProgress(levelProgressList);

                StartCoroutine(LoadSceneWaitForSecond("SceneGame"));
            }
            else
            {
                Debug.Log("All levels completed! Returning to menu...");
                ExitLevel();
            }
        }


        public void RegisterLevelManager(LevelManager manager)
        {
            levelManager = manager;
        }

        public void LevelComplete(int levelIndex, int score)
        {
            playEffectController.PlayWinEffect();
            HandleWaitingGameState();
            StartCoroutine(OnLevelComplete(levelIndex, score));
        }
        


        private IEnumerator OnLevelComplete(int levelIndex, int score)
        {
            yield return new WaitForSeconds(2f);
            if (levelIndex < 0 || levelIndex >= levelProgressList.Count) yield break;

            var progress = levelProgressList[levelIndex];
            progress.bestScore = Mathf.Max(progress.bestScore, score);
            progress.starLevel = levelManager.StarLevel;
            progress.isUnlocked = true;

            if (levelIndex + 1 < levelProgressList.Count)
                levelProgressList[levelIndex + 1].isUnlocked = true;

            SaveSystem.SaveProgress(levelProgressList);
        }

        public void RegisterVFXController(PlayEffectController controller)
        {
            playEffectController = controller;
        }

        #endregion

        #region Game State Control

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
                Time.timeScale = 0;
            }
            else if (currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
                Time.timeScale = 1;
            }
        }

        public void SetGameState(GameState newState)
        {
            currentState = newState;
        }

        public void HandleWaitingGameState()
        {
            SetGameState(GameState.Waiting);
            InputManager.Instance.DisableDrag();
        }

        public void HandleCanMoveGameState()
        {
            SetGameState(GameState.CanMove);
            InputManager.Instance.EnableDrag();
        }

        #endregion

        #region System

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        #endregion
    }
}
