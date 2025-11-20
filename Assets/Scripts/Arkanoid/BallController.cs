using UnityEngine;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Controls ball movement and enforces world boundaries.
    /// Ball bounces from walls and triggers lose condition when leaving bottom.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class BallController : MonoBehaviour
    {
        [Header("Ball Setup")]
        public float Speed = 8f;

        [Header("References")]
        public Camera Cam;

        private Rigidbody2D _rb;
        private float _radius;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _radius = GetComponent<CircleCollider2D>().radius;

            if (Cam == null)
            {
                Cam = Camera.main;
            }
        }

        private void Start()
        {
            // Initial launch
            _rb.velocity = new Vector2(0.4f, 1f).normalized * Speed;
        }

        public void Init(float speed)
        {
            Speed = speed;
        }

        private void FixedUpdate()
        {
            EnforceBoundaries();
        }

        /// <summary>
        /// Keeps ball inside screen area. Bounces from walls and detects lose.
        /// </summary>
        private void EnforceBoundaries()
        {
            Vector3 pos = transform.position;

            float left = Cam.ScreenToWorldPoint(Vector3.zero).x + _radius;
            float right = Cam.ScreenToWorldPoint(new Vector3(Screen.width, 0)).x - _radius;
            float top = Cam.ScreenToWorldPoint(new Vector3(0, Screen.height)).y - _radius;
            float bottom = Cam.ScreenToWorldPoint(Vector3.zero).y - _radius;

            Vector2 velocity = _rb.velocity;

            // Horizontal walls
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

            // Top wall
            if (pos.y > top)
            {
                pos.y = top;
                velocity.y = -Mathf.Abs(velocity.y);
            }

            // Bottom = check if last ball
            if (pos.y < bottom)
            {
                HandleBallLost();
                return;
            }

            transform.position = pos;
            _rb.velocity = velocity;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (!other.collider.CompareTag("Paddle"))
            {
                return;
            }

            float hitX = transform.position.x - other.transform.position.x;
            Vector2 dir = new Vector2(hitX, 1f).normalized;
            _rb.velocity = dir * Speed;
        }

        /// <summary>
        /// Called when this ball falls below the screen.
        /// Checks if it was the last ball to trigger lose.
        /// </summary>
        private void HandleBallLost()
        {
            // Deactivate this ball
            gameObject.SetActive(false);

            // Count remaining active balls
            BallController[] balls = FindObjectsOfType<BallController>();

            bool anyActive = false;

            foreach (BallController ball in balls)
            {
                if (ball.gameObject.activeInHierarchy)
                {
                    anyActive = true;
                    break;
                }
            }

            // If no balls left, player loses
            if (!anyActive)
            {
                ArkanoidController.Instance.Lose();
            }
        }
    }
}
