using TMPro;
using UnityEngine;

namespace MiniIT.CORE
{
    /// <summary>
    /// Singleton that subscribes to score events and updates UI.
    /// </summary>
    public sealed class ScoreManager : MonoBehaviour
    {
        /// <summary>Public instance – the component is automatically initialised on scene load.</summary>
        public static ScoreManager Instance { get; private set; }

        [Header("UI References")]
        public TextMeshProUGUI scoreText = null;
        [SerializeField] private TextMeshProUGUI _scoreDeltaText = null;

        [Header("Animation Settings")]
        [SerializeField] private float _deltaTextDuration = 1.5f;

        // Current score
        private int _currentScore = 0;
        private Coroutine _deltaTextCoroutine = null;

        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            GameSignals.OnScoreAdded += HandleScoreAdded;
            GameSignals.OnScoreChanged += HandleScoreChanged;
        }

        private void OnDisable()
        {
            GameSignals.OnScoreAdded -= HandleScoreAdded;
            GameSignals.OnScoreChanged -= HandleScoreChanged;
        }

        private void Start()
        {
            InitializeUI();
        }

        #endregion

        #region Event Handling

        /// <summary>Called when score is added incrementally.</summary>
        private void HandleScoreAdded(int scoreDelta)
        {
            if (scoreDelta > 0)
            {
                ShowScoreDelta(scoreDelta);
            }
        }

        /// <summary>Called when total score changes.</summary>
        private void HandleScoreChanged(int totalScore)
        {
            _currentScore = totalScore;
            UpdateScoreText();
        }

        #endregion

        #region UI Management

        /// <summary>Initialize UI elements.</summary>
        private void InitializeUI()
        {
            UpdateScoreText();
            
            if (_scoreDeltaText != null)
            {
                _scoreDeltaText.gameObject.SetActive(false);
            }
        }

        /// <summary>Refreshes the TMP text with the current score.</summary>
        private void UpdateScoreText()
        {
            if (scoreText == null)
            {
                return;
            }

            // Format: "Очков: 12,345"
            scoreText.text = $"Очков: {_currentScore:N0}";
        }

        /// <summary>Shows temporary score delta text.</summary>
        private void ShowScoreDelta(int delta)
        {
            if (_scoreDeltaText == null)
            {
                return;
            }

            // Stop existing coroutine if running
            if (_deltaTextCoroutine != null)
            {
                StopCoroutine(_deltaTextCoroutine);
            }

            // Show delta text
            _scoreDeltaText.text = $"+{delta}";
            _scoreDeltaText.gameObject.SetActive(true);

            // Start hide coroutine
            _deltaTextCoroutine = StartCoroutine(HideDeltaTextAfterDelay());
        }

        private System.Collections.IEnumerator HideDeltaTextAfterDelay()
        {
            yield return new WaitForSeconds(_deltaTextDuration);
            
            if (_scoreDeltaText != null)
            {
                _scoreDeltaText.gameObject.SetActive(false);
            }
            
            _deltaTextCoroutine = null;
        }

        #endregion

        #region Public Methods

        /// <summary>Get current score value.</summary>
        public int GetCurrentScore() => _currentScore;

        /// <summary>Reset score to zero.</summary>
        public void ResetScore()
        {
            _currentScore = 0;
            UpdateScoreText();
        }
        
        /// <summary>
        /// Add score to current total and raise events.
        /// </summary>
        public void AddScore(int scoreDelta)
        {
            if (scoreDelta <= 0)
            {
                return;
            }

            _currentScore += scoreDelta;
            
            // Raise both events for flexibility
            GameSignals.RaiseScoreAdded(scoreDelta);
            GameSignals.RaiseScoreChanged(_currentScore);
        }

        /// <summary>
        /// Set absolute score value and raise events.
        /// </summary>
        public void SetScore(int totalScore)
        {
            int delta = totalScore - _currentScore;
            _currentScore = totalScore;
            
            if (delta != 0)
            {
                GameSignals.RaiseScoreAdded(delta);
            }
            
            GameSignals.RaiseScoreChanged(_currentScore);
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            _deltaTextDuration = Mathf.Max(0.1f, _deltaTextDuration);
        }

        #endregion
    }
}