using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyProject
{
    public class SwapSystem
    {
        private BoardManager board;

        public SwapSystem(BoardManager board)
        {
            this.board = board;
        }

        public void TrySwap(Gem gem, Vector2Int dir, float timeReturn)
        {       
            Vector2Int targetPos = gem.gridPos + dir;
            if (!IsValid(targetPos)) return;

            Gem targetGem = board.gems[targetPos.x, targetPos.y];
            if (targetGem == null) return;

            SwapGems(gem, targetGem);
            board.StartCoroutine(CheckSwapResult(gem, targetGem, timeReturn));
        }

        private bool IsValid(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < board.Width && pos.y >= 0 && pos.y < board.Height;
        }


        public void SwapGems(Gem gemA, Gem gemB)
        {
            Vector2Int posA = gemA.gridPos;
            Vector2Int posB = gemB.gridPos;

            gemA.gridPos = posB;
            gemB.gridPos = posA;

            board.gems[posA.x, posA.y] = gemB;
            board.gems[posB.x, posB.y] = gemA;

            gemA.MoveTo(posB);
            gemB.MoveTo(posA);
        }

        // Check swap có match hay là boom không
        private IEnumerator CheckSwapResult(Gem gemA, Gem gemB, float timeReturn)
        {
            yield return new WaitForSeconds(0.1f);

            if ((gemA.GetGemData.IsBoom || gemB.GetGemData.IsBoom))
            {
                board.LevelManager.UseMove();
                board.FindMatches();
                board.TriggerSwapBoom(gemA, gemB);
                GameManager.Instance.SetGameState(GameState.Playing);
                yield break;
            }


            board.FindMatches();

            bool hasMatch = gemA.isMatch || gemB.isMatch;
            if (!hasMatch)
            {
                yield return new WaitForSeconds(timeReturn);
                SwapGems(gemA, gemB);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                board.LevelManager.UseMove();
                board.ClearMatchedGems();
            }
            GameManager.Instance.SetGameState(GameState.Playing);
        }


        #region Detecting Deadlock
        private void SwapPiecesDeadlock(int column, int row, Vector2 Direction)
        {
            Vector2Int targetPos = new Vector2Int(column + (int)Direction.x, row + (int)Direction.y);
            Gem targetGem = board.gems[targetPos.x, targetPos.y];

            board.gems[targetPos.x, targetPos.y] = board.gems[column, row];

            board.gems[column, row] = targetGem;
        }

        private bool CheckForMatches()
        {
            int width = board.Width;
            int height = board.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x < width - 2)
                    {
                        if (board.gems[x, y] != null && board.gems[x + 1, y] != null && board.gems[x + 2, y] != null)
                        {
                            if (board.gems[x, y].TypeOfGem == board.gems[x + 1, y].TypeOfGem && board.gems[x, y].TypeOfGem == board.gems[x + 2, y].TypeOfGem)
                                return true;
                        }
                    }

                    if (y < height - 2)
                    {
                        if (board.gems[x, y] != null && board.gems[x, y + 1] != null && board.gems[x, y + 2] != null)
                        {
                            if (board.gems[x, y].TypeOfGem == board.gems[x, y + 1].TypeOfGem && board.gems[x, y].TypeOfGem == board.gems[x, y + 2].TypeOfGem)
                            {
                                return true;
                            }
                        }

                    }
                }
            }

            return false;
        }

        private bool SwitchAndCheck(int column, int row, Vector2 direction)
        {
            SwapPiecesDeadlock(column, row, direction);
            if (CheckForMatches())
            {
                SwapPiecesDeadlock(column, row, direction);

                return true;
            }
            SwapPiecesDeadlock(column, row, direction);
            return false;
        }


        public bool IsDeadlock()
        {
            int width = board.Width;
            int height = board.Height;

            //int totalGems = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    //if (board.gems[x, y] != null) totalGems++;

                    if (board.gems[x, y] == null)
                        return false;

                    if (board.gems[x, y] != null)
                    {
                        if (x < width - 1)
                        {
                            if (SwitchAndCheck(x, y, Vector2.right))
                            {
                                return false;
                            }
                        }

                        if (y < height - 1)
                        {
                            if (SwitchAndCheck(x, y, Vector2.up))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            //Debug.Log("Tổng số gem sau shuffle: " + totalGems);

            return true;
        }
        public List<Gem> ListMatches()
        {
            int width = board.Width;
            int height = board.Height;

            List<Gem> canMatches = new List<Gem>();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    if (board.gems[x, y] != null)
                    {
                        if (x < width - 1)
                        {
                            if (SwitchAndCheck(x, y, Vector2.right))
                            {
                                canMatches.Add(board.gems[x, y]);
                            }
                        }

                        if (y < height - 1)
                        {
                            if (SwitchAndCheck(x, y, Vector2.up))
                            {
                                canMatches.Add(board.gems[x, y]);
                            }
                        }
                    }
                }
            }

            return canMatches;
        }
        #endregion
    }
}
