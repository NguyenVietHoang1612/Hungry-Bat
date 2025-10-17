using System.Collections;
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

        private void SwapGems(Gem a, Gem b)
        {
            Vector2Int posA = a.gridPos;
            Vector2Int posB = b.gridPos;

            board.gems[posA.x, posA.y] = b;
            board.gems[posB.x, posB.y] = a;

            a.gridPos = posB;
            b.gridPos = posA;

            a.MoveTo(posB);
            b.MoveTo(posA);
        }

        private IEnumerator CheckSwapResult(Gem a, Gem b, float timeReturn)
        {
            yield return new WaitForSeconds(0.1f);

            // Nếu là boom, xử lý nổ ngay
            if (a.GetGemData.IsBoom && b.GetGemData.IsBoom && a.TypeOfGem != b.TypeOfGem)
            {
                board.TriggerBoom(a);
                board.TriggerBoom(b);
                yield break;
            }
            else if (a.GetGemData.IsBoom)
            {
                board.TriggerBoom(a);
                yield break;
            }
            else if (b.GetGemData.IsBoom)
            {
                board.TriggerBoom(b);
                yield break;
            }

            board.FindMatches();

            bool hasMatch = a.isMatch || b.isMatch;
            if (!hasMatch)
            {
                yield return new WaitForSeconds(timeReturn);
                SwapGems(a, b);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                board.ClearMatchedGems();
            }
        }
    }
}
