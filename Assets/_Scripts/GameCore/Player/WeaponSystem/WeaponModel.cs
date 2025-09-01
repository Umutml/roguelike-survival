using UnityEngine;

namespace GameCore.Player.WeaponSystem
{
    public class WeaponModel : MonoBehaviour
    {
        [SerializeField] private Vector3 modelRotationPivot;


        public Vector3 ModelRotationPivot
        {
            get => modelRotationPivot;
            set => modelRotationPivot = value;
        }
    }
}
