using UnityEngine;
using UnityEngine.U2D;

namespace CandyProject
{
    public class BoardTile : MonoBehaviour
    {

        [SerializeField] private SpriteAtlas boardTile;
        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = boardTile.GetSprite("board_tiles_22");
        }
    }
}
