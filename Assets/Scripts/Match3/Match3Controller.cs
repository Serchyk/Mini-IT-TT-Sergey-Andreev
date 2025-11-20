using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniIT.CORE;
using MiniIT.CONFIGS;

namespace MiniIT.MATCH3
{
    /// <summary>
    /// Main controller for Match3 gameplay with swipe input, animated swaps and nice refill.
    /// </summary>
    public class Match3Controller : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GameConfigSO _config = null;

        [SerializeField]
        private Camera _mainCamera = null;

        [SerializeField]
        private Transform _boardTransform = null;

        [SerializeField]
        private GameObject _gemPrefab = null;

        [Header("Board settings")]
        public int Width = 8;
        public int Height = 8;
        public int ColorCount = 6;

        [Header("Swipe settings")]
        public float SwipeThreshold = 0.25f; // world units minimum to consider a swipe
        public float SwapDuration = 0.12f; // seconds for swap animation
        public float FallBaseDelay = 0.03f; // per cell fall delay
        public float ColumnSpawnDelay = 0.06f; // stagger spawn per column

        private GridCore<Gem> grid = null;
        private ObjectPool<Gem> gemPool = null;

        // runtime state
        private bool _isBusy = false; // prevents new input while animations occur
        private SwipeInputState _swipeState = new SwipeInputState();
        private Camera _cachedCamera = null;

        private void Awake()
        {
            if (_config != null)
            {
                Width = _config.Match3Width;
                Height = _config.Match3Height;
                ColorCount = _config.Match3ColorCount;
            }

            grid = new GridCore<Gem>(Width, Height);

            gemPool = new ObjectPool<Gem>(() =>
            {
                GameObject go = Instantiate(_gemPrefab, _boardTransform);
                Gem gem = go.GetComponent<Gem>();
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

            StartCoroutine(InitBoardCo());
        }

        /// <summary>
        /// Initialize board without initial matches.
        /// Uses a deterministic fill that avoids creating matches when placing each gem.
        /// </summary>
        private IEnumerator InitBoardCo()
        {
            // fill sequentially from bottom-left to top-right,
            // selecting random color that does not create a match at placement.
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    int color = GetRandomColorAvoidMatch(x, y);
                    Gem gem = SpawnGemAt(x, y, color);

                    // small spawn pop animation
                    gem.transform.localScale = Vector3.zero;
                    StartCoroutine(ScaleOver(gem.transform, Vector3.one, 0.12f));

                    // small stagger for nicer effect
                    yield return new WaitForSeconds(0.01f);
                }
            }

            // short pause then allow input
            yield return new WaitForSeconds(0.08f);
            _isBusy = false;
        }

        /// <summary>
        /// Get a random color id that does not immediately create a match at (x,y).
        /// </summary>
        private int GetRandomColorAvoidMatch(int x, int y)
        {
            List<int> excluded = new List<int>();

            // check two left in a row
            if (x >= 2)
            {
                Gem left1 = grid.Get(x - 1, y);
                Gem left2 = grid.Get(x - 2, y);

                if (left1 != null && left2 != null && left1.ColorId == left2.ColorId)
                {
                    excluded.Add(left1.ColorId);
                }
            }

            // check two down in a column
            if (y >= 2)
            {
                Gem down1 = grid.Get(x, y - 1);
                Gem down2 = grid.Get(x, y - 2);

                if (down1 != null && down2 != null && down1.ColorId == down2.ColorId)
                {
                    excluded.Add(down1.ColorId);
                }
            }

            int attempt = 0;
            int color = 0;

            // Choose a random color not in excluded; if all excluded (rare), pick random
            do
            {
                color = Random.Range(0, ColorCount);
                attempt++;
                if (attempt > 10)
                {
                    break;
                }
            }
            while (excluded.Contains(color));

            return color;
        }

        private int RandomRange(int minInclusive, int maxExclusive)
        {
            return Random.Range(minInclusive, maxExclusive);
        }

        /// <summary>
        /// Spawn gem from pool, set coordinates and world pos.
        /// </summary>
        private Gem SpawnGemAt(int x, int y, int color)
        {
            Gem gem = gemPool.Rent();
            gem.Init(color);
            gem.X = x;
            gem.Y = y;
            grid.Set(x, y, gem);

            Vector3 pos = BoardToWorld(x, y);
            gem.transform.position = pos;
            return gem;
        }

