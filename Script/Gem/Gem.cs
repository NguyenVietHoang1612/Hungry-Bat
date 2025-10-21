using System.Collections;
using UnityEngine;

namespace CandyProject
{
    public class Gem : MonoBehaviour
    {
        [SerializeField] private GemData gemData;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public Vector2Int gridPos;

        [SerializeField] float aGravity = -20f;
        [SerializeField] float bounce = 0.2f;

        [Header("Flags")]
        public bool grounded = false;
        public bool isMatch = false;

        [SerializeField] private float moveSpeed = 0.1f;

        [Header("Coroutine")]
        private Coroutine moveRoutine;
        private Coroutine gravityRoutine;
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
            Debug.LogWarning("MoveTo");
            StopAllCoroutines();
            gridPos = targetPos;
            StartCoroutine(MoveCoroutine(targetPos));
        }

        private IEnumerator MoveCoroutine(Vector2Int targetPos)
        {
            Vector3 target = new Vector3(targetPos.x, targetPos.y, 0) * BoardManager.Instance.CellSize;
            while (Vector3.Distance(transform.position, target) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed);
                yield return null;
            }
            transform.position = target;
        }

        public void MoveGravityTo(Vector2Int targetPos)
        {

            gridPos = targetPos;
            grounded = false;
            StopAllCoroutines();
            StartCoroutine(MoveCoroutineGravity(targetPos));
        }

        private IEnumerator MoveCoroutineGravity(Vector2Int targetPos)
        {
            Vector3 target = new Vector3(targetPos.x, targetPos.y, 0) * BoardManager.Instance.CellSize;
            Vector3 velocity = Vector3.zero;
            

            Vector3 pos = transform.position;

            while (!grounded)
            {
                velocity.y += aGravity * Time.deltaTime;

                pos += velocity * Time.deltaTime;

                if (pos.y <= target.y)
                {
                    pos.y = target.y;

                    // Nảy nhẹ
                    if (Mathf.Abs(velocity.y) > 1f)
                    {
                        velocity.y = -velocity.y * bounce;
                    }
                    else
                    {
                        grounded = true;
                        pos = target;
                    }
                }

                transform.position = pos;
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
            BoardManager.Instance.StartCoroutine(ReturnEffectToPoolRoutine(fx, gemData.destroyEffect, 0.3f));
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
