using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace CandyProject
{
    public class RefillSystem
    {
        private BoardManager board;

        public RefillSystem(BoardManager board)
        {
            this.board = board;
        }

        // Clear gem khi find Match
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

                        board.LevelManager.ReduceGemGoal(gem);
                        board.LevelManager.AddScore(gem.GetGemData.scoreValue);

                        DamageAdjacentCrates(x, y);
                        gem.ReturnPoolGem(board.GemPrefab);
                        board.gems[x, y] = null;
                    }

                }
            }
            Collapsing();
        }

        // Rơi gem lấp ô trống
        public void Collapsing()
        {
            board.StartCoroutine(CollapsingGem());
        }

        private IEnumerator CollapsingGem()
        {
            yield return GameManager.Instance.delay;

            int width = board.Width;
            int height = board.Height;

            CollapseVertical(width, height);
            CollapseDiagonalAndHorizontal(width, height);

            yield return GameManager.Instance.delay;
            board.StartCoroutine(RefillingGem());
        }

        // rơi thẳng
        private void CollapseVertical(int width, int height)
        {
            for (int x = 0; x < width; x++)
            {
                int emptyY = -1;
                for (int y = 0; y < height; y++)
                {
                    if (board.blankSpaces[x, y] || board.crates[x, y])
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
                        board.gems[x, y].SetTarget(new Vector2Int(x, emptyY));
                        board.gems[x, y].MoveTo(new Vector2Int(x, emptyY));
                        board.gems[x, emptyY] = board.gems[x, y];
                        board.gems[x, y] = null;
                        emptyY++;
                    }
                }
            }
        }

        // Rơi chéo
        private void CollapseDiagonalAndHorizontal(int width, int height)
        {


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gem currentGem = board.gems[x, y];
                    if (currentGem == null) continue;

                    int startX = x;
                    int startY = y;

                    int maxIterations = 9; 
                    int iterations = 0;

                    // Rơi chéo sang phải 
                    while (iterations < maxIterations)
                    {
                        int targetX = startX + 1;
                        int targetY = startY - 1;

                        // điều kiện ô mục tiêu và ô crate nằm trong board
                        if (!IsInside(targetX, targetY) || !IsInside(startX + 1, startY)) break;

                        bool neighbourHasBlock = board.crates[startX + 1, startY] != null;
                        bool targetBelowFree = board.crates[targetX, targetY] == null && board.gems[targetX, targetY] == null;

                        // nếu bên dưới crate là null => rơi chéo xuống ô dưới crate
                        if (neighbourHasBlock && targetBelowFree)
                        {
                            board.gems[startX, startY].SetTarget(new Vector2Int(targetX, targetY));
                            board.gems[startX, startY].MoveTo(new Vector2Int(targetX, targetY));
                            board.gems[targetX, targetY] = board.gems[startX, startY];
                            board.gems[startX, startY] = null;

                            // cập nhật vị trí gem hiện tại để có thể tiếp tục rơi thêm
                            startX = targetX;
                            startY = targetY;
                            currentGem = board.gems[startX, startY];

                            iterations++;
                            continue;
                        }

                        break;
                    }

                    iterations = 0;
                    startX = x;
                    startY = y;
                    // Rơi chéo sang trái
                    while (iterations < maxIterations)
                    {
                        int nx = startX - 1;
                        int ny = startY - 1;

                        if (!IsInside(nx, ny) || !IsInside(startX - 1, startY)) break;

                        bool neighbourHasBlock = board.crates[startX - 1, startY] != null;
                        bool targetBelowFree = board.crates[nx, ny] == null && board.gems[nx, ny] == null;

                        if (neighbourHasBlock && targetBelowFree)
                        {
                            board.gems[startX, startY].SetTarget(new Vector2Int(nx, ny));
                            board.gems[startX, startY].MoveTo(new Vector2Int(nx, ny));
                            board.gems[nx, ny] = board.gems[startX, startY];
                            board.gems[startX, startY] = null;

                            startX = nx;
                            startY = ny;
                            currentGem = board.gems[startX, startY];
                            iterations++;
                            continue;
                        }

                        break;
                    }
                }
            }

            CollapseVertical(width, height);
        }

        private bool IsInside(int xx, int yy) => xx >= 0 && xx < board.Width && yy >= 0 && yy < board.Height;


        // Sinh gem làm đầy board
        private IEnumerator RefillingGem()
        {
            yield return GameManager.Instance.delay;

            int width = board.Width;
            int height = board.Height;

            for (int x = 0; x < width; x++)
            {
                bool anySpawned = false;
                for (int y = 0; y < height; y++)
                {
                    if (board.blankSpaces[x, y]) continue;

                    if (board.crates[x, y]) continue;

                    if (board.gems[x, y] == null)
                    {
                        anySpawned = true;

                        float spawnY = height + (Random.Range(0.1f, 0.3f) * board.CellSize);
                        Vector2 worldPos = new Vector2(x, spawnY) * board.CellSize;
                        Vector2Int gridPos = new Vector2Int(x, y);

                        int maxIndex = Mathf.Max(0, board.GemDatas.Length - board.NumSpecials());
                        GemData randomGem = board.GemDatas[Random.Range(0, maxIndex)];

                        Gem newGem = board.CreateGem(worldPos, randomGem);
                        newGem.gridPos = gridPos;
                        newGem.SetTarget(gridPos);
                        newGem.MoveTo(gridPos);
                        board.gems[x, y] = newGem;
                    }
                }
                if (anySpawned)
                {
                    yield return GameManager.Instance.delay;
                }
            }

            yield return GameManager.Instance.secondDelay;

            board.FindMatches();

            yield return GameManager.Instance.secondDelay;

            if (board.CheckDeadLock)
            {
                Debug.Log("Deadlock detected! Shuffling the board...");
                board.ShuffleBoard();
            }

            yield return GameManager.Instance.twoSecondDelay;

            // Hoàn thành Level
            if (board.LevelManager.IsLevelComplete())
            {
                board.LevelManager.HandleWinLevel();
                yield break;
            }
            else if (!board.LevelManager.IsLevelComplete() && board.LevelManager.RemainingMoves == 0)
            {
                board.LevelManager.HandleLose();
                yield break;
            }

            GameManager.Instance.SetGameState(GameState.Playing);
        }

        private void DamageAdjacentCrates(int x, int y)
        {
            Vector2Int[] dirs = new Vector2Int[]
            {
                new Vector2Int(1, 0),   
                new Vector2Int(-1, 0),  
                new Vector2Int(0, 1), 
                new Vector2Int(0, -1)   
            };

            foreach (var d in dirs)
            {
                int nx = x + d.x;
                int ny = y + d.y;

                if (nx < 0 || nx >= board.Width || ny < 0 || ny >= board.Height)
                    continue;

                if (board.crates[nx, ny] != null)
                {
                    board.crates[nx, ny].Damage(1);

                    if (board.crates[nx, ny].health < 0)
                    {
                        board.crates[nx, ny].ReturnPoolGem(board.CratePrefab);
                        board.LevelManager.ReduceGemGoal(board.crates[nx, ny]);
                        board.crates[nx, ny] = null;
                    }
                }
            }
        }
    }

}
