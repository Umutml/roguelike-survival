using _Scripts.Utilities;
using UnityEditor;
using UnityEngine;
using Interfaces;
using VContainer;

public static class Consume5Energy
{
    [MenuItem("Testing/Energy/Consume 5 Energy")]
    private static void Consume()
    {
        // Find the GameObject with IEnergyService implementation
        var energyServiceProvider = Object.FindObjectOfType<VContainer.Unity.LifetimeScope>();
        
        if (energyServiceProvider == null)
        {
            LoggerNS.LogError("No VContainer LifetimeScope found in the scene!");
            return;
        }

        var energyService = energyServiceProvider.Container.Resolve<IEnergyService>();
        
        if (energyService == null)
        {
            LoggerNS.LogError("Could not resolve IEnergyService. Make sure it is registered in the container!");
            return;
        }

        bool success = energyService.ConsumeEnergy(5);
        
        if (success)
        {
            LoggerNS.Log($"Successfully consumed 5 energy. Current energy: {energyService.CurrentEnergy}");
        }
        else
        {
            LoggerNS.LogWarning($"Not enough energy to consume 5. Current energy: {energyService.CurrentEnergy}");
        }
    }
}
