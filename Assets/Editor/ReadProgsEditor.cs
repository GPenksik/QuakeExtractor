using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using bspMapReader;
using UnityQuake.Utils;

namespace UnityQuake.Progs
{
[CustomEditor(typeof(ReadProgs))]
public class ReadProgsEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ReadProgs thisReader = target as ReadProgs;
        if (GUILayout.Button("Load Header"))
        {
            thisReader.LoadProgs(thisReader.progsFilename);
        }

        if (GUILayout.Button("Update"))
        {
            thisReader.UpdateDdef();
        }
    }
}
}