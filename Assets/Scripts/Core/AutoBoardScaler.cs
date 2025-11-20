using UnityEngine;

namespace MiniIT.CORE
{
    /// <summary>
    /// Universal autoscaling for PC + mobile.
    /// Supports 2 modes:
    /// 1) Grid mode (Match-3, Merge)
    /// 2) Free bounds mode (Arkanoid)
    /// </summary>
    public class AutoBoardScaler : MonoBehaviour
    {
        public enum ScaleMode
        {
            Grid,
            FreeBounds
        }

        [Header("General Settings")]
        [SerializeField] private ScaleMode _mode = ScaleMode.Grid;
        [SerializeField] private Camera _camera = null;
        [SerializeField, Range(0.5f, 1f)]
        private float _padding = 0.95f;


        // ========== GRID MODE ==========
        [Header("Grid Mode Settings")]
        [SerializeField] private int _gridWidth = 0;
        [SerializeField] private int _gridHeight = 0;
        [SerializeField] private float _baseCellSize = 1f;
        [SerializeField] private Transform _cellContainer = null;


        // ========== FREE BOUNDS MODE ==========
        [Header("Free Bounds Mode Settings")]
        [SerializeField] private Vector2 _minBounds = new Vector2(-7f, -4f);
        [SerializeField] private Vector2 _maxBounds = new Vector2(7f, 5f);
        [SerializeField] private Transform _rootToScale = null;


        private void Start()
        {
            if (_camera == null)
                _camera = Camera.main;

            ApplyScaling();
        }

        /// <summary>
        /// Chooses scaling strategy based on mode.
        /// </summary>
        public void ApplyScaling()
        {
            switch (_mode)
            {
                case ScaleMode.Grid:
                    ScaleGrid();
                    break;

                case ScaleMode.FreeBounds:
                    ScaleFreeBounds();
                    break;
            }
        }


        // =======================================================
        // =                     GRID MODE                       =
        // =======================================================
        private void ScaleGrid()
        {
            if (_gridWidth <= 0 || _gridHeight <= 0)
                return;

            float aspect = (float)Screen.width / Screen.height;

            float boardW = _gridWidth * _baseCellSize / _padding;
            float boardH = _gridHeight * _baseCellSize / _padding;

            float orthoH = boardH * 0.5f;
            float orthoW = (boardW / aspect) * 0.5f;

            _camera.orthographicSize = Mathf.Max(orthoW, orthoH);

            // calculate final visible S/H
            float worldHeight = _camera.orthographicSize * 2f;
            float worldWidth = worldHeight * aspect;

            float cellSizeX = (worldWidth * _padding) / _gridWidth;
            float cellSizeY = (worldHeight * _padding) / _gridHeight;

            float finalCellSize = Mathf.Min(cellSizeX, cellSizeY);
            float scale = finalCellSize / _baseCellSize;

            if (_cellContainer != null)
                _cellContainer.localScale = new Vector3(scale, scale, 1);

            if (_cellContainer != null)
            {
                float offsetX = -_gridWidth * 0.5f * finalCellSize + finalCellSize * 0.5f;
                float offsetY = -_gridHeight * 0.5f * finalCellSize + finalCellSize * 0.5f;

                _cellContainer.localPosition = new Vector3(offsetX, offsetY, 0f);
            }
        }


        // =======================================================
        // =                 FREE BOUNDS MODE                    =
        // =======================================================
        private void ScaleFreeBounds()
        {
            float aspect = (float)Screen.width / Screen.height;

            float boundsW = (_maxBounds.x - _minBounds.x) / _padding;
            float boundsH = (_maxBounds.y - _minBounds.y) / _padding;

            float orthoH = boundsH * 0.5f;
            float orthoW = (boundsW / aspect) * 0.5f;

            _camera.orthographicSize = Mathf.Max(orthoW, orthoH);

            // center camera inside bounds
            Vector3 center = new Vector3(
                (_minBounds.x + _maxBounds.x) / 2f,
                (_minBounds.y + _maxBounds.y) / 2f,
                -10f
            );

            _camera.transform.position = center;

            // scale root object if needed
            if (_rootToScale != null)
            {
                float visibleH = _camera.orthographicSize * 2f;
                float visibleW = visibleH * aspect;

                float scaleX = (visibleW * _padding) / boundsW;
                float scaleY = (visibleH * _padding) / boundsH;

                float scale = Mathf.Min(scaleX, scaleY);
                _rootToScale.localScale = new Vector3(scale, scale, 1);
            }
        }
    }
}
