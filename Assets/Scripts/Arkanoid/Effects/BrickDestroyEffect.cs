using UnityEngine;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Simple pop‑up animation for a brick being destroyed.
    /// </summary>
    public class BrickDestroyEffect : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 0.35f;
        [SerializeField] private AnimationCurve _scaleCurve;

        private float _timer;
        private Vector3 _startScale;

        private void Awake() => _startScale = transform.localScale;

        private void Update()
        {
            _timer += Time.deltaTime;
            var t = _timer / _lifeTime;
            var s = _scaleCurve.Evaluate(t);
            transform.localScale = _startScale * s;

            if (_timer >= _lifeTime) Destroy(gameObject);
        }
    }
}