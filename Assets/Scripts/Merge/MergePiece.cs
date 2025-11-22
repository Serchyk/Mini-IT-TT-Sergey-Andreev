using System.Collections;
using UnityEngine;
using MiniIT.CORE;

namespace MiniIT.MERGE
{
    /// <summary>
    /// Merge piece with a level. Implements poolable and grid item.
    /// Visual appearance changes according to the level.
    /// </summary>
    public class MergePiece : MonoBehaviour, IGridItem, IPoolable
    {
        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        public int Level { get; private set; } = 1;

        [Header("Visual Settings")]
        [SerializeField] private SpriteRenderer _renderer = null;
        [SerializeField] private Gradient _levelColorGradient = null;
        [SerializeField] private float _baseScale = 0.8f;
        [SerializeField] private float _scalePerLevel = 0.1f;
        [SerializeField] private float _popScaleMultiplier = 1.3f;
        [SerializeField] private float _popDuration = 0.2f;

        /// <summary>
        /// Initialize piece with given level.
        /// </summary>
        public void Init(int level)
        {
            Level = level;
            UpdateVisual();
            AnimatePop();
        }

        /// <summary>
        /// Update visual appearance according to level.
        /// </summary>
        private void UpdateVisual()
        {
            float scale = _baseScale + Level * _scalePerLevel;
            transform.localScale = Vector3.one * scale;

            if (_renderer != null && _levelColorGradient != null)
            {
                float t = Mathf.Clamp01(Level / 10f);
                _renderer.color = _levelColorGradient.Evaluate(t);
            }
        }

        /// <summary>
        /// Animate piece popping effect on spawn or merge.
        /// </summary>
        private void AnimatePop()
        {
            StopAllCoroutines();
            StartCoroutine(PopRoutine());
        }

        private IEnumerator PopRoutine()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * _popScaleMultiplier;
            float elapsed = 0f;

            while (elapsed < _popDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _popDuration);
                transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < _popDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsed / _popDuration);
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }

            transform.localScale = originalScale;
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
