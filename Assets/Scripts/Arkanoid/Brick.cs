using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// A single brick – can be destroyed by a ball, spawns an optional bonus.
    /// Emits <see cref="OnBrickDestroyed"/> so the controller knows when to check win condition.
    /// </summary>
    public class Brick : MonoBehaviour
    {
        public static event Action<Brick> OnBrickDestroyed;

        [Header("Brick Settings")]
        [SerializeField] private int _health = 1;
        [SerializeField] private int _scoreValue = 100;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject _destroyEffect = null;
        
        [Header("Bonus Settings")]
        [SerializeField] private GameObject _bonusPrefab = null;
        [SerializeField] private float _bonusChance = 0.15f;

        #region Properties

        public int ScoreValue => _scoreValue;
        public int Health => _health;
        public bool IsDestroyed => _health <= 0;

        #endregion

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.collider.CompareTag("Ball"))
            {
                return;
            }

            TakeDamage(1);
        }

        /// <summary>
        /// Apply damage to the brick.
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (IsDestroyed || damage <= 0)
            {
                return;
            }

            _health -= damage;

            if (IsDestroyed)
            {
                DestroyBrick();
            }
        }

        private void DestroyBrick()
        {
            // Spawn visual effects
            if (_destroyEffect != null)
            {
                Instantiate(_destroyEffect, transform.position, Quaternion.identity);
            }

            // Try to spawn bonus
            TrySpawnBonus();

            // Notify about destruction with this brick instance
            OnBrickDestroyed?.Invoke(this);

            // Destroy the brick
            Destroy(gameObject);
        }

        private void TrySpawnBonus()
        {
            if (_bonusPrefab == null)
            {
                return;
            }

            if (Random.value <= _bonusChance)
            {
                Instantiate(_bonusPrefab, transform.position, Quaternion.identity);
            }
        }
        
        public void SetHealth(int hp) => _health = Mathf.Max(1, hp);

        /// <summary>
        /// Configure brick properties.
        /// </summary>
        public void Configure(int health, int scoreValue = 100, float bonusChance = 0.15f)
        {
            _health = Mathf.Max(1, health);
            _scoreValue = Mathf.Max(0, scoreValue);
            _bonusChance = Mathf.Clamp01(bonusChance);
        }

        #region Validation

        private void OnValidate()
        {
            _health = Mathf.Max(1, _health);
            _scoreValue = Mathf.Max(0, _scoreValue);
            _bonusChance = Mathf.Clamp01(_bonusChance);
        }

        #endregion
    }
}