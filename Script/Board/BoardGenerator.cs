using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CandyProject
{
    public class BoardGenerator
    {
        private BoardManager board;
        public BoardGenerator(BoardManager board)
        {
            this.board = board;
        }

        public void GenerateBoard()
        {
            
            GenerateBlankSpaces();
            GenerateBreakable();
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    if (board.obstacle[x, y])
                    {
                        board.gems[x, y] = null;
                        continue;
                    } 

                    SpawnBoardTiles(x, y);
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
                    board.obstacle[tileKind.posX, tileKind.posY] = true;
                }
            }
        }

        public void GenerateBreakable()
        {
            foreach (TileKind tileKind in board.tileKinds)
            {
                if (tileKind.tileType == TileType.Breakable)
                {
                    Vector2 worldPos = new Vector2(tileKind.posX, tileKind.posY) * board.CellSize;
                    GameObject bonusGem = ObjectPoolManager.Instance.Get(board.BonusGemPrefab);
                    BonusGem bnGem = bonusGem.GetComponent<BonusGem>();
                    bonusGem.transform.position = worldPos;
                    bonusGem.transform.rotation = Quaternion.identity;
                    board.bonusGem[tileKind.posX, tileKind.posY] = bnGem;
                }
            }
        }

        private void SpawnBoardTiles(int x, int y)
        {
            Vector2 worldPos = new Vector2(x, y) * board.CellSize;
            GameObject tile = ObjectPoolManager.Instance.Get(board.BoardTile);
            tile.transform.SetParent(board.boardTileTransform);
            tile.transform.position = worldPos;
            tile.transform.rotation = Quaternion.identity;
        }

        private void SpawnGem(Vector2Int gridPos)
        {
            Vector2 worldPos = (Vector2)gridPos * board.CellSize;
            GemData[] gems = board.GemDatas;
            GemData randomGem = gems[Random.Range(0, gems.Length - board.NumSpecials())];

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
            List<Gem> newBoard = new List<Gem>();
            int width = board.Width;
            int height = board.Height;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (board.gems[x,y] != null && !board.obstacle[x,y])
                    {
                        newBoard.Add(board.gems[x, y]);
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (!board.obstacle[x, y] && board.gems[x, y] != null)
                    {
                        int randomGem = Random.Range(0, newBoard.Count);


                        while (MatchesAt(x, y, newBoard[randomGem]))
                        {
                            
                            randomGem = Random.Range(0, newBoard.Count);
                        }

                        Gem gem = newBoard[randomGem];
                        board.SwapGem(gem, board.gems[x, y]);
                        newBoard.Remove(gem);
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
            yield return new WaitForSeconds(1f);
            ShuffleBoard();
        }


        
    }
}
