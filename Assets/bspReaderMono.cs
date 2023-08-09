using bspMapReader;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class bspReaderMono : MonoBehaviour
{
    public Color32[] palette;
    [Header("BSP File")]
    // [SerializeField]
    public DefaultAsset bspFile;
    public string bspFilename;
    public bspMapScriptable mapScriptable;

    [Header("Palette")]
    public DefaultAsset paletteFile;
    public string paletteFilename;

    [Header("Materials")]
    public Material baseMaterial;
    public Material skyMaterial;
    public Material waterMaterial;


    public ReadBSPtoScriptable bspReader = new ReadBSPtoScriptable();
}
