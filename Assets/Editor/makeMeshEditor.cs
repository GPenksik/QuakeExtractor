using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using bspMapReader;

namespace UnityQuake.MapReader
{
[CustomEditor(typeof(MakeMesh))]
public class makeMeshEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MakeMesh thisMaker = target as MakeMesh;
        if (GUILayout.Button("Make Mesh"))
        {
            thisMaker.BuildMesh(false, true);
        }
        if (GUILayout.Button("Make All"))
        {
            thisMaker.BuildMesh(true, true);
        }
        if (GUILayout.Button("Build Prefabs"))
        {
            thisMaker.BuildPrefabs(thisMaker.umodels, thisMaker.rebuildT2DArrays, true);
        }
        if (GUILayout.Button("Destroy Prefabs"))
        {
            thisMaker.DestroyOldModels();
        }


    }
}
}