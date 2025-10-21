using System.Collections;
using System.Diagnostics;
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
            board.StopAllCoroutines();
            board.StartCoroutine(CheckSwapResult(gem, targetGem, timeReturn));
        }

        private bool IsValid(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < board.Width && pos.y >= 0 && pos.y < board.Height;
        }

       
        private void SwapGems(Gem gemA, Gem gemB)
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
                board.FindMatches();
                board.TriggerSwapBoom(gemA, gemB);
                yield break;
            }
           

            board.FindMatches();

            bool hasMatch = gemA.isMatch || gemB.isMatch;
            if (!hasMatch)
            {
                yield return new WaitForSeconds(timeReturn);
                UnityEngine.Debug.Log("Swap lại");
                SwapGems(gemA, gemB);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                UnityEngine.Debug.Log("Không");
                board.ClearMatchedGems();
            }
        }
    }
}
