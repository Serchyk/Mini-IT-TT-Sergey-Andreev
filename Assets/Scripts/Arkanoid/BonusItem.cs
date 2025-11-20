using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MiniIT.ARKANOID
{
    /// <summary>
    /// Enum for all bonus types.
    /// </summary>
    public enum BonusType
    {
        MultiBall,
        LongPaddle
    }
    
    /// <summary>
    /// Falling bonus that applies effect when player catches it.
    /// </summary>
    public class BonusItem : MonoBehaviour
    {
        [SerializeField] private float _fallSpeed = 3f;
        [SerializeField] private BonusType _type = BonusType.MultiBall;
        [SerializeField] private SpriteRenderer _spriteRenderer = null;

        public BonusType Type => _type;

        private void Start()
        {
            SetRandomType();
        }

        private void Update()
        {
            transform.position += Vector3.down * _fallSpeed * Time.deltaTime;

            if (transform.position.y < -6f)
                Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Paddle"))
                return;

            ArkanoidController.Instance.ApplyBonus(_type);
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Sets a random bonus type.
        /// </summary>
        public void SetRandomType()
        {
            BonusType[] values = (BonusType[])System.Enum.GetValues(typeof(BonusType));
            _type = values[Random.Range(0, values.Length)];
            
            if (_spriteRenderer == null)
            {
                return;
            }

            // Map color id to color - simple mapping for demo
            switch (_type)
            {
                case BonusType.MultiBall:
                    _spriteRenderer.color = Color.cyan;
                    break;
                case BonusType.LongPaddle:
                    _spriteRenderer.color = Color.green;
                    break;
                default:
                    _spriteRenderer.color = Color.cyan;
                    break;
            }
        }
    }
}