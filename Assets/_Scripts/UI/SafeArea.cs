using UnityEngine;


public class SafeArea : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private Canvas canvas;

    #endregion


    #region Fields

    private RectTransform _panelSafeArea;

    #endregion


    #region Unity Methods
    
    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        _panelSafeArea = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    #endregion

    #region Private Methods

    private void ApplySafeArea()
    {
        if (_panelSafeArea == null) return;

        var safeArea = Screen.safeArea;
        var anchorMin = safeArea.position;
        var anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;

        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;

        _panelSafeArea.anchorMin = anchorMin;
        _panelSafeArea.anchorMax = anchorMax;
    }

    #endregion
}
