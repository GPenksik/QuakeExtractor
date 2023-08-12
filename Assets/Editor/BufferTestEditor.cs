using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using bspMapReader;

namespace UnityQuake.Progs
{
[CustomEditor(typeof(ProgsBuffer))]
public class BufferTestEditor : Editor
{

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ProgsBuffer thisBuffer = target as ProgsBuffer;
        if (GUILayout.Button("Make Mesh"))
        {
            thisBuffer.TestTheBuffer(thisBuffer.testValue);
        }

    }
}
}