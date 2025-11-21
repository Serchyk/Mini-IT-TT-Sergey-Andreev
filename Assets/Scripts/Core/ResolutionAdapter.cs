using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple adapter to handle screen scaling for PC and Mobile.
/// Attach to Canvas root or bootstrap object.
/// </summary>
namespace MiniIT.CORE
{
    public class ResolutionAdapter : MonoBehaviour
    {
        [Header("Reference Resolution")]
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1920, 1080);
        
        [Header("Match Settings")]
        [SerializeField] [Range(0f, 1f)] private float _matchWidthOrHeight = 0.5f;
        
        [Header("Background References")]
        [SerializeField] private Image _backgroundImage = null;

        private Canvas _canvas = null;
        private CanvasScaler _canvasScaler = null;
        private RectTransform _rectTransform = null;

        #region Unity Lifecycle

        private void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _canvasScaler = GetComponent<CanvasScaler>();
            _rectTransform = GetComponent<RectTransform>();

            SetupCanvasScaler();
            SetupBackground();
        }

        private void Start()
        {
            UpdateBackground();
        }

        private void Update()
        {
            // Optional: update on resolution change
            if (Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight)
            {
                UpdateBackground();
            }
        }

        #endregion

        #region Canvas Setup

        private void SetupCanvasScaler()
        {
            if (_canvasScaler == null)
            {
                return;
            }

            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _canvasScaler.referenceResolution = _referenceResolution;
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _canvasScaler.matchWidthOrHeight = _matchWidthOrHeight;

            // Auto-detect and adjust for mobile
            if (IsMobilePlatform())
            {
                AdjustForMobile();
            }
            else
            {
                AdjustForPC();
            }
        }

        private bool IsMobilePlatform()
        {
            return Application.isMobilePlatform || 
                   SystemInfo.deviceType == DeviceType.Handheld;
        }

        private void AdjustForMobile()
        {
            // Mobile-specific adjustments
            _canvasScaler.matchWidthOrHeight = 1f; // Prefer height matching for mobile
        }

        private void AdjustForPC()
        {
            // PC-specific adjustments
            _canvasScaler.matchWidthOrHeight = 0f; // Prefer width matching for PC
        }

        #endregion

        #region Background System

        private void SetupBackground()
        {
            if (_backgroundImage != null)
            {
                SetupImageBackground();
            }
            else
            {
                CreateDefaultBackground();
            }
        }

        private void SetupImageBackground()
        {
            _backgroundImage.type = Image.Type.Sliced;
            _backgroundImage.preserveAspect = false;
        }

        private void CreateDefaultBackground()
        {
            GameObject bgObject = new GameObject("Background");
            bgObject.transform.SetParent(transform);
            bgObject.transform.SetAsFirstSibling();

            RectTransform bgRect = bgObject.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            Image bgImage = bgObject.AddComponent<Image>();
            bgImage.color = Color.black;

            _backgroundImage = bgImage;
        }

        #endregion

        #region Background Updates

        private int _lastScreenWidth = 0;
        private int _lastScreenHeight = 0;

        private void UpdateBackground()
        {
            _lastScreenWidth = Screen.width;
            _lastScreenHeight = Screen.height;

            UpdateBackgroundSize();
            UpdateBackgroundAspect();
        }

        private void UpdateBackgroundSize()
        {
            if (_backgroundImage != null)
            {
                // Image will automatically stretch with canvas
            }
        }

        private void UpdateBackgroundAspect()
        {
            float screenAspect = (float)Screen.width / Screen.height;
            float referenceAspect = _referenceResolution.x / _referenceResolution.y;

            // Adjust background based on aspect ratio difference
            if (screenAspect > referenceAspect)
            {
                // Wider screen - adjust horizontal
                AdjustBackgroundForWideScreen();
            }
            else
            {
                // Taller screen - adjust vertical
                AdjustBackgroundForTallScreen();
            }
        }

        private void AdjustBackgroundForWideScreen()
        {
            // For wide screens (most mobile in landscape)
            if (_backgroundImage != null)
            {
                _backgroundImage.rectTransform.offsetMin = new Vector2(0, -100);
                _backgroundImage.rectTransform.offsetMax = new Vector2(0, 100);
            }
        }

        private void AdjustBackgroundForTallScreen()
        {
            // For tall screens (mobile in portrait)
            if (_backgroundImage != null)
            {
                _backgroundImage.rectTransform.offsetMin = new Vector2(-450, 0);
                _backgroundImage.rectTransform.offsetMax = new Vector2(450, 0);
            }
        }

        #endregion

        #region Public Methods

        public void SetBackgroundColor(Color color)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.color = color;
            }
        }

        public void SetBackgroundSprite(Sprite sprite)
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.sprite = sprite;
                _backgroundImage.type = Image.Type.Sliced;
            }
        }

        public Vector2 GetCanvasSize()
        {
            return _rectTransform != null ? _rectTransform.rect.size : _referenceResolution;
        }

        public float GetAspectRatio()
        {
            Vector2 size = GetCanvasSize();
            return size.x / size.y;
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            _referenceResolution = new Vector2(
                Mathf.Max(100, _referenceResolution.x),
                Mathf.Max(100, _referenceResolution.y)
            );

            _matchWidthOrHeight = Mathf.Clamp01(_matchWidthOrHeight);
        }

        #endregion
    }
}