using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private Animator damageNumberAnimator;
    [SerializeField] private TMP_Text damageText;

    #endregion


    #region Fields
    
    private Vector3 _initialPosition;
    private readonly Color PlayerColor = new (1, 0.57f, 0.57f, 1);
    private static readonly int Move = Animator.StringToHash("Move");

    #endregion
    

    #region Public Methods

    public void Initialize(Vector3 position, string text, bool isPlayer)
    {
        _initialPosition = position;
        transform.position = new (_initialPosition.x, _initialPosition.y + 2, _initialPosition.z);
        damageText.text = text;
        damageText.color = isPlayer ? PlayerColor : Color.white;
        damageNumberAnimator.SetTrigger(Move);
    }


    public void InitializeHealNumber(Vector3 position, string text)
    {
        _initialPosition = position;
        transform.position = new (_initialPosition.x, _initialPosition.y + 2, _initialPosition.z);
        damageText.text = text;
        damageText.color = Color.green;
        damageNumberAnimator.SetTrigger(Move);
    }

    #endregion
}
