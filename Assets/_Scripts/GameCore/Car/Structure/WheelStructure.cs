using System;
using UnityEngine;

namespace GameCore.Car
{
    [Serializable]
    public struct Wheel
    {
        [SerializeField] private GameObject wheelModel;
        [SerializeField] private TrailRenderer trailObject;
        [SerializeField] private ParticleSystem dustParticle;
        [SerializeField] private CarAxis carAxis;
        [SerializeField] private Transform frontWheelParent;
        [SerializeField] private Transform frontWheelChild;
        
        
        public TrailRenderer TrailObject => trailObject;
        public ParticleSystem DustParticle => dustParticle;
        
        
        public void UpdateWheelPositionAndRotation(float steerInput, float moveInput)
        {
            if (carAxis == CarAxis.Front)
            {
                var steerRotation = Quaternion.Euler(0, steerInput * 50, 0);
                frontWheelParent.transform.localRotation = steerRotation;
                
                frontWheelChild.transform.Rotate(Vector3.right * moveInput * 400 * Time.deltaTime, Space.Self);
            }
            if (carAxis == CarAxis.Back)
            {
                wheelModel.transform.Rotate(Vector3.right * moveInput * 400 * Time.deltaTime);
            }
        }
    }
}