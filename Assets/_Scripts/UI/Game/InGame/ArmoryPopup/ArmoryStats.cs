using System;
using GameCore.Scriptables;
using UI.Game.Architectural;
using UI.Game.InGame.ArmoryPopupConstants;
using UnityEngine;

public class ArmoryStats : Content
{
    #region Fields

    private readonly Color CURRENT_VALUE_COLOR = new(0.46f, 0.96f, 0.35f, 1f);
    private readonly Color NONE_VALUE_COLOR = new(0.86f, 0.65f, 0.45f, 1f);

    #endregion


    #region Private Methods

    public void InitializeStats(WeaponData weaponData, bool isUnlocked)
    {
        SetText(ArmoryPopupConstants.WEAPON_NAME, weaponData.WeaponName);
        SetText(ArmoryPopupConstants.DAMAGE, $"{GetWeaponStatsAmount(weaponData.Damage).statValue}");
        SetText(ArmoryPopupConstants.RADIUS, $"{GetWeaponStatsAmount(weaponData.Radius).statValue}");
        SetText(ArmoryPopupConstants.FIRE_INTERVAL, $"{GetWeaponStatsAmount(weaponData.FireInterval).statValue}");
        SetText(ArmoryPopupConstants.PELLET_COUNT, $"{GetWeaponStatsAmount(weaponData.PelletCount).statValue}");
        SetText(ArmoryPopupConstants.RANGE, $"{GetWeaponStatsAmount(weaponData.Range).statValue}");
        SetText(ArmoryPopupConstants.FIRE_ANGLE, $"{GetWeaponStatsAmount(weaponData.FireAngle).statValue}");

        SetColor(ArmoryPopupConstants.DAMAGE, GetWeaponStatsAmount(weaponData.Damage).statColor);
        SetColor(ArmoryPopupConstants.RADIUS, GetWeaponStatsAmount(weaponData.Radius).statColor);
        SetColor(ArmoryPopupConstants.FIRE_INTERVAL, GetWeaponStatsAmount(weaponData.FireInterval).statColor);
        SetColor(ArmoryPopupConstants.PELLET_COUNT, GetWeaponStatsAmount(weaponData.PelletCount).statColor);
        SetColor(ArmoryPopupConstants.RANGE, GetWeaponStatsAmount(weaponData.Range).statColor);
        SetColor(ArmoryPopupConstants.FIRE_ANGLE, GetWeaponStatsAmount(weaponData.FireAngle).statColor);
        SetGameObject(ArmoryPopupConstants.UNLOCKED_TEXT, !isUnlocked);
        SetText(ArmoryPopupConstants.UNLOCKED_TEXT, weaponData.LockedMessage);
        
    }
 

    private (Color statColor, string statValue) GetWeaponStatsAmount<T>(T value)
    {
        if (value is int intValue)
        {
            return (intValue == 0 ? NONE_VALUE_COLOR : CURRENT_VALUE_COLOR, intValue == 0 ? "-" : intValue.ToString());
        }

        if (value is float floatValue)
            return (Math.Abs(floatValue) < float.Epsilon ? NONE_VALUE_COLOR : CURRENT_VALUE_COLOR,
                    Math.Abs(floatValue) < float.Epsilon ? "-" : floatValue.ToString());

        throw new ArgumentException("Unsupported type");
    }

    #endregion
}
