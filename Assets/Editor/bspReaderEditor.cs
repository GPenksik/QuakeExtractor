using bspMapReader;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(bspReaderMono))]
class bspReaderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();


        if (GUILayout.Button("Parse BSP"))
        {
            bspReaderMono thisReader = target as bspReaderMono;
            thisReader.bspFilename = thisReader.bspFile.name + ".bsp";
            thisReader.paletteFilename = thisReader.paletteFile.name + ".lmp";
            deleteMap(FindMap(thisReader.bspFile.name));
            bspMapScriptable mapScriptable = (bspMapScriptable)ScriptableObject.CreateInstance("bspMapScriptable");
            AssetDatabase.CreateAsset(mapScriptable, "Assets/" + thisReader.bspFile.name + ".asset");
            EditorUtility.SetDirty(mapScriptable);
            thisReader.bspReader.baseMaterial = thisReader.baseMaterial;
            thisReader.bspReader.skyMaterial = thisReader.skyMaterial;
            mapScriptable = thisReader.bspReader.ReadBSP(thisReader.bspFilename, thisReader.paletteFilename, mapScriptable);
            AssetDatabase.SaveAssets();
            thisReader.mapScriptable = mapScriptable;
        }
        if (GUILayout.Button("Parse LM"))
        {
            bspReaderMono thisReader = target as bspReaderMono;
            thisReader.bspFilename = thisReader.bspFile.name + ".bsp";
            thisReader.paletteFilename = thisReader.paletteFile.name + ".lmp";
            EditorUtility.SetDirty(thisReader.mapScriptable);
            thisReader.bspReader.bspFilename = thisReader.bspFilename;
            thisReader.mapScriptable = thisReader.bspReader.getLightMaps(thisReader.mapScriptable);
            AssetDatabase.SaveAssets();
        }
        if (GUILayout.Button("Rebuild Materials"))
        {
            bspReaderMono thisReader = target as bspReaderMono;
            thisReader.bspFilename = thisReader.bspFile.name + ".bsp";
            thisReader.paletteFilename = thisReader.paletteFile.name + ".lmp";
            thisReader.bspReader.baseMaterial = thisReader.baseMaterial;
            thisReader.bspReader.skyMaterial = thisReader.skyMaterial;
            thisReader.bspReader.rebuildMaterials(thisReader.mapScriptable.miptexs);
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
