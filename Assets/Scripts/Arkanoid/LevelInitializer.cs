using UnityEngine;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Builds a brick grid based on config.
    /// Performs world-position alignment.
    /// </summary>
    public class LevelInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject _brickPrefab = null;
        [SerializeField] private float _cellW = 1.2f;
        [SerializeField] private float _cellH = 0.6f;
        [SerializeField] private Transform _container = null;
        
        [Header("Generation")]
        public float emptyChance = 0.15f;
        public int maxHP = 1;
        
        public int BuildLevel(int rows, int cols)
        {
            if (_brickPrefab == null) 
                return 0;

            int count = 0;

            float offsetX = -(cols * _cellW) * 0.5f + _cellW * 0.5f;
            float startY = 2f;

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (Random.value < emptyChance)
                        continue;

                    Vector3 pos = new Vector3(
                        offsetX + c * _cellW,
                        startY + r * _cellH,
                        0
                    );

                    GameObject brick = Instantiate(_brickPrefab, pos, Quaternion.identity, _container);

                    int hp = Mathf.Clamp(Random.Range(1, maxHP + 1) + (r / 3), 1, maxHP);
                    brick.GetComponent<Brick>().SetHealth(hp);

                    count++;
                }
            }

            return count;
        }
    }
}