// using System;
// using System.Collections;
// using System.Threading.Tasks;
// using _Scripts.GameCore.AI.RagdollController;
// using _Scripts.GameCore.Vibration.Constants;
// using DG.Tweening;
// using GameCore.BuffSystem;
// using GameCore.Health;
// using GameCore.Player;
// using GameCore.Scriptables;
// using Pathfinding;
// using UnityEngine;
// using static ZombieStructure;
// using Random = UnityEngine.Random;
//
// namespace GameCore.AI
// {
//     public class ZombieController : MobBase
//     {
//        
//
//         #region Serializable Fields
//
//         [SerializeField] private bool canCrawl;
//         [Header("Movement")]
//         [SerializeField] private float walkSpeed = 2f;
//         [SerializeField] private float crawlSpeed = 1f;
//
//         #endregion
//
//
//         #region Fields
//
//         private bool _canMove;
//         private bool _died;
//
//         private BehaviorType _behaviorType;
//         private BehaviorType _currentBehaviorType;
//         private AIPath _aiPath;
//         private int _aiPathIndex;
//
//         private FollowerEntity _follower;
//         private Transform _playerTransform;
//         private bool _returningToPool, _isAttackOnCooldown;
//         private WalkMode _walkMode;
//         private PlayerCarController _playerCarController;
//         private IDamageable _damageable;
//         private float _carDamage;
//         private DamageInfo _damageInfo = new();
//         private bool _isCrawlClose;
//         private float _cachedSpeed;
//
//         #endregion
//
//
//         #region Properties
//
//         public static PlayerController Player { get; set; }
//
//         public bool IsCrawlClose
//         {
//             get => _isCrawlClose;
//             set
//             {
//                 _isCrawlClose = value;
//
//                 SetWalkMode(canCrawl, _isCrawlClose);
//             }
//         }
//
//         public BehaviorType CurrentBehaviorType
//         {
//             get => _currentBehaviorType;
//             set => _currentBehaviorType = value;
//         }
//
//         public float CarDamage { get => _carDamage; set => _carDamage = value; }
//
//         #endregion
//
//
//         #region Unity Methods
//
//         protected override void Awake()
//         {
//             base.Awake();
//             _follower = GetComponent<FollowerEntity>();
//
//             _playerStatusController = Player.GetComponent<PlayerStatusController>();
//             _playerCarController = Player.GetComponent<PlayerCarController>();
//             _damageNumberManager = Player.GetComponent<PlayerController>().DamageNumberManager;
//             Status = GetComponent<MobStatus>();
//             _damageable = Player.PlayerMovementMode.Equals(PlayerMovementMode.Drive)
//                 ? _playerCarController.CarController.CarStatusController
//                 : _playerStatusController;
//
//
//             Status.Died += OnDied;
//             _playerTransform = Player.transform;
//
//
//             Status.DebuffApplied += HandleDebuffApplied;
//             Status.DebuffRemoved += HandleDebuffRemoved;
//         }
//
//         public void SetBehaviourType(BehaviorType behaviourState)
//         {
//             if (behaviourState == BehaviorType.Patrolling)
//             {
//                 _aiPath = GetClosetPath(out _aiPathIndex);
//                 if (!_aiPath)
//                     behaviourState = BehaviorType.Attacker;
//             }
//
//             if (behaviourState == BehaviorType.Waiting)
//             {
//                 transform.localEulerAngles = new Vector3(0, Random.Range(0, 360), 0);
//             }
//             _behaviorType = behaviourState;
//         }
//
//         public void SetStaticZombie()
//         {
//             _returningToPool = true;
//         }
//
//         public override void Reset()
//         {
//             base.Reset();
//             Status.Reset();
//             _canMove = true;
//             _died = false;
//             _follower.isStopped = false;
//             _follower.enabled = true;
//             _ragdollController?.SetActiveRagdoll(false);
//             animator.enabled = true;
//             animator.SetBool("Dying", false);
//             animator.SetBool("Attacking", false);
//             animator.SetBool("Walking", false);
//             animator.SetBool("Crawling", false);
//         }
//
//         private void Start()
//         {
//             _canMove = true;
//         }
//
//         protected override void Update()
//         {
//             base.Update();
//
//             CheckDistanceAndAttack(); // Check distance to player stop moving and attack if close enough
//
//             if (!_canMove) return;
//
//             int frame = 1;
//             switch (CurrentLOD)
//             {
//                 case MobLOD.High:
//                     frame = 20;
//                     break;
//                 case MobLOD.Low:
//                     frame = 80;
//                     break;
//             }
//
//             if (_follower.enabled)
//             {
//                 if (Time.frameCount % frame == 0)
//                 {
//                     switch (_behaviorType)
//                     {
//                         case BehaviorType.Attacker:
//                             _follower.canMove = true;
//                             _follower.SetDestination(GetPlayerPosition().position);
//                             break;
//                         case BehaviorType.Patrolling:
//                             _follower.canMove = true;
//                             _follower.SetDestination(GetPathPosition());
//                             if (Vector3.Distance(transform.position, GetPathPosition()) < 1)
//                                 _aiPathIndex++;
//                             if (Vector3.Distance(transform.position, GetPlayerPosition().position) < 5f)
//                             {
//                                 _behaviorType = BehaviorType.Attacker;
//                             }
//
//                             break;
//                         case BehaviorType.Waiting:
//                             _follower.canMove = false;
//                             if (Vector3.Distance(transform.position, GetPlayerPosition().position) < 15f)
//                             {
//                                 if (!Player.InBase)
//                                 {
//                                     _behaviorType = BehaviorType.Attacker;
//                                     _follower.canMove = true;
//                                 }
//                             }
//
//                             break;
//                     }
//                 }
//             }
//
//             if (Time.frameCount % 30 == 0)
//                 CheckDistanceAndGoBackToPool();
//
//
//             if (CurrentLOD == MobLOD.Low) return;
//
//             if (_follower.canMove)
//             {
//                 switch (_walkMode)
//                 {
//                     case WalkMode.Walk:
//                         animator.SetBool("Walking", true);
//                         break;
//                     case WalkMode.Crawl:
//                         animator.SetBool("Crawling", true);
//                         break;
//                 }
//             }
//             else
//             {
//                 switch (_walkMode)
//                 {
//                     case WalkMode.Walk:
//                         animator.SetBool("Walking", false);
//                         break;
//                     case WalkMode.Crawl:
//                         animator.SetBool("Crawling", false);
//                         break;
//                 }
//             }
//         }
//
//         private void CheckDistanceAndAttack()
//         {
//             if (Vector3.Distance(transform.position, GetPlayerPosition().position) < 2f)
//             {
//                 _canMove = false;
//                 _follower.isStopped = true;
//
//                 animator.SetBool("Attacking",
//                     true); //TODO: this will be moved to state machine system. prototype use only.
//                 Attack();
//             }
//             else
//             {
//                 animator.SetBool("Attacking", false);
//                 _canMove = true;
//                 _follower.isStopped = false;
//             }
//         }
//
//
//         private void SetWalkMode(bool canCrawl, bool isCrawlClose)
//         {
//             if (canCrawl)
//             {
//                 if (isCrawlClose)
//                 {
//                     _walkMode = WalkMode.Walk;
//                 }
//                 else
//                 {
//                     _walkMode = (WalkMode) Random.Range(0, Enum.GetValues(typeof(WalkMode)).Length);
//                 }
//             }
//             else
//             {
//                 _walkMode = WalkMode.Walk;
//             }
//         }
//
//
//         private void Attack()
//         {
//             if (EnemyType == null)
//             {
//                 LoggerNS.LogError("EnemyType is null");
//                 return;
//             }
//
//             if (Status.IsDead)
//             {
//                 return;
//             }
//
//             //1 damage every 3 secs
//             if (_isAttackOnCooldown) return;
//             _isAttackOnCooldown = true;
//             _damageable = Player.PlayerMovementMode.Equals(PlayerMovementMode.Drive)
//                 ? _playerCarController.CarController.CarStatusController
//                 : _playerStatusController;
//             _damageInfo.Amount = Player.PlayerMovementMode.Equals(PlayerMovementMode.Drive)
//                 ? CarDamage
//                 : EnemyType.attackDamage;
//             _damageable.TakeDamage(_damageInfo);
//             if (Player.PlayerMovementMode.Equals(PlayerMovementMode.Drive))
//                 Player.VibrationManager.TriggerVibration(VibrationEnums.VibrationEventType.HitPlayer);
//
//             new Task(async () =>
//             {
//                 await Task.Delay((int) EnemyType.attackSpeed * 1000);
//                 _isAttackOnCooldown = false;
//             }).Start();
//         }
//
//         protected void OnDestroy()
//         {
//             if (Status)
//             {
//                 Status.Died -= OnDied;
//                 Status.DebuffApplied -= HandleDebuffApplied;
//                 Status.DebuffRemoved -= HandleDebuffRemoved;
//             }
//         }
//
//         #endregion
//
//
//         #region Public Methods
//
//         public override void Setup(EnemyType enemyType)
//         {
//             base.Setup(enemyType);
//             Status.Health = enemyType.health;
//             _follower.maxSpeed = _walkMode == WalkMode.Walk ? enemyType.movementSpeed : crawlSpeed;
//             animator.SetFloat("MovementSpeed", _follower.maxSpeed * 0.35f);
//         }
//
//         public void HitByVehicle(Vector3 carPosition, float collisionForce)
//         {
//             if (RagdollSettings.ActiveRagdoll)
//             {
//                 if (!_died)
//                 {
//                     SetDeathState();
//                     if (Player.IsCompletedTutorial)
//                     {
//                         ExecuteDrop();
//                     }
//
//                     ProcessPlayerStatus(DamageSource.Player);
//                     StartCoroutine(DisappearAfterDelay());
//                     if (RagdollSettings.ActiveRagdoll)
//                     {
//                         animator.enabled = false;
//                         _ragdollController?.SetActiveRagdoll(true, carPosition, collisionForce);
//                         StartCoroutine(DisableRagdollAfterDelay());
//                         return;
//                     }
//
//                     PlayDeathAnimation();
//                     _damageNumberManager.UseDamageNumber(transform.position, _carDamage.ToString(), false);
//                 }
//             }
//         }
//
//         #endregion
//
//
//         #region Private Methods
//
//         private void OnDied(DamageSource killedBy)
//         {
//             SetDeathState();
//
//             if (killedBy == DamageSource.Player && Player.IsCompletedTutorial)
//                 ExecuteDrop();
//
//             ProcessPlayerStatus(killedBy);
//             StartCoroutine(DisappearAfterDelay());
//             if (RagdollSettings.ActiveRagdoll)
//             {
//                 animator.enabled = false;
//                 _ragdollController?.SetActiveRagdoll(true, true);
//                 StartCoroutine(DisableRagdollAfterDelay());
//                 return;
//             }
//
//             PlayDeathAnimation();
//         }
//
//
//         private void SetDeathState()
//         {
//             _died = true;
//             _canMove = false;
//             _follower.isStopped = true;
//             _follower.enabled = false;
//         }
//
//         private void PlayDeathAnimation()
//         {
//             var deathType = Random.Range(0, 2);
//             animator.SetBool("Attacking", false);
//             animator.SetInteger("DeathType", deathType);
//             animator.SetBool("Dying", true);
//         }
//
//         private void ProcessPlayerStatus(DamageSource killedBy)
//         {
//             if (killedBy != DamageSource.Player)
//             {
//                 return;
//             }
//
//             _playerStatusController.RecordKill(EnemyType.baseXpDropValue);
//         }
//
//         private IEnumerator DisappearAfterDelay()
//         {
//             yield return new WaitForSeconds(2f); //TODO: Change this to a constant and will made optimization
//             var nextPosition = transform.position.y - 5;
//             transform.DOMoveY(nextPosition, 4f).OnComplete(GoToPool);
//         }
//
//         private IEnumerator DisableRagdollAfterDelay()
//         {
//             yield return new WaitForSeconds(1f);
//
//             _ragdollController?.SetActiveRagdoll(false);
//         }
//
//         private void GoToPool()
//         {
//             if (_returningToPool || PooledObject == null) return;
//             _returningToPool = true;
//             Reset();
//             PooledObject.Dispose();
//             OnReturnToPool?.Invoke();
//             _returningToPool = false;
//         }
//
//         private void CheckDistanceAndGoBackToPool()
//         {
//             if (!_returningToPool && !IsRendering())
//             {
//                 GoToPool();
//             }
//         }
//
//         private bool IsRendering()
//         {
//             bool isCloset;
//             if(Player.PlayerMovementMode.Equals(PlayerMovementMode.Drive))
//                 isCloset = Vector3.Distance(transform.position, GetPlayerPosition().position) < 75;
//             else
//                 isCloset = Vector3.Distance(transform.position, GetPlayerPosition().position) < 50;
//             return isCloset;
//         }
//
//
//         private Transform GetPlayerPosition() =>
//             Player.PlayerMovementMode.Equals(PlayerMovementMode.Drive)
//                 ? _playerCarController.CarController.transform
//                 : _playerTransform;
//
//         private Vector3 GetPathPosition()
//         {
//             if (_aiPathIndex >= _aiPath.pathPoints.Length)
//                 _aiPathIndex = 0;
//             return _aiPath.pathPoints[_aiPathIndex].position;
//         }
//
//
//         private AIPath GetClosetPath(out int closetIndex) //TODO : Refactor this method, maybe using AIPathManager class
//         {
//             var taggedObjects = GameObject.FindGameObjectsWithTag("PathObject");
//             GameObject closetPathPoint = null;
//             var minDistance = Mathf.Infinity;
//             var currentPosition = transform.position;
//             foreach (var obj in taggedObjects)
//             {
//                 var distance = Vector3.Distance(currentPosition, obj.transform.position);
//                 if (!(distance < minDistance)) continue;
//                 minDistance = distance;
//                 closetPathPoint = obj;
//             }
//
//             closetIndex = 0;
//             if (!closetPathPoint) return null;
//             var closetPath = closetPathPoint.transform.parent.GetComponent<AIPath>();
//             for (var i = 0; i < closetPath.pathPoints.Length; i++)
//             {
//                 if (closetPath.pathPoints[i].name == closetPathPoint.name)
//                     closetIndex = i;
//             }
//
//             return closetPath;
//         }
//
//         //TODO: This will be moved to mob movement controller when refactoring
//         private void HandleDebuffApplied(Debuff debuff)
//         {
//             switch (debuff.Type)
//             {
//                 case Debuff.Debufftype.Stun:
//                     _cachedSpeed = _follower.maxSpeed;
//                     _follower.maxSpeed = 0;
//                     animator.speed = 0;
//                     break;
//                 case Debuff.Debufftype.Slow:
//                     break;
//             }
//         }
//
//         //TODO: This will be moved to mob movement controller when refactoring
//         private void HandleDebuffRemoved(Debuff debuff)
//         {
//             switch (debuff.Type)
//             {
//                 case Debuff.Debufftype.Stun:
//                     _follower.maxSpeed = _cachedSpeed;
//                     animator.speed = 1;
//                     break;
//                 case Debuff.Debufftype.Slow:
//                     break;
//             }
//         }
//
//         #endregion
//     }
// }
