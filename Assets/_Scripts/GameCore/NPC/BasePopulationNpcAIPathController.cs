using System.Collections;
using UnityEngine;

public class BasePopulationNpcAIPathController : MonoBehaviour
{
    [SerializeField] private AIPath aiPath;
    [SerializeField] private int[] idleIndexes;
    [SerializeField] private bool isPending;
    [SerializeField] private bool isInfinite = true;
    [SerializeField] private float targetProximityThreshold = 2f;

    private int _aiPathIndex;
    private Animator _animator;
    private static readonly int IsIdleAnimation = Animator.StringToHash("isIdle");
    private int nextIdleIndex = 0;
    private bool isIdle = false;
    private float moveSpeed = 4f;
    private float rotationSpeed = 5f;

    public bool IsPending
    {
        get => isPending;
        set
        {
            if (!value)
            {
                if (_animator == null)
                {
                    _animator = GetComponentInChildren<Animator>();
                }

                _animator.SetBool(IsIdleAnimation, false);
            }

            isPending = value;
        }
    }

    public bool IsCompleted { get; set; }
    public Transform TargetTransform { get; set; }

    public float MoveSpeed
    {
        get => moveSpeed;
        set => moveSpeed = value;
    }

    public bool IsIdle
    {
        set => isIdle = value;
    }


    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (isPending)
        {
            _animator.SetBool(IsIdleAnimation, true);
            return;
        }

        if (isIdle || aiPath.pathPoints.Length == 0) return;

        Vector3 targetPosition = GetPathOrTargetPosition();

        if (TargetTransform != null &&
            Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(TargetTransform.position.x, TargetTransform.position.z)) <= targetProximityThreshold)
        {
            StopMovement();
            return;
        }

        MoveTowardsTarget(targetPosition);
    }

    private void MoveTowardsTarget(Vector3 targetPosition)
    {
        var direction = (targetPosition - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z), rotationSpeed * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(transform.position, new Vector3(targetPosition.x, transform.position.y, targetPosition.z), moveSpeed * Time.deltaTime);


        if (Vector3.Distance(transform.position, targetPosition) < 0.5f)
        {
            _aiPathIndex++;

            if (_aiPathIndex == idleIndexes[nextIdleIndex])
            {
                SetIdle();

                if (idleIndexes.Length > 1)
                {
                    nextIdleIndex++;
                    if (nextIdleIndex >= idleIndexes.Length)
                        nextIdleIndex = 0;
                }
            }

            if (_aiPathIndex >= aiPath.pathPoints.Length)
                _aiPathIndex = 0;
        }
    }

    private void StopMovement()
    {
        _animator.SetBool(IsIdleAnimation, true);
        isIdle = true;
        if (!isInfinite)
        {
            IsCompleted = true;
        }
    }

    private Vector3 GetPathOrTargetPosition()
    {
        if (TargetTransform != null &&
            Vector3.Distance(transform.position, TargetTransform.position) > targetProximityThreshold)
        {
            return TargetTransform.position;
        }

        return GetPathPosition();
    }

    private void SetIdle()
    {
        StopMovement();
        if (!isInfinite)
        {
            return;
        }

        StartCoroutine(SetWalkingAfterDelay(10));
    }

    private IEnumerator SetWalkingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        isIdle = false;
        _animator.SetBool(IsIdleAnimation, false);
    }

    private Vector3 GetPathPosition()
    {
        if (_aiPathIndex >= aiPath.pathPoints.Length)
            _aiPathIndex = 0;
        return aiPath.pathPoints[_aiPathIndex].position;
    }
}