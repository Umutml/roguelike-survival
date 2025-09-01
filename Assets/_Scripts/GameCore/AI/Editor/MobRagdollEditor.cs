#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using _Scripts.GameCore.AI.RagdollController;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MobRagdollController))]
public class MobRagdollEditor : Editor
{
    private MobRagdollController _mobRagdollController;
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); 
        _mobRagdollController = (MobRagdollController)target;
        if (GUILayout.Button("Create Ragdoll Structure"))
        {
            CreateRagdoll(_mobRagdollController);
        }
    }

    private void CreateRagdoll(MobRagdollController mobRagdollController)
    {
        var allRagdollObjects = mobRagdollController.GetComponentsInChildren<Rigidbody>();
        var ragdollParts = new List<RagdollPart>();
        foreach (var ragdollObject in allRagdollObjects)
        {
            var ragdollPart = new RagdollPart(ragdollObject.gameObject);
            ragdollParts.Add(ragdollPart);
        }
        mobRagdollController.SetRagdollParts(ragdollParts.ToArray());
        mobRagdollController.SetActiveRagdoll(false);
        EditorUtility.SetDirty(_mobRagdollController);
    }
}
#endif

