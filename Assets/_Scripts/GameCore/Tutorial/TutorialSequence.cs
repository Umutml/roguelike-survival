using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using GameCore.Tutorial.Steps;
using UnityEngine;

namespace GameCore.Tutorial
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Tutorial/New Tutorial Sequence")]
    public class TutorialSequence : ScriptableObject
    {
        public event Action<string> OnTutorialStepChanged;
        public event Action<string> OnTutorialStepCompleted;
        public event Action OnSequenceFinished;

        public List<TutorialStepComposite> TutorialStepsCompositie;

        [SerializeField] private bool LogsEnabled = false;

        private int _currentStepIndex;
        private CancellationTokenSource _cancellationTokenSource;

        public async void StartSequence(TutorialCheckPointData? checkPointData, Action onSequenceCompleteCallback = null)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            InitializeCheckPointStartingStep(checkPointData);
            ResetSkipFlags();

            var allSteps = TutorialStepsCompositie.SelectMany(composite => composite.Steps).ToList();

            await ProcessCheckPointInitialSteps(checkPointData, allSteps);

            try
            {
                await ProcessStepsSequentially(allSteps, token);
                if (!token.IsCancellationRequested)
                {
                    OnSequenceFinished?.Invoke();
                    onSequenceCompleteCallback?.Invoke();
                }
            }
            catch (OperationCanceledException)
            {
                Log("Tutorial sequence was cancelled.");
            }
        }

        private void InitializeCheckPointStartingStep(TutorialCheckPointData? checkPoint)
        {
            _currentStepIndex = 0;

            if (!checkPoint.HasValue || string.IsNullOrEmpty(checkPoint.Value.StartStepName)) return;
            _currentStepIndex = GetStepIndexByName(checkPoint.Value.StartStepName);

            if (_currentStepIndex != -1) return;
            Log($"StartStepName '{checkPoint.Value.StartStepName}' not found. Starting from the first step.");
            _currentStepIndex = 0;
        }

        private async UniTask ProcessCheckPointInitialSteps(TutorialCheckPointData? checkPointData, List<TutorialStep> allSteps)
        {
            if (!checkPointData.HasValue || checkPointData.Value.InitialSteps == null) return;
            foreach (var initialStep in checkPointData.Value.InitialSteps)
            {
                var step = allSteps.FirstOrDefault(s => s.name == initialStep.stepName);

                if (step != null)
                {
                    OnTutorialStepChanged?.Invoke(step.name);
                    Log($"Immediately processing {step.name}");

                    if (initialStep.isAwaited)
                    {
                        await step.ProcessStep();
                    }
                    else
                    {
                        _ = step.ProcessStep();
                    }

                    OnTutorialStepCompleted?.Invoke(step.name);
                }
                else
                {
                    Log($"Initial step '{step.name}' not found.");
                }
            }
        }

        private async UniTask ProcessStepsSequentially(List<TutorialStep> allSteps, CancellationToken token)
        {
            foreach (var step in allSteps.Skip(_currentStepIndex))
            {
                if (token.IsCancellationRequested) break;

                if (step.Skip)
                {
                    Log($"Skipping {step.name}");
                    continue;
                }

                OnTutorialStepChanged?.Invoke(step.name);
                Log($"Tutorial Sequence: Processing {step.name}");
                await step.ProcessStep().AttachExternalCancellation(token);
                OnTutorialStepCompleted?.Invoke(step.name);
                Log($"Tutorial Sequence: Completed {step.name}");
            }
        }

        private void ResetSkipFlags()
        {
            foreach (var step in TutorialStepsCompositie.SelectMany(composite => composite.Steps))
            {
                step.Skip = false;
            }
        }

        public int GetStepIndexByName(string stepName)
        {
            var allSteps = TutorialStepsCompositie.SelectMany(composite => composite.Steps).ToList();
            return allSteps.FindIndex(step => step.name == stepName);
        }

        private void Log(string message)
        {
            if (LogsEnabled)
            {
                Debug.Log(message);
            }
        }

        private void OnDisable()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        [Serializable]
        public struct TutorialStepComposite
        {
            public List<TutorialStep> Steps;
        }
    }
}
