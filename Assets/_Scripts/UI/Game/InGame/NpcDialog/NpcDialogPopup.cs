using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _Scripts.Utilities;
using _Utilities;
using GameCore.Player;
using GameCore.PopupSystem;
using GameCore.Tutorial.Steps;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace _Scripts.UI.Game.InGame.NpcDialog
{
    public class NpcDialogPopup : Popup, IPointerDownHandler
    {
        [SerializeField] private GameObject segmentObject;
        [SerializeField] private Transform segmentParent;
        [SerializeField] private GameObject handObject;


        private readonly WaitForSecondsRealtime _wait = new(1f);
        private CancellationTokenSource _cancellationTokenSource;
        private Coroutine _showHandCoroutine;


        private NpcDialogSegment _currentSegment;
        private bool _isInitialized;

        public override void OnOpenPopup()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            if (_showHandCoroutine != null)
            {
                StopCoroutine(_showHandCoroutine);
            }
        }

        public override async void Initialize(object data)
        {
            base.Initialize(data);

            if (data is not List<NpcDialogData> dialogDatas)
            {
                Debug.LogError("Data is not List<NpcDialogData>");
                return;
            }

            foreach (var dialogData in dialogDatas)
            {
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                    break;

                _currentSegment = CreateSegment(dialogData);
                _isInitialized = true;

                try
                {
                    await UniTaskAsyncHelper.WaitWhile(() => _currentSegment.IsPlaying, 300, true,
                        _cancellationTokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                    Debug.Log("Operation was canceled");
                    break;
                }
            }

            if (this == null || gameObject == null || !gameObject.activeInHierarchy)
            {
                return;
            }

            _showHandCoroutine = StartCoroutine(ShowHand());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_isInitialized)
            {
                return;
            }

            if (_currentSegment != null && _currentSegment.IsPlaying)
            {
                _currentSegment.Skip();
                return;
            }

            ClosePopup();
        }

        private IEnumerator ShowHand()
        {
            if (handObject == null)
            {
                LoggerNS.LogError("Hand object is null");
                yield break;
            }

            handObject.SetActive(false);
            yield return _wait;

            handObject.SetActive(true);
        }

        private NpcDialogSegment CreateSegment(NpcDialogData dialogData)
        {
            var segment = Instantiate(segmentObject, segmentParent).GetComponent<NpcDialogSegment>();
            segment.Initialize(dialogData);
            return segment;
        }
    }
}