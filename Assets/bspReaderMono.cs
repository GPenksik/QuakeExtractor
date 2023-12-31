using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class bspReaderMono : MonoBehaviour
{
    [SerializeField]
    public DefaultAsset bspFile;
    public string bspFilename;
    public DefaultAsset paletteFile;
    public string paletteFilename;
    public Material baseMaterial;
    public ReadBSPtoScriptable bspReader = new ReadBSPtoScriptable();
}
