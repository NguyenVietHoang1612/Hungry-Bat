using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CandyProject
{
    public class BoardManager : Singleton<BoardManager>
    {
        [Header("Board Settings")]
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize = 0.6f;

        [SerializeField] private GemData[] gemDatas;
        [SerializeField] private GameObject gemPrefab;
        [SerializeField] private GameObject boardTile;
        [SerializeField] private Gem currentGem;
        private Gem[,] gems;

        [Header("Match Info")]
        [SerializeField] private List<MatchInfo> matchInfos;

        private bool[,] visited;


        private void Start()
        {
            gems = new Gem[width, height];
            int initialSizeBoard = width * height;
            ObjectPoolManager.Instance.CreatePool(gemPrefab, initialSizeBoard);
            ObjectPoolManager.Instance.CreatePool(boardTile, initialSizeBoard);
            GenerateBoard();
        }

        private void GenerateBoard()
        {
            SpawnBoardTile();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SpawnGem(new Vector2Int(x, y));
                    while (MatchesAt(x, y, gems[x, y]))
                    {
                        gems[x, y].ReturnPoolGem(gemPrefab);
                        SpawnGem(new Vector2Int(x, y));
                    }
                }
            }
        }

        private void SpawnBoardTile()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2 worldPos = new Vector2(x, y) * cellSize;
                    GameObject obj = ObjectPoolManager.Instance.Get(boardTile);
                    obj.transform.position = worldPos;
                    obj.transform.rotation = Quaternion.identity;
                }
            }
        }

        private int NumSpecials()
        {
            int count = 0;
            foreach (var gem in gemDatas)
            {
                if (gem != null && gem.IsBoom)
                {
                    count++;
                }
            }

            int numSpecials = gemDatas.Length - count;
            return numSpecials;
        }

        private void SpawnGem(Vector2Int gridPos)
        {

            Vector2 worldPos = (Vector2)gridPos * cellSize;
            GemData randomGem = gemDatas[Random.Range(0, gemDatas.Length - NumSpecials())];

            Gem gem = CreateGem(worldPos, randomGem);
            gem.gridPos = gridPos;

            gems[gridPos.x, gridPos.y] = gem;

        }


        #region Swap

        public void SwapGem(Vector2Int posA, Vector2Int posB)
        {

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

        private IEnumerator CheckSwapResult(Gem gemA, Gem gemB, float timeReturn)
        {
            yield return new WaitForSeconds(0.1f);

            if (gemA.GetGemData.IsBoom && gemB.GetGemData.IsBoom)
            {
                TriggerBoom(gemA);
                TriggerBoom(gemB);
                yield break;
            }
            else if (gemA.GetGemData.gemType == GemType.ArrowHorizontal || gemA.GetGemData.gemType == GemType.ArrowVertical)
            {
                TriggerBoom(gemA);
                yield break;
            }
            else if (gemB.GetGemData.gemType == GemType.ArrowHorizontal || gemB.GetGemData.gemType == GemType.ArrowVertical)
            {
                TriggerBoom(gemB);
                yield break;
            }

            FindMatches();

            if (gems[gemA.gridPos.x, gemA.gridPos.y] != null && gems[gemB.gridPos.x, gemB.gridPos.y] != null)
            {
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
        }

        //Trigger Boom

        private void TriggerBoom(Gem boomGem)
        {
            Vector2Int gridPos = boomGem.gridPos;
            if (boomGem.TypeOfGem == GemType.ArrowVertical)
            {
                GetColumnPieces(gridPos.x);

            }
            else if (boomGem.TypeOfGem == GemType.ArrowHorizontal)
            {
                GetRowPieces(gridPos.y);
            }
            ClearMatchedGems();

        }
        #endregion Swap

        #region Find Matches
        public void FindMatches()
        {
            // Reset isMatch trước khi tìm kiếm
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (gems[x, y] != null)
                        gems[x, y].isMatch = false;
                }
            }

            List<Vector2Int> matches = new List<Vector2Int>();

            // Lưu trữ thông tin về loại match
            matchInfos = new List<MatchInfo>();
            visited = new bool[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    currentGem = gems[x, y];

                    if (currentGem == null) continue;

                    var gemType = currentGem.TypeOfGem;

                    // Kiểm tra cột
                    int countHorizontal = 1;
                    for (int col = x + 1; col < width && gems[col, y] != null && gems[col, y].TypeOfGem == gemType; col++)
                    {
                        countHorizontal += CheckVisitedGem(col, y, gemType);
                    }

                    Debug.Log("Count Horizontal: " + countHorizontal);
                    if (countHorizontal >= 3)
                    {
                        for (int i = 0; i < countHorizontal; i++)
                        {
                            matches.Add(new Vector2Int(x + i, y));

                        }

                        if (countHorizontal == 4)
                        {
                            Vector2Int center = new Vector2Int(x + 1, y);
                            matchInfos.Add(new MatchInfo(center, MatchType.FourVertical));
                        }
                        else if (countHorizontal >= 5)
                        {
                            Vector2Int center = new Vector2Int(x + 2, y);
                            matchInfos.Add(new MatchInfo(center, MatchType.Five));
                        }

                    }
                }
            }
            visited = new bool[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    currentGem = gems[x, y];

                    if (currentGem == null) continue;

                    var gemType = currentGem.TypeOfGem;
                    // Kiểm tra hàng hàng
                    int countVertical = 1;
                    for (int row = y + 1; row < height && gems[x, row] != null && gems[x, row].TypeOfGem == gemType; row++)
                    {
                        countVertical += CheckVisitedGem(x, row, gemType);
                    }

                    if (countVertical >= 3)
                    {
                        for (int i = 0; i < countVertical; i++)
                        {
                            matches.Add(new Vector2Int(x, y + i));
                        }
                        if (countVertical == 4)
                        {
                            Vector2Int center = new Vector2Int(x, y + 1);
                            matchInfos.Add(new MatchInfo(center, MatchType.FourHorizontal));
                        }
                        else if (countVertical >= 5)
                        {
                            Vector2Int center = new Vector2Int(x, y + 2);
                            matchInfos.Add(new MatchInfo(center, MatchType.Five));
                        }
                    }
                }
            }

            if (matches.Count > 0)
            {
                foreach (var pos in matches)
                {
                    gems[pos.x, pos.y].isMatch = true;
                }
                ClearMatchedGems();
            }

            if (matchInfos.Count > 0)
            {
                Debug.Log("Creating booms...");
                StartCoroutine(CreateBooms(matchInfos));
            }


        }

        private int CheckVisitedGem(int x, int y, GemType gemType)
        {
            if (visited[x, y]) return 0;

            if (x < 0 || y < 0 || x >= width || y >= height) return 0;

            if (gems[x, y] == null || gems[x, y].TypeOfGem != gemType)
                return 0;

            visited[x, y] = true;

            return 1;
        }

        private IEnumerator CreateBooms(List<MatchInfo> matchInfos)
        {
            yield return new WaitForSeconds(0.1f);

            foreach (MatchInfo matchInfo in matchInfos)
            {
                Vector2Int centerPos = matchInfo.center;
                Debug.Log("Creating booms tiep");

                if (centerPos.x < 0 || centerPos.x >= width || centerPos.y < 0 || centerPos.y >= height)
                    continue;

                Gem baseGem = gems[centerPos.x, centerPos.y];
                Vector2 worldPos = (Vector2)centerPos * cellSize;

                GemData boomData = null;

                // Xác định loại boom dựa vào loại match
                switch (matchInfo.matchType)
                {
                    case MatchType.FourHorizontal:
                        Debug.Log("Creating booms ArrowHorizontal");
                        boomData = gemDatas.FirstOrDefault(g => g.gemType == GemType.ArrowHorizontal);
                        break;

                    case MatchType.FourVertical:
                        Debug.Log("Creating booms ArrowVertical");
                        boomData = gemDatas.FirstOrDefault(g => g.gemType == GemType.ArrowVertical);
                        break;

                    case MatchType.Five:
                        Debug.Log("Creating booms 5");
                        boomData = gemDatas.FirstOrDefault(g => g.gemType == GemType.BoomColor);
                        break;

                    default:
                        Debug.Log(".......");
                        continue;
                }

                if (boomData == null)
                {
                    Debug.LogWarning($"Không tìm thấy GemData cho loại boom: {matchInfo.matchType}");
                    continue;
                }

                if (baseGem != null)
                {
                    baseGem.ReturnPoolGem(gemPrefab);
                    gems[centerPos.x, centerPos.y] = null;
                }

                Gem boomGem = CreateGem(worldPos, boomData);
                boomGem.isMatch = false;
                boomGem.gridPos = centerPos;
                gems[centerPos.x, centerPos.y] = boomGem;
            }
        }

        private Gem CreateGem(Vector2 worldPos, GemData gemData)
        {
            GameObject obj = ObjectPoolManager.Instance.Get(gemPrefab);
            obj.transform.position = worldPos;

            Gem gem = obj.GetComponent<Gem>();
            gem.Init(gemData);
            return gem;
        }

        private bool MatchesAt(int x, int y, Gem gem)
        {
            if (x > 1 && y > 1)
            {

                if (gems[x - 1, y].TypeOfGem == gem.TypeOfGem &&
                    gems[x - 2, y].TypeOfGem == gem.TypeOfGem)
                {
                    return true;
                }

                if (gems[x, y - 1].TypeOfGem == gem.TypeOfGem &&
                    gems[x, y - 2].TypeOfGem == gem.TypeOfGem)
                {
                    return true;
                }
            }
            else if (x <= 1 || y <= 1)
            {
                if (y > 1)
                {
                    if (gems[x, y - 1].TypeOfGem == gem.TypeOfGem &&
                    gems[x, y - 2].TypeOfGem == gem.TypeOfGem)
                    {
                        return true;
                    }
                }

                if (x > 1)
                {
                    if (gems[x - 1, y].TypeOfGem == gem.TypeOfGem &&
                    gems[x - 2, y].TypeOfGem == gem.TypeOfGem)
                    {
                        return true;
                    }
                }



            }

            return false;

        }
        #endregion

        #region Collapsing and Refilling

        public void ClearMatchedGems()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (gems[x, y] != null && gems[x, y].isMatch)
                    {
                        gems[x, y].PlayDestroyEffect();
                        gems[x, y].ReturnPoolGem(gemPrefab);
                        gems[x, y] = null;
                    }


                }
            }
            StartCoroutine(DropDownGems());
        }


        private IEnumerator DropDownGems()
        {
            yield return new WaitForSeconds(0.25f);

            for (int x = 0; x < width; x++)
            {
                int emptyY = -1;

                for (int y = 0; y < height; y++)
                {
                    if (gems[x, y] == null && emptyY == -1)
                    {
                        emptyY = y;
                    }
                    else if (gems[x, y] != null && emptyY != -1)
                    {
                        gems[x, emptyY] = gems[x, y];
                        gems[x, y] = null;

                        gems[x, emptyY].gridPos = new Vector2Int(x, emptyY);
                        gems[x, emptyY].MoveTo(new Vector2Int(x, emptyY));
                        emptyY++;
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
            StartCoroutine(RefillingGem());
        }

        private IEnumerator RefillingGem()
        {
            yield return new WaitForSeconds(0.1f);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (gems[x, y] == null)
                    {
                        Vector2Int spawnPos = new Vector2Int(x, y);
                        Vector2 worldPos = new Vector2(x, height + 1) * cellSize;


                        GemData randomGem = gemDatas[Random.Range(0, gemDatas.Length - NumSpecials())];
                        Gem newGem = CreateGem(worldPos, randomGem);
                        newGem.Init(randomGem);
                        newGem.gridPos = spawnPos;

                        gems[x, y] = newGem;
                        newGem.MoveTo(spawnPos);
                    }
                }
            }

            yield return new WaitForSeconds(0.3f);
            FindMatches();
        }

        #endregion




        // -------------------- GETTERS --------------------
        public List<Vector2Int> GetRowPieces(int row)
        {
            List<Vector2Int> rowGems = new List<Vector2Int>();
            for (int x = 0; x < width; x++)
            {
                rowGems.Add(new Vector2Int(x, row));
                gems[x, row].isMatch = true;
            }
            return rowGems;
        }

        public List<Vector2Int> GetColumnPieces(int column)
        {
            List<Vector2Int> columnGems = new List<Vector2Int>();
            for (int y = 0; y < height; y++)
            {
                columnGems.Add(new Vector2Int(column, y));
                gems[column, y].isMatch = true;
            }
            return columnGems;
        }

        public int GetWidth() => width;
        public int GetHeight() => height;
        public float GetCellSize() => cellSize;
    }
}
