using UnityEngine;
using UnityEngine.SceneManagement;
using MiniIT.CONFIGS;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Central game manager controlling ball, bricks and bonuses.
    /// </summary>
    public class ArkanoidController : MonoBehaviour
    {
        public static ArkanoidController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameConfigSO _config = null;
        [SerializeField] private GameObject _ballPrefab = null;
        [SerializeField] private Transform _ballSpawn = null;
        [SerializeField] private LevelInitializer _level = null;
        [SerializeField] private PaddleController _paddle = null;

        private int _remainingBricks = 0;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            InitLevel();
            SpawnBall();
        }

        private void InitLevel()
        {
            _remainingBricks = _level.BuildLevel(_config.BricksRows, _config.BricksCols);
            Brick.OnBrickDestroyed += HandleBrickDestroyed;

            // place paddle automatically
            _paddle.PlaceAtScreenBottom();
        }

        private void SpawnBall()
        {
            var obj = Instantiate(_ballPrefab, _ballSpawn.position, Quaternion.identity);
            var ball = obj.GetComponent<BallController>();
            ball.Init(_config.BallSpeed);
            // ball.Launch(Vector2.up + Vector2.right * 0.25f);
        }

        private void HandleBrickDestroyed()
        {
            _remainingBricks--;
            if (_remainingBricks <= 0)
                Win();
        }

        public void Win()
        {
            Restart();
        }

        public void Lose()
        {
            Restart();
        }

        private void Restart()
        {
            Brick.OnBrickDestroyed -= HandleBrickDestroyed;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        // ----------------------------
        // BONUS HANDLING
        // ----------------------------

        public void ApplyBonus(BonusType type)
        {
            switch (type)
            {
                case BonusType.MultiBall:
                    ApplyMultiBall();
                    break;
                case BonusType.LongPaddle:
                    ApplyLongPaddle();
                    break;
            }
        }

        private void ApplyMultiBall()
        {
            BallController[] balls = FindObjectsOfType<BallController>();

            foreach (var b in balls)
            {
                var clone = Instantiate(_ballPrefab, b.transform.position, Quaternion.identity);
                var cBall = clone.GetComponent<BallController>();
                cBall.Init(_config.BallSpeed);
            }
        }

        private void ApplyLongPaddle()
        {
            _paddle.ApplyTemporaryScale(1.5f, 6f); // scale ×1.5 for 6 seconds
        }
    }
}
