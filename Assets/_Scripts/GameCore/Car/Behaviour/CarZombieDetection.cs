using Cathei.LinqGen;
using GameCore.AI;
using GameCore.Health;
using GameCore.Player;
using GameCore.Scriptables;
using Unity.Cinemachine;
using UnityEngine;


public class CarZombieDetection : MonoBehaviour
{
    #region Fields

    private CarController _carController;
    private CinemachineImpulseSource _impulseSource;
    private string _collisionDamageSkillId;

    private const float CollisionRadius = 3f;
    private const float VelocityThreshold = 0.1f;
    private const float ZombieJumpAngleThreshold = 135f;
    private const float ZombieDirectJumpAngleThreshold = 45f;
    private DamageInfo _damageInfo = new DamageInfo();
    private float _carCollisionDamageMultiplier = 25f;

    #endregion

    #region Properties

    public float CarCollisionDamageMultiplier => _carCollisionDamageMultiplier;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        _carController = GetComponent<CarController>();
        _impulseSource = GetComponent<CinemachineImpulseSource>();
    }


    private void FixedUpdate()
    {
        if (!IsCarMoving()) return;
        DetectAndHandleNearbyZombies();
    }

    #endregion

    #region Public Methods

    public void AdjustCollisionDamage(UpgradeDetail upgradeDetail)
    {
        if (upgradeDetail.type != StatUpgradeType.CollisionDamage)
        {
            return;
        }

        PlayerSkillController.Calculate(ref _carCollisionDamageMultiplier, ref _collisionDamageSkillId, upgradeDetail);
    }

    public void ResetCollisionDamage()
    {
        PlayerSkillController.ResetSkill(ref _carCollisionDamageMultiplier, _collisionDamageSkillId);
    }

    #endregion


    #region Private Methods

    private void DetectAndHandleNearbyZombies()
    {
        if (_carController.MobManager == null) return;

        foreach (var mob in _carController.MobManager.ActiveMobs.Gen().Where(IsMobValid))
        {
            if (IsWithinRadius(mob.Position) && ShouldZombieJump(mob))
            {
                JumpZombieTowardsCar(mob);
            }
        }
    }


    private bool ShouldZombieJump(IDamageable mob)
    {
        var directionToMob = (mob.Position - transform.position).normalized;
        var angleToMob = Vector3.Angle(transform.forward, directionToMob);

        if (angleToMob >= ZombieJumpAngleThreshold) return false;

        return angleToMob < ZombieDirectJumpAngleThreshold || Vector3.Dot(transform.right, directionToMob) > 0;
    }


    private void JumpZombieTowardsCar(IDamageable mob)
    {
        _carController.CarEffectController.PlayHitParticle(mob.Position);
        mob.TakeDamageFromVehicle(transform.position, _carController.GetCarSpeed * _carCollisionDamageMultiplier);
    }


    private bool IsCarMoving() => _carController.CharacterController.velocity.magnitude >= VelocityThreshold;
    private bool IsWithinRadius(Vector3 position) => Vector3.Distance(position, transform.position) < CollisionRadius;
    private bool IsMobValid(IDamageable mob) => mob != null && !mob.IsDead;
    private float GetCarDamageByDirection(float angle) => angle < 45 ? 1 : 5;

    #endregion
}
