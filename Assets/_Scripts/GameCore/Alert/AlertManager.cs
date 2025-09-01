using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AlertManager : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private Animator alertAnimator;
    [SerializeField] private TMP_Text alertText;
    [SerializeField] private Image alertBackground;

    #endregion


    #region Fields

    private static readonly int Show = Animator.StringToHash("Show");

    #endregion


    #region Public Methods

    public void CallAlert(string alert)
    {
        alertText.text = alert;
        alertAnimator.SetTrigger(Show);
    }

    #endregion
}
