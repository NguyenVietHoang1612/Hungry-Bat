using System.Collections;
using UnityEngine;

namespace CandyProject
{
    public class Gem : MonoBehaviour
    {
        [SerializeField] private GemData gemData;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Vector2Int gridPos;
        [SerializeField] private GameObject destroyEffect;
        [Header("Flags")]
        public bool isMatch = false;

        [SerializeField] private float moveSpeed = 5f;

        public void Init(GemData data)
        {
            gemData = data;
            spriteRenderer.sprite = gemData.gemIcon;
            spriteRenderer.color = Color.white;
        }

        public void MoveTo(Vector2Int targetPos)
        {
            gridPos = targetPos;
            StopAllCoroutines();
            StartCoroutine(MoveCoroutine(targetPos));
        }

        private IEnumerator MoveCoroutine(Vector2Int targetPos)
        {
            Vector3 target = new Vector3(targetPos.x, targetPos.y, 0) * BoardManager.Instance.GetCellSize();
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
        }

        public void PlayDestroyEffect()
        {
            if (destroyEffect != null)
            {
                GameObject fx = Instantiate(destroyEffect, transform.position, Quaternion.identity);

                Destroy(fx, 0.5f);
            }
            DestroyGem();
        }

        private void DestroyGem()
        {
            Destroy(gameObject);
            Debug.Log("Gem destroyed");
        }

        public GemType TypeOfGem => gemData.gemType;
    }
}
