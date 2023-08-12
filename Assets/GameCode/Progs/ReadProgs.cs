using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using UnityQuake.Progs;
using UnityQuake.Utils;
using static UnityQuake.Progs.ProgDefs;

namespace UnityQuake.Progs
{
public class ReadProgs : MonoBehaviour
{
    public string progsFilename = "progs.dat";
    string progsPath = BspPaths.Progs;

    public dprograms_t progs = new();

    public int functionIndex = 0;

    public dfunction_t function = new();

    public int defIndex = 0;

    public ddef_t ddef = new();

    // public byte[] byteArray;

    public List<dfunction_t> dfunctions = new();

    public void UpdateDdef()
    {
        ddef.Update(defIndex);
    }

    public void LoadProgs(string progsFilename) {

        ByteBuffer.Clear();
        dfunctions.Clear();
        progs = new();
        ByteBuffer.InitializeBufferFromByteArray(ReadBinary.GetByteArray(progsFilename, progsPath));

        MemoryStream ms = new(ByteBuffer.buffer);
        BinaryReader reader = new BinaryReader(ms);

        // using (reader) {
            ProgsReader.Initialize(reader, progs);
            ProgsReader.FillProgs();
        // }
        //     ProgsReader.set(ProgsReader.progs.ofs_functions);
        //     for (int i = 0; i < progs.numfunctions; i++) 
        //     {   
        //         dfunction_t newdfunc = new();
        //         newdfunc.fill();
        //         dfunctions.Add(newdfunc);
        //     }
        // }
    }
}
}