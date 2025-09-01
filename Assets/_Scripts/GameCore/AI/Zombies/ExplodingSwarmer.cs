// using Cysharp.Threading.Tasks;
// using GameCore.Health;
// using GameCore.Spawner;
// using UnityEngine;
// using UnityEngine.AddressableAssets;
//
// public class ExplodingSwarmer : Zombie
// {
//     [SerializeField] private float explosionRadius = 5f;
//     [SerializeField] private AssetReferenceGameObject explosionVFXref;
//     private bool _hasExploded;
//
//     protected override async void OnDied(DamageSource damageSource)
//     {
//
//         // Handle explosion first
//         var playerController = MobManager.TargetPlayer;
//         if (Vector3.Distance(playerController.PlayerTransform.position, transform.position) <= explosionRadius)
//         {
//             playerController.GetDamageable.TakeDamage(new DamageInfo(_attackDamage));
//         }
//
//         // Spawn VFX
//         if (explosionVFXref != null)
//         {
//             var vfx = await ObjectManager.GetObject(explosionVFXref, transform.position, Quaternion.identity);
//             var particleComp = vfx.GetComponent<ParticleSystem>();
//             particleComp.Play();
//             ObjectManager.DisableObjectAfterTime(vfx.gameObject, particleComp.main.duration);
//         }
//
//         await UniTask.NextFrame();
//         base.OnDied(damageSource);
//     }
//
//     private void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, explosionRadius);
//     }
// }
