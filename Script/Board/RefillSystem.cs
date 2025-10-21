using System.Collections;
using UnityEngine;

namespace CandyProject
{
    public class RefillSystem
    {
        private BoardManager board;

        public RefillSystem(BoardManager board)
        {
            this.board = board;
        }

        public void ClearMatchedGems()
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    var gem = board.gems[x, y];
                    if (gem != null && gem.isMatch)
                    {
                        gem.PlayDestroyEffect();
                        gem.ReturnPoolGem(board.GemPrefab);
                        board.gems[x, y] = null;
                    }
                }


            }
            
            board.StartCoroutine(DropDownGems());
        }

        // Rơi gem xuống ô trống
        private IEnumerator DropDownGems()
        {
            yield return new WaitForSeconds(0.2f);
            for (int x = 0; x < board.Width; x++)
            {
                int emptyY = -1;
                for (int y = 0; y < board.Height; y++)
                {
                    if (board.gems[x, y] == null)
                    {
                        Debug.LogWarning("DropDownPlay");
                        if (emptyY == -1) emptyY = y;
                    }
                    else if (emptyY != -1)
                    {
                        board.gems[x, y].MoveGravityTo(new Vector2Int(x, emptyY));
                        board.gems[x, emptyY] = board.gems[x, y];
                        board.gems[x, y] = null;
                        emptyY++;
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
            board.StartCoroutine(RefillingGem());
        }

        // Sinh gem làm đầy board
        private IEnumerator RefillingGem()
        {
            yield return new WaitForSeconds(0.1f);

            int width = board.Width;
            int height = board.Height;

            for (int x = 0; x < width; x++)
            {
                bool anySpawned = false;
                for (int y = 0; y < height; y++)
                {
                    if (board.gems[x, y] == null)
                    {
                        anySpawned = true;

                        float spawnY = height + (Random.Range(0.1f, 0.3f) * board.CellSize);
                        Vector2 worldPos = new Vector2(x, spawnY) * board.CellSize;
                        Vector2Int gridPos = new Vector2Int(x, y);

                        GemData randomGem = board.GemDatas[
                            Random.Range(0, board.GemDatas.Length - board.NumSpecials())
                        ];

                        Gem newGem = board.CreateGem(worldPos, randomGem);
                        newGem.gridPos = gridPos;
                        newGem.MoveGravityTo(gridPos);

                        board.gems[x, y] = newGem;
                    }
                }
                if (anySpawned)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }

            yield return new WaitForSeconds(0.3f);
            board.FindMatches();
        }



    }
}
