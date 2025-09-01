using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _Scripts.Utilities;
using _Utilities;
using Cathei.LinqGen;
using Cysharp.Threading.Tasks;
using GameCore.AI;
using GameCore.EnemyScaleFactor;
using GameCore.Health;
using GameCore.Player;
using GameCore.Player.Input;
using GameCore.Scriptables;
using GameCore.Tutorial;
using GameCore.Wave;
using Interfaces;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using Utilities;
using VContainer;
using static ObjectiveStructure;
using Random = UnityEngine.Random;

namespace GameCore.Spawner
{
    public class MobManager : MonoBehaviour, IMobSpawnService
    {
        #region Serializable Fields

        [SerializeField] private PlayerController player;
        [SerializeField] private WaveManager waveManager;
        [SerializeField] private EnemyScaleFactorManager enemyScaleFactorManager;
        [SerializeField] private LootDropManager lootDropManager;
        [SerializeField] private GameSceneSetupManager gameSceneSetupManager;
        [SerializeField] private MobList mobList;
        [SerializeField] private EnemyTypeData enemyTypeData;
        [SerializeField] private MobSpawnRangeData mobSpawnRangeData;
        [SerializeField] private Vector3 baseWorldPosition;


        [SerializeField] private bool debugMode;

        #endregion

        #region Fields

        //parameters

        private Coroutine _spawnerRoutine;
        public static PlayerController TargetPlayer;
        private readonly List<IDamageable> _activeMobs = new();
        private readonly List<IDamageable> _activeTutorialMobs = new();
        private readonly HashSet<Zombie> _activeMobsSet = new();
        private Camera _camera;
        private PlayerCarController _playerCarController;

        private readonly bool _isSpawning = true;
        private readonly WaitForSeconds _waitPatrol = new(10);
        private PlayerMovementController _playerMovementController;
        private GameParameters _gameParameters;
        private int _currentDifficultyLevel = 1;

        private int _burstSpawnMultipler = 2;       //to quickly fill the scene when waiting pool size is large
        private int _mobSpawnMarginFromCamera = 10; //margin outside the camera to spawn
        private int _mobCountPerSpawn = 3;
        private Tuple<int, int> _randomAngleInterval = new Tuple<int, int>(-25, 25);
        private readonly List<ZombieCountData> _mobCountData = new();
        private readonly Dictionary<ZombieType, int> _mobCount = new();
        private int _standardZombieCount;
        private int _bossZombieCount;
        private CancellationTokenSource _cancellationTokenSource;
        [SerializeField] private bool _ringSystemEnabled = false;
        private ITutorialService _tutorialService;
        private static bool _isFirstLoad = true;
        public static bool FastDespawnForBackMobs = false;

        #endregion

        #region Properties

        public bool IsLocked { get; set; }
        public float MobSpawnSpeed { get; set; } = 1f;
        public int MobCountPerSpawn { get; set; } = 3;
        public int MobSpawnAngle { get; set; } = 45;

        public int MaxMobCount => _tutorialService.IsTutorialCompleted && !waveManager.IsWaveActive
            ? ZombieConstants.FreeRoamZombieCount
            : 50;

        public List<IDamageable> ActiveMobs => _activeMobs;
        public HashSet<Zombie> ActiveMobsSet => _activeMobsSet;
        public List<IDamageable> ActiveTutorialMobs => _activeTutorialMobs;

        public UniTaskCompletionSource PoolCreationCompletionSource { get; } = new UniTaskCompletionSource();

        #endregion

        #region Events

        public static event Action<int> MobCountChanged;
        public static event Action OnZombieKilled;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            TargetPlayer = player;
            MobBase.WaveManager = waveManager;
            MobBase.EnemyScaleFactorManager = enemyScaleFactorManager;
            MobBase.LootDropManager = lootDropManager;

            _camera = Camera.main;
            _playerMovementController = player.GetComponent<PlayerMovementController>();
            _playerCarController = player.GetComponent<PlayerCarController>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            waveManager.WaveUpdated += OnWaveUpdated;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            waveManager.WaveUpdated -= OnWaveUpdated;

