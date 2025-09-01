using System;
using _Scripts.GameCore.AI.RagdollController;
using Cysharp.Threading.Tasks;
using GameCore.AI;
using GameCore.Scriptables;
using UnityEngine;
using static _Utilities.Helper;
using static ZombieStructure;
using Random = UnityEngine.Random;

public class ZombieAnimator : MobBase
{
    private static readonly int Dying = Animator.StringToHash("Dying");
    private static readonly int Attacking = Animator.StringToHash("Attacking");
    private static readonly int Walking = Animator.StringToHash("Walking");
    private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");
    private static readonly int AttackSpeed = Animator.StringToHash("AttackSpeed");
    private static readonly int Standup = Animator.StringToHash("Standup");
    private static readonly int StandupType = Animator.StringToHash("StandupType");
    private static readonly int Crash = Animator.StringToHash("Crash");
    private static readonly int DeathType = Animator.StringToHash("DeathType");
    private static readonly int CrashType = Animator.StringToHash("CrashType");
    private Animator _animator;
    [SerializeField] private Transform pelvisRoot;
    [SerializeField] protected GameObject shadow;
    [SerializeField] private RuntimeAnimatorController defaultAnimatorController;

    private const float _slowHitThreshold = 200;
    private bool _animatorEnteredCrashedState;

    protected override void Awake()
    {
        base.Awake();
        _animator = GetComponent<Animator>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        if (shadow) SwitchShadow(CurrentLOD);
    }

    public override void Reset()
    {
        _animatorEnteredCrashedState = false;
        if (_animator == null)
        {
            Debug.LogError("ZombieAnimator is null!"); // Control for zombie build null reference issue Rogu - 887
            return;
        }

        _animator.enabled = true;
        SetAnimatorState(ZombieState.Idle);
        base.Reset();
    }
    
    protected void SetZombieAnimator(EnemyType type)
    {
        var zombieType = type.enemyCategory;
        if (_animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }

        switch (zombieType)
        {
            case EnemyCategory.Spitter:
                _animator.runtimeAnimatorController = type.spitterRuntimeController;
                break;
            default:
                _animator.runtimeAnimatorController = defaultAnimatorController;
                break;
        }
    }


    protected void SetAnimatorState(ZombieState state)
    {
        if (_animator == null) return;

        switch (state)
        {
            case ZombieState.Idle:
                _animator.SetBool(Dying, false);
                _animator.SetBool(Attacking, false);
                _animator.SetBool(Walking, false);
                _animator.SetBool(Crash, false);
                break;
            case ZombieState.Run:
                _animator.SetBool(Dying, false);
                _animator.SetBool(Attacking, false);
                _animator.SetBool(Walking, true);
                _animator.SetBool(Crash, false);
                break;
            case ZombieState.Attack:
                _animator.SetBool(Dying, false);
                _animator.SetBool(Attacking, true);
                _animator.SetBool(Walking, false);
                _animator.SetBool(Crash, false);
                break;
            case ZombieState.Dead:
                if (shadow) shadow.SetActive(false);
                
                if(_animatorEnteredCrashedState) break; //don't play death animation for deaths after crash state
                _animator.SetFloat(DeathType, Random.Range(0, 5));
                _animator.SetBool(Dying, true);
                _animator.SetBool(Attacking, false);
                _animator.SetBool(Walking, false);
                _animator.SetBool(Crash, false);
                break;
            case ZombieState.Crashed:
                _animatorEnteredCrashedState = true;
                _animator.SetFloat(CrashType, ForcePower <= _slowHitThreshold ? 0 : Random.Range(0, 5));
                _animator.SetBool(Dying, false);
                _animator.SetBool(Attacking, false);
                _animator.SetBool(Walking, false);
                _animator.SetBool(Crash, true);
                break;
        }
    }

    protected override void OnLODChange(MobLOD newLOD)
    {
        base.OnLODChange(newLOD);
        SwitchShadow(newLOD);
    }

    protected async UniTask AnimateDie()
    {
        float jumpForwardForce;
        float slowHitMinimumLaunch = 0f;
        float slowHitMaximumLaunch = 7f;
        float fastHitMinimumLaunch = 10f;
        float fastHitMaximumLaunch = 30f;

        if (ForcePower <= _slowHitThreshold)
            jumpForwardForce = Remap(ForcePower, 0, _slowHitThreshold, slowHitMinimumLaunch, slowHitMaximumLaunch);
        else
            jumpForwardForce = Remap(ForcePower,
                _slowHitThreshold + 1,
                700,
                fastHitMinimumLaunch,
                fastHitMaximumLaunch);

        float jumpHeight;
        if (jumpForwardForce <= slowHitMaximumLaunch)
            jumpHeight = Remap(jumpForwardForce, 0, slowHitMaximumLaunch, 0f, 0.7f);
        else
            jumpHeight = Remap(jumpForwardForce, slowHitMaximumLaunch, 30, 0.7f, 2.5f);

        var direction = (transform.position - ForcePosition).normalized;
        var jumpTarget = transform.position + direction * jumpForwardForce;
        var rayPosition = transform.position;
        rayPosition.y += 1;
        if (Physics.Raycast(rayPosition, direction, out var hit, jumpForwardForce))
            jumpTarget = hit.point;
        jumpTarget.y += jumpHeight;
        await PerformJump(jumpTarget, jumpHeight, 1.5f);
        await UniTask.Delay(TimeSpan.FromSeconds(2));
    }

    private async UniTask PerformJump(Vector3 target, float jumpHeight, float duration)
    {
        var startPos = transform.position;
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            var t = elapsedTime / duration;
            var newPos = Vector3.Lerp(startPos, target, t);
            newPos.y += jumpHeight * Mathf.Sin(t * Mathf.PI);
            transform.position = newPos;
            await UniTask.Yield();
        }

        transform.position = target;
    }

    private void SwitchShadow(MobLOD newLOD)
    {
        switch (newLOD)
        {
            case MobLOD.Low:
                if (shadow) shadow.SetActive(false);
                break;
            case MobLOD.High:
                if (shadow) shadow.SetActive(true);
                break;
        }
    }

    protected void SetStandupState(bool isFaceDown)
    {
        _animator.SetTrigger(Standup);
        _animator.enabled = true;
        _animator.SetInteger(StandupType, isFaceDown ? 1 : 0);
        transform.position = pelvisRoot.position;
    }

    protected void SetMovementSpeed(float speed)
    {
        _animator.SetFloat(MovementSpeed, speed * 0.35f);
        DefaultSpeed = speed;
    }

    protected void SetAttackSpeed(float speed)
    {
        _animator.SetFloat(AttackSpeed, Mathf.Clamp(speed, 0.5f, 1.2f));
    }
}
