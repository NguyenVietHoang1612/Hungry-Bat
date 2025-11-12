using System.Collections.Generic;
using UnityEngine;

namespace CandyProject
{
    public class MatchFinder
    {
        private readonly BoardManager board;
        private readonly BoomHandler boomHandler;

        private List<MatchInfo> matchInfos;

        private bool[,] visited;

        public MatchFinder(BoardManager boardManager, BoomHandler boomHandler)
        {
            board = boardManager;
            this.boomHandler = boomHandler;
        }

        public void FindMatchGems()
        {
            GameManager.Instance.SetGameState(GameState.Waiting);
            Gem[,] allGems = board.gems;
            int width = board.Width;
            int height = board.Height;

            visited = new bool[width, height];
            ResetGemMatchState(allGems);

            matchInfos = new List<MatchInfo>();
            board.matches = new List<Vector2Int>();

            int horizontalCount = ScanByDirection(Vector2Int.right);
            int verticalCount = ScanByDirection(Vector2Int.up);

            HandleMatchResults(horizontalCount, verticalCount);

            if (board.matches.Count >= 3)
            {
                board.ClearMatchedGems();
            }         
        }



        //Quét tìm các viên giống nhau theo hướng
        private int ScanByDirection(Vector2Int direction)
        {
            int totalMatches = 0;
            int width = board.Width;
            int height = board.Height;

            visited = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gem startGem = board.gems[x, y];
                    if (startGem == null) continue;

                    List<Vector2Int> sameGemChain = GetSameGemChain(new Vector2Int(x, y), direction, startGem.TypeOfGem);

                    if (sameGemChain.Count >= 3)
                    {
                        board.matches.AddRange(sameGemChain);
                        totalMatches += sameGemChain.Count;
                        CreateBoomInfo(sameGemChain, direction);
                    }
                }
            }

            return totalMatches;
        }

        // Lấy danh sách các viên cùng loại nối liền nhau
        private List<Vector2Int> GetSameGemChain(Vector2Int startPos, Vector2Int direction, GemType gemType)
        {
            List<Vector2Int> chain = new List<Vector2Int>();

            for (Vector2Int pos = startPos; IsValidPosition(pos, gemType); pos += direction)
            {
                visited[pos.x, pos.y] = true;
                chain.Add(pos);
            }

            return chain;
        }

        // Kiểm tra biên board và đã đi qua chưa
        private bool IsValidPosition(Vector2Int pos, GemType gemType)
        {
            if (pos.x < 0 || pos.y < 0 || pos.x >= board.Width || pos.y >= board.Height)
                return false;

            if (visited[pos.x, pos.y]) return false;

            Gem gem = board.gems[pos.x, pos.y];
            return gem != null && gem.TypeOfGem == gemType;
        }


        private void CreateBoomInfo(List<Vector2Int> chain, Vector2Int direction)
        {
            int count = chain.Count;

            if (count >= 5)
            {
                Vector2Int centerPos = chain[count / 2];
                matchInfos.Add(new MatchInfo(centerPos, MatchType.Five));
            }
            else if (count == 4)
            {
                Vector2Int centerPos = chain[count/ 2];
                MatchType type = (direction == Vector2Int.right)
                    ? MatchType.FourVertical
                    : MatchType.FourHorizontal;

                matchInfos.Add(new MatchInfo(centerPos, type));
            }
        }

        private void HandleMatchResults(int horizontalCount, int verticalCount)
        {
            if (board.matches.Count == 0)
                return;

            foreach (Vector2Int pos in board.matches)
                board.gems[pos.x, pos.y].isMatch = true;

            bool isSameTypemMatched = false;
            for (int i = 0; i < board.matches.Count - 1; i++)
            {
                Vector2Int currentPosMatch = board.matches[i];
                Vector2Int nextPosMatch = board.matches[i + 1];

                
                if (board.gems[currentPosMatch.x, currentPosMatch.y].TypeOfGem == board.gems[nextPosMatch.x, nextPosMatch.y].TypeOfGem)
                {
                    isSameTypemMatched = true;
                    continue; 
                }
                isSameTypemMatched = false;
                break;
            }

            if (board.matches.Count >= 5 && (horizontalCount >= 3 && horizontalCount < 5 ) && (verticalCount >= 3 && verticalCount < 5) && isSameTypemMatched)
            {
                matchInfos.Clear();
                Vector2Int center = board.matches[board.matches.Count / 2];
                matchInfos.Add(new MatchInfo(center, MatchType.boomWrapped));
            }

            if (matchInfos.Count > 0)
            { 
                board.StartCoroutine(boomHandler.CreateBooms(matchInfos));
            }
        }


        private void ResetGemMatchState(Gem[,] allGems)
        {
            foreach (var gem in allGems)
                if (gem != null)
                    gem.isMatch = false;
        }
    }
}
