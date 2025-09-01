using System;
using UnityEngine;

public class ObjectiveTrigger : MonoBehaviour
{
    [SerializeField] private ObjectiveHub objectiveHub;
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            objectiveHub.StartObjective();
        }
    }
}
