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
                        //Debug.Log("Creating booms ArrowHorizontal");
                        boomData = board.GemDatas.FirstOrDefault(g => g.gemType == GemType.ArrowHorizontal);
                        break;

                    case MatchType.FourVertical:
                        //Debug.Log("Creating booms ArrowVertical");
                        boomData = board.GemDatas.FirstOrDefault(g => g.gemType == GemType.ArrowVertical);
                        break;

                    case MatchType.Five:
                        //Debug.Log("Creating booms 5");
                        boomData = board.GemDatas.FirstOrDefault(g => g.gemType == GemType.BoomColor);
                        break;

                    default:
                        Debug.Log(".......");
                        continue;
                }

                if (boomData == null)
                {
                    //Debug.LogWarning($"Không tìm thấy GemData cho loại boom: {matchInfo.matchType}");
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

        public void TriggerBoom(Gem boomGem)
        {
            Vector2Int pos = boomGem.gridPos;

            switch (boomGem.TypeOfGem)
            {
                case GemType.ArrowHorizontal:
                    GetRow(pos.y);
                    break;
                case GemType.ArrowVertical:
                    GetColumn(pos.x);
                    break;
                case GemType.BoomColor:
                    GetAllSameType(boomGem.TypeOfGem);
                    break;
                default:
                    Debug.Log("Co gi dau");
                    break;
            }

            board.ClearMatchedGems();
        }


        private void GetRow(int row)
        {
            for (int x = 0; x < board.Width; x++)
            {
                if (board.gems[x, row] != null)
                {
                    board.gems[x, row].isMatch = true;
                }

                if (board.gems[x, row] != null && board.gems[x, row].isMatch && board.gems[x, row].TypeOfGem == GemType.ArrowVertical)
                {
                    GetColumn(x);
                }
                    
            }
        }

        private void GetColumn(int column)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (board.gems[column, y] != null)
                {
                    board.gems[column, y].isMatch = true;
                }

                if (board.gems[column, y] != null && board.gems[column, y].isMatch && board.gems[column, y].TypeOfGem == GemType.ArrowHorizontal)
                {
                    GetRow(y);
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
    }
}
