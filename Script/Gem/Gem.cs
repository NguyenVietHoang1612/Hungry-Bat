using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

namespace CandyProject
{
    public class Gem : MonoBehaviour
    {
        [SerializeField] protected GemData gemData;
        [SerializeField] protected SpriteRenderer spriteRenderer;

        protected VisualEffect VisualEffect;
        public Vector2Int gridPos;
        public bool isVertical;
        [Header("Flags")]
        public bool isMatch = false;
        public bool isMoving = false;

        [SerializeField] private float moveSpeed = 6f;

        public int health = 1;
        private void Start()
        {
            
            if (gemData != null && gemData.destroyEffect != null)
                ObjectPoolManager.Instance.CreatePool(gemData.destroyEffect, 3);
        }

        public virtual void Init(GemData data)
        {
            gemData = data;
            spriteRenderer.sprite = gemData.GetSprite();
            spriteRenderer.color = Color.white;
            name = data.gemName;
            isMatch = false;
        }

        #region Handle Match
        //public virtual void Damage(int amount)
        //{
        //    height-=amount;

        //    if (height <= 0)
        //    {
        //        PlayDestroyEffect();
        //        ReturnToPoolWithDelay();
        //    }
        //}

        //protected void ReturnToPoolWithDelay()
        //{
        //    GameManager.Instance.StartCoroutine(ReturnGemRoutine());
        //}

        //protected IEnumerator ReturnGemRoutine()
        //{
        //    yield return new WaitForSeconds(0.2f);
        //    ReturnPoolGem(GameManager.Instance.Board.GemPrefab);
        //}
        #endregion

        public void MoveTo(Vector2Int targetPos)
        {
            gridPos = targetPos;
            StartCoroutine(MoveCoroutine(targetPos));
        }

        private IEnumerator MoveCoroutine(Vector2Int targetPos)
        {
            Vector3 start = transform.position;
            Vector3 target = new Vector3(targetPos.x, targetPos.y, 0) * GameManager.Instance.Board.CellSize;

            float elapsed = 0f;
            float duration = 1f / moveSpeed;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            transform.position = target;
        }

        public void MoveGemtoCondition(Vector2 uiWorldPos)
        {
            StartCoroutine(MoveToUIRoutine(uiWorldPos));
        }

        private IEnumerator MoveToUIRoutine(Vector2 uiWorldPos)
        {
            isMoving = true;

            Vector3 start = transform.position;
            Vector3 target = new Vector3(uiWorldPos.x, uiWorldPos.y, 0);

            float elapsed = 0f;
            float duration = 0.4f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }

            transform.position = target;
            isMoving = false;
        }


        public void PlayDestroyEffect()
        {
            if (gemData == null || gemData.destroyEffect == null) return;

            GameObject fx = ObjectPoolManager.Instance.Get(gemData.destroyEffect);
            VisualEffect = fx.GetComponentInChildren<VisualEffect>();
            VisualEffect.Play();
            SoundManager.Instance.PlayOneShotSfx(gemData.destroySound);
            fx.transform.position = transform.position;
            fx.transform.rotation = Quaternion.identity;
            GameManager.Instance.StartCoroutine(ReturnEffectToPoolRoutine(fx, gemData.destroyEffect, 1.3f));
        }

        protected IEnumerator ReturnEffectToPoolRoutine(GameObject fx, GameObject prefab, float delay)
        {
            yield return new WaitForSeconds(delay);
            ObjectPoolManager.Instance.Return(prefab, fx);
        }

        public void ReturnPoolGem(GameObject prefab)
        {
            StartCoroutine(ReturnPoolWhenStopMoving(prefab));
        }

        private IEnumerator ReturnPoolWhenStopMoving(GameObject prefab)
        {
            // Đợi cho tới khi gem không còn di chuyển
            while (isMoving)
                yield return null;

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
