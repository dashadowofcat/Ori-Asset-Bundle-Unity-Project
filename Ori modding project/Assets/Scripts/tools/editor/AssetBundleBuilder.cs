using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleBuilder : MonoBehaviour
{
    [MenuItem("Assets/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string assetBundleDirectory = "Assets/Output";
        
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

#if UNITY_EDITOR
        GameObject Level = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/level data/Level.prefab");

        if(Level != null)
        {
            Debug.Log("Saving level assets...");
            SaveLevelAssets(Level.transform);
        }
#endif

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.UncompressedAssetBundle, BuildTarget.StandaloneWindows);

        Debug.Log("Built AssetBundle");
    }

    static void SaveLevelAssets(Transform p)
    {
        for(int i = 0; i < p.childCount; ++i)
        {
            Transform c = p.GetChild(i);

            ILevelAsset levelAsset = c.GetComponent<ILevelAsset>();
            if(levelAsset != null)
            {
                levelAsset.SaveAsset();
            }

            SaveLevelAssets(c);
        }
    }
}; 
