using System;
using _Scripts.GameCore.NPC;
using GameCore.Player;
using GameCore.Spawner;
using Interfaces;
using UnityEngine;
using VContainer;

public class PlayerBaseDistanceChecker : MonoBehaviour
{
    private PlayerController _playerController;
    private MobManager _mobManager;
    private BasePopulationNpcsManager _basePopulationNpcsManager;
    private float _distanceToBase;
    private int _secondsOutsideBase;
    private DifficultyLevel _difficultyLevel;
    private bool basePopulationNpcsSetted = false;
    private ITutorialService _tutorialService;

    public enum DifficultyLevel
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }
    public float DistanceToBase
    {
        get => _distanceToBase;
        set => _distanceToBase = value;
    }
    
    public int SecondsOutsideBase
    {
        get => _secondsOutsideBase;
        set => _secondsOutsideBase = value;
    }

    [Inject]
    private void Init(ITutorialService tutorialService)
    {
        _tutorialService = tutorialService;
    }

    private void Awake()
    {
        _playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        InvokeRepeating(nameof(CheckDistanceToBase), 0f, 1f);
    }
    
    public void SetMobManager(MobManager mobManager)
    {
        if(mobManager == null) return;
        
        _mobManager = mobManager;
        
        _mobManager.SetDifficultyLevel(1);
    }
    
    public void SetBasePopulationNpcsManager(BasePopulationNpcsManager basePopulationNpcsManager)
    {
        if(basePopulationNpcsManager == null) return;
        
        _basePopulationNpcsManager = basePopulationNpcsManager;
    }

    private void CheckDistanceToBase()
    {
        if (_playerController == null) return;
        if (!_tutorialService.IsTutorialCompleted) return;
        if (_playerController.CenterOfBase == null) return;
        if(_basePopulationNpcsManager == null) return;
        if (_playerController.WaveManager.IsWaveActive)
        {
            if(_mobManager != null)
            {
                _mobManager.SetDifficultyLevel(1);
            }
            
            return;
        }
        if (_playerController.InBase)
        {
            _secondsOutsideBase = 0;
            
            if(_mobManager != null)
            {
                _mobManager.SetDifficultyLevel(1);
            }
            
            return;
        }

        _secondsOutsideBase++;

        _distanceToBase = Vector3.Distance(transform.position, _playerController.CenterOfBase.position);
        
        if(_distanceToBase < 50 && !basePopulationNpcsSetted)
        {
            _basePopulationNpcsManager.SetEnableAllChildren(true);
            basePopulationNpcsSetted = true;
        }
        else if(_distanceToBase >= 50 && basePopulationNpcsSetted)
        {
            _basePopulationNpcsManager.SetEnableAllChildren(false);
            basePopulationNpcsSetted = false;
        }
        
        _difficultyLevel = (DifficultyLevel)CalculateDifficultyLevel();
        
        switch (_difficultyLevel)
        {
            case DifficultyLevel.Easy:
                _mobManager.SetDifficultyLevel(1);
                break;
            case DifficultyLevel.Medium:
                _mobManager.SetDifficultyLevel(2);
                break;
            case DifficultyLevel.Hard:
                _mobManager.SetDifficultyLevel(3);
                break;
        }
    }
    
    private int CalculateDifficultyLevel()
    {
        // Normalize distance and time to a 0-1 range
        float normalizedDistance = Mathf.Clamp01((_distanceToBase-15) / 250f);
        float normalizedTime = Mathf.Clamp01(_secondsOutsideBase / 300f);
        
        float difficultyFactor = Mathf.Max(normalizedDistance, normalizedTime);

        return difficultyFactor switch
        {
            <= 0.4f => 1,
            <= 0.8f => 2,
            _ => 3
        };
    }


    private void OnDisable()
    {
        CancelInvoke(nameof(CheckDistanceToBase));
    }
}