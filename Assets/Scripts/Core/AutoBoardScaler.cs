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
        [SerializeField] private ScaleMode _mode = ScaleMode.FreeBounds;
        [SerializeField] private Camera _camera = null;
        [SerializeField, Range(0.5f, 1f)]
        private float _padding = 0.95f;
        [SerializeField] private bool _autoDetectPlatform = true;

        [Header("Platform Specific Settings")]
        [SerializeField] private Vector2 _pcMinBounds = new Vector2(-7f, -4f);
        [SerializeField] private Vector2 _pcMaxBounds = new Vector2(7f, 5f);
        [SerializeField] private Vector2 _mobileMinBounds = new Vector2(-4.5f, -1f);
        [SerializeField] private Vector2 _mobileMaxBounds = new Vector2(4.5f, 2f);

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
        [SerializeField] private bool _usePlatformSpecificBounds = true;

        private Vector2 _effectiveMinBounds;
        private Vector2 _effectiveMaxBounds;
        private float _lastAspectRatio = 0f;

        #region Unity Lifecycle

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            // Re-apply scaling if aspect ratio changes (window resize, orientation change)
            float currentAspect = (float)Screen.width / Screen.height;
            if (Mathf.Abs(currentAspect - _lastAspectRatio) > 0.01f)
            {
                ApplyScaling();
                _lastAspectRatio = currentAspect;
            }
        }

        #endregion

        #region Initialization

        private void Initialize()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null)
            {
                Debug.LogError("AutoBoardScaler: No camera assigned and no Camera.main found!");
                return;
            }

            // Ensure camera is orthographic
            if (!_camera.orthographic)
            {
                Debug.LogWarning("AutoBoardScaler: Camera is not orthographic. Forcing orthographic mode.");
                _camera.orthographic = true;
            }

            CalculateEffectiveBounds();
            ApplyScaling();
        }

        private void CalculateEffectiveBounds()
        {
            if (_usePlatformSpecificBounds && _autoDetectPlatform)
            {
                if (IsMobilePlatform())
                {
                    _effectiveMinBounds = _mobileMinBounds;
                    _effectiveMaxBounds = _mobileMaxBounds;
                }
                else
                {
                    _effectiveMinBounds = _pcMinBounds;
                    _effectiveMaxBounds = _pcMaxBounds;
                }
            }
            else
            {
                _effectiveMinBounds = _minBounds;
                _effectiveMaxBounds = _maxBounds;
            }

            // Validate bounds
            if (_effectiveMinBounds.x >= _effectiveMaxBounds.x || _effectiveMinBounds.y >= _effectiveMaxBounds.y)
            {
                Debug.LogWarning("AutoBoardScaler: Invalid bounds! Min should be less than Max. Using fallback bounds.");
                _effectiveMinBounds = new Vector2(-5f, -3f);
                _effectiveMaxBounds = new Vector2(5f, 3f);
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Chooses scaling strategy based on mode.
        /// </summary>
        public void ApplyScaling()
        {
            if (_camera == null)
            {
                return;
            }

            CalculateEffectiveBounds();

            switch (_mode)
            {
                case ScaleMode.Grid:
                    ScaleGrid();
                    break;

                case ScaleMode.FreeBounds:
                    ScaleFreeBounds();
                    break;
            }

            _lastAspectRatio = (float)Screen.width / Screen.height;
        }

        /// <summary>
        /// Set custom bounds for FreeBounds mode.
        /// </summary>
        public void SetBounds(Vector2 minBounds, Vector2 maxBounds)
        {
            _minBounds = minBounds;
            _maxBounds = maxBounds;
            _usePlatformSpecificBounds = false;
            ApplyScaling();
        }

        /// <summary>
        /// Enable platform-specific bounds detection.
        /// </summary>
        public void UsePlatformBounds(bool usePlatformBounds)
        {
            _usePlatformSpecificBounds = usePlatformBounds;
            ApplyScaling();
        }

        #endregion

        #region Scaling Methods

        // =======================================================
        // =                     GRID MODE                       =
        // =======================================================
        private void ScaleGrid()
        {
            if (_gridWidth <= 0 || _gridHeight <= 0)
            {
                return;
            }

            float aspect = (float)Screen.width / Screen.height;

            // Calculate required size to fit the grid
            float requiredWidth = _gridWidth * _baseCellSize;
            float requiredHeight = _gridHeight * _baseCellSize;

            // Calculate orthographic size based on aspect ratio
            float orthoSizeBasedOnWidth = (requiredWidth / aspect) * 0.5f / _padding;
            float orthoSizeBasedOnHeight = requiredHeight * 0.5f / _padding;

            _camera.orthographicSize = Mathf.Max(orthoSizeBasedOnWidth, orthoSizeBasedOnHeight);

            // Calculate and apply scaling to fit the grid perfectly
            float visibleWorldHeight = _camera.orthographicSize * 2f;
            float visibleWorldWidth = visibleWorldHeight * aspect;

            float scaleX = (visibleWorldWidth * _padding) / requiredWidth;
            float scaleY = (visibleWorldHeight * _padding) / requiredHeight;

            float finalScale = Mathf.Min(scaleX, scaleY);

            if (_cellContainer != null)
            {
                _cellContainer.localScale = new Vector3(finalScale, finalScale, 1f);

                // Center the grid
                float offsetX = -_gridWidth * 0.5f * _baseCellSize * finalScale + _baseCellSize * finalScale * 0.5f;
                float offsetY = -_gridHeight * 0.5f * _baseCellSize * finalScale + _baseCellSize * finalScale * 0.5f;

                _cellContainer.localPosition = new Vector3(offsetX, offsetY, 0f);
            }
        }

        // =======================================================
        // =                 FREE BOUNDS MODE                    =
        // =======================================================
        private void ScaleFreeBounds()
        {
            float aspect = (float)Screen.width / Screen.height;

            // Calculate bounds dimensions
            float boundsWidth = _effectiveMaxBounds.x - _effectiveMinBounds.x;
            float boundsHeight = _effectiveMaxBounds.y - _effectiveMinBounds.y;

            if (boundsWidth <= 0 || boundsHeight <= 0)
            {
                Debug.LogError("AutoBoardScaler: Invalid bounds dimensions!");
                return;
            }

            // Calculate required orthographic size to fit bounds
            float orthoSizeBasedOnWidth = (boundsWidth / aspect) * 0.5f / _padding;
            float orthoSizeBasedOnHeight = boundsHeight * 0.5f / _padding;

            // Use the larger size to ensure everything fits
            _camera.orthographicSize = Mathf.Max(orthoSizeBasedOnWidth, orthoSizeBasedOnHeight);

            // Center camera on bounds
            Vector3 boundsCenter = new Vector3(
                (_effectiveMinBounds.x + _effectiveMaxBounds.x) * 0.5f,
                (_effectiveMinBounds.y + _effectiveMaxBounds.y) * 0.5f,
                _camera.transform.position.z
            );

            _camera.transform.position = boundsCenter;

            // Optional: Scale root object to fit bounds exactly
            ScaleRootObject(boundsWidth, boundsHeight, aspect);
        }

        private void ScaleRootObject(float boundsWidth, float boundsHeight, float aspect)
        {
            if (_rootToScale == null)
            {
                return;
            }

            // Calculate visible world dimensions
            float visibleWorldHeight = _camera.orthographicSize * 2f;
            float visibleWorldWidth = visibleWorldHeight * aspect;

            // Calculate scale factors
            float scaleX = (visibleWorldWidth * _padding) / boundsWidth;
            float scaleY = (visibleWorldHeight * _padding) / boundsHeight;

            // Use the smaller scale to ensure everything fits within bounds
            float finalScale = Mathf.Min(scaleX, scaleY);

            _rootToScale.localScale = new Vector3(finalScale, finalScale, 1f);
        }

        #endregion

        #region Platform Detection

        private bool IsMobilePlatform()
        {
            return Application.isMobilePlatform || 
                   SystemInfo.deviceType == DeviceType.Handheld ||
                   IsMobileByAspectRatio();
        }

        private bool IsMobileByAspectRatio()
        {
            float aspect = (float)Screen.width / Screen.height;
            
            // Common mobile aspect ratios
            bool isCommonMobileAspect = aspect >= 0.56f && aspect <= 0.6f; // 9:16 to 10:16
            bool isTabletAspect = aspect >= 0.75f && aspect <= 0.8f; // 3:4 to 4:5
            
            return isCommonMobileAspect || isTabletAspect;
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying)
            {
                CalculateEffectiveBounds();
            }

            // Draw bounds visualization
            Gizmos.color = Color.green;
            Vector3 center = new Vector3(
                (_effectiveMinBounds.x + _effectiveMaxBounds.x) * 0.5f,
                (_effectiveMinBounds.y + _effectiveMaxBounds.y) * 0.5f,
                0f
            );

            Vector3 size = new Vector3(
                _effectiveMaxBounds.x - _effectiveMinBounds.x,
                _effectiveMaxBounds.y - _effectiveMinBounds.y,
                0.1f
            );

            Gizmos.DrawWireCube(center, size);

            // Draw camera frustum if camera is set
            if (_camera != null && _camera.orthographic)
            {
                Gizmos.color = Color.yellow;
                float height = _camera.orthographicSize * 2f;
                float width = height * _camera.aspect;
                Gizmos.DrawWireCube(_camera.transform.position, new Vector3(width, height, 0.1f));
            }
        }

        #endregion

        #region Public Properties

        public Vector2 EffectiveMinBounds => _effectiveMinBounds;
        public Vector2 EffectiveMaxBounds => _effectiveMaxBounds;
        public bool IsUsingMobileBounds => _usePlatformSpecificBounds && IsMobilePlatform();

        #endregion

        #region Validation

        private void OnValidate()
        {
            _padding = Mathf.Clamp(_padding, 0.1f, 1f);
            _gridWidth = Mathf.Max(0, _gridWidth);
            _gridHeight = Mathf.Max(0, _gridHeight);
            _baseCellSize = Mathf.Max(0.1f, _baseCellSize);

            // Ensure min bounds are less than max bounds
            if (_minBounds.x >= _maxBounds.x) _minBounds.x = _maxBounds.x - 1f;
            if (_minBounds.y >= _maxBounds.y) _minBounds.y = _maxBounds.y - 1f;
            if (_pcMinBounds.x >= _pcMaxBounds.x) _pcMinBounds.x = _pcMaxBounds.x - 1f;
            if (_pcMinBounds.y >= _pcMaxBounds.y) _pcMinBounds.y = _pcMaxBounds.y - 1f;
            if (_mobileMinBounds.x >= _mobileMaxBounds.x) _mobileMinBounds.x = _mobileMaxBounds.x - 1f;
            if (_mobileMinBounds.y >= _mobileMaxBounds.y) _mobileMinBounds.y = _mobileMaxBounds.y - 1f;
        }

        #endregion
    }
}