using UnityEngine;

namespace CandyProject
{
    public class BoardManager : MonoBehaviour
    {
        [SerializeField] private int width;

        [SerializeField] private int height;

        [SerializeField] private float cellSize;

        [SerializeField] private GemData[] gemDatas;
        public GameObject gemPrefab;

        [SerializeField] private BackGroundTiles[,] backgroundTiles;

        private void Start()
        {
            backgroundTiles = new BackGroundTiles[width, height];
            GenerateBoard();
        }

        private void GenerateBoard()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);

                    SpawnGem(x, y, position);
                }
            }
        }


        private void SpawnGem(int x, int y, Vector3 pos)
        {
            GemData randomGem = gemDatas[Random.Range(0, gemDatas.Length)];

            Transform gemTranform = Instantiate(gemPrefab, pos, Quaternion.identity).transform;

            Gem gem = gemTranform.GetComponent<Gem>();

            gem.Init(randomGem);

            gemTranform.SetParent(this.transform);
            backgroundTiles[x, y] = new BackGroundTiles(gem);
        }

    }
}

