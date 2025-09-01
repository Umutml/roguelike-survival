using UnityEngine;

namespace GameCore.Player.WeaponSystem
{
    public class WeaponSlot : MonoBehaviour
    {
        [SerializeField] private Transform slotHoldPoint;
        [SerializeField] private SlotType slotType;
        [SerializeField] private Vector3 weaponScale = Vector3.one;
        


        public enum SlotType
        {
            LeftHand,
            RightHand,
            Melee
        }


        public Weapon CurrentWeapon { get; private set; }

        public SlotType SlotPlacement
        {
            get => slotType;
            set => slotType = value;
        }


        public void InstallWeapon(Weapon weapon)
        {
            CurrentWeapon = weapon;
            weapon.transform.parent = slotHoldPoint;
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
            weapon.transform.localScale = weaponScale;
        }

        public void RemoveWeapon()
        {
            if (CurrentWeapon == null) return;

            CurrentWeapon.transform.parent = null;
            Destroy(CurrentWeapon.gameObject);
            CurrentWeapon = null;
        }
    }
}
