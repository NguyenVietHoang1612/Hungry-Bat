using UnityEngine;
using UnityEngine.InputSystem;

namespace CandyProject
{
    public class InputManager : Singleton<InputManager>
    {
        private InputSystem_Actions inputActions;

        [SerializeField] private Vector2 mouseDownPos;
        private bool isDragging;

        [SerializeField] private Vector2 WorldPos;

        private float dragThreshold = 0.5f;

        [SerializeField] private Gem selectedGem;


        [SerializeField] float timeMove = 0.1f;
        private void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new InputSystem_Actions();
            }
            inputActions.UI.Point.performed += ctx => WorldPos = ctx.ReadValue<Vector2>();

            inputActions.UI.Click.started += OnClickDown;
            inputActions.UI.Click.canceled += OnClickUp;



            inputActions.Enable();
        }

        private void OnDisable()
        {
            if (inputActions != null)
            {
                inputActions.UI.Point.performed -= ctx => WorldPos = ctx.ReadValue<Vector2>();

                inputActions.UI.Click.started -= OnClickDown;
                inputActions.UI.Click.canceled -= OnClickUp;
                inputActions.Disable();
            }
        }

        private void OnClickDown(InputAction.CallbackContext ctx)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(
            new Vector3(WorldPos.x, WorldPos.y, 0));

            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null)
            {
                Gem gem = hit.GetComponent<Gem>();
                
                if (gem != null)
                {
                    Debug.Log("Hit " + gem.TypeOfGem);
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
                BoardManager.Instance.TrySwap(selectedGem, moveDir, timeMove);
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

        
    }
}
