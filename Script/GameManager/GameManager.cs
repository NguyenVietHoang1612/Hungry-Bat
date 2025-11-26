using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CandyProject
{
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameFinish,
        Waiting
    }

    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private GameState currentState;

        [SerializeField] private List<LevelData> allLevelData;
        public List<LevelData> AllLevelData => allLevelData;

        private List<LevelProgress> levelProgressList = new List<LevelProgress>();

        [SerializeField] private LevelManager levelManager;
        [SerializeField] private int currentLevelIndex;
        public LevelData CurrentLevelData { get; private set; }
        public GameState CurrentState => currentState;

        public BoardManager Board;

        [SerializeField] private PlayEffectController playEffectController;

        [SerializeField] AudioClip musicMenu;

        [Header("Flags")]
        public bool AutoOpenLevelSelect { get; set; } = false;
        [field: SerializeField] public bool Effects { get; set; } = true;

        [Header("Coroutine")]
        public readonly WaitForSeconds delay = new WaitForSeconds(0.1f);
        public readonly WaitForSeconds secondDelay = new WaitForSeconds(0.3f);
        public readonly WaitForSeconds halfSecondDelay = new WaitForSeconds(0.5f);
        public readonly WaitForSeconds oneSecondDelay = new WaitForSeconds(1f);
        public readonly WaitForSeconds twoSecondDelay = new WaitForSeconds(2f);
        protected override void Awake()
        {
            base.Awake();
            #if UNITY_ANDROID || UNITY_IOS
                Application.targetFrameRate = 60;
            #endif
            LoadSettings();
        }

        private void Update()
        {
            if (HandleWaitingGameState())
            {
                InputManager.Instance.DisableDrag();
            }

            if (HandleCanMoveGameState())
            {
                InputManager.Instance.EnableDrag();
            }
            
        }

        #region Save Load

        public void LoadProgress()
        {
            InitializeProgressList();
            levelProgressList = SaveSystem.LoadProgress(levelProgressList);
        }
      

        public void SaveResources()
        {
            ResourceData resourceData = new ResourceData() 
            { 
                gold = ResourceManager.Instance.CurrentGold, 
                health = ResourceManager.Instance.CurrentHealth, 
                boosters = ResourceManager.Instance.BoostersIV 
            };
            SaveSystem.SaveResource(resourceData);
        }

        public void LoadResources()
        {
            var data = SaveSystem.LoadResource();
            if (data == null) return;

            ResourceManager.Instance.SetGold(data.gold);
            ResourceManager.Instance.SetHealth(data.health);
            ResourceManager.Instance.SetBoosterIV(data.boosters);
        }

        public void SaveSettings()
        {
            SettingsData settingsData = new SettingsData();
            settingsData.volumeMusic = SoundManager.Instance.VolumnMusic;
            settingsData.volumeSFX = SoundManager.Instance.VolumnSFX;
            settingsData.enableEffect = Effects;
            SaveSystem.SaveSettings(settingsData);
        }
        public void LoadSettings()
        {
            var data = SaveSystem.LoadSetings();
            if (data == null) return;

            SoundManager.Instance.SetMusicVolume(data.volumeMusic);
            SoundManager.Instance.SetSFXVolume(data.volumeSFX);
            Effects = data.enableEffect;
        }

        #endregion

        #region Level Control

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

        public void SetCurrentIndex(int index)
        {
            currentLevelIndex = Mathf.Clamp(index, 0, allLevelData.Count - 1);
        }

        public void LoadScene()
        {
            StartCoroutine(LoadSceneWaitForSecond("SceneGame"));
        }

        private IEnumerator LoadSceneWaitForSecond(string sceneName)
        {
            StartCoroutine(FadeController.Instance.FadeOut());
            yield return oneSecondDelay; 
            SceneManager.LoadScene(sceneName);
            Debug.Log("Load scene: " + sceneName);
            StartCoroutine(FadeController.Instance.FadeIn());
        }

        public void ExitLevel()
        {
            currentLevelIndex = 0;
            levelManager = null;

            if (Board != null)
                Board.ClearBoard();

            StartCoroutine(LoadSceneWaitForSecond("SceneMenu"));

            SoundManager.Instance.PlayMusic(musicMenu);
        }

        public void RestartLevel()
        {
            playEffectController.StopWinEffect();

            StartLevel();
            StartCoroutine(LoadSceneWaitForSecond("SceneGame"));
            GameManager.Instance.SetGameState(GameState.Playing);
        }

        public void StartLevel()
        {
            GetReferences();

            if (Board != null)
                Board.ClearBoard();

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

        public void LevelComplete(int score)
        {
            playEffectController.PlayWinEffect();
            SetGameState(GameState.Waiting);
            StartCoroutine(OnLevelComplete(score));
        }
        
        private IEnumerator OnLevelComplete(int score)
        {
            yield return twoSecondDelay;
            if (currentLevelIndex < 0 || currentLevelIndex >= levelProgressList.Count || levelManager == null) yield break;

            var progress = levelProgressList[currentLevelIndex];
            progress.bestScore = Mathf.Max(progress.bestScore, score);
            progress.starLevel = levelManager.StarLevel;
            progress.isUnlocked = true;

            if (currentLevelIndex + 1 < levelProgressList.Count)
                levelProgressList[currentLevelIndex + 1].isUnlocked = true;

            SaveSystem.SaveProgress(levelProgressList);
            SaveResources();
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
                //SetGameState(GameState.Paused);
                SetGameState(GameState.Waiting);

                //Time.timeScale = 0;
            }
            else if (currentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
                //Time.timeScale = 1;
            }
        }

        public void SetGameState(GameState newState)
        {
            currentState = newState;
        }

        public bool HandleWaitingGameState()
        {
            if (currentState == GameState.Waiting)
            { 
                return true;
            }

            return false;
            
        }

        public bool HandleCanMoveGameState()
        {
            if (currentState == GameState.Playing)
            {
                
                return true;
            }

            return false;
            
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


        public LevelData GetCurrentLevelData() => AllLevelData[currentLevelIndex];
    }
}
