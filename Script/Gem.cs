using UnityEngine;

namespace CandyProject
{
    public class Gem : MonoBehaviour
    {
        [SerializeField] private GemData gemData;

        [SerializeField] private SpriteRenderer spriteRenderer;

        public Vector2 gridPos;

        private Vector3 targetPos;

        [Header("Flags")]
        private bool isMoving;
        public bool isMatch = false;


        [SerializeField] private float moveSpeed = 5f;

        public void Init(GemData data)
        {
            gemData = data;
            spriteRenderer.sprite = gemData.gemIcon;

        }
        private void Update()
        {
            if (isMatch)
            {
                spriteRenderer.color = Color.black;
            }

            if (isMoving)
            {
                Vector2 boardCellSize = targetPos * BoardManager.Instance.GetCellSize();
                transform.position = Vector3.Slerp(transform.position, boardCellSize, moveSpeed * Time.deltaTime);

                if (Vector3.Distance(transform.position, boardCellSize) < 0.1f)
                {
                    transform.position = boardCellSize;
                    isMoving = false;
                }
            }
                
        }

        public void MoveTo(Vector2 newPos)
        {
            targetPos = newPos;
            gridPos = targetPos;
            isMoving = true;
        }

        
       

        public string GemName => gemData.gemName;
        public int Score => gemData.scoreValue;
    }
}
