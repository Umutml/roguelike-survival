using UnityEngine;
using UnityEngine.AddressableAssets;

public class DamageNumberManager : MonoBehaviour
{
    #region Serializable Fields

    [SerializeField] private AssetReference damageNumberPrefab;

    #endregion
    

    #region Public Methods

    public async void UseDamageNumber(Vector3 position, string text, bool isPlayer)
    {
        var damageNumber = await ObjectManager.GetObject(damageNumberPrefab, position, Quaternion.identity).BindTo(gameObject, damageNumberPrefab);
        damageNumber.GetComponent<DamageNumber>().Initialize(position, text, isPlayer);
    }
    
    
    public async void UseHealDamageNumber(Vector3 position, string text)
    {
        var damageNumber = await ObjectManager.GetObject(damageNumberPrefab, position, Quaternion.identity).BindTo(gameObject, damageNumberPrefab);
        damageNumber.GetComponent<DamageNumber>().InitializeHealNumber(position, text);
    }

    #endregion
}
