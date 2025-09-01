using System;
using GameCore.Scriptables;
using UnityEngine;

public class ZombieBehaviourArea : MonoBehaviour
{
    public SpawnBehaviorState spawnBehaviorState;
    private void OnEnable()
    {
        GetComponent<MeshRenderer>().enabled = false;
    }

}
