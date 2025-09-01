#define ENABLE_TEST_SROPTIONS
using System.ComponentModel;
using _Scripts.GameCore.AI.RagdollController;
using _Scripts.Utilities;
using _Utilities;
using GameCore.AI;
using GameCore.Inventory;
using GameCore.Player;
using GameCore.Player.WeaponSystem;
using GameCore.Scriptables;
using GameCore.Tutorial;
using GameCore.Wave;
using UnityEngine;
using Interfaces;
using Unity.Entities.UniversalDelegates;
using VContainer;

public partial class SROptions
{
#if ENABLE_TEST_SROPTIONS

    [Category("Test")]
    [DisplayName("Enable Debug Mode")]
    public bool EnableDebugMode
    {
        get => InGameDebuggerUIReference.EnableDebugMode;
        set => InGameDebuggerUIReference.EnableDebugMode = value;
    }
    
    [Category("Test")]
    [DisplayName("Activate All Debug Logs")]
    public bool ActivateAllDebugLogs
    {
        get => LoggerNS.EnableLog;
        set => LoggerNS.SetLogStatus(value);
    }

    [Category("Test")]
    [DisplayName("Activate Performance UI")]
    public void ActivatePerformanceUI()
    {
        InGameDebuggerUIReference.ActivatePerformanceUI();
    }


