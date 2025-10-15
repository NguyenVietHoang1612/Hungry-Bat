using System.Collections;
using UnityEngine;

namespace CandyProject
{
    public class Gem : MonoBehaviour
    {
        [SerializeField] private GemData gemData;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Vector2Int gridPos;

        [Header("Flags")]
        public bool isMatch = false;

        [SerializeField] private float moveSpeed = 5f;

        private void Start()
        {
            if (gemData != null && gemData.destroyEffect != null)
                ObjectPoolManager.Instance.CreatePool(gemData.destroyEffect, 3);
        }

        public void Init(GemData data)
        {
            gemData = data;
            spriteRenderer.sprite = gemData.GetSprite();
            spriteRenderer.color = Color.white;
            name = data.gemName;
            isMatch = false;
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
            if (gemData == null || gemData.destroyEffect == null) return;

            GameObject fx = ObjectPoolManager.Instance.Get(gemData.destroyEffect);
            fx.transform.position = transform.position;
            fx.transform.rotation = Quaternion.identity;
            BoardManager.Instance.StartCoroutine(ReturnEffectToPoolRoutine(fx, gemData.destroyEffect, 10f));
        }

        private IEnumerator ReturnEffectToPoolRoutine(GameObject fx, GameObject prefab, float delay)
        {
            yield return new WaitForSeconds(delay);
            ObjectPoolManager.Instance.Return(prefab, fx);
        }

        public void ReturnPoolGem(GameObject prefab)
        {
            ObjectPoolManager.Instance.Return(prefab, gameObject);
        }

        public void SetColorGemSelected()
        {
            Color color = spriteRenderer.color;
            color.a = 0.7f;
            spriteRenderer.color = color;
        }

        public void ResetColor()
        {
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }

        public GemType TypeOfGem => gemData.gemType;
        public GemData GetGemData => gemData;

    }
}
