using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityQuake.Utils;
using static UnityQuake.Progs.ProgDefs;

namespace UnityQuake.Progs
{
[ExecuteInEditMode]
public class ReadProgs : MonoBehaviour
{
    public string progsFilename = "progs.dat";
    string progsPath = BspPaths.Progs;

    public dprograms_t progs = new();

    [Header("dfunction_t")]
    [ContextMenuItem("Get index","UpdateFuncByIndex")]
    public int functionIndex = 0;

    [ContextMenuItem("Find by name","FindFunctionByName")]
    public string functionNameToFind = "";

    public dfunction_t function = new();

    [Header("ddef_t")]
    [ContextMenuItem("Get index","UpdateDefByIndex")]
    public int defIndex = 0;
    // [ContextMenuItem("Find by name","FindFuncByName")]
    public string defNameToFind = "";

    public ddef_t ddef = new();

    // public byte[] byteArray;

    public List<dfunction_t> dfunctions = new();

    public void UpdateFuncByIndex()
    {
        function.Update(functionIndex);
    }
    public void UpdateDefByIndex()
    {
        ddef.Update(defIndex);

    }
    public void FindFunctionByName()
    {
        functionIndex = function.FindFunctionByName(functionNameToFind);
    }

    

    public void LoadProgs(string progsFilename) {

        ByteBuffer.Clear();
        dfunctions.Clear();
        progs = new();
        ByteBuffer.InitializeBufferFromByteArray(ReadBinary.GetByteArray(progsFilename, progsPath));

        MemoryStream ms = new(ByteBuffer.buffer);
        BinaryReader reader = new BinaryReader(ms);

        ProgsReader.Initialize(reader, progs);
        ProgsReader.FillProgs();

    }
}
}