        /// <summary>
        /// Convert board coordinates to world position (board centered at origin).
        /// </summary>
        private Vector3 BoardToWorld(int x, int y)
        {
            float offsetX = -Width / 2.0f + 0.5f;
            float offsetY = -Height / 2.0f + 0.5f;
            return new Vector3(x + offsetX, y + offsetY, 0.0f);
        }

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
        /// Process swipe input (classic Candy Crush style).
        /// Detect press -> drag -> release; perform immediate swap on sufficient drag.
        /// </summary>
        private void ProcessInput()
        {
            // Touch input
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    OnPointerDown(_cachedCamera.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y, _cachedCamera.nearClipPlane)));
                }
                else if (t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary)
                {
                    OnPointerMove(_cachedCamera.ScreenToWorldPoint(new Vector3(t.position.x, t.position.y, _cachedCamera.nearClipPlane)));
                }
                else if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
                {
                    OnPointerUp();
                }

                return;
            }

            // Mouse input
            if (Input.GetMouseButtonDown(0))
            {
                OnPointerDown(_cachedCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _cachedCamera.nearClipPlane)));
            }
            else if (Input.GetMouseButton(0))
            {
                OnPointerMove(_cachedCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _cachedCamera.nearClipPlane)));
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
            if (!_swipeState.IsStarted)
            {
                return;
            }

            if (_swipeState.IsSwipePerformed)
            {
                return;
            }

            Vector3 delta = world - _swipeState.StartWorld;
            if (delta.magnitude < SwipeThreshold)
            {
                return;
            }

            // determine primary direction
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                // horizontal swipe
                if (delta.x > 0)
                {
                    TryPerformSwipe(Direction.Right);
                }
                else
                {
                    TryPerformSwipe(Direction.Left);
                }
            }
            else
            {
                // vertical swipe
                if (delta.y > 0)
                {
                    TryPerformSwipe(Direction.Up);
                }
                else
                {
                    TryPerformSwipe(Direction.Down);
                }
            }

            _swipeState.MarkPerformed();
        }

        private void OnPointerUp()
        {
            _swipeState.Reset();
        }

        /// <summary>
        /// Try to perform swipe in given direction from start cell (if valid).
        /// </summary>
        private void TryPerformSwipe(Direction dir)
        {
            (int sx, int sy) = WorldToBoard(_swipeState.StartWorld);

            if (sx < 0 || sx >= Width || sy < 0 || sy >= Height)
            {
                return;
            }

            int tx = sx;
            int ty = sy;

            switch (dir)
            {
                case Direction.Left:
                    tx = sx - 1;
                    ty = sy;
                    break;
                case Direction.Right:
                    tx = sx + 1;
                    ty = sy;
                    break;
                case Direction.Up:
                    tx = sx;
                    ty = sy + 1;
                    break;
                case Direction.Down:
                    tx = sx;
                    ty = sy - 1;
                    break;
            }

            if (tx < 0 || tx >= Width || ty < 0 || ty >= Height)
            {
                return;
            }

            Gem a = grid.Get(sx, sy);
            Gem b = grid.Get(tx, ty);
            if (a == null || b == null)
            {
                return;
            }

            // perform animated swap and match processing
            StartCoroutine(DoSwapAndProcess(a, b));
        }

        private (int x, int y) WorldToBoard(Vector3 world)
        {
            float offsetX = -Width / 2.0f + 0.5f;
            float offsetY = -Height / 2.0f + 0.5f;
            int x = Mathf.RoundToInt(world.x - offsetX);
            int y = Mathf.RoundToInt(world.y - offsetY);
            return (x, y);
        }

        /// <summary>
        /// Swap two gems with animation, check for matches, handle swap-back if no matches.
        /// </summary>
        private IEnumerator DoSwapAndProcess(Gem a, Gem b)
        {
            if (_isBusy)
            {
                yield break;
            }

            _isBusy = true;

            // cache positions
            int ax = a.X;
            int ay = a.Y;
            int bx = b.X;
            int by = b.Y;

            Vector3 aTarget = BoardToWorld(bx, by);
            Vector3 bTarget = BoardToWorld(ax, ay);

            // start parallel animations
            Coroutine ca = StartCoroutine(a.AnimateMove(aTarget, SwapDuration));
            Coroutine cb = StartCoroutine(b.AnimateMove(bTarget, SwapDuration));
            yield return ca;
            yield return cb;

            // swap in grid and internal coords
            grid.Set(ax, ay, b);
            grid.Set(bx, by, a);

            a.X = bx;
            a.Y = by;
            b.X = ax;
            b.Y = ay;

            // search for matches for both swapped positions
            List<(int x, int y)> ma = MatchFinder.FindMatch(grid, ax, ay);
            List<(int x, int y)> mb = MatchFinder.FindMatch(grid, bx, by);

            if (ma.Count >= 3 || mb.Count >= 3)
            {
                // handle matches and cascades
                HandleMatches(ma);
                HandleMatches(mb);

                yield return StartCoroutine(ProcessGravityAndRefillAnimated());
            }
            else
            {
                // no match => swap back with animation
                Coroutine ca2 = StartCoroutine(a.AnimateMove(BoardToWorld(ax, ay), SwapDuration));
                Coroutine cb2 = StartCoroutine(b.AnimateMove(BoardToWorld(bx, by), SwapDuration));
                yield return ca2;
                yield return cb2;

                // restore grid
                grid.Set(ax, ay, a);
                grid.Set(bx, by, b);

                a.X = ax;
                a.Y = ay;
                b.X = bx;
                b.Y = by;
            }

            _isBusy = false;
        }

        /// <summary>
        /// Handle match list: remove gems and return them to pool.
        /// </summary>
        private void HandleMatches(List<(int x, int y)> match)
        {
            if (match == null)
            {
                return;
            }

            foreach ((int x, int y) in match)
            {
                Gem g = grid.Get(x, y);
                if (g != null)
                {
                    grid.Set(x, y, null);

                    // optional explode VFX can be invoked here

                    gemPool.Return(g);
                    GameSignals.RaiseScoreChanged(1);
                }
            }
        }

        /// <summary>
        /// Animated gravity and refill: gems fall with delays per cell, new gems spawn at top with stagger per column.
        /// </summary>
        private IEnumerator ProcessGravityAndRefillAnimated()
        {
            // Apply gravity per column, animate falling
            for (int x = 0; x < Width; ++x)
            {
                int writeY = 0;
                for (int y = 0; y < Height; ++y)
                {
                    Gem g = grid.Get(x, y);
                    if (g != null)
                    {
                        if (y != writeY)
                        {
                            grid.Set(x, writeY, g);
                            grid.Set(x, y, null);
                            g.Y = writeY;

                            Vector3 target = BoardToWorld(x, writeY);
                            // animate fall with slight delay depending on distance
                            float delay = (y - writeY) * FallBaseDelay;
                            if (delay > 0f)
                            {
                                yield return new WaitForSeconds(delay);
                            }

                            yield return StartCoroutine(g.AnimateMove(target, 0.08f + (y - writeY) * 0.02f));
                        }

                        writeY++;
                    }
                }

                // fill remaining cells in this column from top
                for (int ny = writeY; ny < Height; ++ny)
                {
                    int color = RandomRange(0, ColorCount);
                    Gem newGem = SpawnGemAt(x, ny, color);

                    // spawn above board and drop
                    Vector3 abovePos = BoardToWorld(x, Height + 1);
                    newGem.transform.position = abovePos;

                    Vector3 downTarget = BoardToWorld(x, ny);

                    // small stagger per column for nicer effect
                    yield return new WaitForSeconds(ColumnSpawnDelay * x);

                    // animate drop
                    yield return StartCoroutine(newGem.AnimateMove(downTarget, 0.12f + (Height - ny) * 0.02f));
                }
            }

            // brief pause to allow cascades to be detected visually
            yield return new WaitForSeconds(0.05f);

            // After refill, check for new matches (cascades)
            bool foundCascade = false;
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    List<(int x, int y)> m = MatchFinder.FindMatch(grid, x, y);
                    if (m.Count >= 3)
                    {
                        foundCascade = true;
                        HandleMatches(m);
                    }
                }
            }

            if (foundCascade)
            {
                // process next cascade
                yield return StartCoroutine(ProcessGravityAndRefillAnimated());
            }
        }

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
                float f = Mathf.Clamp01(elapsed / duration);
                t.localScale = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, f));
                yield return null;
            }

            t.localScale = target;
        }

        #region Nested types

        /// <summary>
        /// Simple swipe input state.
        /// </summary>
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
