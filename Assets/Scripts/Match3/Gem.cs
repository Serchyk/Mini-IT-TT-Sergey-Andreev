using System.Collections;
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
        public int ColorId { get; private set; } = 0;

        private SpriteRenderer spriteRenderer = null;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>Initialize gem with color id and visual update.</summary>
        public void Init(int colorId)
        {
            ColorId = colorId;
            UpdateVisual();
        }

        /// <summary>Update sprite color/appearance according to ColorId.</summary>
        private void UpdateVisual()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            // Map color id to color - simple mapping for demo
            switch (ColorId)
            {
                case 0:
                    spriteRenderer.color = Color.red;
                    break;
                case 1:
                    spriteRenderer.color = Color.green;
                    break;
                case 2:
                    spriteRenderer.color = Color.blue;
                    break;
                case 3:
                    spriteRenderer.color = Color.yellow;
                    break;
                case 4:
                    spriteRenderer.color = Color.magenta;
                    break;
                default:
                    spriteRenderer.color = Color.cyan;
                    break;
            }
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
