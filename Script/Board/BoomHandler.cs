using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

namespace CandyProject
{
    public class BoomHandler
    {
        private BoardManager board;
        private HashSet<Vector2Int> visitedBooms = new HashSet<Vector2Int>();

        public BoomHandler(BoardManager board)
        {
            this.board = board;
        }

        public IEnumerator CreateBooms(List<MatchInfo> matchInfos)
        {
            yield return new WaitForSeconds(0.1f);

            foreach (MatchInfo matchInfo in matchInfos)
            {
                Vector2Int centerPos = matchInfo.center;

                if (centerPos.x < 0 || centerPos.x >= board.Width || centerPos.y < 0 || centerPos.y >= board.Height)
                    continue;

                Gem baseGem = board.gems[centerPos.x, centerPos.y];
                Vector2 worldPos = (Vector2)centerPos * board.CellSize;

                GemData boomData = null;

                switch (matchInfo.matchType)
                {
                    case MatchType.FourHorizontal:
                        boomData = board.GemDatas.FirstOrDefault(g => g.gemType == GemType.ArrowHorizontal);
                        break;

                    case MatchType.FourVertical:
                        boomData = board.GemDatas.FirstOrDefault(g => g.gemType == GemType.ArrowVertical);
                        break;

                    case MatchType.Five:
                        boomData = board.GemDatas.FirstOrDefault(g => g.gemType == GemType.BoomColor);
                        break;
                    case MatchType.boomWrapped:
                        boomData = board.GemDatas.FirstOrDefault(g => g.gemType == GemType.BoomWrapped);
                        break;
                    default:
                        Debug.Log(".......");
                        continue;
                }

                if (boomData == null)
                {
                    continue;
                }

                if (baseGem != null)
                {
                    baseGem.ReturnPoolGem(board.GemPrefab);
                    board.gems[centerPos.x, centerPos.y] = null;
                }

                Gem boomGem = board.CreateGem(worldPos, boomData);
                boomGem.isMatch = false;
                boomGem.gridPos = centerPos;
                board.gems[centerPos.x, centerPos.y] = boomGem;
            }
        }

        // Trigger boom khi swap gem
        public void TriggerSwapBoom(Gem gemA, Gem gemB)
        {
            visitedBooms.Clear();

            if (gemA.TypeOfGem == GemType.BoomColor && gemB.TypeOfGem == GemType.BoomColor)
            {
                Debug.Log("All gem Match");
                GetAllGem();
                return;
            }

            if (gemA.GetGemData.IsBoom)
            {
                switch (gemA.TypeOfGem)
                {
                    case GemType.ArrowHorizontal:
                        GetRow(gemA.gridPos.y);
                        break;
                    case GemType.ArrowVertical:
                        GetColumn(gemA.gridPos.x);
                        break;
                    case GemType.BoomColor:
                        gemA.isMatch = true;
                        GetAllSameType(gemB.TypeOfGem);
                        break;
                    case GemType.BoomWrapped:
                        GetArroundGem(gemA);
                        break;
                    default:
                        Debug.Log("Khong co loai boom");
                        break;
                }
            }

            if (gemB.GetGemData.IsBoom)
            {
                switch (gemB.TypeOfGem)
                {
                    case GemType.ArrowHorizontal:
                        GetRow(gemB.gridPos.y);
                        break;
                    case GemType.ArrowVertical:
                        GetColumn(gemB.gridPos.x);
                        break;
                    case GemType.BoomColor:
                        gemB.isMatch = true;
                        GetAllSameType(gemA.TypeOfGem);
                        break;
                    case GemType.BoomWrapped:
                        GetArroundGem(gemB);
                        break;
                    default:
                        Debug.Log("Khong co loai boom");
                        break;
                }
            }
            board.ClearMatchedGems();
        }

        public void TriggerBoom(Gem gemA)
        {
            visitedBooms.Clear();

            switch (gemA.TypeOfGem)
            {
                case GemType.ArrowHorizontal:
                    GetRow(gemA.gridPos.y);
                    break;
                case GemType.ArrowVertical:
                    GetColumn(gemA.gridPos.x);
                    break;
                case GemType.BoomWrapped:
                    GetArroundGem(gemA);
                    break;
                default:
                    Debug.Log("Khong co loai boom");
                    break;
            }

            board.ClearMatchedGems();
        }


