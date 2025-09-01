using System;
using System.Collections.Generic;
using System.Linq;
using _Scripts.GameCore.NPC;
using DG.Tweening;
using GameCore.Health;
using GameCore.Spawner;
using UnityEngine;
using VContainer;
using static ObjectiveStructure;
using IDamageable = GameCore.Health.IDamageable;
using Random = UnityEngine.Random;

public class NpcObjective : NpcObjectiveAnimator
{
    [SerializeField] private Transform modelTransform;
    [SerializeField] private Transform modelSpineTransform;
    [SerializeField] private GameObject[] npcModels;
    [SerializeField] private PatrolRoute[] patrolPoints;
    [SerializeField] private float moveSpeed=1.5f;
    [SerializeField] private NpcObjectiveSpeechBubble npcObjectiveSpeechBubble;
    private NpcState _npcState;
    private DamageNumberManager _damageNumberManager;
    private IDamageableRegisterService _damageableRegisterService;
    private IDamageable _currentTarget;
    private Quaternion _targetRotation;
    private NpcState NpcState
    {
        get => _npcState;
        set
        {
            if(_npcState!=value)
                OnNpcStateChanged(value);
            _npcState = value;
        }
    }

    private void Awake()
    {
        Transform = transform;
        MobManager = FindFirstObjectByType<MobManager>();
        mobHealthManager.gameObject.SetActive(true);
    }

    private void OnEnable() => Died += OnNpcDied;

    private void OnDisable() => Died -= OnNpcDied;
    public void SetNpcStats(NpcStats npcStats,NpcState npcState = NpcState.Idle)
    {
        maxHealth = npcStats.health;
        Health = maxHealth;
        FindTargetCooldown = npcStats.targetCooldown;
        attackRate = npcStats.attackRate;
        var npcWeapon = _rangedWeapon;
        npcWeapon.Damage = npcStats.damage;
        npcWeapon.Range = npcStats.attackRange;
        npcWeapon.FireInterval = npcStats.fireRate;
        for (var i = 0; i < npcModels.Length; i++)
            npcModels[i].SetActive(i==npcStats.npcType);
        if(npcStats.npcType<0 || npcStats.npcType>=npcModels.Length)
            npcModels[Random.Range(0,npcModels.Length)].SetActive(true);
        NpcState = npcState;
        SetAttackAnimationSpeed(attackRate);
        mobHealthManager?.SetHealthText(Health,maxHealth);
    }
    private void Update()
    {
        if(NpcState!=NpcState.Attack)
            return;
        OnNpcStateChanged(NpcState);
    }

    #region Objective Evets
    public void SetNpcState(FunctionParameter objectFunction)
    {
        if (!Enum.TryParse(objectFunction.GetParameter<string>(), out NpcState npcState))
            return;
        NpcState = npcState;
    }
    public void MovePath(FunctionParameter objectFunction)
    {
        if(NpcState==NpcState.Dead)
            return;
        transform.eulerAngles = new Vector3(0,transform.eulerAngles.y,0);
        var pathIndex = objectFunction.GetParameter<int>();
        var pathPositions = patrolPoints[pathIndex].points.Select(t => t.position).ToArray();
        if(pathPositions.Length<2)
            return;
        float pathDuration=0;
        for (var i = 1; i < pathPositions.Length; i++)
            pathDuration += Vector3.Distance(pathPositions[i - 1], pathPositions[i]);
        var oldState = NpcState;
        NpcState = NpcState.Move;
        transform.DOPath(pathPositions, pathDuration/moveSpeed).SetEase(Ease.Linear).SetLookAt(0.01f).OnComplete(() =>
        {
            NpcState = oldState;
        });
    }
    public void ShowDialog(FunctionParameter objectFunction)
    {
        if(NpcState==NpcState.Dead)
            return;
        npcObjectiveSpeechBubble?.ExecuteShowSpeechBubble(objectFunction.GetParameter<string>());
    }
    public void ChangeAnimationType(FunctionParameter objectFunction)
    {
        var animationParameter = objectFunction.GetParameter<string>();
        var animationValue = objectFunction.GetParameter<float>();
        SetAnimationType(animationParameter,animationValue);
    }
    public void SetNpcDamageable(FunctionParameter objectFunction)
    {
        var isDamageable = objectFunction.GetParameter<float>();
        IsNotDamageable = isDamageable > 0.5f;
    }
    #endregion
    
    public void SetNpcState(NpcState npcState) => NpcState = npcState;
    public override void TakeDamage(DamageInfo damageInfo)
    {
        base.TakeDamage(damageInfo);
    }
    private void OnNpcDied(DamageSource damageSource)
    {
        mobHealthManager.gameObject.SetActive(false);
        NpcState = NpcState.Dead;
    }
}
