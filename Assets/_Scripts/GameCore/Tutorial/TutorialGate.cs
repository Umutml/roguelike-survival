using System;
using _Scripts.Utilities;
using DG.Tweening;
using GameCore.DynamicGridObstacle;
using Interfaces;
using UnityEngine;
using VContainer;

namespace _Scripts.GameCore.Tutorial
{
    public class TutorialGate : MonoBehaviour
    {
        [SerializeField] private DynamicObstacle dynamicObstacle;
        [SerializeField] private float openedXGap = 6f;
        [SerializeField] private float animSpeed = 1f;
        private Vector3 _closedPosition;
        private IAnalyticsService _analyticsService;

        private void Awake()
        {
            _closedPosition = transform.position;
        }

        private void Start()
        {
            dynamicObstacle.ToggleObstacle(true);
            dynamicObstacle.ToggleTrigger(false);
        }

        [Inject]
        public void Initialize(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public void OpenDoor()
        {
            var calculatedX = CalculateOpenedPosition();
            this.transform.DOMoveX(calculatedX, animSpeed).OnComplete(() => { dynamicObstacle.ToggleObstacle(false); });
            // Analytics
            _analyticsService.LogEvent(new EventParameters<string>
                { EventName = "tt_barrier_destroyed", AdjustToken = AdjustNsEventTokens.TtBarrierDestroyed });
        }

        private float CalculateOpenedPosition()
        {
            var calculatedPos = _closedPosition.x - openedXGap;
            return calculatedPos;
        }
    }
}