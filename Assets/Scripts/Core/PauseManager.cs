using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace MiniIT.CORE
{
    /// <summary>
    /// Global pause manager that handles game pause state across all game types.
    /// Supports multiple pause sources and graceful state restoration.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        [Header("Pause Settings")]
        [SerializeField] private bool _enablePauseSystem = true;
        [SerializeField] private KeyCode _pauseKey = KeyCode.Escape;
        [SerializeField] private bool _pauseTimeScale = true;
        [SerializeField] private bool _pauseAudio = true;

        [Header("Events")]
        public UnityEvent OnGamePaused;
        public UnityEvent OnGameResumed;

        private static PauseManager _instance;
        private bool _isPaused = false;
        private readonly HashSet<object> _pauseSources = new HashSet<object>();
        private float _prePauseTimeScale = 1f;
        private float _prePauseAudioVolume = 1f;

        #region Unity Lifecycle

        private void Update()
        {
            HandlePauseInput();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        #endregion
        
        #region Public Pause Methods

        /// <summary>
        /// Toggle pause state - if no specific sources, uses global toggle.
        /// </summary>
        public void TogglePause()
        {
            if (_isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        /// <summary>
        /// Pause the game with a specific source (menu, dialog, etc.).
        /// </summary>
        public void PauseGame(object pauseSource = null)
        {
            if (!_enablePauseSystem || _isPaused)
            {
                return;
            }

            if (pauseSource != null)
            {
                _pauseSources.Add(pauseSource);
            }

            // Only actually pause if this is the first source
            if (_pauseSources.Count == 1 && pauseSource != null)
            {
                ApplyPauseState();
            }
            else if (_pauseSources.Count == 0)
            {
                // Global pause
                ApplyPauseState();
            }
        }

        /// <summary>
        /// Resume the game for a specific source.
        /// </summary>
        public void ResumeGame(object pauseSource = null)
        {
            if (!_isPaused)
            {
                return;
            }

            if (pauseSource != null)
            {
                _pauseSources.Remove(pauseSource);
            }
            else
            {
                // Global resume - clear all sources
                _pauseSources.Clear();
            }

            // Only actually resume if no sources remain
            if (_pauseSources.Count == 0)
            {
                ApplyResumeState();
            }
            
            _isPaused = false;
        }

        /// <summary>
        /// Force resume the game regardless of pause sources.
        /// </summary>
        public void ForceResume()
        {
            _pauseSources.Clear();
            ApplyResumeState();
        }

        /// <summary>
        /// Check if game is paused by a specific source.
        /// </summary>
        public bool IsPausedBySource(object pauseSource)
        {
            return _pauseSources.Contains(pauseSource);
        }

        #endregion

        #region Private Methods

        private void HandlePauseInput()
        {
            if (!_enablePauseSystem || !Input.GetKeyDown(_pauseKey))
            {
                return;
            }

            TogglePause();
        }

        private void ApplyPauseState()
        {
            if (_isPaused)
            {
                return;
            }

            _isPaused = true;

            // Save pre-pause state
            _prePauseTimeScale = Time.timeScale;
            _prePauseAudioVolume = AudioListener.volume;

            // Apply pause effects
            if (_pauseTimeScale)
            {
                Time.timeScale = 0f;
            }

            if (_pauseAudio)
            {
                AudioListener.volume = 0f;
            }

            // Invoke events
            OnGamePaused?.Invoke();
        }

        private void ApplyResumeState()
        {
            if (!_isPaused)
            {
                return;
            }

            _isPaused = false;

            // Restore pre-pause state
            if (_pauseTimeScale)
            {
                Time.timeScale = _prePauseTimeScale;
            }

            if (_pauseAudio)
            {
                AudioListener.volume = _prePauseAudioVolume;
            }

            // Invoke events
            OnGameResumed?.Invoke();
        }

        private void Cleanup()
        {
            // Always ensure time scale is restored when destroyed
            if (_isPaused)
            {
                Time.timeScale = _prePauseTimeScale;
                AudioListener.volume = _prePauseAudioVolume;
            }

            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        #region Scene Management

        /// <summary>
        /// Load menu scene and resume time scale.
        /// </summary>
        public void LoadMenuScene(string menuSceneName = "MainMenu")
        {
            ForceResume();
            SceneManager.LoadScene(menuSceneName);
        }

        /// <summary>
        /// Load menu scene by build index.
        /// </summary>
        public void LoadMenuScene(int sceneBuildIndex = 0)
        {
            ForceResume();
            SceneManager.LoadScene(sceneBuildIndex);
        }

        /// <summary>
        /// Restart current scene.
        /// </summary>
        public void RestartCurrentScene()
        {
            ForceResume();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            // Ensure time scale is never negative
            if (_prePauseTimeScale < 0f)
            {
                _prePauseTimeScale = 0f;
            }

            // Ensure audio volume is valid
            if (_prePauseAudioVolume < 0f)
            {
                _prePauseAudioVolume = 0f;
            }
            else if (_prePauseAudioVolume > 1f)
            {
                _prePauseAudioVolume = 1f;
            }
        }

        #endregion
    }
}