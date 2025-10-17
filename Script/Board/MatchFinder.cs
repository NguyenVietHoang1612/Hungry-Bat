using System.Collections.Generic;
using UnityEngine;

namespace CandyProject
{
    public class MatchFinder
    {
        private BoardManager board;
        private BoomHandler boomHandler;


        private List<MatchInfo> matchInfos;
        private bool[,] visited;

        public MatchFinder(BoardManager board, BoomHandler boomHandler)
        {
            this.board = board;
            this.boomHandler = boomHandler;
        }

        // Tìm Match, tạo boom 
        public void FindMatches()
        {
            Gem[,] gems = board.gems;
            int width = board.Width;
            int height = board.Height;

            visited = new bool[width, height];

            foreach (var gem in gems)
                if (gem != null) gem.isMatch = false;

            matchInfos = new List<MatchInfo>();
            matchInfos.Clear();
            List<Vector2Int> matches = new List<Vector2Int>();

            ScanHorizontal(matches);
            ScanVertical(matches);

            if (matches.Count > 0)
            {
                foreach (var pos in matches)
                {
                                    
                    gems[pos.x, pos.y].isMatch = true;
                }
            }    
                
            if (matchInfos.Count > 0)
            {
                Debug.Log("Creating booms...");
                board.StartCoroutine(boomHandler.CreateBooms(matchInfos));

                foreach (var info in matchInfos)
                {
                    if (gems[info.center.x, info.center.y] != null && gems[info.center.x, info.center.y].GetGemData.IsBoom && gems[info.center.x, info.center.y].isMatch)
                    {
                        Debug.Log("Hello");
                        boomHandler.TriggerBoom(gems[info.center.x, info.center.y]);
                    }
                }
            }

            board.ClearMatchedGems();
        }

        
        private void ScanHorizontal(List<Vector2Int> matches)
        {
            visited = new bool[board.Width, board.Height];
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Gem gem = board.gems[x, y];
                    if (gem == null) continue;

                    int count = 1;
                    for (int col = x + 1; col < board.Width && board.gems[col, y] != null && board.gems[col, y].TypeOfGem == gem.TypeOfGem; col++)
                        count += CheckVisitedGem(col, y, gem.TypeOfGem);

                    if (count >= 3)
                    {
                        for (int i = 0; i < count; i++)
                            matches.Add(new Vector2Int(x + i, y));

                        if (count == 4)
                        {
                            Vector2Int center = new Vector2Int(x + 1, y);
                            matchInfos.Add(new MatchInfo(center, MatchType.FourVertical));
                        }
                        else if (count >= 5)
                        {
                            Vector2Int center = new Vector2Int(x + 2, y);
                            matchInfos.Add(new MatchInfo(center, MatchType.Five));
                        }
                    }


                }
            }
        }

        private void ScanVertical(List<Vector2Int> matches)
        {
            visited = new bool[board.Width, board.Height];
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Gem gem = board.gems[x, y];
                    if (gem == null) continue;

                    int count = 1;
                    for (int row = y + 1; row < board.Height && board.gems[x, row] != null && board.gems[x, row].TypeOfGem == gem.TypeOfGem; row++)
                        count += CheckVisitedGem(x, row, gem.TypeOfGem);

                    if (count >= 3)
                    {
                        for (int j = 0; j < count; j++)
                            matches.Add(new Vector2Int(x, y + j));

                        if (count == 4)
                        {
                            Vector2Int center = new Vector2Int(x, y + 1);
                            matchInfos.Add(new MatchInfo(center, MatchType.FourHorizontal));
                        }
                        else if (count >= 5)
                        {
                            Vector2Int center = new Vector2Int(x, y + 2);
                            matchInfos.Add(new MatchInfo(center, MatchType.Five));
                        }
                    }


                }
            }
        }


        private int CheckVisitedGem(int x, int y, GemType gemType)
        {
            if (visited[x, y]) return 0;

            if (x < 0 || y < 0 || x >= board.Width || y >= board.Height) return 0;

            if (board.gems[x, y] == null || board.gems[x, y].TypeOfGem != gemType)
                return 0;

            visited[x, y] = true;

            return 1;
        }
    }
}
