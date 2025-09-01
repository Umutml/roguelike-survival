using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using _Scripts.Utilities;
using _Utilities;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using GameCore.EnemyScaleFactor;
using GameCore.Scriptables;
using GameCore.Spawner;
using GameCore.Wave;
using Interfaces;
using MyBox;
using UnityEngine;
using Utilities;
using VContainer;
using static ZombieStructure;
using Random = UnityEngine.Random;

namespace GameCore.AI
{
    public class MobBase : MobStatus
    {
        #region Serializable Fields

        [SerializeField] protected bool debug;
        [SerializeField] protected List<Transform> damagePoints;
        [SerializeField] private MonoBehaviour highlighter;
        [SerializeField] private Transform rootBone;
        [SerializeField] private Transform[] skinnedMeshRendererbones;
        [SerializeField] private SkinnedMeshRenderer sampleSkinnedMeshRenderer;
        [SerializeField] protected bool usesDifferentSkins = true;
        [SerializeField] private string[] possibleStandardZombieSkins;
        [SerializeField] private ZombieType zombieType;
        [SerializeField] private Transform firePoint;
        

        #endregion

        #region Enums

        public enum ZombieType
        {
            Standard,
            Boss
        }

        #endregion

        #region Fields

        protected int _xpDropChance;
        protected int _coinDropChance;
        private int _currentSkinIndex;
        protected Camera MainCamera;
        protected bool _forceUpdate = true;
        protected bool _initialHighLODTransitionMade = false;
        protected int _poolRotationID = 0;
        protected float _attackDamage;
        protected float _attackSpeed;
        private SkinnedMeshRenderer _skinnedMeshRenderer;
        protected CancellationTokenSource _cancellationTokenSource;
        private MobLOD _currentLOD;
        private GameObject _skinInstance;
        private Transform _skinPoolOriginalParent;
        private bool _isFirstEnable = true;
        protected bool AlwaysUpdate,LateReturnToPool;
        protected string Skinname;
        private TweenerCore<float, float, FloatOptions> _tween;
        protected ITutorialService _tutorialService;
        private bool _isOutOfBoundsPoolerRunning = false;

        protected MobLOD CurrentLOD
        {
            get => _currentLOD;
            set
            {
                if (_currentLOD != value)
                {
                    _poolRotationID++;
                    try
                    {
                        OnLODChange(value);
                    }
                    catch (Exception e)
                    {
                        LoggerNS.LogError($"Error in OnLODChange: {e.Message}");
                    }
                }

                _currentLOD = value;
            }
        }

        protected bool IsUpdate
        {
            get
            {
                if (_forceUpdate)
                {
                    _forceUpdate = false;
                    return true;
                }

                var frameRate = CurrentLOD == MobLOD.High ? 15 : 120;
                return Time.frameCount % frameRate == 0;
            }
        }
        
        protected Transform FirePoint
        {
            get
            {
                if (firePoint)
                    return firePoint;
                return transform;
            }
        }


        #endregion

        #region Properties

        protected EnemyType EnemyType { get; set; }
        public static WaveManager WaveManager { get; set; }
        public static EnemyScaleFactorManager EnemyScaleFactorManager { get; set; }
        public static LootDropManager LootDropManager { get; set; }

        #endregion

        #region Unity Methods

        protected virtual void Awake()
        {
            MobBase = this;
            MainCamera = Camera.main;
        }

        protected virtual void OnEnable()
        {
            //cts only persists per enable-disable cycle due to pooling
            _cancellationTokenSource = new CancellationTokenSource();
            AlwaysUpdate = false;
            LateReturnToPool = false;
            
            _poolRotationID++;
            SetLOD();
        }

        protected virtual void OnDisable()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
            }
            _isOutOfBoundsPoolerRunning = false;
        }

        private void OnDestroy()
        {
            if (_cancellationTokenSource != null)
            {
                try
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = null;
                }
                catch
                {
                }
            }
        }

        protected virtual void Update()
        {
            SetLOD();
            ToggleSkin();
        }

        protected void SetDissolve(bool isActive)
        {
            if (!_skinnedMeshRenderer)
                return;
            if (!isActive)
            {
                _skinnedMeshRenderer.material.SetFloat("_DissolveFloat", 0);
                return;
            }

            _tween = _skinnedMeshRenderer.material.DOFloat(1, "_DissolveFloat", 2f);
        }

        //This method ensures that the dissolve effect is disabled
        protected async void CheckDissolveEnable(CancellationToken token)
        {
            if (token.IsCancellationRequested || _skinnedMeshRenderer == null)
                return;

            try
            {
                await UniTask.Delay(1000, cancellationToken: token);
            }
            catch (OperationCanceledException e)
            {
                return;
            }

            if (_skinnedMeshRenderer != null && _skinnedMeshRenderer.material.GetFloat("_DissolveFloat") > 0.9f)
                _skinnedMeshRenderer.material.SetFloat("_DissolveFloat", 0);
        }

        protected virtual void OnLODChange(MobLOD newLOD)
        {
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!debug) return;
            EditorHelper.DrawString($"Lod: {CurrentLOD.ToString()}", transform.position + Vector3.up, Color.red);
            EditorHelper.DrawString($"Skin: {Skinname}", transform.position + Vector3.up * 2, Color.red);
        }
