using System;
using System.Collections;
using Interfaces;
using TMPro;
using UnityEngine;
using VContainer;

public class InGameDebuggerUIReference : MonoBehaviour
{
    [SerializeField] private GameObject performanceUI;
    [SerializeField] private TextMeshProUGUI timer;
    private IMobSpawnService _mobSpawnService;

    private static GameObject _performanceUI;
    private static bool _enableDebugMode;
    private DateTime _startTime;
    private Coroutine _timerCoroutine;

    [Inject]
    public void Init(IMobSpawnService mobSpawnService)
    {
        _mobSpawnService = mobSpawnService;
        _mobSpawnService.EnableDebugMode = EnableDebugMode;
    }

    public static bool EnableDebugMode
    {
        get => _enableDebugMode;
        set
        {
            _enableDebugMode = value;
            _performanceUI?.SetActive(value);
        }
    }


    private void Awake()
    {
        _performanceUI = performanceUI;

        if (EnableDebugMode)
            ActivatePerformanceUI();
    }


    public static void ActivatePerformanceUI()
    {
        _performanceUI.SetActive(!_performanceUI.activeSelf);
    }

    public void StartTimer()
    {
        if (_timerCoroutine != null)
            StopCoroutine(_timerCoroutine);
        
        _startTime = DateTime.Now;
        _timerCoroutine = StartCoroutine(TimerTick());
    }

    private IEnumerator TimerTick()
    {
        timer.text = (DateTime.Now - _startTime).Seconds.ToString();
        
        yield return new WaitForSecondsRealtime(1);
        _timerCoroutine = StartCoroutine(TimerTick());
    }

    public void OnSpawn100()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(100);
    }

    public void OnSpawn200()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(200);
    }

    public void OnSpawn300()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(300);
    }

    public void OnSpawn400()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(400);
    }

    public void OnSpawn500()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(500);
    }

    public void OnSpawn600()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(600);
    }

    public void OnSpawn700()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(700);
    }

    public void OnSpawn800()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(800);
    }

    public void OnSpawn900()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(900);
    }


    public void OnSpawn1000()
    {
        StartTimer();
        _mobSpawnService.ClearActiveMobs();
        _mobSpawnService.SpawnRandomWithCount(1000);
    }
}