    [Category("Cheats")]
    [DisplayName("Clear PlayerPrefs")]
    public void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
    }
    [DisplayName("Objective Index")]
    public int ObjectiveIndex
    {
        get => ObjectiveManager.DebuggerObjectiveIndex;
        set => ObjectiveManager.DebuggerObjectiveIndex = value;
    }
    [DisplayName("Spawn Objective Mission")]
    public void StartObjective()
    {
        var objectiveManager = Object.FindObjectOfType<ObjectiveManager>();
        if (objectiveManager == null)
        {
            LoggerNS.LogError("No ObjectiveManager found in the scene!");
            return;
        }
        if (objectiveManager.IsProgress)
        {
            LoggerNS.LogError("ObjectiveManager is already in progress!");
            return;
        }
        objectiveManager.SpawnObjectiveByIndex();
    }
    [DisplayName("Complete Objective Mission")]
    public void SkipObjectiveComplete()
    {
        var objectiveManager = Object.FindObjectOfType<ObjectiveManager>();
        if (objectiveManager == null)
        {
            LoggerNS.LogError("No ObjectiveManager found in the scene!");
            return;
        }
        if (!objectiveManager.IsProgress)
        {
            LoggerNS.LogError("ObjectiveManager is not in progress!");
            return;
        }
        objectiveManager.SkipObjectiveCompleted();
    }
    [DisplayName("Complete Objective Mission")]
    public void SkipObjectiveFailed()
    {
        var objectiveManager = Object.FindObjectOfType<ObjectiveManager>();
        if (objectiveManager == null)
        {
            LoggerNS.LogError("No ObjectiveManager found in the scene!");
            return;
        }
        if (!objectiveManager.IsProgress)
        {
            LoggerNS.LogError("ObjectiveManager is not in progress!");
            return;
        }
        objectiveManager.SkipObjectiveFailed();
    }

    [DisplayName("Enable Zombie Behaviour Settings")]
    public bool EnableZombieBehaviour
    {
        get => WaveManager.EnableZombieBehaviourSetting;
        set => WaveManager.EnableZombieBehaviourSetting = value;
    }

    [DisplayName("Attacker Zombie Probability")]
    public int AttackerZombieProbability
    {
        get => WaveManager.AttackerZombieProbability;
        set => WaveManager.AttackerZombieProbability = value;
    }

    [DisplayName("Waiting Zombie Probability")]
    public int WaitingZombieProbability
    {
        get => WaveManager.WaitingZombieProbability;
        set => WaveManager.WaitingZombieProbability = value;
    }

    [DisplayName("Petrol Zombie Probability")]
    public int PetrolZombieProbability
    {
        get => WaveManager.PetrolZombieProbability;
        set => WaveManager.PetrolZombieProbability = value;
    }

    [DisplayName("Active Ragdoll")]
    public bool ActiveRagdoll
    {
        get => RagdollSettings.ActiveRagdoll;
        set => RagdollSettings.ActiveRagdoll = value;
    }

    [Category("Tutorial")]
    [DisplayName("Is Tutorial Completed")]
    public bool IsTutorialCompleted
    {
        get => SaveLoadHelper.TryLoadPersistentData<TutorialData>().IsCompleted;
        set
        {
            var data = SaveLoadHelper.TryLoadPersistentData<TutorialData>();

            data.IsCompleted = value;

            SaveLoadHelper.SaveData(data);
        }
    }
    
    // add new buttons here 
    [Category("Tutorial")]
    [DisplayName("Delete All Cached Data")]
    public void DeleteAllCachedData()
    {
        SaveLoadHelper.DeleteAllSavedData();
    }
    
    [Category("Mediation")]
    [DisplayName("Show RewardAD Test Revive")]
    public void ShowRewardAdRevive()
    {
        Debug.Log ("unity-script: ShowCoinRewardAD called");
        if (IronSource.Agent.isRewardedVideoAvailable ()) {
            IronSource.Agent.showRewardedVideo ("Revive_Reward");
        } else {
            Debug.LogError("unity-script: IronSource.Agent.isRewardedVideoAvailable - False");
        }
    }
    
    [Category("Mediation")]
    [DisplayName("Show RewardAD Test Coin")]
    public void ShowRewardAdCoin()
    {
        Debug.Log ("unity-script: ShowCoinRewardAD called");
        if (IronSource.Agent.isRewardedVideoAvailable ()) {
            IronSource.Agent.showRewardedVideo ("Coin_Reward");
        } else {
            Debug.LogError("unity-script: IronSource.Agent.isRewardedVideoAvailable - False");
        }
    }
    
    
    [Category("Currency")]
    [DisplayName("Give 1000 Coin")]
    public void Give1000Coin()
    {
        var inventoryService = Object.FindObjectOfType<InventoryBase>();
        
        if (inventoryService == null)
        {
            Debug.LogError("No InventoryBase found in the scene!");
            return;
        }

        inventoryService.ModifyCurrencyBalance(new PurchaseDetails(1000, PurchaseOptions.Coin));
    }

    [Category("Currency")]
    [DisplayName("Give 1000 Gem")]
    public void Give1000Gem()
    {
        var inventoryService = Object.FindObjectOfType<InventoryBase>();
        
        if (inventoryService == null)
        {
            Debug.LogError("No InventoryBase found in the scene!");
            return;
        }

        inventoryService.ModifyCurrencyBalance(new PurchaseDetails(1000, PurchaseOptions.Gem));
    }
    
    
    
    [Category("Energy")]
    [DisplayName("Give 5 Energy")]
    public void Give5Energy()
    {
        var energyServiceProvider = Object.FindObjectOfType<VContainer.Unity.LifetimeScope>();
        
        if (energyServiceProvider == null)
        {
            Debug.LogError("No VContainer LifetimeScope found in the scene!");
            return;
        }

        var energyService = energyServiceProvider.Container.Resolve<IEnergyService>();
        
        if (energyService == null)
        {
            Debug.LogError("Could not resolve IEnergyService. Make sure it is registered in the container!");
            return;
        }

        energyService.GiveEnergy(5);
        Debug.Log($"Successfully added 5 energy. Current energy: {energyService.CurrentEnergy}");
    }

    [Category("Energy")]
    [DisplayName("Consume 5 Energy")]
    public void Consume5Energy()
    {
        var energyServiceProvider = Object.FindObjectOfType<VContainer.Unity.LifetimeScope>();
        
        if (energyServiceProvider == null)
        {
            Debug.LogError("No VContainer LifetimeScope found in the scene!");
            return;
        }

        var energyService = energyServiceProvider.Container.Resolve<IEnergyService>();
        
        if (energyService == null)
        {
            Debug.LogError("Could not resolve IEnergyService. Make sure it is registered in the container!");
            return;
        }

        bool success = energyService.ConsumeEnergy(5);
        
        if (success)
        {
            Debug.Log($"Successfully consumed 5 energy. Current energy: {energyService.CurrentEnergy}");
        }
        else
        {
            Debug.LogWarning($"Not enough energy to consume 5. Current energy: {energyService.CurrentEnergy}");
        }
    }
    
    [Category("Weapon")]
    [DisplayName("Ranged Weapon Name")]
    public string RangedWeaponName { get; set; }

    [Category("Weapon")]
    [DisplayName("Change Ranged Weapon")]
    public void ChangeRangedWeapon()
    {
        var serviceProvider = Object.FindObjectOfType<VContainer.Unity.LifetimeScope>();
        
        if (serviceProvider == null)
        {
            Debug.LogError("No VContainer LifetimeScope found in the scene!");
            return;
        }

        var player = serviceProvider.Container.Resolve<PlayerController>();
        
        if (player == null)
        {
            Debug.LogError("Could not resolve Player. Make sure it is registered in the container!");
            return;
        }

        player.WeaponController.SwitchToWeapon(RangedWeaponName, WeaponSlot.SlotType.RightHand);
    }
    
    [Category("Weapon")]
    [DisplayName("Meelee Weapon Name")]
    public string MeeleeWeaponName { get; set; }

    [Category("Weapon")]
    [DisplayName("Change Meelee Weapon")]
    public void ChangeMeeleeWeapon()
    {
        var serviceProvider = Object.FindObjectOfType<VContainer.Unity.LifetimeScope>();
        
        if (serviceProvider == null)
        {
            Debug.LogError("No VContainer LifetimeScope found in the scene!");
            return;
        }

        var player = serviceProvider.Container.Resolve<PlayerController>();
        
        if (player == null)
        {
            Debug.LogError("Could not resolve Player. Make sure it is registered in the container!");
            return;
        }

        player.WeaponController.SwitchToWeapon(MeeleeWeaponName, WeaponSlot.SlotType.Melee);
    }

    [Category("IOS Test")]
    [DisplayName("Open Battery Settings")]
    public void OpenBatterySettings()
    {
#if UNITY_IOS
        Application.OpenURL("App-Prefs:root=Battery");
#endif
    }

#endif
}