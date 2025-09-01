using System.Collections.Generic;
using UnityEngine;
using VContainer;

namespace GameCore.Player
{
    public class PlayerObscureCamera : MonoBehaviour
    {
        public Material transparentMaterial;
        public Material originalMaterial;
        public List<Renderer> buildingRenderers = new List<Renderer>();
        private Transform _playerTransform;

        [Inject]
        private void Construct(PlayerController playerController)
        {
            _playerTransform = playerController.transform;
        }
        
 
        void Update() {
            Vector3 cameraToPlayer = _playerTransform.position - transform.position;
        
            // Update shader parameters
            Shader.SetGlobalVector("_CameraPosition", transform.position);
            Shader.SetGlobalVector("_PlayerPosition", _playerTransform.position);
            Shader.SetGlobalFloat("_TransparencyRadius", 2.0f); // Adjust as needed
        }
    }
}
