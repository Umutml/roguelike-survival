using System;
using GameCore.Player.WeaponSystem;
using RootMotion.FinalIK;
using UnityEngine;
using VContainer;

namespace GameCore.Player
{
    public class PlayerSkinController : MonoBehaviour
    {
        [SerializeField] private WeaponSlot[] weaponSlots;
        [SerializeField] private Animator animator;
        [SerializeField] private ArmIK leftArmIk;
        [SerializeField] private ArmIK rightArmIk;
        
        public WeaponSlot[] WeaponSlots
        {
            get => weaponSlots;
            set => weaponSlots = value;
        }

        public Animator Animator
        {
            get => animator;
            set => animator = value;
        }

        public ArmIK LeftArmIk
        {
            get => leftArmIk;
            set => leftArmIk = value;
        }

        public ArmIK RightArmIk
        {
            get => rightArmIk;
            set => rightArmIk = value;
        }
        
        public void AdjustIK(Transform leftTarget, Transform leftBendGoal, Transform rightTarget, Transform rightBendGoal)
        {
            leftArmIk.solver.arm.target = leftTarget;
            rightArmIk.solver.arm.target = rightTarget;
            
            leftArmIk.solver.arm.bendGoal = leftBendGoal;
            rightArmIk.solver.arm.bendGoal = rightBendGoal;
        }
        
    }
}
