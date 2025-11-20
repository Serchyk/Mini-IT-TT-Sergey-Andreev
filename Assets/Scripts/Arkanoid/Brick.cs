using UnityEngine;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Brick with health, destruction animation and bonus spawning.
    /// </summary>
    public class Brick : MonoBehaviour
    {
        public static event System.Action OnBrickDestroyed;

        [SerializeField] private int _health = 1;
        [SerializeField] private GameObject _destroyEffect = null;
        [SerializeField] private GameObject _bonusPrefab = null;
        [SerializeField] private float _bonusChance = 0.15f;

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (!col.collider.CompareTag("Ball"))
                return;

            _health--;

            if (_health <= 0)
                DestroyBrick();
        }

        private void DestroyBrick()
        {
            if (_destroyEffect != null)
                Instantiate(_destroyEffect, transform.position, Quaternion.identity);

            TrySpawnBonus();

            Destroy(gameObject);
            OnBrickDestroyed?.Invoke();
        }

        private void TrySpawnBonus()
        {
            if (_bonusPrefab == null)
                return;

            if (Random.value <= _bonusChance)
                Instantiate(_bonusPrefab, transform.position, Quaternion.identity);
        }
        
        public void SetHealth(int hp)
        {
            _health = hp;
        }
    }
}