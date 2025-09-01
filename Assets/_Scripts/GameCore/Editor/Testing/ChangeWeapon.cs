using GameCore.Player;
using GameCore.Player.WeaponSystem;
using UnityEditor;
using UnityEngine;
using VContainer;

public class ChangeWeapon : EditorWindow
{
    private string weaponName = "";
    private WeaponSlot.SlotType selectedSlotType = WeaponSlot.SlotType.RightHand;
    
    [MenuItem("Testing/Change Weapon")]
    public static void ShowWindow()
    {
        var window = GetWindow<ChangeWeapon>("Change Weapon");
        window.minSize = new Vector2(300, 100);
        window.maxSize = new Vector2(300, 100);
    }
    
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Weapon Settings", EditorStyles.boldLabel);
        
        weaponName = EditorGUILayout.TextField("Weapon Name", weaponName);
        selectedSlotType = (WeaponSlot.SlotType)EditorGUILayout.EnumPopup("Weapon Slot", selectedSlotType);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Change Weapon"))
        {
            Change(weaponName, selectedSlotType);
        }
    }

    public static void Change(string weaponName, WeaponSlot.SlotType weaponSlot)
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

        player.WeaponController.SwitchToWeapon(weaponName, weaponSlot);
    }
}
