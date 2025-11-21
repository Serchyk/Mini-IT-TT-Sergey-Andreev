using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniIT.CORE;
using MiniIT.CONFIGS;
using TMPro;

namespace MiniIT.MATCH3
{
    /// <summary>
    /// Main controller for Match3 gameplay with swipe input, animated swaps and nice refill.
    /// </summary>
    public class Match3Controller : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameConfigSO _config = null;
        [SerializeField] private Camera _mainCamera = null;
        [SerializeField] private Transform _boardTransform = null;
        [SerializeField] private GameObject _gemPrefab = null;

        [Header("Board settings")]
        public int Width = 8;
        public int Height = 8;
        public int ColorCount = 6;

        [Header("Swipe settings")]
        public float SwipeThreshold = 0.25f;
        public float SwapDuration = 0.12f;
        public float FallBaseDelay = 0.03f;
        public float ColumnSpawnDelay = 0.06f;
        
        [Header("UI References")]
        [SerializeField] public TextMeshProUGUI _scoreText = null;

        private GridCore<Gem> _grid = null;
        private ObjectPool<Gem> _gemPool = null;

        // runtime state
        private bool _isBusy = false;
        private SwipeInputState _swipeState = new SwipeInputState();
        private Camera _cachedCamera = null;

        // Board offset calculation
        private float _boardOffsetX = 0f;
        private float _boardOffsetY = 0f;

        #region Unity Lifecycle

        private void Awake()
        {
            if (_config != null)
            {
                Width = _config.Match3Width;
                Height = _config.Match3Height;
                ColorCount = _config.Match3ColorCount;
            }

            _grid = new GridCore<Gem>(Width, Height);
            CalculateBoardOffsets();

            _gemPool = new ObjectPool<Gem>(() =>
            {
                GameObject go = Instantiate(_gemPrefab, _boardTransform);
                Gem gem = go.GetComponent<Gem>();
                if (gem == null)
                {
                    gem = go.AddComponent<Gem>();
                }
                go.name = "Gem";
                return gem;
            }, Width * Height);

            _cachedCamera = (_mainCamera != null) ? _mainCamera : Camera.main;
        }

        private void Start()
        {
            if (_cachedCamera == null)
            {
                _cachedCamera = Camera.main;
            }
            
            ScoreManager.Instance.scoreText = _scoreText;
            ScoreManager.Instance.ResetScore();

            StartCoroutine(InitBoardCo());
        }

        #endregion

        #region Board Initialization

        /// <summary>
        /// Calculate board offsets to center the grid properly.
        /// </summary>
        private void CalculateBoardOffsets()
        {
            _boardOffsetX = -Width * 0.5f + 0.5f;
            _boardOffsetY = -Height * 0.5f + 0.5f;
        }

