// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
// namespace MiniIT.ARKANOID
// {
//     /// <summary>
//     /// Manages bonus item spawning and effect application.
//     /// </summary>
//     public class BonusManager : MonoBehaviour
//     {
//         [SerializeField] private GameObject _ballPrefab = null;
//
//         private readonly MiniIT.CORE.ObjectPool<BonusItem> _bonusPool;
//         private readonly Dictionary<BonusType, Func<BonusEffect>> _effectFactory;
//
//         public BonusManager()
//         {
//             _effectFactory = new Dictionary<BonusType, Func<BonusEffect>>()
//             {
//                 { BonusType.LongPaddle, () => new GameObject("LongPaddleBonus").AddComponent<LongPaddleBonus>() },
//                 { BonusType.MultiBall, () => new GameObject("MultiBallBonus").AddComponent<MultiBallBonus>() }
//             };
//
//             _bonusPool = new MiniIT.CORE.ObjectPool<BonusItem>(() =>
//             {
//                 GameObject obj = new GameObject("BonusItem");
//                 return obj.AddComponent<BonusItem>();
//             }, 5);
//         }
//
//         public void SpawnBonus(BonusType type, Vector3 position)
//         {
//             BonusItem bonus = _bonusPool.Rent();
//             bonus.transform.position = position;
//             bonus.SetType(type);
//             bonus.OnCaught += HandleCaught;
//         }
//
//         private void HandleCaught(BonusItem bonus, PaddleController paddle)
//         {
//             bonus.OnCaught -= HandleCaught;
//             _bonusPool.Return(bonus);
//
//             if (_effectFactory.TryGetValue(bonus.Type, out Func<BonusEffect> effectCreator))
//             {
//                 BonusEffect effect = effectCreator.Invoke();
//                 effect.Apply(paddle);
//
//                 if (effect.Duration > 0)
//                 {
//                     StartCoroutine(MiniIT.CORE.CoroutineHelper.WaitAndExecute(effect.Duration, () => effect.Revert(paddle)));
//                 }
//             }
//         }
//     }
// }