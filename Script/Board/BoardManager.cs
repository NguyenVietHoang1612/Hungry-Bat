using System;
using System.Collections.Generic;
using UnityEngine;

namespace CandyProject
{
    public enum TileType
    {
        BlankSpace,
        Breakable,
        none
    }

    [Serializable]
    public class TileKind
    {
        public int posX;
        public int posY;
        public TileType tileType;
    }

    public class BoardManager : Singleton<BoardManager>
    {
        [Header("Board Settings")]
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize = 0.6f;

        [Header("Prefabs & Data")]
        [SerializeField] private GemData[] gemDatas;
        [SerializeField] private GameObject gemPrefab;
        [SerializeField] private GameObject boardTile;
        [SerializeField] private GameObject bonusGemPrefab;

        [Header("Runtime Data")]
        public Gem[,] gems;
        public List<Vector2Int> matches;
        
        [Header("Subsystems")]
        private BoardGenerator boardGenerator;
        private SwapSystem swapSystem;
        private MatchFinder matchFinder;
        private BoomHandler boomHandler;
        private RefillSystem refillSystem;

        [Header("Tile Gem Bonus")]
        public bool[,] obstacle;
        public TileKind[] tileKinds;
        public BonusGem[,] bonusGem;

        [Header("Score Manager")]
        public ScoreBarController scoreManager;

        private void Start()
        {
            boardGenerator = new BoardGenerator(this);
            swapSystem = new SwapSystem(this);
            boomHandler = new BoomHandler(this);
            matchFinder = new MatchFinder(this, boomHandler);
            refillSystem = new RefillSystem(this);

            gems = new Gem[width, height];
            obstacle = new bool[width, height];
            bonusGem = new BonusGem[width, height];
            for (int i = 0; i < obstacle.GetLength(0); i++)
            {
                for (int j = 0; j < obstacle.GetLength(1); j++)
                {
                    obstacle[i, j] = false;
                }
            }
         
            int initialSize = width * height;

            ObjectPoolManager.Instance.CreatePool(gemPrefab, initialSize);
            ObjectPoolManager.Instance.CreatePool(boardTile, initialSize);
            ObjectPoolManager.Instance.CreatePool(bonusGemPrefab, initialSize);
            
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
        public GameObject BoardTile => boardTile;
        public GameObject BonusGemPrefab => bonusGemPrefab;


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

        public void TriggerSwapBoom(Gem gemA, Gem gemB) =>
            boomHandler.TriggerSwapBoom(gemA, gemB);

        public void TriggerBoom(Gem gem) =>
            boomHandler.TriggerBoom(gem);

        public bool CheckDeadLock =>
            swapSystem.IsDeadlock();

        public void ShuffleBoard() =>
            boardGenerator.ShuffleBoard();

        public List<Gem> ListMatchesSwap() 
            => swapSystem.ListMatches();
    }
}
