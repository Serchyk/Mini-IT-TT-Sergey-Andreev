using System;

namespace MiniIT.CORE
{
    /// <summary>
    /// Global game signals (events). Use static events for simplicity.
    /// </summary>
    public static class GameSignals
    {
        /// <summary>Score added event - use this for incremental score changes.</summary>
        public static event Action<int> OnScoreAdded;

        /// <summary>Score changed event - use this for absolute score changes.</summary>
        public static event Action<int> OnScoreChanged;

        /// <summary>Raise score added event.</summary>
        public static void RaiseScoreAdded(int scoreDelta)
        {
            OnScoreAdded?.Invoke(scoreDelta);
        }

        /// <summary>Raise score changed event.</summary>
        public static void RaiseScoreChanged(int totalScore)
        {
            OnScoreChanged?.Invoke(totalScore);
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