using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CandyProject
{
    public enum InputMode
    {
        Normal,
        UseInventoryGem
    }

    public class InputManager : Singleton<InputManager>
    {
        private InputSystem_Actions inputActions;

        [SerializeField] private Vector2 mouseDownPos;
        private bool isDragging;

        [SerializeField] private Vector2 worldPos;

        private float dragThreshold = 0.5f;

        [SerializeField] private Gem selectedGem;


        [SerializeField] float timeMove = 0.1f;

        [SerializeField] HintManager hintManager;

        [field: SerializeField] public InputMode Mode { get; private set; } = InputMode.Normal;

        private GemData pendingGemData = null;

        public static event Action OnItemUsed;
        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new InputSystem_Actions();
            }
            inputActions.UI.Point.performed += ctx => worldPos = ctx.ReadValue<Vector2>();

            inputActions.UI.Click.started += OnClickDown;
            inputActions.UI.Click.canceled += OnClickUp;


            inputActions.Enable();
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.UI.Point.performed -= ctx => worldPos = ctx.ReadValue<Vector2>();

                inputActions.UI.Click.started -= OnClickDown;
                inputActions.UI.Click.canceled -= OnClickUp;
                inputActions.Disable();
            }
        }

        public void DisableDrag()
        {
            if (inputActions.UI.enabled)
                inputActions.UI.Disable();


        }

        public void EnableDrag()
        {
            if (!inputActions.UI.enabled)
                inputActions.UI.Enable();
        }

        private void OnClickDown(InputAction.CallbackContext ctx)
        {
            if (hintManager != null)
            { 
                hintManager.ClearHintMark(); 
            }

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(
                new Vector3(worldPos.x, worldPos.y, 0));

            if (GameManager.Instance.Board != null)
            {
                Vector2Int grid = WorldToGrid(mousePos);
                if (Mode == InputMode.UseInventoryGem)
                {
                    TryUseInventoryGem(grid);
                    return;
                }
            }
            
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null)
            {
                Gem gem = hit.GetComponent<Gem>();

                if (gem != null)
                {
                    gem.SetColorGemSelected();
                    selectedGem = gem;
                    mouseDownPos = mousePos;
                    isDragging = true;
                }
            }
        }

        private void OnClickUp(InputAction.CallbackContext ctx)
        {
            if (!isDragging || selectedGem == null) return;

            selectedGem.ResetColor();
            Vector2 mouseWorld = Camera.main.ScreenToWorldPoint(inputActions.UI.Point.ReadValue<Vector2>());
            Vector2 dragDir = mouseWorld - mouseDownPos;

            if (dragDir.magnitude > dragThreshold)
            {
                Vector2Int moveDir = GetMoveDirection(dragDir);
                GameManager.Instance.Board.TrySwap(selectedGem, moveDir, timeMove);
            }

            isDragging = false;
            selectedGem = null;
        }

        private Vector2Int GetMoveDirection(Vector2 dragDir)
        {
            // Tìm hướng kéo 
            if (Mathf.Abs(dragDir.x) > Mathf.Abs(dragDir.y))
                return dragDir.x > 0 ? Vector2Int.right : Vector2Int.left;
            else
                return dragDir.y > 0 ? Vector2Int.up : Vector2Int.down;
        }

        private void TryUseInventoryGem(Vector2Int grid)
        {
            BoardManager board = GameManager.Instance.Board;

            if (!board.IsInsideBoard(grid))
            {
                ExitUseItemMode();
                return;
            }
            

            if (board.crates[grid.x, grid.y] != null) return;
            if (board.obstacle[grid.x, grid.y]) return;
           

            Gem oldGem = board.gems[grid.x, grid.y];

            if (oldGem != null)
            {
                oldGem.ReturnPoolGem(board.GemPrefab);
                board.gems[grid.x, grid.y] = null;
            }

            Vector2 spawnWorld = (Vector2)grid * board.CellSize;

            Gem newGem = board.CreateGem(spawnWorld, pendingGemData);
            newGem.gridPos = grid;

            board.gems[grid.x, grid.y] = newGem;

            ResourceManager.Instance.UseItem(pendingGemData);
            GameManager.Instance.SaveResources();
            ExitUseItemMode();

            OnItemUsed?.Invoke();
        }


        private Vector2Int WorldToGrid(Vector2 worldPos)
        {
            int x = Mathf.RoundToInt(worldPos.x / GameManager.Instance.Board.CellSize);
            int y = Mathf.RoundToInt(worldPos.y / GameManager.Instance.Board.CellSize);
            return new Vector2Int(x, y);
        }

        public void EnterUseItemMode(GemData gemData)
        {
            pendingGemData = gemData;
            
            if (ResourceManager.Instance.BoostersIV[pendingGemData] <= 0)
            {
                ExitUseItemMode();
            }
            else
            {
                Mode = InputMode.UseInventoryGem;
            }
            
        }

        public void ExitUseItemMode()
        {
            Mode = InputMode.Normal;
            pendingGemData = null;
        }

        public void RegisterHintManager(HintManager hintManager)
        {
            this.hintManager = hintManager;
        }



    }
}
