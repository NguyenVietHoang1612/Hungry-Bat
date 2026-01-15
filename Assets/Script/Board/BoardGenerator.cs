using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CandyProject
{
    public class BoardGenerator
    {
        private BoardManager board;

        private List<Gem> tempGemList = new List<Gem>();
        public BoardGenerator(BoardManager board)
        {
            this.board = board;
        }

        public void GenerateBoard()
        {
            
            GenerateBlankSpaces();
            GenerateCrates();
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (board.crates[x, y] != null)
                    {
                        board.gems[x, y] = null;
                        continue;
                    }

                    if (board.blankSpaces[x, y])
                    {
                        board.gems[x, y] = null;
                        continue;
                    } 

                    //SpawnBoardTiles(x, y);
                    SpawnGem(new Vector2Int(x, y));
                    while (MatchesAt(x, y, board.gems[x, y]))
                    {
                        board.gems[x, y].ReturnPoolGem(board.GemPrefab);
                        SpawnGem(new Vector2Int(x, y));
                    }
                }
            }
        }

        public void GenerateBlankSpaces()
        {
            foreach (TileKind tileKind in board.tileKinds)
            {
                if (tileKind.tileType == TileType.BlankSpace)
                {
                    board.blankSpaces[tileKind.posX, tileKind.posY] = true;
                }
            }
        }

        public void GenerateCrates()
        {
            foreach (var tile in board.tileKinds)
            {
                if (tile.tileType == TileType.Crate)
                {
                    Vector2 pos = new Vector2(tile.posX, tile.posY) * board.CellSize;

                    GameObject gm = ObjectPoolManager.Instance.Get(board.CratePrefab);
                    gm.transform.position = pos;

                    Crate crate = gm.GetComponent<Crate>();
                    board.crates[tile.posX, tile.posY] = crate;
                }
            }
        }

        private void SpawnGem(Vector2Int gridPos)
        {
            Vector2 worldPos = (Vector2)gridPos * board.CellSize;
            GemData[] gems = board.GemDatas;

            int maxIndex = Mathf.Max(0, board.GemDatas.Length - board.NumSpecials());
            GemData randomGem = board.GemDatas[Random.Range(0, maxIndex)];

            GameObject obj = ObjectPoolManager.Instance.Get(board.GemPrefab);
            obj.transform.position = worldPos;
            Gem gem = obj.GetComponent<Gem>();
            gem.Init(randomGem);
            gem.gridPos = gridPos;

            board.gems[gridPos.x, gridPos.y] = gem;
        }


        private bool MatchesAt(int x, int y, Gem gem)
        {
            Gem[,] g = board.gems;

            if (x > 1 && g[x - 1, y] != null && g[x - 2, y] != null)
                if (g[x - 1, y].TypeOfGem == gem.TypeOfGem && g[x - 2, y].TypeOfGem == gem.TypeOfGem)
                    return true;

            if (y > 1 && g[x, y - 1] != null && g[x, y - 2] != null)
                if (g[x, y - 1].TypeOfGem == gem.TypeOfGem && g[x, y - 2].TypeOfGem == gem.TypeOfGem)
                    return true;

            return false;
        }

        public void ShuffleBoard()
        {
            tempGemList.Clear();
            int width = board.Width;
            int height = board.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board.gems[x, y] == null && board.blankSpaces[x, y]) continue;

                    if (board.crates[x, y] != null) continue;

                    tempGemList.Add(board.gems[x, y]);
                    
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!board.blankSpaces[x, y] && board.gems[x, y] != null)
                    {
                        int randomGem = Random.Range(0, tempGemList.Count);

                        while (MatchesAt(x, y, tempGemList[randomGem]))
                        {
                            randomGem = Random.Range(0, tempGemList.Count);
                        }

                        Gem gem = tempGemList[randomGem];
                        if (gem == null) continue;

                        board.SwapGem(gem, board.gems[x, y]);
                        tempGemList.Remove(gem);
                    }
                }
            }

            if (board.CheckDeadLock)
            {
                board.StartCoroutine(WaitShuffleBoard());
            }
        }

        private IEnumerator WaitShuffleBoard()
        {
            yield return GameManager.Instance.secondDelay;
            ShuffleBoard();
        }


        
    }
}
