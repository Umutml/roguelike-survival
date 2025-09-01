using UnityEngine;
using UnityEditor;

public class ReplaceLitWithSimpleLitEditor
{
    [MenuItem("Tools/Replace URP Lit with Simple Lit")]
    static void ReplaceShaders()
    {
        Shader simpleLitShader = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (!simpleLitShader)
        {
            Debug.LogError("Simple Lit shader not found!");
            return;
        }

        string urpLitShaderName = "Universal Render Pipeline/Lit";
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");

        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat.shader.name == urpLitShaderName)
            {
                mat.shader = simpleLitShader;
                EditorUtility.SetDirty(mat);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Replaced all URP/Lit materials with Simple Lit.");
    }
}
