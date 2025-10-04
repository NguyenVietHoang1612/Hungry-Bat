using System.Collections.Generic;
using UnityEngine;


namespace CandyProject
{
    public class BoardManager : Singleton<BoardManager>
    {
        [SerializeField] private int width;

        [SerializeField] private int height;

        [SerializeField] private float cellSize = 0.6f;

        [SerializeField] private GemData[] gemDatas;
        public GameObject gemPrefab;
        [SerializeField] private Gem[,] gems;

        private bool[,] visited;


        private int[,] directions = new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

        private void Start()
        {
            gems = new Gem[width, height];
            GenerateBoard();
            FindMatches();
        }

        private void GenerateBoard()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Spawn object
                    Vector2 worldPos = new Vector2(x, y) * cellSize;
                    SpawnGem(worldPos);
                }
            }
        }

        public void Update()
        {
            FindMatches();
        }


        private void SpawnGem(Vector2 gridPos)
        {
            GemData randomGem = gemDatas[Random.Range(0, gemDatas.Length)];

            Transform gemTranform = Instantiate(gemPrefab, gridPos, Quaternion.identity).transform;

            Gem gem = gemTranform.GetComponent<Gem>();

            gem.Init(randomGem);

            gemTranform.SetParent(this.transform);
            Vector2 posGemWorld = gridPos / cellSize;
            gems[Mathf.RoundToInt(posGemWorld.x), Mathf.RoundToInt(posGemWorld.y)] = gem;
            gem.gridPos = new Vector2(posGemWorld.x, posGemWorld.y);
        }

        public void SwapGem(Vector2 posA, Vector2 posB)
        {
            int xPosA = (int)(posA.x);
            int yPosA = (int)(posA.y);

            Gem gemA = gems[xPosA, yPosA];

            int xPosB = (int)(posB.x);
            int yPosB = (int)(posB.y);

            Gem gemB = gems[xPosB, yPosB];

            gems[xPosA, yPosA] = gemB;
            gems[xPosB, yPosB] = gemA;

            gemA.MoveTo(posB);
            gemB.MoveTo(posA);
        }

        public void FindMatches()
        {
            if (gems.Length <= 0) return;

            visited = new bool[width, height];

            for (int row = 0; row < width; row++)
            {
                for (int col = 0; col < height; col++)
                {
                    if (!visited[row,col])
                    {
                        List<Vector2Int> connected = new List<Vector2Int>();
                        DFS(row, col, gems[row, col].GemName, connected);
                        if (connected.Count >= 3)
                    {
                        Debug.Log($"Match found! Size: {connected.Count}");

                        // Đánh dấu để phá
                        foreach (var pos in connected)
                        {
                            // Ví dụ set = -1 nghĩa là sẽ bị phá
                            gems[pos.x, pos.y].isMatch = true;
                        }
                    }
                    }
                }
            }


        }

        public void DFS(int row, int col, string gemName, List<Vector2Int> connected)
        {
            if (row >= width || col >= height || row < 0 || col < 0) return;
            if (visited[row, col]) return;
            if (gems[row, col].GemName != gemName) return;

            visited[row, col] = true;

            connected.Add(new Vector2Int(row, col));

            for (int i = 0; i < 4; i++)
            {
                int nRow = row + directions[i, 0];
                int nCol = col + directions[i, 1];
                DFS(nRow, nCol, gemName, connected);
            }


        }

        public int GetWidth() => width;

        public int GetHeight() => height;   

        public float GetCellSize() => cellSize;
    }
}

