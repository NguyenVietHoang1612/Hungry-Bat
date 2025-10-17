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
            SpawnBoardTiles();

            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    SpawnGem(new Vector2Int(x, y));

                    while (MatchesAt(x, y, board.gems[x, y]))
                    {
                        board.gems[x, y].ReturnPoolGem(board.GemPrefab);
                        SpawnGem(new Vector2Int(x, y));
                    }
                }
            }
        }

        private void SpawnBoardTiles()
        {
            for (int x = 0; x < board.Width; x++)
            {
                for (int y = 0; y < board.Height; y++)
                {
                    Vector2 worldPos = new Vector2(x, y) * board.CellSize;
                    GameObject tile = ObjectPoolManager.Instance.Get(board.BoardTile);
                    tile.transform.position = worldPos;
                    tile.transform.rotation = Quaternion.identity;
                }
            }
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
    }
}
