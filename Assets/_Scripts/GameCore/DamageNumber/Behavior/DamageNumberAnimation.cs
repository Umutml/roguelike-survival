using UnityEngine;

public class DamageNumberAnimation : MonoBehaviour
{
    [SerializeField] private DamageNumber damageNumber;


    public void Reset()
    {
        damageNumber.gameObject.SetActive(false);
    }
}
