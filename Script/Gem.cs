using UnityEngine;

namespace CandyProject
{
    public class Gem : MonoBehaviour
    {
        [SerializeField] private GemData gemData;

        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Init(GemData data)
        {
            gemData = data;
            spriteRenderer.sprite = gemData.gemIcon;

        }

        public string GemName => gemData.gemName;
        public int Score => gemData.scoreValue;
    }
}
