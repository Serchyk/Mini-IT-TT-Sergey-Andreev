// using System;
// using UnityEngine;
//
// namespace MiniIT.ARKANOID
// {
//
//     /// <summary>
//     /// Abstract bonus effect base class.
//     /// Implements IPoolable for pooling.
//     /// </summary>
//     public abstract class BonusEffect : MonoBehaviour, MiniIT.CORE.IPoolable
//     {
//         public virtual float Duration => 5f;
//
//         /// <summary>Apply effect to paddle.</summary>
//         public abstract void Apply(PaddleController paddle);
//
//         /// <summary>Revert effect from paddle.</summary>
//         public abstract void Revert(PaddleController paddle);
//
//         #region IPoolable
//         public virtual void OnSpawned()
//         {
//             gameObject.SetActive(true);
//         }
//
//         public virtual void OnDespawned()
//         {
//             gameObject.SetActive(false);
//         }
//         #endregion
//     }
//
//     /// <summary>
//     /// Long paddle bonus.
//     /// </summary>
//     public class LongPaddleBonus : BonusEffect
//     {
//         [SerializeField] private float _scaleMultiplier = 1.5f;
//
//         public override void Apply(PaddleController paddle)
//         {
//             if (paddle == null)
//                 return;
//
//             paddle.ApplyTemporaryScale(_scaleMultiplier, Duration);
//         }
//
//         public override void Revert(PaddleController paddle)
//         {
//             if (paddle == null)
//                 return;
//
//             paddle.ResetScale();
//         }
//     }
//
//     /// <summary>
//     /// MultiBall bonus.
//     /// </summary>
//     public class MultiBallBonus : BonusEffect
//     {
//         [SerializeField] private GameObject _ballPrefab = null;
//
//         public override void Apply(PaddleController paddle)
//         {
//             if (_ballPrefab == null)
//                 return;
//
//             BallController[] balls = FindObjectsOfType<BallController>();
//
//             foreach (BallController ball in balls)
//             {
//                 GameObject clone = Instantiate(_ballPrefab, ball.transform.position, Quaternion.identity);
//                 BallController cBall = clone.GetComponent<BallController>();
//                 cBall.Init(ArkanoidController.Instance.BallSpeed);
//             }
//         }
//
//         public override void Revert(PaddleController paddle)
//         {
//             // MultiBall has no revert
//         }
//     }
// }
