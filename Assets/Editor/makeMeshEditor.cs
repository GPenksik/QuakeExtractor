using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using bspMapReader;

[CustomEditor(typeof(MakeMesh))]
public class makeMeshEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        MakeMesh thisMaker = target as MakeMesh;
        if (GUILayout.Button("Make Mesh"))
        {
            thisMaker.BuildMesh(false, false);
        }
        if (GUILayout.Button("Make All"))
        {
            thisMaker.BuildMesh(true);
        }
        if (GUILayout.Button("Build Prefabs"))
        {
            thisMaker.BuildPrefabs(thisMaker.umodels);
        }
        if (GUILayout.Button("Destroy Prefabs"))
        {
            thisMaker.DestroyOldModels();
        }


    }

}
