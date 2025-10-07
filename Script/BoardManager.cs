using System.Collections;
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
        [SerializeField] private GameObject gemPrefab;

        private Gem[,] gems;

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
                    SpawnGem(new Vector2Int(x, y));
                }
            }
        }

        private void SpawnGem(Vector2Int gridPos)
        {
            Vector2 worldPos = (Vector2)gridPos * cellSize;
            GemData randomGem = gemDatas[Random.Range(0, gemDatas.Length)];

            Transform gemTransform = Instantiate(gemPrefab, worldPos, Quaternion.identity).transform;
            gemTransform.SetParent(this.transform);

            Gem gem = gemTransform.GetComponent<Gem>();
            gem.Init(randomGem);
            gem.gridPos = gridPos;

            gems[gridPos.x, gridPos.y] = gem;
        }


        #region Swap
        public void TrySwap(Gem gem, Vector2Int dir, float timeReturn)
        {
            Vector2Int targetPos = gem.gridPos + dir;

            if (targetPos.x < 0 || targetPos.x >= width || targetPos.y < 0 || targetPos.y >= height)
                return;

            Gem targetGem = gems[targetPos.x, targetPos.y];

            if (targetGem == null) return;

            SwapGem(gem.gridPos, targetPos);
            StartCoroutine(CheckSwapResult(gem, targetGem, timeReturn));
        }

        

        public void SwapGem(Vector2Int posA, Vector2Int posB)
        {
            // Swap aray gem

            Gem gemA = gems[posA.x, posA.y];
            Gem gemB = gems[posB.x, posB.y];

            gems[posA.x, posA.y] = gemB;
            gems[posB.x, posB.y] = gemA;

            Vector2Int temp = gemA.gridPos;
            gemA.gridPos = gemB.gridPos;
            gemB.gridPos = temp;

            gemA.MoveTo(posB);
            gemB.MoveTo(posA);
        }

        private IEnumerator CheckSwapResult(Gem gemA, Gem gemB, float timeReturn)
        {
            yield return new WaitForSeconds(0.1f);

            FindMatches();

            bool hasMatch = gems[gemA.gridPos.x, gemA.gridPos.y].isMatch ||
                            gems[gemB.gridPos.x, gemB.gridPos.y].isMatch;

            if (!hasMatch)
            {
                yield return new WaitForSeconds(timeReturn);
                SwapGem(gemA.gridPos, gemB.gridPos);
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
                ClearMatchedGems();
                Debug.Log("Match found!");
            }
        }
        #endregion Swap

        #region Find Matches
        public void FindMatches()
        {
            // Reset flag match
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (gems[x, y] != null)
                        gems[x, y].isMatch = false;
                }
            }

            HashSet<Vector2Int> matches = new HashSet<Vector2Int>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gem currentGem = gems[x, y];
                    if (currentGem == null) continue;

                    var gemType = currentGem.TypeOfGem;

                    // Kiểm tra hàng ngang
                    if (x < width - 2 &&
                        gems[x + 1, y] != null && gems[x + 2, y] != null &&
                        gems[x + 1, y].TypeOfGem == gemType &&
                        gems[x + 2, y].TypeOfGem == gemType)
                    {
                        int endCol = x + 2;
                        while (endCol + 1 < width &&
                               gems[endCol + 1, y] != null &&
                               gems[endCol + 1, y].TypeOfGem == gemType)
                        {
                            endCol++;
                        }

                        for (int i = x; i <= endCol; i++)
                            matches.Add(new Vector2Int(i, y));
                    }

                    // Kiểm tra hàng dọc
                    if (y < height - 2 &&
                        gems[x, y + 1] != null && gems[x, y + 2] != null &&
                        gems[x, y + 1].TypeOfGem == gemType &&
                        gems[x, y + 2].TypeOfGem == gemType)
                    {
                        int endRow = y + 2;
                        while (endRow + 1 < height &&
                               gems[x, endRow + 1] != null &&
                               gems[x, endRow + 1].TypeOfGem == gemType)
                        {
                            endRow++;
                        }

                        for (int i = y; i <= endRow; i++)
                            matches.Add(new Vector2Int(x, i));
                    }
                }
            }

            if (matches.Count > 0)
            {
                foreach (var pos in matches)
                {
                    gems[pos.x, pos.y].isMatch = true;
                    ClearMatchedGems();
                }
            }

            
        }

        public void ClearMatchedGems()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (gems[x, y] != null && gems[x,y].isMatch)
                    {
                        gems[x, y].PlayDestroyEffect();
                        
                    }
                }
            }
            StartCoroutine(DropDownGems());
        }


        private IEnumerator DropDownGems()
        {
            yield return new WaitForSeconds(0.1f); 

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (gems[x, y] == null)
                    {
                        for (int aboveY = y + 1; aboveY < height; aboveY++)
                        {
                            if (gems[x, aboveY] != null)
                            {
                                gems[x, y] = gems[x, aboveY];
                                gems[x, aboveY] = null;

                                gems[x, y].gridPos = new Vector2Int(x, y);
                                gems[x, y].MoveTo(new Vector2Int(x, y));
                                break;
                            }
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.2f);
            FindMatches();
        }
        #endregion


        // -------------------- GETTERS --------------------
        public int GetWidth() => width;
        public int GetHeight() => height;
        public float GetCellSize() => cellSize;
    }
}
