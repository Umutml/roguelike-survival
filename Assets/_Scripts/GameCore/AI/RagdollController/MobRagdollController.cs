using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _Scripts.GameCore.AI.RagdollController;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MobRagdollController : MonoBehaviour
{
    [SerializeField] private RagdollPart[] ragdollParts;
    public void SetActiveRagdoll(bool isActive,bool addForce=false)
    {
        if(!RagdollSettings.ActiveRagdoll)
            return;
        foreach (var ragdollPart in ragdollParts)
            ragdollPart.SetActiveRagdoll(isActive);
        if (addForce)
            ragdollParts[0].ragdollRigidbody.AddForce(transform.forward * (-1 * 50f), ForceMode.Impulse);
    }
    public void SetActiveRagdoll(bool isActive,Vector3 hitPosition,float hitForce)
    {
        if(!RagdollSettings.ActiveRagdoll)
            return;
        foreach (var ragdollPart in ragdollParts)
            ragdollPart.SetActiveRagdoll(isActive);
        var forceDirection = (ragdollParts[0].ragdollRigidbody.position - hitPosition).normalized;
        ragdollParts[0].ragdollRigidbody.AddForce(forceDirection * hitForce, ForceMode.Impulse);
    }
    public void SetRagdollParts(RagdollPart[] newRagdollParts) => ragdollParts = newRagdollParts;
}
