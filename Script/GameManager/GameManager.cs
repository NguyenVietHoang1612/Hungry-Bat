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

        [SerializeField] AudioClip musicMenu;
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
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

        public void LoadScene()
        {
            StartCoroutine(FadeController.Instance.FadeOut());
            StartCoroutine(LoadSceneWaitForSecond("SceneGame"));
        }

        private IEnumerator LoadSceneWaitForSecond(string sceneName)
        {
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
            StartLevel();
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

        public void RegisterLevelManager(LevelManager manager)
        {
            levelManager = manager;
        }

        public void OnLevelComplete(int levelIndex, int score)
        {
            if (levelIndex < 0 || levelIndex >= levelProgressList.Count) return;

            var progress = levelProgressList[levelIndex];
            progress.bestScore = Mathf.Max(progress.bestScore, score);
            progress.starLevel = levelManager.StarLevel;
            progress.isUnlocked = true;

            if (levelIndex + 1 < levelProgressList.Count)
                levelProgressList[levelIndex + 1].isUnlocked = true;

            SaveSystem.SaveProgress(levelProgressList);
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
