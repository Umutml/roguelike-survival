using System;
using GameCore.Scriptables;
using UI.Game.Architectural;
using UI.Game.InGame.ArmoryPopupConstants;
using UnityEngine;

public class ArmoryWeaponSegment : Content
{
    #region Fields

    private readonly Color ENABLED_COLOR = new(0.99f, 0.93f, 0.87f, 1);
    private readonly Color DISABLED_COLOR = new(0.21f, 0.16f, 0.14f, 1f);

    private Sprite _armoryArt;

    #endregion

    #region Properties

    public WeaponData WeaponData { get; private set; }
    public Transform SelectTransform => GetGameObject(ArmoryPopupConstants.SELECT).transform;

    #endregion

    #region Public Methods

    public async void InitializeSegment(ArmoryPopup armoryPopup, WeaponData weaponData, Sprite priceIcon,
        Action buyAction, Action selectAction, bool isEnoughCurrency)
    {
        WeaponData = weaponData;
        _armoryArt = await weaponData.WeaponArt();


        SetGameObject(ArmoryPopupConstants.PRICE, !weaponData.WeaponBuyType.Equals(WeaponBuyType.Ad));
        SetGameObject(ArmoryPopupConstants.SELECTED,
            armoryPopup.PlayerWeaponData.usingWeapon.Equals(weaponData.WeaponName));
        SetGameObject(ArmoryPopupConstants.TICK,
            armoryPopup.PlayerWeaponData.usingWeapon.Contains(weaponData.WeaponName));
        SetGameObject(ArmoryPopupConstants.SELECT,
            armoryPopup.PlayerWeaponData.unlockedWeapons.Contains(weaponData.WeaponName));
        SetColor(ArmoryPopupConstants.BACKGROUND, weaponData.IsEnable ? ENABLED_COLOR : DISABLED_COLOR);
        SetGameObject(ArmoryPopupConstants.BUY_ICON, !weaponData.WeaponBuyType.Equals(WeaponBuyType.None));
        SetGameObject(ArmoryPopupConstants.BUY_ICON, !weaponData.WeaponPrice.Equals(0));
        SetGameObject(ArmoryPopupConstants.DISABLE_BUY_ICON, !weaponData.WeaponBuyType.Equals(WeaponBuyType.None));
        SetText(ArmoryPopupConstants.WEAPON_NAME, weaponData.ShownName);
        SetText(ArmoryPopupConstants.PRICE, weaponData.WeaponPrice == 0 ? "Free" : $"{weaponData.WeaponPrice}");
        SetText(ArmoryPopupConstants.DISABLE_PRICE, $"{weaponData.WeaponPrice}");
        SetColor(ArmoryPopupConstants.PRICE, isEnoughCurrency ? Color.white : Color.red);
        SetImage(ArmoryPopupConstants.WEAPON_IMAGE, _armoryArt);
        SetImage(ArmoryPopupConstants.BUY_ICON, priceIcon);
        SetImage(ArmoryPopupConstants.DISABLE_BUY_ICON, priceIcon);
        SetGameObject(ArmoryPopupConstants.BUY,
            !armoryPopup.PlayerWeaponData.unlockedWeapons.Contains(weaponData.WeaponName));
        SetGameObject(ArmoryPopupConstants.DISABLE, !weaponData.IsEnable);
        SetGameObject(ArmoryPopupConstants.LOCK, !weaponData.IsEnable);
        OnClickListen(ArmoryPopupConstants.BUY, buyAction);
        OnClickListen(ArmoryPopupConstants.SELECT, selectAction);
        OnClickListen(ArmoryPopupConstants.BACKGROUND, () => armoryPopup.WeaponData = weaponData);
    }

    public void SetActiveState(bool isSelected)
    {
        GetButton(ArmoryPopupConstants.BUY).interactable = isSelected;
    }

    public void UpdateSegment(ArmoryPopup armoryPopup, WeaponData weaponData, bool isEnoughCurrency)
    {
        SetGameObject(ArmoryPopupConstants.SELECT,
            armoryPopup.PlayerWeaponData.unlockedWeapons.Contains(weaponData.WeaponName));
        SetGameObject(ArmoryPopupConstants.SELECTED,
            armoryPopup.PlayerWeaponData.usingWeapon.Equals(weaponData.WeaponName));
        SetGameObject(ArmoryPopupConstants.TICK,
            armoryPopup.PlayerWeaponData.usingWeapon.Contains(weaponData.WeaponName));
        SetGameObject(ArmoryPopupConstants.BUY,
            !armoryPopup.PlayerWeaponData.unlockedWeapons.Contains(weaponData.WeaponName));
        SetColor(ArmoryPopupConstants.PRICE, isEnoughCurrency ? Color.white : Color.red);
    }

    #endregion
}