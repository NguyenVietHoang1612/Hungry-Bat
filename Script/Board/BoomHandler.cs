using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
            yield return GameManager.Instance.secondDelay;

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
                if(matchInfo.matchType == MatchType.FourHorizontal)
                {
                    boomGem.isVertical = false;
                }
                else if(matchInfo.matchType == MatchType.FourVertical)
                {
                    boomGem.isVertical = true;
                }
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
                board.ClearMatchedGems();
                return;
            }

            if (gemA.GetGemData.IsBoom)
            {
                if (gemA.TypeOfGem == GemType.ArrowHorizontal || gemA.TypeOfGem == GemType.ArrowVertical)
                {
                    ArrowBlast(gemA);
                }
                else
                {
                    switch (gemA.TypeOfGem)
                    {
                        case GemType.BoomColor:
                            GetAllSameType(gemB.TypeOfGem);
                            gemA.isMatch = true;
                            gemB.isMatch = true;
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
            }

            if (gemB.GetGemData.IsBoom)
            {
                if (gemB.TypeOfGem == GemType.ArrowHorizontal || gemB.TypeOfGem == GemType.ArrowVertical)
                {
                    ArrowBlast(gemA);
                }
                else
                {
                    switch (gemB.TypeOfGem)
                    {
                        case GemType.BoomColor:
                            gemA.isMatch = true;
                            gemB.isMatch = true;
                            GetAllSameType(gemB.TypeOfGem);
                            break;
                        case GemType.BoomWrapped:
                            GetArroundGem(gemB);
                            break;
                        default:
                            Debug.Log("Khong co loai boom");
                            break;
                    }
                    board.ClearMatchedGems();
                }
            }

        }

        public void TriggerBoom(Gem gemA)
        {
            visitedBooms.Clear();

            if (gemA.TypeOfGem == GemType.ArrowHorizontal || gemA.TypeOfGem == GemType.ArrowVertical)
            {
                ArrowBlast(gemA);
            }
            else
            {
                switch (gemA.TypeOfGem)
                {
                    case GemType.BoomColor:
                        gemA.isMatch = true;
                        GetAllSameType(gemA.TypeOfGem);
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
        }

        public void ArrowBlast(Gem gem)
        {
            ClearOneGem(gem.gridPos);

            Vector2Int dir = gem.isVertical ? Vector2Int.up : Vector2Int.right;

            board.StartCoroutine(AnimateClearance(gem.gridPos + dir, dir));
            board.StartCoroutine(AnimateClearance(gem.gridPos - dir, -dir));
        }


        private IEnumerator AnimateClearance(Vector2Int pos, Vector2Int dir)
        {
            while (board.IsInsideBoard(pos))
            {
                ClearOneGem(pos);

                pos += dir;
                yield return GameManager.Instance.delay;
            }

            board.CollapsingGem();
        }

        private void ClearOneGem(Vector2Int pos)
        {
            Gem gem = board.gems[pos.x, pos.y];
            if (gem != null)
            {
                gem.isMatch = true;
                gem.PlayDestroyEffect();

                board.LevelManager.AddScore(gem.GetGemData.scoreValue);
                board.LevelManager.ReduceGemGoal(gem);

                gem.isMoving = false;
                gem.ReturnPoolGem(board.GemPrefab);
                board.gems[pos.x, pos.y] = null;
            }

            if (board.crates[pos.x, pos.y] != null)
            {
                board.crates[pos.x, pos.y].Damage(1);

                if (board.crates[pos.x, pos.y].health < 0)
                {
                    board.crates[pos.x, pos.y].ReturnPoolGem(board.CratePrefab);
                    board.LevelManager.ReduceGemGoal(board.crates[pos.x, pos.y]);
                    board.crates[pos.x, pos.y] = null;
                }
            }

            if (gem != null &&
                        gem.isMatch &&
                        (gem.TypeOfGem == GemType.ArrowHorizontal ||
                         gem.TypeOfGem == GemType.ArrowVertical))
            {
                ArrowBlast(gem);
            }
            else if (gem != null && gem.isMatch && gem.TypeOfGem == GemType.BoomColor)
            {
                GemType typeOfGem = board.GetRandomGemType();
                GetAllSameType(typeOfGem);
            }
            else if (gem != null && gem.isMatch && gem.TypeOfGem == GemType.BoomWrapped)
            {
                GetArroundGem(gem);
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

                    if (board.crates[x, y] != null)
                    {
                        board.crates[x, y].Damage(1);

                        if (board.crates[x, y].health < 0)
                        {
                            board.crates[x, y].ReturnPoolGem(board.CratePrefab);
                            board.LevelManager.ReduceGemGoal(board.crates[x, y]);
                            board.crates[x, y] = null;
                        }
                    }

                    if (board.gems[x, y] != null &&
                        board.gems[x, y].isMatch &&
                        (board.gems[x, y].TypeOfGem == GemType.ArrowHorizontal ||
                         board.gems[x, y].TypeOfGem == GemType.ArrowVertical))
                    {
                        ArrowBlast(board.gems[x, y]);
                    }
                    else if (board.gems[x, y] != null && board.gems[x, y].isMatch && board.gems[x, y].TypeOfGem == GemType.BoomColor)
                    {
                        GemType typeOfGem = board.GetRandomGemType();
                        GetAllSameType(typeOfGem);
                    }
                    else if (board.gems[x, y] != null && board.gems[x, y].isMatch && board.gems[x, y].TypeOfGem == GemType.BoomWrapped)
                    {
                        GetArroundGem(board.gems[x, y]);
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

                    if (board.crates[x, y] != null)
                    {
                        board.crates[x, y].Damage(1);

                        if (board.crates[x, y].health < 0)
                        {
                            board.crates[x, y].ReturnPoolGem(board.CratePrefab);
                            board.LevelManager.ReduceGemGoal(board.crates[x, y]);
                            board.crates[x, y] = null;
                        }
                    }
                }
            }
        }
    }
}
