using System;
using GameCore.Player;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace UI.Game.InGame.Weapon
{
    public class WeaponPanel : MonoBehaviour
    {
        [SerializeField] private Image weaponImage;
        
        private PlayerController _playerController;

        [Inject]
        private void Construct(PlayerController playerController)
        {
            _playerController = playerController;
        }

        private void Awake()
        {
            _playerController.WeaponInitialized += OnWeaponInitialized;
            _playerController.WeaponSwitched    += OnWeaponSwitched;
        }

        private void OnWeaponSwitched(GameCore.Player.WeaponSystem.Weapon oldWeapon, GameCore.Player.WeaponSystem.Weapon newWeapon)
        {
            SetWeaponImage(newWeapon.WeaponIcon);
        }

        private void OnWeaponInitialized(GameCore.Player.WeaponSystem.Weapon weapon)
        {
            SetWeaponImage(weapon.WeaponIcon);
        }
        
        private void SetWeaponImage(Sprite weaponIcon)
        {
            weaponImage.color = Color.white;
            weaponImage.sprite = weaponIcon;
        }
        
    }
}
