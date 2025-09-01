using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Scripts.GameCore.AI.RagdollController
{
    public static class RagdollSettings
    {
        public static bool ActiveRagdoll=true;
    }
    [System.Serializable]
    public class RagdollPart
    {
        public GameObject ragdollObject;
        public Rigidbody ragdollRigidbody;
        public CharacterJoint ragdollCharacterJoint;
        public Rigidbody ragdollCharacterConnectedBody;
        public Collider[] ragdollCollider;
        public RagdollPart(GameObject ragdollObject)
        {
            this.ragdollObject = ragdollObject;
            ragdollRigidbody = ragdollObject.GetComponent<Rigidbody>();
            ragdollCharacterJoint = ragdollObject.GetComponent<CharacterJoint>();
            if(ragdollCharacterJoint)
                ragdollCharacterConnectedBody = ragdollCharacterJoint.connectedBody;
            ragdollCollider = new List<Collider>(ragdollObject.GetComponents<Collider>()).ToArray();
        }

        public void SetActiveRagdoll(bool isActive)
        {
            foreach (var currentCollider in ragdollCollider)
            {
                if(currentCollider == null) continue;
                currentCollider.enabled = isActive;
            }
            if (isActive)
            {
                if(ragdollRigidbody)
                    ragdollRigidbody.isKinematic = false;
                if(ragdollCharacterJoint)
                    ragdollCharacterJoint.connectedBody = ragdollCharacterConnectedBody;
            }
            else
            {
                if(ragdollRigidbody)
                    ragdollRigidbody.isKinematic = true;
                if(ragdollCharacterJoint)
                    ragdollCharacterJoint.connectedBody = null;
            }
        }
    }
}