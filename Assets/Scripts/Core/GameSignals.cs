using System;

namespace MiniIT.CORE
{
    /// <summary>
    /// Global game signals (events). Use static events for simplicity.
    /// </summary>
    public static class GameSignals
    {
        /// <summary>Score changed event.</summary>
        public static event Action<int> OnScoreChanged;

        /// <summary>Raise score changed.</summary>
        public static void RaiseScoreChanged(int score)
        {
            OnScoreChanged?.Invoke(score);
        }

        /// <summary>Generic level complete event.</summary>
        public static event Action<string> OnLevelComplete;

        /// <summary>Raise level complete.</summary>
        public static void RaiseLevelComplete(string levelId)
        {
            OnLevelComplete?.Invoke(levelId);
        }
    }
}