#endif

        #endregion
        
        #region Public Methods

        public Transform SelectRandomValidDamagePoint() => damagePoints.PickRandom();

        /// <summary>
        /// Only used to set bones from a sample skinned mesh renderer
        /// </summary>
        [ButtonMethod]
        public void SetBones()
        {
            if (sampleSkinnedMeshRenderer)
                skinnedMeshRendererbones = sampleSkinnedMeshRenderer.bones;
        }

        public async void SetSkinInstance(CancellationToken token)
        {
            if (_tween != null)
                _tween.Kill();
            var skinKey = await GetZombieSkinKey();
            
            if (string.IsNullOrEmpty(skinKey))
            {
                Debug.LogError("Skin key is null or empty. Cannot load skin.");
                return;
            }
            try
            {
                _skinInstance = await ObjectManager.GetObject(skinKey, Vector3.zero, Quaternion.identity, token);
            }
            catch (OperationCanceledException e)
            {
                //on task cancel
                return;
            }

            Skinname = _skinInstance.name;
            _skinPoolOriginalParent = _skinInstance.transform.parent;
            _skinInstance.transform.parent = transform;
            _skinInstance.transform.localPosition = Vector3.zero;
            _skinInstance.transform.localRotation = Quaternion.identity;

            _skinnedMeshRenderer = _skinInstance.GetComponent<SkinnedMeshRenderer>();
            _skinnedMeshRenderer.rootBone = rootBone;
            _skinnedMeshRenderer.bones = skinnedMeshRendererbones;

            var previousLOD = CurrentLOD;
            CurrentLOD = MobLOD.High;
            _skinnedMeshRenderer.enabled = true;
            SetDissolve(false);
            CurrentLOD = previousLOD;
            CheckDissolveEnable(token);
        }

        private Task<string> GetZombieSkinKey()
        {
            return Task.FromResult(!string.IsNullOrEmpty(EnemyType.skinKey) ? EnemyType.skinKey : possibleStandardZombieSkins.PickRandom());
        }

        #endregion

        #region Private Methods

        [Inject] //injects on the basis of gamescene scope
        private void Initialize(ITutorialService tutorialService)
        {
            _tutorialService = tutorialService;
        }

        private void ToggleSkin()
        {
            if (!_skinnedMeshRenderer) return;

            switch (CurrentLOD)
            {
                case MobLOD.Low:
                    _skinnedMeshRenderer.enabled = false;
                    break;
                case MobLOD.High:
                    _skinnedMeshRenderer.enabled = true;
                    break;
            }
        }

        private async void SpawnDropPod(int chance, int count, DropPodType type)
        {
            if (!Helper.CalculateRngChange(chance))
            {
                return;
            }

            var podObject = await LootDropManager.GetDropObject(type,
                transform.position + new Vector3(Random.Range(-1f, 1f), 1, Random.Range(-1f, 1f)));
            podObject.GetComponent<IDropItem>().Initialize(count);
        }

        protected void ExecuteDrop()
        {
            if (WaveManager is null || LootDropManager is null)
            {
                return;
            }

            if (WaveManager.IsWaveActive)
            {
                SpawnDropPod(_xpDropChance, (int)EnemyType.xpDropValue, DropPodType.Xp);
            }

            var currencyDropMaxCount = (int)(WaveManager.IsWaveActive
                ? EnemyType.maxSoftCurrencyInWave
                : EnemyType.maxSoftCurrencyInFreeRoam);

            var currencyDropMinCount = (int)(WaveManager.IsWaveActive
                ? EnemyType.minSoftCurrencyInWave
                : EnemyType.minSoftCurrencyInFreeRoam);

            var type = new List<DropPodType> { DropPodType.Coin, DropPodType.Gem }.PickRandom();

            if (currencyDropMaxCount <= 0 || currencyDropMinCount <= 0)
            {
                return;
            }

            SpawnDropPod(_coinDropChance, Random.Range(currencyDropMinCount, currencyDropMaxCount), type);
        }

        protected void GoToPool(MobManager.ReturnToPoolReason returnToPoolReason)
        {
            if (this == null) return;

            if (zombieType == ZombieType.Standard && _skinInstance)
            {
                _skinInstance.SetActive(false);
                _skinInstance.transform.SetParent(_skinPoolOriginalParent);
                _skinInstance.transform.localPosition = Vector3.zero;
                _skinInstance.transform.localRotation = Quaternion.identity;
            }

            Reset();
            gameObject.SetActive(false);
            OnReturnToPool?.Invoke(returnToPoolReason);
        }

        #endregion

        #region IPoolable Members

        public Action<MobManager.ReturnToPoolReason> OnReturnToPool { get; set; }

        #endregion

        #region IResettable Members

        public override void Reset()
        {
            base.Reset();
            SetDissolve(false);
            _forceUpdate = true;
            _initialHighLODTransitionMade = false;
            _skinPoolOriginalParent = null;
            _skinInstance = null;
            _skinnedMeshRenderer = null;
            Skinname = string.Empty;
        }

        #endregion

        private void SetLOD()
        {
            if (MainCamera.IsInViewport(transform.position, 0.15f))
            {
                CurrentLOD = MobLOD.High;

                if (!_initialHighLODTransitionMade)
                {
                    _initialHighLODTransitionMade = true;
                    OnInitialTransitionToHighLOD();
                }
            }
            else
            {
                CurrentLOD = MobLOD.Low;
                
                if (!_isOutOfBoundsPoolerRunning && _cancellationTokenSource != null)
                {
                    _isOutOfBoundsPoolerRunning = true;
                    OutOfBoundsPooler(_poolRotationID, _cancellationTokenSource.Token);
                }
            }
        }

        //This is called only once when the mob is first transitioned to high LOD (comes into view)
        private void OnInitialTransitionToHighLOD()
        {
            _forceUpdate = true; //force update without waiting for the frame count once to prevent delays
        }

        private async void OutOfBoundsPooler(int methodRotationID, CancellationToken token)
        {
            try
            {
                if (token.IsCancellationRequested || methodRotationID != _poolRotationID)
                {
                    _isOutOfBoundsPoolerRunning = false;
                    return;
                }

                await UniTask.Delay(
                    1000, 
                    DelayType.DeltaTime,
                    PlayerLoopTiming.Update, 
                    token);
                
                if (token.IsCancellationRequested || methodRotationID != _poolRotationID || MobBase == null)
                {
                    _isOutOfBoundsPoolerRunning = false;
                    return;
                }

                if (MobManager.FastDespawnForBackMobs)
                {
                    var toMob = MobBase.transform.position - MobManager.TargetPlayer.transform.position;
                    var dotProduct = Vector3.Dot(toMob.normalized, MobManager.TargetPlayer.transform.forward);

                    if (dotProduct < 0)
                    {
                        _isOutOfBoundsPoolerRunning = false;
                        GoToPool(MobManager.ReturnToPoolReason.OutOfBounds);
                        return;
                    }
                }
                
                if (token.IsCancellationRequested || methodRotationID != _poolRotationID || MobBase == null)
                {
                    _isOutOfBoundsPoolerRunning = false;
                    return;
                }
                
                int delaySeconds = MobManager.TargetPlayer.PlayerMovementMode.Equals(PlayerMovementMode.Drive) ? 2 : 4;
                if (LateReturnToPool)
                    delaySeconds = 60;
                
                await UniTask.Delay(
                    TimeSpan.FromSeconds(delaySeconds),
                    DelayType.DeltaTime,
                    PlayerLoopTiming.Update,
                    token);
                
                if (CurrentLOD == MobLOD.Low && methodRotationID == _poolRotationID && MobBase != null)
                {
                    _isOutOfBoundsPoolerRunning = false;
                    GoToPool(MobManager.ReturnToPoolReason.OutOfBounds);
                }
                else
                {
                    _isOutOfBoundsPoolerRunning = false;
                }
            }
            catch (OperationCanceledException)
            {
                _isOutOfBoundsPoolerRunning = false;
            }
            catch (Exception ex)
            {
                _isOutOfBoundsPoolerRunning = false;
                Debug.LogWarning($"Exception in OutOfBoundsPooler: {ex.Message}");
            }
        }
    }
}