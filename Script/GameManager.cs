using UnityEngine;

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

        public GameState CurrentState
        {
            get => currentState;
            private set { currentState = value; }
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                Time.timeScale = 0;
                currentState = GameState.Paused;
            }
            else if (currentState == GameState.Paused)
            {
                Time.timeScale = 1;
                currentState = GameState.Playing;
            }
        }

        public void WaitingFindMatch()
        {
            currentState = GameState.Waiting;
            InputManager.Instance.DisableDrag();

        }

        public void HandleCanMoveGameState()
        {
            currentState = GameState.CanMove;
            InputManager.Instance.EnableDrag();

        }



        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

    }
}