        private void GetRow(int row)
        {

            for (int x = 0; x < board.Width; x++)
            {
                if (board.gems[x, row] == null) continue;


                Vector2Int pos = board.gems[x, row].gridPos;
                if (visitedBooms.Contains(pos)) continue;

                visitedBooms.Add(pos);

                if (board.gems[x, row] != null)
                {
                    board.gems[x, row].isMatch = true;
                }

                if (board.gems[x, row] != null && board.gems[x, row].isMatch && board.gems[x, row].TypeOfGem == GemType.ArrowVertical)
                {
                    GetColumn(x);
                }
                else if (board.gems[x, row] != null && board.gems[x, row].isMatch && board.gems[x, row].TypeOfGem == GemType.BoomWrapped)
                {
                    GetArroundGem(board.gems[x, row]);
                }
                else if (board.gems[x, row] != null && board.gems[x, row].isMatch && board.gems[x, row].TypeOfGem == GemType.BoomColor)
                {
                    GemType typeOfGem = board.GetRandomGemType();
                    GetAllSameType(typeOfGem);
                }

            }
        }

        private void GetColumn(int column)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (board.gems[column, y] == null) continue;

                Vector2Int pos = new Vector2Int(column, y);
                if (visitedBooms.Contains(pos)) continue;

                visitedBooms.Add(pos);

                if (board.gems[column, y] != null)
                {
                    board.gems[column, y].isMatch = true;
                }

                if (board.gems[column, y] != null && board.gems[column, y].isMatch && board.gems[column, y].TypeOfGem == GemType.ArrowHorizontal)
                {
                    GetRow(y);
                }
                else if (board.gems[column, y] != null && board.gems[column, y].isMatch && board.gems[column, y].TypeOfGem == GemType.BoomWrapped)
                {
                    GetArroundGem(board.gems[column, y]);
                }
                else if (board.gems[column, y] != null && board.gems[column, y].isMatch && board.gems[column, y].TypeOfGem == GemType.BoomColor)
                {
                    GemType typeOfGem = board.GetRandomGemType();
                    GetAllSameType(typeOfGem);
                }
            }
        }

        private void GetAllSameType(GemType type)
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (board.gems[x, y] != null && board.gems[x, y].TypeOfGem == type)
                    {
                        board.gems[x, y].isMatch = true;
                    }

                }
            }
        }

        private void GetArroundGem(Gem gem)
        {
            Vector2Int pos = gem.gridPos;
            for (int x = pos.x - 1; x <= pos.x + 1; x++)
            {
                for (int y = pos.y - 1; y <= pos.y + 1; y++)
                {
                    
                    if (x < 0 || y < 0 || x >= board.Width || y >= board.Height)
                    {
                        continue;
                    }

                    Vector2Int posVisited = new Vector2Int(x, y);
                    if (visitedBooms.Contains(posVisited)) continue;

                    visitedBooms.Add(posVisited);

                    if (board.gems[x, y] != null)
                    {
                        board.gems[x, y].isMatch = true;
                    }

                    if (board.gems[x, y] != null && board.gems[x, y].isMatch && board.gems[x, y].TypeOfGem == GemType.ArrowHorizontal)
                    {
                        GetRow(y);
                    }
                    else if (board.gems[x, y] != null && board.gems[x, y].isMatch && board.gems[x, y].TypeOfGem == GemType.ArrowVertical)
                    {
                        GetColumn(x);
                    }
                    else if (board.gems[x, y] != null && board.gems[x, y].isMatch && board.gems[x, y].TypeOfGem == GemType.BoomColor)
                    {
                        GemType typeOfGem = board.GetRandomGemType();
                        GetAllSameType(typeOfGem);
                    }
                }
            }
        }

        private void GetAllGem()
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (board.gems[x, y] != null)
                    {
                        board.gems[x, y].isMatch = true;
                    }

                }
            }
        }
    }
}
