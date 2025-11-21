using UnityEngine;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Handles player input (mouse / touch) and temporary bonus scaling.
    /// </summary>
    public class PaddleController : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 10f;
        private Camera _camera;

        private Vector3 _baseScale;
        private float leftLimit, rightLimit;
        private float _bonusTimer;
        private bool _bonusActive;

        private void Awake()
        {
            _camera = Camera.main;
            _baseScale = transform.localScale;
            UpdateBounds();
        }

        private void Update()
        {
            UpdateBounds();

            var x = GetInputX();
            if (x.HasValue)
            {
                Vector3 pos = transform.position;
                pos.x = Mathf.Lerp(pos.x, x.Value, Time.deltaTime * _moveSpeed);
                pos.x = Mathf.Clamp(pos.x, leftLimit, rightLimit);
                transform.position = pos;
            }

            UpdateBonus();
        }

        private void UpdateBounds()
        {
            float halfW = transform.localScale.x * 0.5f;

            leftLimit = _camera.ScreenToWorldPoint(Vector3.zero).x + halfW;
            rightLimit = _camera.ScreenToWorldPoint(new Vector3(Screen.width, 0)).x - halfW;
        }

        public void PlaceAtScreenBottom()
        {
            var p = _camera.ScreenToWorldPoint(
                new Vector3(Screen.width / 2f, 40f, 10f));
            transform.position = new Vector3(0, p.y, 0);
        }

        private float? GetInputX()
        {
            if (Input.touchCount > 0)
                return _camera.ScreenToWorldPoint(Input.GetTouch(0).position).x;

            if (Input.GetMouseButton(0))
                return _camera.ScreenToWorldPoint(Input.mousePosition).x;

            return null;
        }

        public void ApplyTemporaryScale(float multiplier, float duration)
        {
            transform.localScale = _baseScale * multiplier;
            _bonusTimer = duration;
            _bonusActive = true;
        }

        private void UpdateBonus()
        {
            if (!_bonusActive) return;

            _bonusTimer -= Time.deltaTime;
            if (_bonusTimer <= 0f)
            {
                transform.localScale = _baseScale;
                _bonusActive = false;
            }
        }
    }
}