using System;
using _Scripts.Utilities;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using GameCore.Health;
using GameCore.Player;
using GameCore.Spawner;
using Pathfinding;
using UnityEngine;
using Path = DG.Tweening.Plugins.Core.PathCore.Path;
using Random = UnityEngine.Random;

public class ZombieMovement : ZombieAnimator
{
    private Vector3[] _patrolPath;
    private protected Transform PlayerTransform;
    protected TweenerCore<Vector3, Path, PathOptions> PatrolTween;

    protected float EnemyAttackRange(PlayerMovementMode playerMovementMode)
    {
        var attackRange = EnemyType.attackRange; //+ ZombieConstants.ZombieAttackRange;
        if (playerMovementMode == PlayerMovementMode.Drive)
            attackRange += 0.5f;
        return attackRange;
    }

    protected override void Awake()
    {
        base.Awake();
        Follower = GetComponent<FollowerEntity>();
    }

    protected void SetDestinationToPlayer()
    {
        Follower.enabled = true;
        if (DOTween.IsTweening(transform))
            transform.DOKill();
        Follower.SetDestination(PlayerTransform.position);
    }

    protected void SetDestinationToObject(Transform targetObject)
    {
        Follower.enabled = true;
        if (DOTween.IsTweening(transform))
            transform.DOKill();
        Follower.SetDestination(targetObject.position);
    }

    protected void SetDestinationToPatrol()
    {
        if (IsDead)
        {
            DOTween.Kill(transform);
            return;
        }

        _patrolPath ??= GetClosetPath();
        if (DOTween.IsTweening(transform) || !Follower || _patrolPath.Length == 0 || _patrolPath == null)
            return;
        Follower.enabled = true;
        var pathDistance = Vector3.Distance(transform.position, _patrolPath[0]);
        if (pathDistance > 10)
        {
            Follower.SetDestination(_patrolPath[0]);
            return;
        }

        Follower.enabled = false;
        PatrolTween = transform.DOPath(_patrolPath, 20 - Follower.maxSpeed, PathType.Linear, PathMode.Full3D)
            .SetEase(Ease.Linear).SetLookAt(0.01f).OnComplete(SetPatrolPath);
    }

    protected void SetPatrolPath()
    {
        _patrolPath = GetClosetPath();
    }

    protected void SetNavigationStop(bool isStop)
    {
        if (gameObject == null || transform == null || Follower == null || this == null)
        {
            // TODO: Configure this null section through debug checks and remove it which is not necessary
            LoggerNS.LogError(
                $"Set navigation stop called total null check failed: this?.gameObject == {this?.gameObject == null}, transform == {transform == null}, Follower == {Follower == null}");
            return;
        }

        Follower.isStopped = isStop;
        Follower.canMove = !isStop;
    }

    public override void Reset()
    {
        if (this == null) return;
        if (transform)
            transform.DOKill();
        _patrolPath = null;
        SetNavigationStop(false);
        base.Reset();
    }

    protected float GetDistanceToPlayer => Vector3.Distance(transform.position, PlayerTransform.position);
    protected float GetDistanceToObject(IDamageable target)
    {
        return Vector3.Distance(transform.position, target.GetClosetPoint(transform.position));
    }

    private Vector3[] GetClosetPath()
    {
        var taggedObjects = GameObject.FindGameObjectsWithTag("PathObject");
        GameObject closetPathPoint = null;
        var minDistance = Mathf.Infinity;
        var currentPosition = transform.position;
        foreach (var obj in taggedObjects)
        {
            var distance = Vector3.Distance(currentPosition, obj.transform.position);
            if (!(distance < minDistance)) continue;
            minDistance = distance;
            closetPathPoint = obj;
        }

        if (!closetPathPoint) return null;
        var closetPath = closetPathPoint.GetComponent<AIPath>();
        if (!closetPath)
        {
            LoggerNS.LogError(closetPathPoint.name + " has no AIPath component");
            return null;
        }

        closetPath.InitPathPoints();
        var newPath = closetPath.pathPositions;
        for (int i = 0; i < newPath.Length; i++)
        {
            var newVector3 = newPath[i];
            newVector3.y = transform.position.y;
            newPath[i] = newVector3 + new Vector3(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
        }

        return newPath;
    }

    protected IDamageable GetClosetDamageable(IDamageable[] targetObjects)
    {
        if (targetObjects == null || targetObjects.Length == 0) return MobManager.TargetPlayer.GetDamageable;
        IDamageable closetPathPoint = null;
        var minDistance = Mathf.Infinity;
        var currentPosition = transform.position;
        foreach (var obj in targetObjects)
        {
            if (obj == null) continue;
            if (!obj.Transform) continue;
            if (obj.IsNotDamageable) continue;
            var distance = Vector3.Distance(currentPosition, obj.Transform.position);
            if (obj.IsDead) continue;
            if (!(distance < minDistance)) continue;
            minDistance = distance;
            closetPathPoint = obj;
        }

        return closetPathPoint;
    }
}