            if (_spawnerRoutine != null)
                StopCoroutine(_spawnerRoutine);
        }

        private async void Start()
        {
            AssignZombieCountsToDictionary();
            _activeMobs.Clear();
            await gameSceneSetupManager.SceneLoadTaskCompletionSource.Task;
            await CreatePools();
            if (EnableDebugMode) return;

            if (_tutorialService != null && !_tutorialService.IsTutorialCompleted)
                await UniTask.Delay(3000);

            SpawnMobs();

            InvokeRepeating(nameof(CheckZombieCountInPlayerRange), 0, 1);
        }

        private void CheckZombieCountInPlayerRange()
        {
            if (player == null || player.transform == null || player.AudioManager == null)
                return;
            var zombieCount = GetMobsInRange(player.transform.position, 10).Count;
            if (zombieCount >= 2)
                player.AudioManager.PlayZombieGroupSound();
            else
                player.AudioManager.StopZombieGroupSound();
        }
        private async Task CreatePools()
        {
            foreach (var mobPair in mobList.Mobs)
            {
                await ObjectManager.CreatePool(mobPair.Name, mobPair.PoolWarmupCount);
            }

            PoolCreationCompletionSource.TrySetResult();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.name.Equals("GameScene") && scene.isLoaded)
            {
                _activeMobs.Clear();
                _activeTutorialMobs.Clear();
                _activeMobsSet.Clear();

                if (!_isFirstLoad) ObjectManager.ClearAllPools();
                _isFirstLoad = false;
            }
        }

        public void SpawnMobs()
        {
            _spawnerRoutine = StartCoroutine(SpawnMobsCoroutine());
        }

        /// <summary>
        /// Debug only method to create single pool with count
        /// </summary>
        /// <param name="count">Mobs to spawn</param>
        private async void CreateTestPoolsWithCount(int count)
        {
            var mobPair = mobList.Mobs[0];
            await ObjectManager.CreatePool(mobPair.Name, count);
        }

        private string GetMobNameByChance(BehaviorType behaviorType = BehaviorType.Attacker, EnemyType enemyType = null)
        {
            if (waveManager.IsWaveActive && enemyType != null)
            {
                return enemyType.prefabPath;
            }

            var filteredMobs = !waveManager.IsWaveActive || !_tutorialService.IsTutorialCompleted
                ? mobList.Mobs.Where(mob => mob.Type.Equals(MobType.Standard))
                : mobList.Mobs;

            if (behaviorType == BehaviorType.Waiting)
            {
                filteredMobs = filteredMobs.Where(mob => mob.Type != MobType.Boss);
            }

            var mobPairs = filteredMobs as MobList.MobPair[] ?? filteredMobs.ToArray();
            var totalChance = mobPairs.Sum(mob => mob.SpawnChance);


            if (totalChance <= 0)
                return mobPairs.FirstOrDefault().Name ?? "StandardZombie";

            var roll = Random.Range(0, totalChance);
            float currentChance = 0;

            foreach (var mob in mobPairs)
            {
                currentChance += mob.SpawnChance;
                if (roll <= currentChance)
                    return mob.Name;
            }

            return mobPairs.First().Name;
        }

        private IEnumerator SpawnMobsCoroutine(bool debug = false)
        {
            if (_isSpawning)
            {
                SpawnMobRadialScatter();
            }

            yield return new WaitForSeconds(debug ? .25f : MobSpawnSpeed);
            _spawnerRoutine = StartCoroutine(SpawnMobsCoroutine(debug));
        }

        #endregion

        #region Public Methods

        public async Task<Zombie> SpawnMobAtPosition(Vector3 spawnPosition, BehaviorType behaviorType,
            bool isObjectiveMob = false,bool lateReturnPool=false, int enemyDifficulty = 10, params ObjectiveDamageable[] damageables)
        {
            var enemyType = enemyTypeData.GetRandomEnemyType(FreeRoamEnabled);
            var mobType = GetMobNameByChance(behaviorType, enemyType);
            if (waveManager.IsWaveActive)
            {
                if (_mobCountData is not {Count: > 0})
                {
                    return null;
                }

                var mobCountData = _mobCountData.Gen().Where(data => data.ZombieType == enemyType.zombieType)
                    .FirstOrDefault();
                if (_mobCount[enemyType.zombieType] >= mobCountData.Count)
                {
                    return null;
                }

                _mobCount[enemyType.zombieType]++;
            }
            else
            {
                var maxMobCount = isObjectiveMob ? 200 : GetMaximumSpawnCountPerRange();
                if (_activeMobs.Count >= maxMobCount)
                {
                    return null;
                }
            }

            GameObject mobInstance;
            try
            {
                mobInstance = await ObjectManager.GetObject(mobType, spawnPosition, Quaternion.identity);
            }
            catch (Exception e)
            {
                LoggerNS.LogError(e.Message);
                throw;
            }

            if (mobInstance == null) return null;
            if (!mobInstance.TryGetComponent(out Zombie zombieController)) return null;
            var damageableComponent = mobInstance.GetComponent<IDamageable>();
            if (isObjectiveMob)
                zombieController.SetBehaviourType(behaviorType, true,lateReturnPool, damageables);
            else
                zombieController.SetBehaviourType(player.InBase ? BehaviorType.Patrolling : behaviorType);
            zombieController.Setup(enemyType);
            enemyScaleFactorManager.ApplyScaleFactor(zombieController);

            void OnZombieReturnToPool(ReturnToPoolReason returnToPoolReason)
            {
                OnZombieKilled?.Invoke();
                UnregisterMob(returnToPoolReason, enemyType.zombieType, damageableComponent);
                zombieController.OnReturnToPool -= OnZombieReturnToPool;
            }

            zombieController.OnReturnToPool += OnZombieReturnToPool;
            mobInstance.transform.position = spawnPosition;
            mobInstance.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            _activeMobsSet.Add(zombieController);
            _activeMobs.Add(damageableComponent);
            MobCountChanged?.Invoke(_activeMobs.Count);
            return zombieController;
        }

        public void KillAllMobs()
        {
            foreach (var mob in _activeMobs)
            {
                if (mob is not Zombie zombie) continue;
                if (zombie.IsDead) continue;
                zombie.Die(DamageSource.Npc);
            }
        }

        public IDamageable GetClosestMob(Vector3 position, float? customDistance = null,
            bool addPaddingToViewPort = false)
        {
            IDamageable closestMob = null;

            float closestDistance = customDistance ?? Mathf.Infinity;

            foreach (var mob in _activeMobs.Gen().Where(dmgbl => dmgbl != null && !dmgbl.IsDead))
            {
                if (mob == null || mob.Transform == null) continue;

                var distance = Vector3.Distance(position, mob.Position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestMob = mob;
                }
            }

            if (addPaddingToViewPort)
            {
                if (closestMob != null && _camera.IsInViewport(closestMob.Position, 10f))
                    return closestMob;
            }


            if (closestMob != null && _camera.IsInViewport(closestMob.Position))
                return closestMob;

            return null;
        }

        public List<IDamageable> GetMobsInRange(Vector3 position, float maxDistance)
        {
            var mobsInRange = new List<IDamageable>();

            foreach (var mob in _activeMobs.Gen().Where(dmgbl => dmgbl != null && !dmgbl.IsDead))
            {
                if (mob == null) continue;

                var distance = Vector3.Distance(position, mob.Position);
                if (distance <= maxDistance && _camera.IsInViewport(mob.Position)) mobsInRange.Add(mob);
            }

            return mobsInRange;
        }

        public List<IDamageable> GetMobsInConeDirection(Vector3 position, Vector3 direction, float coneAngle,
            float maxDistance)
        {
            var mobsInConeAngle = new List<IDamageable>();

            foreach (var mob in _activeMobs.Gen().Where(dmgbl => dmgbl != null && !dmgbl.IsDead))
            {
                if (mob == null) continue;

                var distance = Vector3.Distance(position, mob.Position);
                if (distance <= maxDistance && _camera.IsInViewport(mob.Position))
                {
                    var angle = Vector3.Angle(direction, mob.Position - position);
                    if (angle <= coneAngle)
                    {
                        mobsInConeAngle.Add(mob);
                    }
                }
            }

            return mobsInConeAngle;
        }

        public bool IsAnyMobInView()
        {
            for (int i = 0; i < _activeMobs.Count; i++)
            {
                var mob = _activeMobs[i];
                if (mob != null && _camera.IsInViewport(mob.Position))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsEnemyInRangeAndVisible(float range)
        {
            if (_activeMobs is not {Count: > 0})
            {
                return false;
            }

            if (_camera == null)
            {
                return false;
            }

            if (player == null)
            {
                return false;
            }

            for (int i = 0; i < _activeMobs.Count; i++)
            {
                var mob = _activeMobs[i];

                if (mob == null || mob.Transform == null)
                {
                    continue;
                }

                if (_camera.IsInViewport(mob.Position) &&
                    Vector3.Distance(mob.Position, player.transform.position) < range)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetDifficultyLevel(int level)
        {
            _currentDifficultyLevel = level;
        }

        #endregion

        #region Private Methods

        private void AssignZombieCountsToDictionary()
        {
            _mobCount.Add(ZombieType.StandardZombie, _standardZombieCount);
            _mobCount.Add(ZombieType.ZombieBoss, _bossZombieCount);
        }

        private void OnWaveUpdated(Scriptables.Wave wave)
        {
            _mobCountData.Clear();

            _mobCountData.Add(new ZombieCountData
            {
                ZombieType = ZombieType.StandardZombie,
                Count = enemyTypeData.CalculateZombieCountForHorde(wave, ZombieType.StandardZombie)
            });

            _mobCountData.Add(new ZombieCountData
            {
                ZombieType = ZombieType.ZombieBoss,
                Count = enemyTypeData.CalculateZombieCountForHorde(wave, ZombieType.ZombieBoss)
            });
        }

        private void SpawnMobRadialScatter()
        {
            if (IsLocked || _activeMobs.Count >= MaxMobCount) return;
            
            if (player == null || _camera == null || _playerMovementController == null)
            {
                Debug.LogError("Required components are not initialized." + nameof(MobManager));
                return;
            }
            
            var spawnBehaviourPosition =
                GetPlayerTransform().position + GetPlayerTransform().forward * GetFieldOfViewRange();
            var spawnBehaviourState = GetBehaviourState(spawnBehaviourPosition);
            if (spawnBehaviourState.isOnlyTutorialActive && _tutorialService.IsTutorialCompleted) return;
            spawnBehaviourState.spawnCount *= _currentDifficultyLevel;

            switch (spawnBehaviourState.spawnType)
            {
                case SpawnType.Cluster:
                    for (int i = 0; i < spawnBehaviourState.spawnCount; i++)
                    {
                        float x = Random.Range(-20 / 2, 20 / 2);
                        float z = Random.Range(-20 / 2, 20 / 2);
                        var position = new Vector3(x, 0, z);
                        var spawnPosition = spawnBehaviourPosition + position;
                        var walkablePos = AstarPathHelper.FindNearestWalkablePosition(spawnPosition);
                        if (walkablePos == null) continue;
                        var behaviourState = spawnBehaviourState.GetBehaviourType();
                        _ = SpawnMobAtPosition((Vector3) walkablePos, behaviourState);
                    }

                    break;
                case SpawnType.PizzaSlice:
                    var multiplier = 1;
                    var playerTransform = GetPlayerTransform();

                    int availableMobCountToSpawn = GetMaximumSpawnCountPerRange() - _activeMobs.Count;
                    int mobCountToSpawn = Math.Min(_mobCountPerSpawn * 4, availableMobCountToSpawn);
                    var spawnCounts = GetWeightedSpawnPositions(mobCountToSpawn, multiplier);

                    for (int i = 0; i < 4; i++)
                    {
                        int mobSpawnCountForPosition;
                        switch (i)
                        {
                            case 0:
                                mobSpawnCountForPosition = spawnCounts.Top;
                                break;
                            case 1:
                                mobSpawnCountForPosition = spawnCounts.Right;
                                break;
                            case 2:
                                mobSpawnCountForPosition = spawnCounts.Bottom;
                                break;
                            case 3:
                                mobSpawnCountForPosition = spawnCounts.Left;
                                break;
                            default:
                                mobSpawnCountForPosition = 0;
                                break;
                        }

                        for (var j = 0; j < mobSpawnCountForPosition; j++)
                        {
                            var randomAngle = Random.Range(_randomAngleInterval.Item1, _randomAngleInterval.Item2);
                            float angle = randomAngle + i * 90;
                            var viewportPoint = _camera.WorldToViewportPoint(playerTransform.position);

                            var spawnPosition = playerTransform.position + Quaternion.Euler(0, angle, 0) *
                                Vector3.forward * (viewportPoint.x >= 0.5f ? 15 : 10);

                            while (_camera.IsInViewport(spawnPosition))
                                spawnPosition += Quaternion.Euler(0, angle, 0) * Vector3.forward *
                                    _mobSpawnMarginFromCamera;

                            var directionFromPlayer = (spawnPosition - playerTransform.position).normalized;
                            if (j > 0) spawnPosition += directionFromPlayer * j;

                            var walkablePos = AstarPathHelper.FindNearestWalkablePosition(spawnPosition, _camera);

                            if (walkablePos == null) continue;

                            if (debugMode)
                                Debug.DrawLine((Vector3) walkablePos,
                                    (Vector3) walkablePos + Vector3.up * 4,
                                    Color.red,
                                    MobSpawnSpeed);

                            var behaviourState = spawnBehaviourState.GetBehaviourType();
                            _ = SpawnMobAtPosition((Vector3) walkablePos, behaviourState);
                        }
                    }

                    break;
            }
        }

        public void SpawnObjectiveMob(Transform point, int mobCount, EnemyDifficulty enemyDifficulty,
            BehaviorType behaviorType,bool lateReturnPool, Action onKilled, params ObjectiveDamageable[] damageables)
        {
            var spawnBehaviourState = new SpawnBehaviorState();
            var spawnBehaviourPosition = point.position;
            while (_camera.IsInViewport(spawnBehaviourPosition))
            {
                var direction = (spawnBehaviourPosition - GetPlayerTransform().transform.position).normalized;
                spawnBehaviourPosition += direction * 10;
            }

            spawnBehaviourState.spawnCount = mobCount;
            spawnBehaviourState.spawnType = SpawnType.PizzaSlice;
            for (var i = 0; i < spawnBehaviourState.spawnCount; i++)
            {
                float x = Random.Range(-5, 5);
                float z = Random.Range(-5, 5);
                var position = new Vector3(x, 0, z);
                var spawnPosition = spawnBehaviourPosition + position;
                while (_camera.IsInViewport(spawnPosition))
                {
                    var direction = (spawnPosition - GetPlayerTransform().transform.position).normalized;
                    spawnPosition += direction * 5;
                }
                var walkablePos = AstarPathHelper.FindNearestWalkablePosition(spawnPosition) ?? spawnPosition;
                _ = SpawnObjectiveMobAtPosition(walkablePos, behaviorType,lateReturnPool, onKilled, enemyDifficulty, damageables);
            }
        }

        private async Task<Zombie> SpawnObjectiveMobAtPosition(Vector3 spawnPosition, BehaviorType behaviorType,bool lateReturnPool, Action onKilled, EnemyDifficulty enemyDifficulty, params ObjectiveDamageable[] damageables)
        {
            var enemyType = enemyTypeData.GetRandomEnemyType(true);
            var mobType = GetMobNameByChance(behaviorType, enemyType);
            GameObject mobInstance;
            try
            {
                mobInstance = await ObjectManager.GetObject(mobType, spawnPosition, Quaternion.identity);
            }
            catch (Exception e)
            {
                LoggerNS.LogError(e.Message);
                throw;
            }

            if (!mobInstance) return null;
            if (!mobInstance.TryGetComponent(out Zombie zombieController)) return null;
            var damageableComponent = mobInstance.GetComponent<IDamageable>();
            zombieController.SetBehaviourType(behaviorType, true,lateReturnPool, damageables);
            zombieController.Setup(enemyType, enemyDifficulty);
            zombieController.OnReturnToPool += OnZombieReturnToPool;
            zombieController.OnKilled += onKilled;
            mobInstance.transform.position = spawnPosition;
            mobInstance.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            _activeMobsSet.Add(zombieController);
            _activeMobs.Add(damageableComponent);
            MobCountChanged?.Invoke(_activeMobs.Count);
            return zombieController;

            void OnZombieReturnToPool(ReturnToPoolReason returnToPoolReason)
            {
                OnZombieKilled?.Invoke();
                UnregisterMob(returnToPoolReason, enemyType.zombieType, damageableComponent);
                zombieController.OnReturnToPool -= OnZombieReturnToPool;
                zombieController.OnKilled?.Invoke();
            }
        }

        private SpawnBehaviorState GetBehaviourState(Vector3 spawnPosition)
        {
            spawnPosition.y += 3;
            if (!Physics.CheckSphere(spawnPosition, 1)) return new SpawnBehaviorState();
            var colliders = Physics.OverlapSphere(spawnPosition, 1);
            foreach (var overlapCollider in colliders)
            {
                if (overlapCollider.CompareTag("BehaviourArea"))
                {
                    return overlapCollider.GetComponent<ZombieBehaviourArea>().spawnBehaviorState;
                }
            }

            return new SpawnBehaviorState();
        }

        private bool CheckNonSpawnPoint(Vector3 position)
        {
            if (!Physics.CheckSphere(position, 1)) return false;
            var colliders = Physics.OverlapSphere(position, 1);
            foreach (var overlapCollider in colliders)
            {
                if (overlapCollider.CompareTag("BehaviourArea"))
                {
                    return true;
                }
            }

            return false;
        }

        private float GetFieldOfViewRange()
        {
            return player.PlayerMovementMode.Equals(PlayerMovementMode.Drive) ? 70 : 45;
        }

        private void UnregisterMob(ReturnToPoolReason returnToPoolReason, ZombieType zombieType, IDamageable damageable)
        {
            if (returnToPoolReason == ReturnToPoolReason.OutOfBounds && waveManager.IsWaveActive)
            {
                _mobCount[zombieType] = Math.Max(_mobCount[zombieType] -= 1, 0);
            }

            _activeMobs.Remove(damageable);
            _activeTutorialMobs.Remove(damageable);
            MobCountChanged?.Invoke(_activeMobs.Count);
        }


        private Transform GetPlayerTransform()
        {
            return player.PlayerMovementMode.Equals(PlayerMovementMode.Drive)
                ? _playerCarController.CarController.transform
                : player.PlayerTransform;
        }


        private int GetMaximumSpawnCountPerRange()
        {
            if (!FreeRoamEnabled)
            {
                return mobSpawnRangeData.MobSpawnRanges.Last().MaxMobCount;
            }
            
            if (!RingSystemEnabled)
            {
                return mobSpawnRangeData.DefaultMobCount;
            }

            float range = Vector3.Distance(baseWorldPosition, player.transform.position);

            foreach (var mobSpawnRange in mobSpawnRangeData.MobSpawnRanges)
            {
                if (range <= mobSpawnRange.MaxRange)
                {
                    return mobSpawnRange.MaxMobCount;
                }
            }

            return mobSpawnRangeData.MobSpawnRanges.Last().MaxMobCount;
        }

        #endregion

        public void SpawnRandomWithCount(int count)
        {
            CreateTestPoolsWithCount(count);
            _spawnerRoutine = StartCoroutine(SpawnMobsCoroutine(true));
        }

        public void ClearActiveMobs()
        {
            if (_spawnerRoutine != null)
                StopCoroutine(_spawnerRoutine);

            _activeMobs.Clear();
        }

        private MobSpawnPositions GetWeightedSpawnPositions(int totalEnemies, int multiplier)
        {
            totalEnemies *= multiplier;
            MobSpawnPositions positions = new MobSpawnPositions();
            Vector2 input = _playerMovementController.MovementInput;

            if (totalEnemies <= 4)
            {
                positions.Top = positions.Bottom = positions.Left = positions.Right = 0;

                float absX = Mathf.Abs(input.x);
                float absY = Mathf.Abs(input.y);

                if (absX == 0 && absY == 0)
                {
                    int enemiesPerPosition = totalEnemies / 4;
                    int remainder = totalEnemies % 4;

                    positions.Top = positions.Bottom = positions.Left = positions.Right = enemiesPerPosition;

                    if (remainder >= 1) positions.Top++;
                    if (remainder >= 2) positions.Right++;
                    if (remainder >= 3) positions.Bottom++;
                    if (remainder >= 4) positions.Left++;

                    return positions;
                }

                while (totalEnemies > 0)
                {
                    if (input.y > 0 && input.y >= input.x && input.y >= -input.x)
                    {
                        positions.Top++;
                        totalEnemies--;
                    }
                    else if (input.y < 0 && -input.y >= input.x && -input.y >= -input.x)
                    {
                        positions.Bottom++;
                        totalEnemies--;
                    }
                    else if (input.x < 0)
                    {
                        positions.Left++;
                        totalEnemies--;
                    }
                    else
                    {
                        positions.Right++;
                        totalEnemies--;
                    }
                }

                return positions;
            }

            float totalWeight = Mathf.Abs(input.x) + Mathf.Abs(input.y);
            float topWeight = Mathf.Max(0, input.y);
            float bottomWeight = Mathf.Max(0, -input.y);
            float leftWeight = Mathf.Max(0, -input.x);
            float rightWeight = Mathf.Max(0, input.x);

            if (totalWeight == 0)
            {
                positions.Top = positions.Bottom = positions.Left = positions.Right = totalEnemies / 4;
                int remainder = totalEnemies % 4;

                if (remainder >= 1) positions.Top++;
                if (remainder >= 2) positions.Right++;
                if (remainder >= 3) positions.Bottom++;
                if (remainder >= 4) positions.Left++;
            }
            else
            {
                positions.Top = Mathf.RoundToInt(totalEnemies * (topWeight / totalWeight));
                positions.Bottom = Mathf.RoundToInt(totalEnemies * (bottomWeight / totalWeight));
                positions.Left = Mathf.RoundToInt(totalEnemies * (leftWeight / totalWeight));
                positions.Right = Mathf.RoundToInt(totalEnemies * (rightWeight / totalWeight));

                if (positions.Top + positions.Bottom + positions.Left + positions.Right < totalEnemies)
                {
                    int remaining = totalEnemies -
                        (positions.Top + positions.Bottom + positions.Left + positions.Right);

                    if (topWeight >= bottomWeight && topWeight >= leftWeight && topWeight >= rightWeight)
                        positions.Top += remaining;
                    else if (bottomWeight >= topWeight && bottomWeight >= leftWeight && bottomWeight >= rightWeight)
                        positions.Bottom += remaining;
                    else if (leftWeight >= topWeight && leftWeight >= bottomWeight && leftWeight >= rightWeight)
                        positions.Left += remaining;
                    else
                        positions.Right += remaining;
                }
            }

            return positions;
        }

        [Inject]
        private void Initialize(ITutorialService tutorialService)
        {
            _tutorialService = tutorialService;
        }

        [Serializable]
        public struct ZombieCountData
        {
            public ZombieType ZombieType;
            public int Count;
        }

        public enum ReturnToPoolReason
        {
            OutOfBounds,
            Killed,
        }

        public bool EnableDebugMode { get; set; }
        public bool FreeRoamEnabled { get; set; } = true;

        public bool RingSystemEnabled
        {
            get
            {
                if (!_ringSystemEnabled && _tutorialService.IsTutorialCompleted)
                    _ringSystemEnabled = true;

                return _ringSystemEnabled;
            }

            set => _ringSystemEnabled = value;
        }

        private struct MobSpawnPositions
        {
            public int Top;
            public int Bottom;
            public int Left;
            public int Right;
        }
    }
}
