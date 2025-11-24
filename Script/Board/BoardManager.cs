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
        public List<Vector2Int> matches;
        //public Transform boardTileTransform;

        [Header("Subsystems")]
        private BoardGenerator boardGenerator;
        private SwapSystem swapSystem;
        private MatchFinder matchFinder;
        private BoomHandler boomHandler;
        private RefillSystem refillSystem;

        [Header("Tile Gem Bonus")]
        public bool[,] obstacle;
        public TileKind[] tileKinds;
        public Crate[,] crates;

        [Header("Level Manager")]
        public LevelManager LevelManager { get; private set; }

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


            tileKinds = new TileKind[LevelManager.LevelData.tileKinds.Count()];

            for(int i = 0; i < tileKinds.Count(); i++)
            {
                tileKinds[i] = new TileKind(
                    LevelManager.LevelData.tileKinds[i].posX, 
                    LevelManager.LevelData.tileKinds[i].posY, 
                    LevelManager.LevelData.tileKinds[i].tileType);
            }


            gems = new Gem[width, height];
            obstacle = new bool[width, height];
            crates = new Crate[width, height];

            for (int i = 0; i < obstacle.GetLength(0); i++)
            {
                for (int j = 0; j < obstacle.GetLength(1); j++)
                {
                    obstacle[i, j] = false;
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
            List<Gem> validGems = new List<Gem>();

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Gem g = gems[x, y];
                    if (g != null && !g.GetGemData.IsBoom)
                    {
                        validGems.Add(g);
                    }
                }
            }

            if (validGems.Count == 0)
                return GemType.Red;

            int randomIndex = UnityEngine.Random.Range(0, validGems.Count);
            return validGems[randomIndex].TypeOfGem;
        }

        public void TrySwap(Gem gem, Vector2Int dir, float timeReturn) =>
            swapSystem.TrySwap(gem, dir, timeReturn);

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
