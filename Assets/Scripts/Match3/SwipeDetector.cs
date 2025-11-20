using UnityEngine;

namespace MiniIT.MATCH3
{
    /// <summary>
    /// Detects swipe direction from touch or mouse drag.
    /// </summary>
    public class SwipeDetector
    {
        private readonly float _minSwipeDistance;

        private Vector2 _start;
        private bool _started = false;

        public SwipeDetector(float minSwipeDistance = 0.2f)
        {
            _minSwipeDistance = minSwipeDistance;
        }

        public void Begin(Vector2 pos)
        {
            _started = true;
            _start = pos;
        }

        public bool TryGetSwipe(Vector2 current, out Vector2Int dir)
        {
            dir = Vector2Int.zero;
            if (!_started) return false;

            Vector2 delta = current - _start;
            if (delta.magnitude < _minSwipeDistance)
            {
                return false;
            }

            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                dir = delta.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else
            {
                dir = delta.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            _started = false;
            return true;
        }
    }
}