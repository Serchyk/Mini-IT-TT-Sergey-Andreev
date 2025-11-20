using UnityEngine;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Simple brick destruction animation: scale pop + auto destroy.
    /// </summary>
    public class BrickDestroyEffect : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 0.35f;
        [SerializeField] private AnimationCurve _scaleCurve = null;

        private float _timer = 0f;
        private Vector3 _startScale;

        private void Awake()
        {
            _startScale = transform.localScale;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            float t = _timer / _lifeTime;
            float scale = _scaleCurve.Evaluate(t);
            transform.localScale = _startScale * scale;

            if (_timer >= _lifeTime)
                Destroy(gameObject);
        }
    }
}