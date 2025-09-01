using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts.GameCore.Badge
{
    public class BadgeDropdown : MonoBehaviour
    {
        public GameObject dropdownContainer;
        public float autoCloseTime = 5f;
        private float itemSpacing = 220f; // Spacing between items
        [SerializeField] private Image dropdownImage;
        [SerializeField] private Sprite openedSprite;
        [SerializeField] private Sprite closedSprite;

        private Coroutine closeCoroutine;

        public void ToggleDropdown()
        {
            bool isActive = dropdownContainer.activeSelf;

            if (!isActive)
            {
                dropdownContainer.SetActive(true);
                AnimateOpen();
                UpdateToggleImage(true);
            }
            else
            {
                dropdownContainer.SetActive(false);
                CloseDropdown();
                DOTween.KillAll();
                UpdateToggleImage(false);
            }
        }
        
        private void AnimateOpen()
        {
            int index = 0;
            float firstItemOffset = -50f; // Offset for the first item
            foreach (Transform child in dropdownContainer.transform)
            {
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                float calculatedDownPosition;
                if (index == 0)
                    calculatedDownPosition = rectTransform.localPosition.y + firstItemOffset;
                else
                    calculatedDownPosition = rectTransform.localPosition.y - (itemSpacing * index);
                rectTransform.DOLocalMoveY(calculatedDownPosition, 0.2f).SetEase(Ease.OutBack);
                index++;
            }
        }
        
        private void CloseDropdown()
        {
            foreach (Transform child in dropdownContainer.transform)
            {
                RectTransform rectTransform = child.GetComponent<RectTransform>();
                rectTransform.localPosition = Vector3.zero;
            }
        }
        
        private void UpdateToggleImage(bool isOpen)
        {
            if (dropdownImage != null)
            {
                dropdownImage.sprite = isOpen ? openedSprite : closedSprite;
            }
        }

        IEnumerator AutoClose()
        {
            yield return new WaitForSeconds(autoCloseTime);
            dropdownContainer.SetActive(false);
        }
    }
}
