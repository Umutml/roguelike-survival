using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using Utilities;

namespace GameCore.Scriptables
{
    [CreateAssetMenu(fileName = "WeaponResources", menuName = "ScriptableObjects/WeaponResources", order = 1)]
    public class WeaponResources : ScriptableObject
    {
        [SerializeField] private List<WeaponData> weaponList = new();

        public List<WeaponData> Weapons => weaponList;
        public WeaponData GetWeapon(string weaponName) => weaponList.FirstOrDefault(x => x.WeaponName.Equals(weaponName));
    }


    [Serializable]
    public struct WeaponData
    {
        [SerializeField, Tooltip("This is internal weapon name")] private string weaponName;
        [SerializeField, Tooltip("This is the name shown on UI or other user interfaces")] private string shownName;
        [SerializeField] private string lockedMessage;
        [SerializeField] private AssetReference weaponArt;
        [SerializeField] private WeaponBuyType weaponBuyType;
        [SerializeField] private PurchaseOptions purchaseOptions;
        [SerializeField] private float weaponPrice;
        [SerializeField] private int damage;
        [SerializeField] private float fireInterval;
        [SerializeField] private int range;
        [SerializeField] private int radius;
        [SerializeField] private int pelletCount;
        [SerializeField] private int fireAngle;
        [SerializeField] private bool isEnable;


        public string WeaponName => weaponName;
        public string ShownName => shownName;
        public string LockedMessage => lockedMessage;
        public WeaponBuyType WeaponBuyType => weaponBuyType;
        public PurchaseOptions PurchaseOptions => purchaseOptions;
        public async UniTask<Sprite> WeaponArt() => await AssetManager<Sprite>.LoadObject(weaponArt);
        public float WeaponPrice => weaponPrice;
        public int Damage => damage;
        public float FireInterval => fireInterval;
        public int Range => range;
        public int Radius => radius;
        public int PelletCount => pelletCount;
        public int FireAngle => fireAngle;
        public bool IsEnable => isEnable;
    }


    public enum WeaponBuyType
    {
        Coin,
        Gem,
        Ad,
        Free,
        None
    }
}

