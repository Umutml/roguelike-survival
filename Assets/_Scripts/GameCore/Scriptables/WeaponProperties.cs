using System;
using UnityEngine;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "WeaponProperties", menuName = "ScriptableObjects/WeaponProperties")]
    public class WeaponProperties : ScriptableObject
    {
        [SerializeField] private WeaponProperty[] weaponProperties;

        public WeaponProperty GetPropertyByKey(string key)
        {
            foreach (var weaponProperty in weaponProperties)
            {
                if (weaponProperty.WeaponKey == key)
                {
                    return weaponProperty;
                }
            }

            return default;
        }

        [Serializable]
        public struct WeaponProperty
        {
            public string WeaponKey;
            public string InGameName;
        }
    }
}
