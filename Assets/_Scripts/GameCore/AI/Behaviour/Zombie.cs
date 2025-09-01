using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _Scripts.GameCore.AI;
using _Scripts.Utilities;
using DG.Tweening;
using GameCore.Health;
using GameCore.Player;
using GameCore.Player.WeaponSystem.GameCore.Player.Weapon;
using GameCore.Scriptables;
using GameCore.Spawner;
using JetBrains.Annotations;
using UnityEngine;
using static ObjectiveStructure;
using static ZombieStructure;
using Random = UnityEngine.Random;

public class Zombie : ZombieMovement
{
    private ZombieState _currentState;

    private ZombieState CurrentState
    {
        get => _currentState;
        set
        {
            if (_currentState != value)
            {
                OnZombieStateChanged(value);
            }

            _currentState = value;
        }
    }

    private BehaviorType _currentBehaviour;
    private PlayerController _playerController;
    private float _attackTime;
    private IDamageable[] _targetObjects;
    internal Action OnKilled;
    private GameObject _idleAuraVFX;

    private IDamageable TargetObject
    {
        get
        {
            var target = GetClosetDamageable(_targetObjects);
            if (target != null) return target;
            SetBehaviourType(BehaviorType.Attacker);
            return null;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        _playerController = MobManager.TargetPlayer;
        PlayerTransform = _playerController.transform;
        PlayerStatusController = _playerController.GetComponent<PlayerStatusController>();
        DamageNumberManager = _playerController.GetComponent<PlayerController>().DamageNumberManager;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Died += OnDied;
        Crashed += OnCrashed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        Died -= OnDied;
        Crashed -= OnCrashed;
    }

    public void Setup(EnemyType enemyType, EnemyDifficulty enemyDifficulty = null)
    {
        EnemyType = enemyType;
        _xpDropChance = (int) enemyType.xpDropChance;
        _coinDropChance = (int) enemyType.softCurrencyChance;
        _attackSpeed = enemyType.attackSpeed;
        maxHealth = enemyType.health;
        Health = enemyType.health;
        Follower.maxSpeed = enemyType.movementSpeed;
        _attackDamage = enemyType.attackDamage;
        if (enemyDifficulty != null)
        {
            maxHealth *= enemyDifficulty.healthDifficulty / 5f;
            Health = maxHealth;
            _attackSpeed *= enemyDifficulty.attackSpeed / 5f;
            _attackDamage *= enemyDifficulty.attackDamage / 5f;
        }

        healthManager.gameObject.SetActive(false);
        SetZombieAnimator(enemyType);
        if(usesDifferentSkins) SetZombieSkin();
        SetMovementSpeed(enemyType.movementSpeed);
        SetAttackSpeed(_attackSpeed);
        SpecificTypeSetupActions();
    }

    private void SetZombieSkin()
    {
        if (_cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
        }
        _cancellationTokenSource = new CancellationTokenSource();
        SetSkinInstance(_cancellationTokenSource.Token);
    }

    public void Setup()
    {
        if (EnemyType is null)
        {
            return;
        }

        _xpDropChance = (int) EnemyType.xpDropChance;
        _coinDropChance = (int) EnemyType.softCurrencyChance;
        maxHealth = EnemyType.health;
        Health = EnemyType.health;
        _attackSpeed = EnemyType.attackSpeed;
        Follower.maxSpeed = EnemyType.movementSpeed;
        _attackDamage = EnemyType.attackDamage;
        SetMovementSpeed(EnemyType.movementSpeed);
        SetAttackSpeed(_attackSpeed);
    }

    public void SetEnemyScaleFactor(ScaleCondition condition)
    {
        switch (condition.scaleType)
        {
            case ScaleType.Health:
                Health = EnemyScaleFactorManager.CalculateScaleFactor(Health, condition);
                maxHealth = maxHealth < Health ? Health : maxHealth;
                break;
            case ScaleType.MovementSpeed:
                Follower.maxSpeed = EnemyScaleFactorManager.CalculateScaleFactor(Follower.maxSpeed, condition);
                SetMovementSpeed(Follower.maxSpeed);
                break;
            case ScaleType.AttackDamage:
                _attackDamage = EnemyScaleFactorManager.CalculateScaleFactor(_attackDamage, condition);
                break;
            case ScaleType.AttackSpeed:
                _attackSpeed = EnemyScaleFactorManager.CalculateScaleFactor(_attackSpeed, condition);
                SetAttackSpeed(_attackSpeed);
                break;
            case ScaleType.XPDrop:
                _xpDropChance = (int) EnemyScaleFactorManager.CalculateScaleFactor(_xpDropChance, condition);
                break;
        }
    }

    public void SetBehaviourType(BehaviorType behaviourState, bool objectiveSpawn = false, bool lateReturnPool = false,
        params IDamageable[] targetObject)
    {
        AlwaysUpdate = objectiveSpawn;
        LateReturnToPool = lateReturnPool;
        if (targetObject.Length > 0)
        {
            var newTargetObjects = targetObject.ToList();
            newTargetObjects.Add(PlayerStatusController);
            _targetObjects = newTargetObjects.ToArray();
            _currentBehaviour = BehaviorType.AttackToObject;
            return;
        }

        LateReturnToPool = false;

        if (behaviourState == BehaviorType.AttackToObject && targetObject.Length == 0)
        {
            _currentBehaviour = BehaviorType.Attacker;
            behaviourState = BehaviorType.Attacker;
        }

        _currentBehaviour = behaviourState;
        switch (behaviourState)
        {
            case BehaviorType.Attacker:
                break;
            case BehaviorType.Patrolling:
                SetPatrolPath();
                break;
            case BehaviorType.Waiting:
                transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
                break;
        }
    }

    public void OnAttackHappened()
    {
        AttackToPlayer();
    }

    protected override void Update()
    {
        if (!MainCamera) return;
        base.Update();
        if (!IsUpdate && !AlwaysUpdate) return;
        if (Health <= 0 && CurrentState != ZombieState.Dead)
        {
            _isDead = false;
            Die();
        }

        switch (_currentBehaviour)
        {
            case BehaviorType.AttackToObject:
                switch (CurrentState)
                {
                    case ZombieState.Idle:
                        CurrentState = ZombieState.Run;
                        break;
                    case ZombieState.Run:
                        if (TargetObject == null)
                            _currentBehaviour = BehaviorType.Attacker;
                        if (GetDistanceToObject(TargetObject) < EnemyAttackRange(_playerController.PlayerMovementMode))
                            CurrentState = ZombieState.Attack;
                        else if (TargetObject != null) SetDestinationToObject(TargetObject.Transform);
                        break;
                    case ZombieState.Attack:
                        _attackTime += Time.deltaTime * EnemyType.attackSpeed;
                        if (_attackTime >= ZombieConstants.ZombieAttackCooldown)
                        {
                            AttackToTarget();
                            _attackTime = 0;
                        }

                        if (GetDistanceToObject(TargetObject) > EnemyAttackRange(_playerController.PlayerMovementMode))
                            CurrentState = ZombieState.Run;
                        break;
                }

                break;
            case BehaviorType.Attacker:
                if (_playerController.InBase && !IsDead)
                    SetBehaviourType(BehaviorType.Patrolling);
                switch (CurrentState)
                {
                    case ZombieState.Idle:
                        CurrentState = ZombieState.Run;
                        break;
                    case ZombieState.Run:
                        if (GetDistanceToPlayer < EnemyAttackRange(_playerController.PlayerMovementMode))
                            CurrentState = ZombieState.Attack;
                        else if (!_playerController.InBase)
                            SetDestinationToPlayer();
                        break;
                    case ZombieState.Attack:
                        // old attack timing
                        //_attackTime += Time.deltaTime * EnemyType.attackSpeed;
                        // if (_attackTime >= ZombieConstants.ZombieAttackCooldown)
                        // {
                        //     AttackToPlayer();
                        //     _attackTime = 0;
                        // }

                        if (GetDistanceToPlayer > EnemyAttackRange(_playerController.PlayerMovementMode) &&
                            !_playerController.InBase)
                            CurrentState = ZombieState.Run;
                        break;
                    case ZombieState.Dead:
                        break;
                }

                break;
            case BehaviorType.Patrolling:
                if(IsDead) return;
                SetDestinationToPatrol();
                CurrentState = ZombieState.Run;
                if (GetDistanceToPlayer < ZombieConstants.ZombiePatrolDetectionRadius && !_playerController.InBase)
                    _currentBehaviour = BehaviorType.Attacker;
                break;
            case BehaviorType.Waiting:
                CurrentState = ZombieState.Idle;
                if (GetDistanceToPlayer < ZombieConstants.ZombieWaitingDetectionRadius && !_playerController.InBase)
                    _currentBehaviour = BehaviorType.Attacker;
                break;
        }
    }

    private async void OnCrashed()
    {
        if (!this || CurrentState == ZombieState.Dead || CurrentState == ZombieState.Crashed) return;
        SetNavigationStop(true);
        CurrentState = ZombieState.Crashed;
        _playerController.PlayOneShotAudio("ZombieCrashed");

        if (Health < 1 || !_tutorialService.IsTutorialCompleted)
        {
            IsHealthbarDisabled = true;                //force the helathbar to stay shut
            healthManager.gameObject.SetActive(false); //if it's going to die, disable the health bar beforehand
        }

        await AnimateDie();
        if (Health < 1 || !_tutorialService.IsTutorialCompleted)
        {
            Die();
        }
        else
        {
            var isFaceDown = Vector3.Dot(transform.up, Vector3.up) < 0;
            SetStandupState(isFaceDown);
            await Task.Delay(1000);
            IsCrashed = false;
            CurrentState = ZombieState.Run;
        }

        SetNavigationStop(false);
    }

    private async void OnDied(DamageSource damageSource)
    {
        if (CurrentState == ZombieState.Dead || _isDead) return;
        if (Follower) Follower.enabled = false;
        PatrolTween?.Kill();
        OnKilled?.Invoke();
        OnKilled = null;
        CurrentState = ZombieState.Dead;
        SpecificZombieDeadActions();
        if (damageSource == DamageSource.Player)
        {
            var random = Random.Range(1, 9);
            var zombieDeadSound = $"ZombieDead{random}";
            _playerController.PlayOneShotAudio(zombieDeadSound, .5f);
            if (_tutorialService.IsTutorialCompleted)
                ExecuteDrop();
            PlayerStatusController.RecordKill(EnemyType);
        }

        await Task.Delay(2000);
        SetDissolve(true);
        await Task.Delay(2000);
        GoToPool(MobManager.ReturnToPoolReason.Killed);
        if (Follower) Follower.enabled = true;
    }

    private void SpecificZombieDeadActions()
    {
        switch (EnemyType.enemyCategory)
        {
            case EnemyCategory.ExplodingSwarmer:
                Explode();
                break;
            case EnemyCategory.ToxicBrute:
                DisableIdleAuraVFX();
                break;
        }
    }

    private void DisableIdleAuraVFX()
    {
        if (_idleAuraVFX == null) return;
        _idleAuraVFX?.GetComponent<ParticleSystem>().Stop();
        _idleAuraVFX?.SetActive(false);
        _idleAuraVFX = null;
    }

    private void SpecificTypeSetupActions()
    {
        switch (EnemyType.enemyCategory)
        {
            case EnemyCategory.ToxicBrute:
                SetupToxicBrute();
                break;
        }
    }

    private async void SetupToxicBrute()
    {
        _idleAuraVFX = await ObjectManager.GetObject(EnemyType.toxicVFXReference, transform.position + Vector3.up);
        _idleAuraVFX.transform.rotation = Quaternion.Euler(-90, 0, 0); // Set the rotation of the aura default
        _idleAuraVFX.transform.SetParent(transform);
        _idleAuraVFX.GetComponent<ParticleSystem>().Play();
    }

    private async void Explode() // Exploding on death zombies
    {
        var explosionRadius = EnemyType.attackRange * 4;
        var ourPosition = transform.position;
        // Player Damage Section
        if (IsInRange(ourPosition, _playerController.PlayerTransform.position, explosionRadius))
            _playerController.GetDamageable.TakeDamage(new DamageInfo(_attackDamage));
        // Target Damageable Section
        if (IsInRange(ourPosition, TargetObject.Transform.position, explosionRadius))
            TargetObject?.TakeDamage(new DamageInfo(_attackDamage));
        // Spawn Explosion VFX
        if (EnemyType.deathVFXReference != null)
        {
            var vfx = await ObjectManager.GetObject(EnemyType.deathVFXReference, transform.position + Vector3.up);
            var particleComp = vfx.GetComponent<ParticleSystem>();
            particleComp.Play();
            ObjectManager.DisableObjectAfterTime(vfx.gameObject, particleComp.main.duration);
        }
    }
    
    private bool IsInRange(Vector3 sourcePosition, Vector3 targetPosition, float range)
    {
        return Vector3.Distance(sourcePosition, targetPosition) <= range;
    }

    private async void ThrowProjectile()
    {
        var projectile = await ObjectManager.GetObject(EnemyType.projectileReference, transform.position, Quaternion.identity);
        if (projectile is null)
            return;
        var parabolicProjectile = projectile.GetComponent<MobParabolicProjectile>();
        if (parabolicProjectile is null)
            return;
        if (TargetObject == null)
            return;
        TurnToPlayer();
        if (TargetObject.Transform == PlayerTransform)
            parabolicProjectile.Setup(FirePoint, _playerController.GetDamageable, null, new DamageInfo(_attackDamage), default, null, _playerController);
        else
            parabolicProjectile.Setup(FirePoint, TargetObject, null, new DamageInfo(_attackDamage), default, null, _playerController);
    }

    private void TurnToPlayer()
    {
        var direction = PlayerTransform.position - transform.position;
        direction.y = 0; // Zero out the Y component to constrain rotation to the Y-axis
        var rotation = Quaternion.LookRotation(direction);
        transform.rotation = rotation;
    }

    private void AttackToTarget()
    {
        if (TargetObject == null)
        {
            SetBehaviourType(BehaviorType.Attacker);
            return;
        }

        switch (EnemyType.enemyCategory)
        {
            case EnemyCategory.Spitter:
                ThrowProjectile();
                break;
            case EnemyCategory.ExplodingSwarmer:
                Die(); // Exploding on die action 
                break;
            default:
                if (TargetObject.Transform == PlayerTransform)
                    _playerController.GetDamageable.TakeDamage(new DamageInfo(_attackDamage));
                else
                    TargetObject?.TakeDamage(new DamageInfo(_attackDamage));
                break;
        }
    }

    private void AttackToPlayer()
    {
        if (!EnemyType)
        {
            LoggerNS.LogError("EnemyType is null");
            return;
        }

        switch (EnemyType.enemyCategory)
        {
            case EnemyCategory.Spitter:
                ThrowProjectile();
                break;
            case EnemyCategory.ExplodingSwarmer:
                Die(); // Exploding on die action
                break;
            default:
                _playerController.GetDamageable.TakeDamage(new DamageInfo(_attackDamage));
                break;
        }
    }

    private void OnZombieStateChanged(ZombieState state)
    {
        _attackTime = 0;
        SetAnimatorState(state);
        switch (state)
        {
            case ZombieState.Idle:
                SetNavigationStop(true);
                break;
            case ZombieState.Run:
                SetNavigationStop(false);
                break;
            case ZombieState.Attack:
                SetNavigationStop(true);
                break;
            case ZombieState.Dead:
                SetNavigationStop(true);
                break;
        }
    }

    public override void Reset()
    {
        CurrentState = ZombieState.Idle;
        _currentBehaviour = BehaviorType.Waiting;
        _targetObjects = null;
        DisableIdleAuraVFX();
        base.Reset();
    }
}
