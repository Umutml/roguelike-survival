using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TopBarIcon : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private Image iconImage;

    #endregion


    #region Fields

    private Sequence _sequence;

    #endregion


    #region Public Methods

    public void PlayIcon(TopBarIconAnimations topBarIconAnimations, Sprite icon, Vector3 targetPosition, float duration)
    {
        iconImage.sprite = icon;
        rectTransform.localScale = Vector2.zero;
        rectTransform.anchoredPosition = new Vector2(Random.Range(-50, 50), Random.Range(-50, 50));

        _sequence = DOTween.Sequence().SetUpdate(true);

        _sequence.Append(rectTransform.DOScale(Vector3.one * 0.7f, duration).SetEase(Ease.OutBack).SetUpdate(true));
        _sequence.Append(rectTransform.DOMove(targetPosition, duration).SetEase(Ease.OutBack).SetDelay(0.2f)
            .SetUpdate(true));
        _sequence.Append(rectTransform.DOScale(Vector3.zero, duration).OnComplete(() =>
        {
            topBarIconAnimations.IconsQueue.Enqueue(this);
            gameObject.SetActive(false);
        }).SetUpdate(true));
    }

    #endregion
}