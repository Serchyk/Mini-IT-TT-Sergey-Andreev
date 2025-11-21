using System;
using MiniIT.CORE;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Handles physics, collision response and life‑cycle of a ball.
    /// Implements <see cref="IPoolable"/> so it can be reused from the pool.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BallController : MonoBehaviour, IPoolable
    {
        [Header("Ball Setup")] public float Speed = 8f;

        [Header("References")] public Camera Cam;
        
        // Event for when ball is lost
        public event Action<BallController> OnBallLost;

        private Rigidbody2D _rb;
        private float _radius;
        private bool _isActive = false;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _radius = GetComponent<CircleCollider2D>().radius;
            if (Cam == null) Cam = Camera.main;
        }

        public void Init(float speed)
        {
            Speed = speed;
            LaunchRandomDirection();
            _isActive = true;
        }

        private void FixedUpdate() => EnforceBoundaries();

        #region IPoolable

        public void OnSpawned()
        {
            gameObject.SetActive(true);
            _rb.velocity = Vector2.zero; // reset velocity
            _isActive = true;
        }

        public void OnDespawned()
        {
            gameObject.SetActive(false);
            _rb.velocity = Vector2.zero;
            _isActive = false;
            
            OnBallLost = null;
        }

        #endregion

        #region Collision & physics

        private void LaunchRandomDirection() =>
            _rb.velocity = new Vector2(Random.value, 1f).normalized * Speed;

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.collider.CompareTag("Paddle")) return;

            var hitX = transform.position.x - other.transform.position.x;
            var dir = new Vector2(hitX, 1f).normalized;
            _rb.velocity = dir * Speed;
        }

        #endregion

        #region Boundary enforcement

        private void EnforceBoundaries()
        {
            var pos = transform.position;
            var left = Cam.ScreenToWorldPoint(Vector3.zero).x + _radius;
            var right = Cam.ScreenToWorldPoint(new Vector3(Screen.width, 0)).x - _radius;
            var top = Cam.ScreenToWorldPoint(new Vector3(0, Screen.height)).y - _radius;
            var bottom = Cam.ScreenToWorldPoint(Vector3.zero).y - _radius;

            var velocity = _rb.velocity;

            if (pos.x < left)
            {
                pos.x = left;
                velocity.x = Mathf.Abs(velocity.x);
            }
            else if (pos.x > right)
            {
                pos.x = right;
                velocity.x = -Mathf.Abs(velocity.x);
            }

            if (pos.y > top)
            {
                pos.y = top;
                velocity.y = -Mathf.Abs(velocity.y);
            }

            if (pos.y < bottom)
            {
                HandleBallLost();
                return;
            }

            transform.position = pos;
            _rb.velocity = velocity;
        }

        private void HandleBallLost()
        {
            if (!_isActive) return;

            _isActive = false;

            // Notify controller about ball loss
            OnBallLost?.Invoke(this);

            // Deactivate immediately
            gameObject.SetActive(false);
        }

        #endregion
    }
}
