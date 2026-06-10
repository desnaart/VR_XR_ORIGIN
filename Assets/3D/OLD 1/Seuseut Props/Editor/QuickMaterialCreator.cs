using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.ShortcutManagement;

public class CreateMaterialHere
{
    [MenuItem("Assets/Quick Create/Material Here", false, 0)]
    public static void CreateMaterialInSelectedFolder()
    {
        string path = GetSelectedPathOrFallback();
        string materialPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, "New URP Material.mat"));

        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader == null)
        {
            Debug.LogWarning("URP Shader not found! Make sure URP is set up in your project.");
            urpShader = Shader.Find("Standard");
        }

        Material newMaterial = new Material(urpShader);
        AssetDatabase.CreateAsset(newMaterial, materialPath);
        AssetDatabase.SaveAssets();

        Selection.activeObject = newMaterial;
        EditorGUIUtility.PingObject(newMaterial);
    }

    private static string GetSelectedPathOrFallback()
    {
        string path = "Assets";

        foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
        {
            string currentPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(currentPath))
            {
                if (File.Exists(currentPath))
                    currentPath = Path.GetDirectoryName(currentPath);

                path = currentPath;
                break;
            }
        }
        return path;
    }

    // ✅ Keyboard Shortcut: CTRL + SHIFT + M
    [Shortcut("QuickCreate/Material Here URP", KeyCode.M, ShortcutModifiers.Shift | ShortcutModifiers.Control)]
    public static void ShortcutCreateMaterial()
    {
        CreateMaterialInSelectedFolder();
    }
}
