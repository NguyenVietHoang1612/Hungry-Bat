using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CandyProject
{
    public enum TileType
    {
        BlankSpace,
        Crate,
        none
    }

    [Serializable]
    public class TileKind
    {
        public int posX;
        public int posY;
        public TileType tileType;

        public TileKind(int posX, int posY, TileType tileType)
        {
            this.posX = posX;
            this.posY = posY;
            this.tileType = tileType;
        }
    }

    public class BoardManager : MonoBehaviour
    {
        [Header("Board Settings")]
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize = 0.6f;

        [Header("Prefabs & Data")]
        [SerializeField] private GemData[] gemDatas;
        [SerializeField] private GameObject gemPrefab;

        //BG Board 
        //[SerializeField] private GameObject boardTile;
        [SerializeField] private GameObject cratePrefab;

        [Header("Runtime Data")]
        public Gem[,] gems;
        public List<Vector2Int> matches = new List<Vector2Int>();
        //public Transform boardTileTransform;

        [Header("Subsystems")]
        private BoardGenerator boardGenerator;
        private SwapSystem swapSystem;
        private MatchFinder matchFinder;
        private BoomHandler boomHandler;
        private RefillSystem refillSystem;

        [Header("Tile Gem")]
        public bool[,] blankSpaces;
        public TileKind[] tileKinds;
        public Crate[,] crates;

        [Header("Level Manager")]
        public LevelManager LevelManager { get; private set; }

        private List<Gem> tempValidGems = new List<Gem>();

        private void Start()
        {
            LevelManager = FindFirstObjectByType<LevelManager>();

            boardGenerator = new BoardGenerator(this);
            swapSystem = new SwapSystem(this);
            boomHandler = new BoomHandler(this);
            matchFinder = new MatchFinder(this, boomHandler);
            refillSystem = new RefillSystem(this);

            width = LevelManager.LevelData.width;
            height = LevelManager.LevelData.height;

            int lenght = LevelManager.LevelData.tileKinds.Length;
            tileKinds = new TileKind[lenght];

            for(int i = 0; i < tileKinds.Length; i++)
            {
                tileKinds[i] = new TileKind(
                    LevelManager.LevelData.tileKinds[i].posX, 
                    LevelManager.LevelData.tileKinds[i].posY, 
                    LevelManager.LevelData.tileKinds[i].tileType);
            }


            gems = new Gem[width, height];
            blankSpaces = new bool[width, height];
            crates = new Crate[width, height];

            for (int i = 0; i < blankSpaces.GetLength(0); i++)
            {
                for (int j = 0; j < blankSpaces.GetLength(1); j++)
                {
                    blankSpaces[i, j] = false;
                }
            }

            GameManager.Instance.LoadSettings();
            int initialSize = width * height;

            ObjectPoolManager.Instance.CreatePool(gemPrefab, initialSize);
            //ObjectPoolManager.Instance.CreatePool(boardTile, initialSize);
            ObjectPoolManager.Instance.CreatePool(cratePrefab, 5);

            boardGenerator.GenerateBoard();

            if (CheckDeadLock)
            {
                Debug.Log("Deadlock detected! Shuffling the board...");
                boardGenerator.ShuffleBoard();
            }
        }

        // Sinh gem
        public Gem CreateGem(Vector2 worldPos, GemData gemData)
        {
            GameObject obj = ObjectPoolManager.Instance.Get(GemPrefab);
            obj.transform.position = worldPos;

            Gem gem = obj.GetComponent<Gem>();
            gem.Init(gemData);
            return gem;
        }

        public void ClearBoard()
        {
            if (gems != null)
            {
                for (int x = 0; x < Width; x++)
                {
                    for (int y = 0; y < Height; y++)
                    {
                        if (gems[x, y] != null)
                        {
                            ObjectPoolManager.Instance.Return(gemPrefab, gems[x, y].gameObject);
                            gems[x, y] = null;
                        }

                        if (crates[x, y] != null)
                        {
                            ObjectPoolManager.Instance.Return(cratePrefab, crates[x, y].gameObject);
                            crates[x, y] = null;
                        }
                    }
                }
            }

            //var tiles = GameObject.FindGameObjectsWithTag("BoardTile");
            //foreach (var tile in tiles)
            //{
            //    ObjectPoolManager.Instance.Return(boardTile, tile);
            //}

            Debug.Log("Board cleared successfully.");
        }


        public int NumSpecials()
        {
            int count = 0;
            foreach (var gem in GemDatas)
            {
                if (gem != null && !gem.IsBoom)
                {
                    count++;
                }
            }

            int numSpecials = GemDatas.Length - count;
            return numSpecials;
        }

        public int Width => width;
        public int Height => height;
        public float CellSize => cellSize;
        public GemData[] GemDatas => gemDatas;
        public GameObject GemPrefab => gemPrefab;

        //public GameObject BoardTile => boardTile;
        public GameObject CratePrefab => cratePrefab;

        public bool IsInsideBoard(Vector2Int p)
        {
            return p.x >= 0 && p.x < width &&
                p.y >= 0 && p.y < height;
        }


        public GemType GetRandomGemType()
        {
            tempValidGems.Clear();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gem g = gems[x, y];
                    if (g != null && !g.GetGemData.IsBoom)
                    {
                        tempValidGems.Add(g);
                    }
                }
            }

            if (tempValidGems.Count == 0)
                return GemType.Red;

            int randomIndex = UnityEngine.Random.Range(0, tempValidGems.Count);
            return tempValidGems[randomIndex].TypeOfGem;
        }

        public IEnumerator MoveAllGemsCoroutine(float moveSpeed = 12f)
        {
            bool anyMoving;

            do
            {
                anyMoving = false;

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        Gem gem = gems[x, y];

                        if (blankSpaces[x, y]) continue;
                        if (crates[x, y]) continue;

                        if (gem != null && gem.isMoving)
                        {
                            anyMoving = true;
                            Vector3 start = gem.transform.position;
                            Vector3 target = new Vector3(gem.targetGridPos.x, gem.targetGridPos.y, 0) * cellSize;

                            gem.transform.position = Vector3.MoveTowards(start, target, moveSpeed * Time.deltaTime);

                            if (Vector3.Distance(gem.transform.position, target) < 0.01f)
                            {
                                gem.transform.position = target;
                                gem.gridPos = gem.targetGridPos;
                                gem.isMoving = false;
                            }
                        }
                    }
                }

                yield return null;

            } while (anyMoving);
        }

        public void TrySwap(Gem gem, Vector2Int dir) =>
            swapSystem.TrySwap(gem, dir);

        public void SwapGem(Gem gemA, Gem gemB) =>
            swapSystem.SwapGems(gemA, gemB);

        public void FindMatches() =>
            matchFinder.FindMatchGems();

        public void ClearMatchedGems() =>
            refillSystem.ClearMatchedGems();

        public void CollapsingGem() => refillSystem.Collapsing();

        public void TriggerSwapBoom(Gem gemA, Gem gemB) =>
            boomHandler.TriggerSwapBoom(gemA, gemB);

        //public void TriggerBoom(Gem gem) =>
        //    boomHandler.TriggerBoom(gem);

        public bool CheckDeadLock =>
            swapSystem.IsDeadlock();

        public void ShuffleBoard() =>
            boardGenerator.ShuffleBoard();

        public List<Gem> ListMatchesSwap()
            => swapSystem.ListMatches();
    }
}
