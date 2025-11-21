using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using MiniIT.CONFIGS;
using MiniIT.CORE;
using TMPro;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Singleton that orchestrates the entire game flow.
    /// Responsible for level setup, ball spawning, bonus handling and win/lose logic.
    /// </summary>
    public class ArkanoidController : MonoBehaviour
    {
        public static ArkanoidController Instance { get; private set; }

        [Header("References")]
        [SerializeField] private GameConfigSO _config = null;
        [SerializeField] private Transform _ballSpawn = null;
        [SerializeField] private LevelInitializer _level = null;
        [SerializeField] private PaddleController _paddle = null;

        [Header("Pool Prefabs")]
        [SerializeField] private GameObject _ballPrefab = null;
        [SerializeField] private GameObject _bonusPrefab = null;
        
        [Header("UI References")]
        [SerializeField] public TextMeshProUGUI _scoreText = null;

        // Pool instances (initialised in Awake)
        public ObjectPool<BallController> BallPool { get; private set; }
        public ObjectPool<BonusItem> BonusPool { get; private set; }

        private int _remainingBricks = 0;
        private readonly HashSet<BallController> _activeBalls = new HashSet<BallController>();
        private bool _isGameOver = false;

        #region Properties

        public int ActiveBallCount => _activeBalls.Count;
        public bool IsGameOver => _isGameOver;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            InitialisePools();
        }

        private void Start()
        {
            InitGame();
        }

        private void OnDestroy()
        {
            Brick.OnBrickDestroyed -= HandleBrickDestroyed;
        }

        #endregion

        #region Game Flow

        /// <summary>Initialises the level, registers callbacks and spawns the first ball.</summary>
        private void InitGame()
        {
            _isGameOver = false;
            _activeBalls.Clear();

            ScoreManager.Instance.scoreText = _scoreText;
            ScoreManager.Instance.ResetScore();

            // Build level
            _remainingBricks = _level.BuildLevel(_config.BricksRows, _config.BricksCols);
            
            // Subscribe to events
            Brick.OnBrickDestroyed += HandleBrickDestroyed;
            
            // Setup paddle and ball
            _paddle.PlaceAtScreenBottom();
            SpawnBall();
        }

        /// <summary>Return to the first scene (used by both win and lose).</summary>
        private void Restart()
        {
            if (_isGameOver) return;

            _isGameOver = true;
            Brick.OnBrickDestroyed -= HandleBrickDestroyed;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle brick destruction - add score and check win condition.
        /// </summary>
        private void HandleBrickDestroyed(Brick destroyedBrick)
        {
            if (destroyedBrick != null)
            {
                // Add the brick's score value to current score
                ScoreManager.Instance.AddScore(destroyedBrick.ScoreValue);
            }

            _remainingBricks--;

            if (_remainingBricks <= 0)
            {
                Win();
            }
        }

        #endregion

        #region Ball & Bonus Handling

        /// <summary>
        /// Handle ball lost - only lose game when last ball is lost.
        /// </summary>
        private void HandleBallLost(BallController lostBall)
        {
            if (_isGameOver || lostBall == null)
            {
                return;
            }

            // Remove ball from active set
            _activeBalls.Remove(lostBall);
            lostBall.OnBallLost -= HandleBallLost;

            // Return ball to pool
            if (BallPool != null)
            {
                BallPool.Return(lostBall);
            }


            // Check if this was the last ball
            if (_activeBalls.Count == 0)
            {
                Lose();
            }
        }
        
        private void SpawnBall()
        {
            BallController ball = BallPool.Rent();
            if (ball != null)
            {
                SetupBall(ball);
            }
        }

        private void SetupBall(BallController ball)
        {
            if (ball == null) return;

            ball.transform.position = _ballSpawn.position;
            ball.Init(_config.BallSpeed);
            ball.gameObject.SetActive(true);

            // Subscribe to ball lost event
            ball.OnBallLost += HandleBallLost;
            
            // Add to active balls
            _activeBalls.Add(ball);
        }

        public void ApplyBonus(BonusType type)
        {
            switch (type)
            {
                case BonusType.MultiBall:
                    ApplyMultiBallBonus();
                    break;
                    
                case BonusType.LongPaddle:
                    _paddle.ApplyTemporaryScale(1.5f, 6f);
                    break;
            }
        }

        private void ApplyMultiBallBonus()
        {
            // Get current active balls and spawn additional ones
            var currentBalls = _activeBalls.ToArray();
            
            foreach (var ball in currentBalls)
            {
                if (ball != null && ball.gameObject.activeInHierarchy)
                {
                    // Spawn 1 additional ball per existing ball
                    BallController newBall = BallPool.Rent();
                    if (newBall != null)
                    {
                        newBall.transform.position = ball.transform.position + Vector3.right * 0.5f;
                        SetupBall(newBall);
                    }
                }
            }
        }

        public void Win()
        {
            // Add bonus for completing level
            ScoreManager.Instance.AddScore(1000);
            Restart();
        }

        public void Lose()
        {
            Restart();
        }

        #endregion

        #region Helper Methods

        /// <summary>Initialise object pools with prefab factories.</summary>
        private void InitialisePools()
        {
            BallPool = new ObjectPool<BallController>(
                factory: () => CreatePooled<BallController>(_ballPrefab),
                initial: 5
            );

            BonusPool = new ObjectPool<BonusItem>(
                factory: () => CreatePooled<BonusItem>(_bonusPrefab),
                initial: 10
            );
        }

        private T CreatePooled<T>(GameObject prefab) where T : Component
        {
            if (prefab == null)
            {
                Debug.LogError("Prefab is null for pool creation!");
                return null;
            }

            var obj = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            obj.SetActive(false); // objects start inactive
            return obj.GetComponent<T>();
        }

        #endregion
    }
}