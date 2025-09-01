using System;
using System.Collections;
using System.Threading.Tasks;
using GameCore.Level;
using GameCore.Tutorial;
using GameCore.Wave;
using Interfaces;
using UI.Game.Architectural;
using UnityEngine;
using VContainer;

namespace UI.Game
{
    public class StatusInfoController : Content
    {
        private const string TITLE_TEXT = "TitleText";
        private const string TITLE_AREA = "TitleArea";

        private WaveManager _waveManager;
        private Coroutine _showCoroutine;
        private ILevelService _levelService;
        private TutorialSequenceController _tutorialSequenceController;

        [Inject]
        private void Initialize(WaveManager waveManager, ILevelService levelService,
            TutorialSequenceController tutorialSequenceController)
        {
            _tutorialSequenceController = tutorialSequenceController;
            _waveManager = waveManager;
            _levelService = levelService;
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _tutorialSequenceController.ChangedStatusPrompt += UpdateStatus;
            _waveManager.WaveStatusUpdated += UpdateStatus;
            _levelService.WaveLevelChanged += _ => HideStatus();
        }

        private void UpdateStatus(string status)
        {
            CancelCurrentCoroutine();
            SetGameObject(TITLE_AREA, true);
            SetText(TITLE_TEXT, status);
            _showCoroutine = StartCoroutine(HideStatusAfterDelay());
        }

        private IEnumerator HideStatusAfterDelay()
        {
            yield return new WaitForSecondsRealtime(1);
            HideStatus();
        }

        private void HideStatus()
        {
            SetGameObject(TITLE_AREA, false);
        }

        private void CancelCurrentCoroutine()
        {
            if (_showCoroutine == null) return;
            StopCoroutine(_showCoroutine);
            _showCoroutine = null;
        }
    }
}
