using System;
using System.Collections.Generic;
using _Scripts.GameCore.NPC;
using _Scripts.Utilities;
using _Utilities;
using DG.Tweening;
using GameCore.Player;
using Interfaces;
using MyBox;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class InGameTutorialIndicator : MonoBehaviour
{
    [Header("Indicator Settings")] [SerializeField]
    private SpriteRenderer indicatorImage;

    [SerializeField] private Transform targetArrowTransform;

    [SerializeField] private Transform indicatorParent;


    [SerializeField] private List<IndicatorTarget> tutorialIndicatorTargets;
    [SerializeField] private string hideIndicatorStepName;

    private readonly float _scaleAnimationSpeed = 0.5f;
    private Camera _mainCamera;
    private Transform _playerTransform;
    private PlayerController _playerController;
    private ManagementNpcController _managementNpcController;
    private Transform _target;
    private ITutorialService _tutorialService;
    private IndicatorTarget? _currentIndicatorTarget;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void Start()
    {
        AnimateIndicatorScale();
    }

    private void Update()
    {
        if (_mainCamera == null)
        {
            return;
        }

        if (_target == null) return;

        UpdateIndicator();
        UpdateTargetIndicator();
    }

    private void OnDestroy()
    {
        UnsubscribeFromTutorialEvents();
    }

    [Inject]
    private void Initialize(ITutorialService tutorialService, PlayerController playerController,
        ManagementNpcController managementNpcController)
    {
        _playerTransform = playerController.transform;
        _playerController = playerController;
        _tutorialService = tutorialService;
        _managementNpcController = managementNpcController;
        SubscribeToTutorialEvents();
    }

    #region Serializable Structures

    [Serializable]
    public struct IndicatorTarget
    {
        [Header("Target Settings")] public string TutorialStepName;
        public string IndicatorTargetName;

        [Header("Target Arrow Settings")] public bool hasTargetArrow;

        [ConditionalField(nameof(hasTargetArrow), false)]
        public float arrowOffset;

        [ConditionalField(nameof(hasTargetArrow), false)]
        public bool hasTargetArrowMoved;
    }

    #endregion

    #region Tutorial Event Handlers

    private void SubscribeToTutorialEvents()
    {
        _playerController.PlayerInCarStateChanged += SetIndicatorPositionByMovementMode;
        _tutorialService.TutorialStepChanged += OnTutorialStepChanged;
        _tutorialService.TutorialStepCompleted += OnTutorialStepCompleted;
        _managementNpcController.OnShowIndicator += SetTargetByName;
        _managementNpcController.OnDisableIndicator += HideIndicator;
    }

    private void UnsubscribeFromTutorialEvents()
    {
        _playerController.PlayerInCarStateChanged -= SetIndicatorPositionByMovementMode;
        _tutorialService.TutorialStepChanged -= OnTutorialStepChanged;
        _tutorialService.TutorialStepCompleted -= OnTutorialStepCompleted;
        _managementNpcController.OnShowIndicator -= SetTargetByName;
        _managementNpcController.OnDisableIndicator -= HideIndicator;
    }


    private void OnTutorialStepChanged(string stepName)
    {
        SetIndicatorPositionByMovementMode(_playerController.PlayerMovementMode);
        foreach (var indicatorTarget in tutorialIndicatorTargets)
        {
            if (indicatorTarget.TutorialStepName == stepName)
            {
                _currentIndicatorTarget = indicatorTarget;
                hideIndicatorStepName = indicatorTarget.TutorialStepName;
                SetTargetByIndicatorTarget(indicatorTarget);
                return;
            }
        }

        // HideIndicator(); // Hide indicator if no target found
    }

    private void OnTutorialStepCompleted(string stepName)
    {
        if (stepName == hideIndicatorStepName)
            HideIndicator();
    }

    #endregion

    #region Indicator Logic

    private void UpdateIndicator()
    {
        if (!indicatorImage.gameObject.activeSelf)
        {
            return;
        }

        var screenPosition = _mainCamera.WorldToScreenPoint(_target.position);
        var distance = Vector3.Distance(_playerTransform.position, _target.position);
        if (distance >= 1) // Only update rotation if distance is 1 or more
        {
            UpdateIndicatorPositionAndRotation(screenPosition);
        }

        UpdateIndicatorColor(distance); // Always update color based on distance
    }

    private void UpdateTargetIndicator()
    {
        if (_target == null)
        {
            return;
        }

        if (_currentIndicatorTarget is not { hasTargetArrow: true, hasTargetArrowMoved: true })
        {
            return;
        }

        targetArrowTransform.transform.LookAt(_mainCamera.transform);
        targetArrowTransform.position = new Vector3(_target.transform.position.x,
            _target.transform.position.y + _currentIndicatorTarget.Value.arrowOffset, _target.transform.position.z);
    }

    private void AnimateIndicatorScale()
    {
        var upScale = new Vector3(0.18f, 0.15f, 0.15f);
        var downScale = new Vector3(0.15f, 0.15f, 0.15f);
        indicatorImage.transform.DOScale(upScale, _scaleAnimationSpeed).SetLoops(-1, LoopType.Yoyo).From(downScale);
    }


    private void SetIndicatorPositionByMovementMode(PlayerMovementMode movementMode)
    {
        indicatorImage.transform.localPosition = new Vector3(indicatorImage.transform.localPosition.x,
            indicatorImage.transform.localPosition.y,
            SetIndicatorPosition(movementMode));
    }

    private void SetIndicatorPositionByMovementMode(bool inCar)
    {
        indicatorImage.transform.localPosition = new Vector3(indicatorImage.transform.localPosition.x,
            indicatorImage.transform.localPosition.y,
            SetIndicatorPosition(inCar ? PlayerMovementMode.Drive : PlayerMovementMode.Walk));
    }


    private void UpdateIndicatorPositionAndRotation(Vector3 screenPosition)
    {
        //var playerScreenPosition = Camera.main.WorldToScreenPoint(_playerTransform.position);
        //var direction = screenPosition - playerScreenPosition;
        //var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        indicatorParent.transform.LookAt(_target);
        //indicatorParent.transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void UpdateIndicatorColor(float distance)
    {
        indicatorImage.color = CalculateIndicatorColor(distance);
    }

    private Color CalculateIndicatorColor(float distance)
    {
        var color = Color.white;
        color.a = Helper.Remap(distance - 1, 1, 5, 0f, 1);

        if (distance < 0.4f)
            color.a = 0; // Fully transparent if too close

        return color;
    }

    #endregion

    #region Target Management

    private void SetTargetByIndicatorTarget(IndicatorTarget target)
    {
        var targetObject = GameObject.Find(target.IndicatorTargetName);
        indicatorImage.enabled = true;

        if (targetObject != null)
        {
            indicatorImage.gameObject.SetActive(true);
            if (target.hasTargetArrow)
            {
                targetArrowTransform.gameObject.SetActive(true);
                targetArrowTransform.position = new Vector3(targetObject.transform.position.x,
                    targetObject.transform.position.y + target.arrowOffset, targetObject.transform.position.z);
                targetArrowTransform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                targetArrowTransform.gameObject.SetActive(false);
            }

            SetTarget(targetObject.transform);
        }
        else
        {
            LoggerNS.LogError($"Target with name '{target.IndicatorTargetName}' not found.");
            HideIndicator();
        }
    }

    private void SetTargetByName(string targetName)
    {
        var targetObject = GameObject.Find(targetName);
        indicatorImage.enabled = true;

        if (targetObject != null)
        {
            indicatorImage.gameObject.SetActive(true);
            targetArrowTransform.gameObject.SetActive(false);
            SetTarget(targetObject.transform);
        }
        else
        {
            LoggerNS.LogError($"Target with name '{targetName}' not found.");
            HideIndicator();
        }
    }

    private void HideIndicator()
    {
        indicatorImage.gameObject.SetActive(false);
        targetArrowTransform.gameObject.SetActive(false);
    }

    private void SetTarget(Transform target)
    {
        _target = target;
        if (_playerTransform == null)
            LoggerNS.LogError("Player transform not found. Ensure the Player object is tagged properly.");

        indicatorImage.gameObject.SetActive(true);
    }


    private float SetIndicatorPosition(PlayerMovementMode movementMode)
    {
        return movementMode.Equals(PlayerMovementMode.Drive) ? 4.5f : 3f;
    }

    #endregion
}