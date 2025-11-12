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

                        board.LevelManager.AddScore(gem.GetGemData.scoreValue);
                        board.LevelManager.ReduceGemGoal(gem);

                        gem.ReturnPoolGem(board.GemPrefab);
                        board.gems[x, y] = null;

                        if (board.bonusGem[x, y] != null)
                        {
                            board.bonusGem[x, y].Damage(1);
                        }
                    }
              
                }
            }

            board.StartCoroutine(CollapsingGem());
        }

        // Rơi gem lấp ô trống
        private IEnumerator CollapsingGem()
        {
            yield return new WaitForSeconds(0.2f);

            int width = board.Width;
            int height = board.Height;

            CollapseVertical(width, height);
            //CollapseDiagonalAndHorizontal(width, height);

            yield return new WaitForSeconds(0.1f);
            board.StartCoroutine(RefillingGem());
        }

        private void CollapseVertical(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                int emptyY = -1;
                for (int y = 0; y < height; y++)
                {
                    if (board.obstacle[x, y])
                    {
                        emptyY = -1;
                        continue;

                    }

                    if (board.gems[x, y] == null)
                    {

                        if (emptyY == -1) emptyY = y;
                    }
                    else if (emptyY != -1)
                    {

                        board.gems[x, y].MoveTo(new Vector2Int(x, emptyY));
                        board.gems[x, emptyY] = board.gems[x, y];
                        board.gems[x, y] = null;
                        emptyY++;
                    }
                }
            }
        }

        // Rơi chéo
        //private void CollapseDiagonalAndHorizontal(int width, int height)
        //{
        //    for (int x = 0; x < width; x++)
        //    {
        //        for (int y = 0; y < height; y++)
        //        {
        //            if (x + 1 >= width || x + 2 >= width || x - 1 < 0 || x - 2 < 0 || y + 1 >= height || y - 1 < 0)
        //                continue;

        //            Gem currentGem = board.gems[x, y]; ;

        //            while (currentGem != null && board.obObstacle[x + 1, y] && !board.obObstacle[x + 1, y - 1] && board.gems[x + 1, y - 1] == null)
        //            {

        //                board.gems[x, y].MoveTo(new Vector2Int(x + 1, y - 1));
        //                board.gems[x + 1, y - 1] = board.gems[x, y];
        //                board.gems[x, y] = null;

        //                CollapseVertical(width, height);
        //            }

        //            while (currentGem != null && board.obObstacle[x - 1, y] && !board.obObstacle[x - 1, y - 1] && board.gems[x - 1, y - 1] == null)
        //            {

        //                board.gems[x, y].MoveTo(new Vector2Int(x - 1, y - 1));
        //                board.gems[x - 1, y - 1] = board.gems[x, y];
        //                board.gems[x, y] = null;

        //                CollapseVertical(width, height);
        //            }




        //            int randomDrop = Random.Range(0, 10);

        //            if (randomDrop > 5)
        //            {
        //                if (currentGem != null && !board.obObstacle[x + 1, y] && board.obObstacle[x, y + 1] && board.obObstacle[x + 1, y + 1] && board.obObstacle[x + 2, y + 1] && board.gems[x + 1, y] == null)
        //                {
        //                    board.gems[x, y].MoveTo(new Vector2Int(x + 1, y));
        //                    board.gems[x + 1, y] = board.gems[x, y];
        //                    board.gems[x, y] = null;
        //                }
        //            }
        //            else
        //            {
        //                if (currentGem != null && !board.obObstacle[x - 1, y] && board.obObstacle[x, y + 1] && board.obObstacle[x - 1, y + 1] && board.obObstacle[x - 2, y + 1] && board.gems[x - 1, y] == null)
        //                {
        //                    board.gems[x, y].MoveTo(new Vector2Int(x - 1, y));
        //                    board.gems[x - 1, y] = board.gems[x, y];
        //                    board.gems[x, y] = null;
        //                }
        //            }

        //        }
        //    }
        //}


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
                    if (board.obstacle[x, y]) continue;

                    if (board.gems[x, y] == null)
                    {
                        anySpawned = true;

                        float spawnY = height + (Random.Range(0.1f, 0.3f) * board.CellSize);
                        Vector2 worldPos = new Vector2(x, spawnY) * board.CellSize;
                        Vector2Int gridPos = new Vector2Int(x, y);

                        GemData randomGem = board.GemDatas[
                            Random.Range(0, board.GemDatas.Length - 7)
                        ];

                        Gem newGem = board.CreateGem(worldPos, randomGem);
                        newGem.gridPos = gridPos;
                        newGem.MoveTo(gridPos);

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

            yield return new WaitForSeconds(1f);
            
            if (board.CheckDeadLock)
            {
                Debug.Log("Deadlock detected! Shuffling the board...");
                board.ShuffleBoard();
            }

            yield return new WaitForSeconds(0.5f);
            // Kiểm tra lại lần cuối sau khi refill và match hoàn toàn xong
            if (board.LevelManager.IsLevelComplete())
            {
                GameManager.Instance.LevelComplete(
                    board.LevelManager.LevelData.levelIndex - 1,
                    board.LevelManager.CurrentScore
                );
                board.LevelManager.uiAchievedResults.ShowAchievedResults();
                Debug.Log("Level Complete!");
            }
            else if (!board.LevelManager.IsLevelComplete() && board.LevelManager.RemainingMoves <= 0)
            {
                Debug.Log("You Lose!");
            }

            GameManager.Instance.SetGameState(GameState.Playing);
        }



    }
}
