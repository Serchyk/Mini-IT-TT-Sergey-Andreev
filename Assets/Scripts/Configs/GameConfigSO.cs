using UnityEngine;

namespace MiniIT.CONFIGS
{
    /// <summary>
    /// Global config for basic parameters used by modes.
    /// Create SO in Assets/Configs.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConfig", menuName = "MiniIT/Configs/GameConfig")]
    public class GameConfigSO : ScriptableObject
    {
        [Header("Match3")]
        public int Match3Width = 8;
        public int Match3Height = 8;
        public int Match3ColorCount = 6;

        [Header("Merge")]
        public int MergeWidth = 5;
        public int MergeHeight = 6;
        public float MergeSpawnInterval = 3.0f;

        [Header("Arkanoid")]
        public float BallSpeed = 6f;
        public int BricksRows = 5;
        public int BricksCols = 8;
    }
}