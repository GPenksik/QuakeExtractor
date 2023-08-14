using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityQuake.Utils;
using static UnityQuake.Progs.ProgDefs;

namespace UnityQuake.Progs
{

    public static class Progs
    {
        public static string progsFilename = "progs.dat";

        public static dprograms_t progs = new();
        public static List<dfunction_t> dfunctions = new();

        public static void PR_LoadProgs()
        {
            ByteBuffer.Clear();
            dfunctions.Clear();
            progs = new();

            ByteBuffer.InitializeBufferFromByteArray(ReadBinary.GetByteArray(progsFilename, BspPaths.Progs));

            MemoryStream ms = new(ByteBuffer.buffer);
            BinaryReader reader = new BinaryReader(ms);

            ProgsReader.Initialize(reader, progs);
            ProgsReader.FillProgs();

            // TODO Initialize all the rest of the data

        }
    }

    [ExecuteInEditMode]
    public class ReadProgs : MonoBehaviour
    {

        public dprograms_t progs = new();

        [Header("dfunction_t")]
        [ContextMenuItem("Get index", "UpdateFuncByIndex")]
        public int functionIndex = 0;

        [ContextMenuItem("Find by name", "FindFunctionByName")]
        public string functionNameToFind = "";

        public dfunction_t function = new();

        [Header("ddef_t")]
        [ContextMenuItem("Get index", "UpdateDefByIndex")]
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

        public void LoadProgs(string progsFilename)
        {

            ByteBuffer.Clear();
            dfunctions.Clear();
            progs = new();
            ByteBuffer.InitializeBufferFromByteArray(ReadBinary.GetByteArray(progsFilename, BspPaths.Progs));

            MemoryStream ms = new(ByteBuffer.buffer);
            BinaryReader reader = new BinaryReader(ms);

            ProgsReader.Initialize(reader, progs);
            ProgsReader.FillProgs();

        }
    }
}