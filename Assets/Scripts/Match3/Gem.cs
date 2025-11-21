using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniIT.CORE;

namespace MiniIT.MATCH3
{
    /// <summary>
    /// Gem component used by Match3 board. Implement IPoolable.
    /// </summary>
    public class Gem : MonoBehaviour, IGridItem, IPoolable
    {
        /// <summary>Grid X coordinate.</summary>
        public int X { get; set; } = 0;

        /// <summary>Grid Y coordinate.</summary>
        public int Y { get; set; } = 0;

        /// <summary>Color id (0..N-1).</summary>
        public int SpriteId { get; private set; } = 0;
        
        [SerializeField]
        private List<Sprite> sprites = null;

        private SpriteRenderer spriteRenderer = null;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>Initialize gem with color id and visual update.</summary>
        public void Init(int spriteId)
        {
            SpriteId = spriteId;
            UpdateVisual();
        }

        /// <summary>Update sprite color/appearance according to ColorId.</summary>
        private void UpdateVisual()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.sprite = sprites[SpriteId];
        }
        
        public IEnumerator AnimateMove(Vector3 target, float duration = 0.15f)
        {
            Vector3 start = transform.position;
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }

            transform.position = target;
        }

        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
        }
    }
}