        /// <summary>
        /// Initialize board without initial matches.
        /// </summary>
        private IEnumerator InitBoardCo()
        {
            _isBusy = true;

            // Clear any existing gems
            ClearBoard();

            // Fill board sequentially
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    int spriteId = GetRandomColorAvoidMatch(x, y);
                    Gem gem = SpawnGemAt(x, y, spriteId);

                    // Ensure gem is properly positioned immediately
                    Vector3 targetPos = BoardToWorld(x, y);
                    gem.transform.position = targetPos;

                    // Small spawn pop animation
                    gem.transform.localScale = Vector3.zero;
                    StartCoroutine(ScaleOver(gem.transform, Vector3.one, 0.12f));

                    // Small stagger for nicer effect
                    yield return new WaitForSeconds(0.01f);
                }
            }

            // Verify all gems are properly placed
            yield return StartCoroutine(VerifyBoardPlacement());

            // Short pause then allow input
            yield return new WaitForSeconds(0.08f);
            _isBusy = false;
        }

        /// <summary>
        /// Verify that all gems are properly placed on the board.
        /// </summary>
        private IEnumerator VerifyBoardPlacement()
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Gem gem = _grid.Get(x, y);
                    if (gem != null)
                    {
                        Vector3 expectedPos = BoardToWorld(x, y);
                        if (Vector3.Distance(gem.transform.position, expectedPos) > 0.1f)
                        {
                            gem.transform.position = expectedPos;
                        }

                        // Ensure coordinates are correct
                        if (gem.X != x || gem.Y != y)
                        {
                            gem.X = x;
                            gem.Y = y;
                        }
                    }
                }
            }
            yield return null;
        }

        /// <summary>
        /// Clear the board and return all gems to pool.
        /// </summary>
        private void ClearBoard()
        {
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    Gem gem = _grid.Get(x, y);
                    if (gem != null)
                    {
                        _grid.Set(x, y, null);
                        _gemPool.Return(gem);
                    }
                }
            }
        }

        #endregion

        #region Gem Management

        /// <summary>
        /// Get a random color id that does not immediately create a match at (x,y).
        /// </summary>
        private int GetRandomColorAvoidMatch(int x, int y)
        {
            List<int> excludedColors = new List<int>();

            // Check horizontal matches (two left in a row)
            if (x >= 2)
            {
                Gem left1 = _grid.Get(x - 1, y);
                Gem left2 = _grid.Get(x - 2, y);

                if (left1 != null && left2 != null && left1.SpriteId == left2.SpriteId)
                {
                    excludedColors.Add(left1.SpriteId);
                }
            }

            // Check vertical matches (two down in a column)
            if (y >= 2)
            {
                Gem down1 = _grid.Get(x, y - 1);
                Gem down2 = _grid.Get(x, y - 2);

                if (down1 != null && down2 != null && down1.SpriteId == down2.SpriteId)
                {
                    excludedColors.Add(down1.SpriteId);
                }
            }

            // Choose a random color not in excluded list
            List<int> availableColors = new List<int>();
            for (int i = 0; i < ColorCount; i++)
            {
                if (!excludedColors.Contains(i))
                {
                    availableColors.Add(i);
                }
            }

            if (availableColors.Count > 0)
            {
                return availableColors[Random.Range(0, availableColors.Count)];
            }

            // Fallback: return random color if all are excluded
            return Random.Range(0, ColorCount);
        }

        /// <summary>
        /// Spawn gem from pool, set coordinates and world position.
        /// </summary>
        private Gem SpawnGemAt(int x, int y, int color)
        {
            Gem gem = _gemPool.Rent();
            if (gem == null)
            {
                Debug.LogError("Failed to spawn gem from pool!");
                return null;
            }

            gem.Init(color);
            gem.X = x;
            gem.Y = y;
            gem.gameObject.SetActive(true);

            // Set position immediately
            Vector3 worldPos = BoardToWorld(x, y);
            gem.transform.position = worldPos;

            _grid.Set(x, y, gem);

            return gem;
        }

        /// <summary>
        /// Convert board coordinates to world position (board centered at origin).
        /// </summary>
        private Vector3 BoardToWorld(int x, int y)
        {
            return new Vector3(x + _boardOffsetX, y + _boardOffsetY, 0.0f);
        }

        /// <summary>
        /// Convert world position to board coordinates.
        /// </summary>
        private (int x, int y) WorldToBoard(Vector3 world)
        {
            int x = Mathf.RoundToInt(world.x - _boardOffsetX);
            int y = Mathf.RoundToInt(world.y - _boardOffsetY);
            return (x, y);
        }

        #endregion

        #region Input Handling

        private void Update()
        {
            if (_cachedCamera == null)
            {
                _cachedCamera = Camera.main;
            }

            if (_isBusy)
            {
                return;
            }

            ProcessInput();
        }

        /// <summary>
        /// Process swipe input.
        /// </summary>
        private void ProcessInput()
        {
            // Touch input
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                Vector3 worldPos = _cachedCamera.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y, -_cachedCamera.transform.position.z));

                switch (t.phase)
                {
                    case TouchPhase.Began:
                        OnPointerDown(worldPos);
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        OnPointerMove(worldPos);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        OnPointerUp();
                        break;
                }
                return;
            }

            // Mouse input
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPos = _cachedCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -_cachedCamera.transform.position.z));
                OnPointerDown(worldPos);
            }
            else if (Input.GetMouseButton(0))
            {
                Vector3 worldPos = _cachedCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -_cachedCamera.transform.position.z));
                OnPointerMove(worldPos);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnPointerUp();
            }
        }

        private void OnPointerDown(Vector3 world)
        {
            _swipeState.Start(world);
        }

        private void OnPointerMove(Vector3 world)
        {
            if (!_swipeState.IsStarted || _swipeState.IsSwipePerformed)
            {
                return;
            }

            Vector3 delta = world - _swipeState.StartWorld;
            if (delta.magnitude < SwipeThreshold)
            {
                return;
            }

            // Determine primary direction
            Direction swipeDirection = GetSwipeDirection(delta);
            TryPerformSwipe(swipeDirection);
            _swipeState.MarkPerformed();
        }

        private void OnPointerUp()
        {
            _swipeState.Reset();
        }

        private Direction GetSwipeDirection(Vector3 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? Direction.Right : Direction.Left;
            }
            else
            {
                return delta.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        #endregion

        #region Game Logic

        /// <summary>
        /// Try to perform swipe in given direction from start cell.
        /// </summary>
        private void TryPerformSwipe(Direction dir)
        {
            (int startX, int startY) = WorldToBoard(_swipeState.StartWorld);

            if (!IsValidPosition(startX, startY))
            {
                return;
            }

            (int targetX, int targetY) = GetTargetPosition(startX, startY, dir);

            if (!IsValidPosition(targetX, targetY))
            {
                return;
            }

            Gem gemA = _grid.Get(startX, startY);
            Gem gemB = _grid.Get(targetX, targetY);

            if (gemA == null || gemB == null)
            {
                return;
            }

            StartCoroutine(DoSwapAndProcess(gemA, gemB));
        }

        private (int x, int y) GetTargetPosition(int x, int y, Direction dir)
        {
            return dir switch
            {
                Direction.Left => (x - 1, y),
                Direction.Right => (x + 1, y),
                Direction.Up => (x, y + 1),
                Direction.Down => (x, y - 1),
                _ => (x, y)
            };
        }

        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        /// <summary>
        /// Swap two gems with animation, check for matches.
        /// </summary>
        private IEnumerator DoSwapAndProcess(Gem a, Gem b)
        {
            if (_isBusy)
            {
                yield break;
            }

            _isBusy = true;

            // Cache original positions
            int aX = a.X, aY = a.Y;
            int bX = b.X, bY = b.Y;

            // Swap positions in world space
            Vector3 aTargetPos = BoardToWorld(bX, bY);
            Vector3 bTargetPos = BoardToWorld(aX, aY);

            // Animate swap
            yield return StartCoroutine(AnimateSwap(a, aTargetPos, b, bTargetPos));

            // Update grid and coordinates
            _grid.Set(aX, aY, b);
            _grid.Set(bX, bY, a);
            a.X = bX; a.Y = bY;
            b.X = aX; b.Y = aY;

            // Check for matches
            List<(int x, int y)> matchesA = MatchFinder.FindMatch(_grid, aX, aY);
            List<(int x, int y)> matchesB = MatchFinder.FindMatch(_grid, bX, bY);

            bool hasMatch = matchesA.Count >= 3 || matchesB.Count >= 3;

            if (hasMatch)
            {
                HandleMatches(matchesA);
                HandleMatches(matchesB);
                yield return StartCoroutine(ProcessGravityAndRefillAnimated());
            }
            else
            {
                // No match - swap back
                yield return StartCoroutine(AnimateSwap(a, BoardToWorld(aX, aY), b, BoardToWorld(bX, bY)));

                // Restore original grid state
                _grid.Set(aX, aY, a);
                _grid.Set(bX, bY, b);
                a.X = aX; a.Y = aY;
                b.X = bX; b.Y = bY;
            }

            _isBusy = false;
        }

        /// <summary>
        /// Animate swapping of two gems.
        /// </summary>
        private IEnumerator AnimateSwap(Gem gemA, Vector3 targetA, Gem gemB, Vector3 targetB)
        {
            Coroutine moveA = StartCoroutine(gemA.AnimateMove(targetA, SwapDuration));
            Coroutine moveB = StartCoroutine(gemB.AnimateMove(targetB, SwapDuration));
            yield return moveA;
            yield return moveB;
        }

        /// <summary>
        /// Handle match list: remove gems and return them to pool.
        /// </summary>
        private void HandleMatches(List<(int x, int y)> match)
        {
            if (match == null || match.Count < 3)
            {
                return;
            }

            foreach ((int x, int y) in match)
            {
                Gem gem = _grid.Get(x, y);
                if (gem != null)
                {
                    _grid.Set(x, y, null);
                    _gemPool.Return(gem);
                    ScoreManager.Instance.AddScore(1);
                }
            }
        }

        /// <summary>
        /// Animated gravity and refill.
        /// </summary>
        private IEnumerator ProcessGravityAndRefillAnimated()
        {
            // Apply gravity
            for (int x = 0; x < Width; ++x)
            {
                int writeY = 0;
                for (int readY = 0; readY < Height; ++readY)
                {
                    Gem gem = _grid.Get(x, readY);
                    if (gem != null)
                    {
                        if (readY != writeY)
                        {
                            // Move gem down
                            _grid.Set(x, writeY, gem);
                            _grid.Set(x, readY, null);
                            gem.Y = writeY;

                            Vector3 targetPos = BoardToWorld(x, writeY);
                            float delay = (readY - writeY) * FallBaseDelay;
                            
                            if (delay > 0f)
                            {
                                yield return new WaitForSeconds(delay);
                            }

                            yield return StartCoroutine(gem.AnimateMove(targetPos, 0.04f + (readY - writeY) * 0.02f));
                        }
                        writeY++;
                    }
                }

                // Spawn new gems for empty slots
                for (int newY = writeY; newY < Height; ++newY)
                {
                    yield return new WaitForSeconds(ColumnSpawnDelay);

                    int color = Random.Range(0, ColorCount);
                    Gem newGem = SpawnGemAt(x, newY, color);

                    // Spawn above and drop down
                    Vector3 spawnPos = BoardToWorld(x, Height + (newY - writeY) + 1);
                    newGem.transform.position = spawnPos;

                    Vector3 targetPos = BoardToWorld(x, newY);
                    yield return StartCoroutine(newGem.AnimateMove(targetPos, 0.12f + (Height - newY) * 0.02f));
                }
            }

            // Check for cascades
            yield return new WaitForSeconds(0.05f);

            bool foundCascade = false;
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    List<(int x, int y)> match = MatchFinder.FindMatch(_grid, x, y);
                    if (match.Count >= 3)
                    {
                        foundCascade = true;
                        HandleMatches(match);
                    }
                }
            }

            if (foundCascade)
            {
                yield return StartCoroutine(ProcessGravityAndRefillAnimated());
            }
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Smoothly scale transform from current to target over duration.
        /// </summary>
        private IEnumerator ScaleOver(Transform t, Vector3 target, float duration)
        {
            Vector3 start = t.localScale;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);
                t.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, progress));
                yield return null;
            }

            t.localScale = target;
        }

        #endregion

        #region Nested Types

        private class SwipeInputState
        {
            public Vector3 StartWorld { get; private set; } = Vector3.zero;
            public bool IsStarted { get; private set; } = false;
            public bool IsSwipePerformed { get; private set; } = false;

            public void Start(Vector3 world)
            {
                StartWorld = world;
                IsStarted = true;
                IsSwipePerformed = false;
            }

            public void MarkPerformed()
            {
                IsSwipePerformed = true;
            }

            public void Reset()
            {
                StartWorld = Vector3.zero;
                IsStarted = false;
                IsSwipePerformed = false;
            }
        }

        private enum Direction
        {
            Left,
            Right,
            Up,
            Down,
        }

        #endregion
    }
}
