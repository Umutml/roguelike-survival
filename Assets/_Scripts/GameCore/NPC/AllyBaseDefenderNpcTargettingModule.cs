using System;
using _Scripts.GameCore.NPC;
using Cysharp.Threading.Tasks;
using GameCore.Health;
using GameCore.Spawner;
using UnityEngine;
using VContainer;

public class AllyBaseDefenderNpcTargettingModule : MonoBehaviour
{
    [SerializeField] private Transform modelTransform;
    [SerializeField] private Transform modelSpineTransform;
    [SerializeField] private bool GetDatasFromScene = false;
    private AllyBaseDefenderAnimationModule _allyBaseDefenderAnimationModule;
    private MobManager _mobManager;
    private DamageNumberManager _damageNumberManager;
    private IDamageableRegisterService _damageableRegisterService;
    private IDamageable _currentTarget;
    private float findTargetCooldown = 1f;
    private DateTime _lastTargetAcquisitionTime = DateTime.MinValue;
    private Quaternion _targetRotation;

    private void Awake()
    {
        _allyBaseDefenderAnimationModule = GetComponent<AllyBaseDefenderAnimationModule>();
    }
    
    private void Start()
    {
        if (GetDatasFromScene)
        {
            _mobManager = FindFirstObjectByType<MobManager>();
        }
    }

    [Inject]
    public void Init(MobManager mobManager, DamageNumberManager damageNumberManager,
        IDamageableRegisterService damageableRegisterService)
    {
        if(GetDatasFromScene) return;
        
        _mobManager = mobManager;
        _damageNumberManager = damageNumberManager;
        _damageableRegisterService = damageableRegisterService;
    }

    private void Update()
    {
        FindTarget();
    }

    private void FindTarget()
    {
        if (_mobManager == null) return;
        
        if(_allyBaseDefenderAnimationModule.IsReloading) return;
        
        if (_lastTargetAcquisitionTime.AddSeconds(findTargetCooldown) < DateTime.Now)
        {
            _currentTarget = GetClosestDamageable(transform.position, 15);
            _allyBaseDefenderAnimationModule.SetCurrentTarget(_currentTarget);
            _lastTargetAcquisitionTime = DateTime.Now;
        }
        
        if (_currentTarget != null)
        {
            if(Vector3.Distance(transform.position,_currentTarget.Position) > 15)
            {
                _currentTarget = null;
                _allyBaseDefenderAnimationModule.SetCurrentTarget(_currentTarget);
                return;
            }
            // Body rotation
            
            Vector3 bodyLookDirection = _currentTarget.Position - transform.position;
            
            Quaternion targetBodyRotation = Quaternion.LookRotation(bodyLookDirection);
            var bodyRotation = Quaternion.Euler(0, targetBodyRotation.eulerAngles.y, 0);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                bodyRotation,
                Time.deltaTime * 480
            );
        }
        
    }
    
    private IDamageable GetClosestDamageable(Vector3 position, float range)
    {
        var closestMob = _mobManager.GetClosestMob(position, range, true);

        return closestMob;
    }
}
