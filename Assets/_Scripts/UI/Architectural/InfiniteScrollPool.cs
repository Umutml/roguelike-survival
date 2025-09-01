using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Architectural
{
    [RequireComponent(typeof(ScrollRect))]
    public class InfiniteScrollPool : MonoBehaviour
    {
        private Dictionary<RectTransform, int> _contentIndexMap;
        private Action<Component, int, bool> _setSegmentAction;
        private List<RectTransform> _segmentList;
        private List<RectTransform> _emptySegmentList;
        private bool _isInitialized;
        private float _previousNormalizedPosition;
        private int _startIndex;

        public bool IsReverseArrangement =>
            ScrollRect.content.GetComponent<HorizontalOrVerticalLayoutGroup>().reverseArrangement;

        public ScrollRect ScrollRect { get; private set; }

        private void Awake()
        {
            ScrollRect = GetComponent<ScrollRect>();
            ScrollRect.onValueChanged.AddListener(OnScroll);
        }

        public void SetupScroll(List<RectTransform> segmentList, int totalCount,
            Action<Component, int, bool> setSegmentAction)
        {
            ResetData(_emptySegmentList);
            _previousNormalizedPosition = 0;
            _segmentList = segmentList;
            _emptySegmentList = GetEmptySegments(totalCount - segmentList.Count).ToList();
            _setSegmentAction = setSegmentAction;
            SetupContentIndexMap();
            _startIndex = GetStartIndex();
            _isInitialized = true;
        }

        public void OnScroll(Vector2 position)
        {
            if (!_isInitialized)
            {
                return;
            }

            var scrollDirection = GetScrollDirection();

            var emptySegment = _emptySegmentList.FirstOrDefault(x => IsSegmentVisible(x, scrollDirection));
            if (emptySegment is null)
            {
                return;
            }

            var segment = _segmentList.Where(x => !IsSegmentVisible(x, scrollDirection))
                .OrderBy(x =>
                    scrollDirection == (IsReverseArrangement ? ScrollDirection.Up : ScrollDirection.Down)
                        ? GetIndex(x)
                        : -GetIndex(x)).FirstOrDefault();
            if (segment is null)
            {
                return;
            }

            var segmentIndex = Math.Max(_startIndex, GetIndex(segment));
            var emptyIndex = Math.Max(_startIndex, GetIndex(emptySegment));

            segment.SetSiblingIndex(emptyIndex);
            emptySegment.SetSiblingIndex(segmentIndex);

            _contentIndexMap[segment] = emptyIndex;
            _contentIndexMap[emptySegment] = segmentIndex;

            UpdateSegment(segment, emptyIndex, true);
        }

        private void SetupContentIndexMap()
        {
            _contentIndexMap = new Dictionary<RectTransform, int>();
            for (var i = 0; i < ScrollRect.content.childCount; i++)
            {
                var segment = ScrollRect.content.GetChild(i).GetComponent<RectTransform>();
                _contentIndexMap.Add(segment, i);
            }
        }
        

        private void UpdateSegment(Component segmentRectTransform, int index, bool isScroll = false)
        {
            _setSegmentAction?.Invoke(segmentRectTransform, index, isScroll);
        }

        private void ResetData<T>(List<T> listObjects) where T : Transform
        {
            if (listObjects == null)
            {
                return;
            }

            foreach (var segment in listObjects)
            {
                Destroy(segment.gameObject);
            }

            listObjects.Clear();
        }

        private IEnumerable<RectTransform> GetEmptySegments(int targetCount)
        {
            for (var i = 0; i < targetCount; i++)
            {
                var emptySegment = new GameObject();
                emptySegment.transform.SetParent(ScrollRect.content, false);
                emptySegment.AddComponent<RectTransform>();
                emptySegment.GetComponent<RectTransform>().sizeDelta = new Vector2(0, GetSegmentHeight());
                yield return emptySegment.GetComponent<RectTransform>();
            }
        }

        private int GetIndex(RectTransform segmentRectTransform)
        {
            return _contentIndexMap[segmentRectTransform];
        }

        private bool IsSegmentVisible(Transform segmentRectTransform, ScrollDirection scrollDirection)
        {
            var offset = scrollDirection == ScrollDirection.Down
                ? Vector3.up * GetSegmentHeight()
                : Vector3.down * GetSegmentHeight();
            return RectTransformUtility.RectangleContainsScreenPoint(ScrollRect.viewport,
                segmentRectTransform.position + offset);
        }

        private int GetStartIndex()
        {
            return GetIndex(_segmentList.FirstOrDefault());
        }

        private float GetSegmentHeight()
        {
            return _segmentList.FirstOrDefault()?.sizeDelta.y ?? 0;
        }

        private ScrollDirection GetScrollDirection()
        {
            if (_previousNormalizedPosition is 0)
            {
                _previousNormalizedPosition = ScrollRect.verticalNormalizedPosition;
                return ScrollDirection.Down;
            }

            var currentNormalizedPosition = ScrollRect.verticalNormalizedPosition;
            var scrollDirection = _previousNormalizedPosition.CompareTo(currentNormalizedPosition) switch
            {
                1 => ScrollDirection.Down,
                -1 => ScrollDirection.Up,
                _ => ScrollDirection.Down
            };

            _previousNormalizedPosition = currentNormalizedPosition;
            return scrollDirection;
        }

        private enum ScrollDirection
        {
            Up,
            Down
        }
    }
}