using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniIT.CORE;
using MiniIT.CONFIGS;

namespace MiniIT.MERGE
{
    /// <summary>
    /// Main controller for Merge gameplay with drag & drop and animated spawn.
    /// </summary>
    public class MergeController : MonoBehaviour
    {
        [SerializeField] private GameConfigSO _config = null;
        [SerializeField] private Camera _mainCamera = null;
        [SerializeField] private Transform _boardTransform = null;
        [SerializeField] private GameObject _piecePrefab = null;

        public int Width = 5;
        public int Height = 6;
        public float SpawnInterval = 3.0f;

        private GridCore<MergePiece> grid = null;
        private ObjectPool<MergePiece> pool = null;

        private MergePiece _draggingPiece = null;
        private Vector3 _dragOffset;

        private void Awake()
        {
            if (_config != null)
            {
                Width = _config.MergeWidth;
                Height = _config.MergeHeight;
                SpawnInterval = _config.MergeSpawnInterval;
            }

            grid = new GridCore<MergePiece>(Width, Height);

            pool = new ObjectPool<MergePiece>(() =>
            {
                GameObject go = Instantiate(_piecePrefab, _boardTransform);
                MergePiece p = go.GetComponent<MergePiece>();
                go.name = "MergePiece";
                go.SetActive(false);
                return p;
            }, Width * Height / 2);
        }

        private void Start()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            StartCoroutine(SpawnLoop());
        }

        /// <summary>
        /// Spawn pieces periodically.
        /// </summary>
        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(SpawnInterval);
                SpawnRandomLevel1Animated();
            }
        }

        /// <summary>
        /// Spawn a level-1 piece in a random free cell with animation.
        /// </summary>
        private void SpawnRandomLevel1Animated()
        {
            List<(int x, int y)> free = new List<(int, int)>();
            for (int x = 0; x < Width; ++x)
            {
                for (int y = 0; y < Height; ++y)
                {
                    if (grid.Get(x, y) == null)
                    {
                        free.Add((x, y));
                    }
                }
            }

            if (free.Count == 0) return;

            int idx = Random.Range(0, free.Count);
            (int fx, int fy) = free[idx];
            StartCoroutine(SpawnPieceAnimated(fx, fy, 1));
        }

        /// <summary>
        /// Spawn a piece off-screen and animate it to its target cell.
        /// </summary>
        private IEnumerator SpawnPieceAnimated(int x, int y, int level)
        {
            MergePiece p = pool.Rent();
            p.Init(level);
            p.X = x;
            p.Y = y;
            grid.Set(x, y, p);
            p.gameObject.SetActive(true);

            Vector3 startPos = BoardToWorld(x, Height + 2);
            Vector3 targetPos = BoardToWorld(x, y);

            p.transform.position = startPos;

            float duration = 0.4f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                t = Mathf.SmoothStep(0f, 1f, t);
                p.transform.position = Vector3.Lerp(startPos, targetPos, t);
                yield return null;
            }

            p.transform.position = targetPos;
        }

        private Vector3 BoardToWorld(int x, int y)
        {
            float offsetX = -Width / 2.0f + 0.5f;
            float offsetY = -Height / 2.0f + 0.5f;
            return new Vector3(x + offsetX, y + offsetY, 0f);
        }

        private (int x, int y) WorldToBoard(Vector3 world)
        {
            float offsetX = -Width / 2.0f + 0.5f;
            float offsetY = -Height / 2.0f + 0.5f;
            int x = Mathf.RoundToInt(world.x - offsetX);
            int y = Mathf.RoundToInt(world.y - offsetY);
            return (x, y);
        }

        private void Update()
        {
            ProcessDrag();
        }

        /// <summary>
        /// Handles drag & drop logic for merge pieces.
        /// </summary>
        private void ProcessDrag()
        {
            Vector3 pointerWorld = InputAdapter.GetPointerWorldPosition(_mainCamera);

            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                (int gx, int gy) = WorldToBoard(pointerWorld);
                MergePiece clicked = grid.Get(gx, gy);
                if (clicked != null)
                {
                    _draggingPiece = clicked;
                    _dragOffset = _draggingPiece.transform.position - pointerWorld;
                }
            }

            if (_draggingPiece != null)
            {
                _draggingPiece.transform.position = pointerWorld + _dragOffset;

                if (Input.GetMouseButtonUp(0) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
                {
                    (int gx, int gy) = WorldToBoard(_draggingPiece.transform.position);
                    MergePiece target = grid.Get(gx, gy);

                    if (target != null && target != _draggingPiece && target.Level == _draggingPiece.Level)
                    {
                        MergePieces(_draggingPiece, target);
                    }
                    else
                    {
                        _draggingPiece.transform.position = BoardToWorld(_draggingPiece.X, _draggingPiece.Y);
                    }

                    _draggingPiece = null;
                }
            }
        }

        /// <summary>
        /// Merge two pieces into a higher level piece.
        /// </summary>
        private void MergePieces(MergePiece a, MergePiece b)
        {
            int targetLevel = a.Level + 1;
            int tx = b.X;
            int ty = b.Y;

            grid.Set(a.X, a.Y, null);
            grid.Set(b.X, b.Y, null);

            pool.Return(a);
            pool.Return(b);

            StartCoroutine(SpawnPieceAnimated(tx, ty, targetLevel));

            GameSignals.RaiseScoreChanged(1);
        }
    }
}
