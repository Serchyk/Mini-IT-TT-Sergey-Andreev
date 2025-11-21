using System;
using MiniIT.CORE;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Enum for all bonus types.
    /// </summary>
    public enum BonusType
    {
        MultiBall,
        LongPaddle
    }
    
    /// <summary>
    /// Falling bonus that applies an effect when caught by the paddle.
    /// Implements <see cref="IPoolable"/> for pooling support.
    /// </summary>
    public class BonusItem : MonoBehaviour, IPoolable
    {
        [SerializeField] private float _fallSpeed = 3f;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Camera _camera;
        public BonusType Type { get; private set; }

        #region IPoolable

        public void OnSpawned()
            => gameObject.SetActive(true);

        public void OnDespawned()
            => gameObject.SetActive(false);

        #endregion

        private void Start()
        {
            _camera = Camera.main;
            SetRandomType();   
        }

        private void Update()
        {
            transform.position += Vector3.down * _fallSpeed * Time.deltaTime;

            var bottomY = _camera.ScreenToWorldPoint(Vector3.zero).y;
            if (transform.position.y < bottomY)
                ArkanoidController.Instance.BonusPool.Return(this);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Paddle")) return;

            ArkanoidController.Instance.ApplyBonus(Type);
            ArkanoidController.Instance.BonusPool.Return(this);
        }

        public void SetRandomType()
        {
            var values = (BonusType[])Enum.GetValues(typeof(BonusType));
            Type = values[Random.Range(0, values.Length)];

            if (_spriteRenderer == null) return;

            switch (Type)
            {
                case BonusType.MultiBall:
                    _spriteRenderer.color = Color.cyan;
                    break;
                case BonusType.LongPaddle:
                    _spriteRenderer.color = Color.green;
                    break;
                default:
                    _spriteRenderer.color = Color.white;
                    break;
            }
        }
    }
}