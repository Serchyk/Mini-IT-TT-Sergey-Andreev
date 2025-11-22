using UnityEngine;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Builds a grid of bricks based on configuration.
    /// Keeps all bricks as children of a container for easy cleanup.
    /// </summary>
    public class LevelInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject _brickPrefab;
        [SerializeField] private float _cellW = 1.2f, _cellH = 0.6f;
        [SerializeField] private Transform _container;

        [Header("Generation")]
        public float EmptyChance = 0.15f;
        public int MaxHP = 1;

        /// <summary>
        /// Builds rows × cols bricks and returns the number of instantiated bricks.
        /// </summary>
        public int BuildLevel(int rows, int cols)
        {
            if (_brickPrefab == null) return 0;

            int count = 0;
            float offsetX = -(cols * _cellW) * 0.5f + _cellW * 0.5f;
            float startY   = 2f;

            for (int r = 0; r < rows; ++r)
            for (int c = 0; c < cols; ++c)
            {
                if (Random.value < EmptyChance) continue;

                var pos = new Vector3(
                    offsetX + c * _cellW,
                    startY + r * _cellH, 0);

                GameObject brickGO =
                    Instantiate(_brickPrefab, pos, Quaternion.identity, _container);

                int hp = Mathf.Clamp(Random.Range(1, MaxHP + 1) + (r / 3), 1, MaxHP);
                brickGO.GetComponent<Brick>().SetHealth(hp);

                count++;
            }

            return count;
        }
    }
}