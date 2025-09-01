using _Scripts.Utilities;
using GameCore.Health;
using GameCore.Player;
using UnityEditor;
using UnityEngine;
using Interfaces;
using VContainer;

public static class KillPlayer
{
    [MenuItem("Testing/Kill Player")]
    private static void Kill()
    {
        var provider = Object.FindObjectOfType<VContainer.Unity.LifetimeScope>();
        if (provider == null)
        {
            LoggerNS.LogError("No VContainer LifetimeScope found in the scene!");
            return;
        }

        var statusController = provider.Container.Resolve<PlayerStatusController>();
        statusController.TakeDamage(new DamageInfo(1000, DamageSource.Environment));
        
        
        if (statusController == null)
        {
            LoggerNS.LogError("Could not resolve PlayerStatusController. Make sure it is registered in the container!");
            return;
        }
        
    }
} 