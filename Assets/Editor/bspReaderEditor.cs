using System.IO;
using bspMapReader;
using UnityEditor;
using UnityEngine;

namespace UnityQuake.MapReader 
{
[CustomEditor(typeof(bspReaderMono))]
class BspReaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Parse BSP"))
        {
            bspReaderMono thisReader = target as bspReaderMono;
            thisReader.bspFilename = thisReader.bspFile.name + ".bsp";
            thisReader.paletteFilename = thisReader.paletteFile.name + ".lmp";
            // deleteMap(FindMap(thisReader.bspFile.name));

            string fullAssetPath = "Assets/" + thisReader.bspFile.name + ".asset";

            bspMapScriptable mapScriptable;
            if (File.Exists(fullAssetPath)) {
                mapScriptable = AssetDatabase.LoadAssetAtPath<bspMapScriptable>(fullAssetPath);
            } else {
                mapScriptable = (bspMapScriptable)ScriptableObject.CreateInstance("bspMapScriptable");
                AssetDatabase.CreateAsset(mapScriptable, "Assets/" + thisReader.bspFile.name + ".asset");
            }
            EditorUtility.SetDirty(mapScriptable);

            thisReader.bspReader.baseMaterial = thisReader.baseMaterial;

            thisReader.bspReader.skyMaterial = thisReader.skyMaterial;

            EditorUtility.CopySerialized(thisReader.bspReader.ReadBSP(thisReader.bspFilename, thisReader.paletteFilename, mapScriptable),mapScriptable);
            
            AssetDatabase.SaveAssets();
            
            thisReader.mapScriptable = mapScriptable;

            thisReader.palette = thisReader.bspReader.colorPalette;;


        }
        if (GUILayout.Button("Parse LM"))
        {
            bspReaderMono thisReader = target as bspReaderMono;
            thisReader.bspFilename = thisReader.bspFile.name + ".bsp";
            thisReader.paletteFilename = thisReader.paletteFile.name + ".lmp";
            
            thisReader.bspReader.bspFilename = thisReader.bspFilename;

            EditorUtility.SetDirty(thisReader.mapScriptable);
            EditorUtility.CopySerialized(thisReader.mapScriptable,thisReader.bspReader.GetLightMaps(thisReader.mapScriptable));
            
            AssetDatabase.SaveAssets();
        }

        if (GUILayout.Button("Rebuild Materials"))
        {
            bspReaderMono thisReader = target as bspReaderMono;
            thisReader.bspFilename = thisReader.bspFile.name + ".bsp";
            thisReader.paletteFilename = thisReader.paletteFile.name + ".lmp";
            thisReader.bspReader.baseMaterial = thisReader.baseMaterial;
            thisReader.bspReader.skyMaterial = thisReader.skyMaterial;
            thisReader.bspReader.RebuildMaterials(thisReader.mapScriptable.miptexs);
        }

        if (GUILayout.Button("Rebuild Textures"))
        {
            bspReaderMono thisReader = target as bspReaderMono;
            thisReader.bspFilename = thisReader.bspFile.name + ".bsp";
            thisReader.paletteFilename = thisReader.paletteFile.name + ".lmp";
            thisReader.bspReader.baseMaterial = thisReader.baseMaterial;
            thisReader.bspReader.skyMaterial = thisReader.skyMaterial;
            thisReader.bspReader.GenerateAllTextures(thisReader.mapScriptable);
        }


    }

    private bspMapScriptable FindMap(string filename)
    {
        string[] guids = AssetDatabase.FindAssets(filename + " t:bspMapScriptable");

        foreach (string mapGUID in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(mapGUID);

            bspMapScriptable potentialSession = (bspMapScriptable)AssetDatabase.LoadAssetAtPath(assetPath, typeof(bspMapScriptable));

            return (bspMapScriptable)AssetDatabase.LoadAssetAtPath(assetPath, typeof(bspMapScriptable)); ;
        }

        return null;
    }

    private void deleteMap(bspMapScriptable mapScriptable)
    {
        if (mapScriptable != null)
        {
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(mapScriptable));
        }
    }
}
}