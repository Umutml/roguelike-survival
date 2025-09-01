using _Scripts.Utilities;
using UnityEditor;
using UnityEngine;
using Interfaces;
using VContainer;

public static class Give5Energy
{
    [MenuItem("Testing/Energy/Give 5 Energy")]
    private static void Give()
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

        energyService.GiveEnergy(5);
        LoggerNS.Log($"Successfully added 5 energy. Current energy: {energyService.CurrentEnergy}");
    }
} 