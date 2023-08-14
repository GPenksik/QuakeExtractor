using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityQuake.Progs.ProgDefs;


#region ProgsReader
public static class ProgsReader
{
    public static dprograms_t progs;
    public static BinaryReader reader;
    // static long savedPosition;
    public static bool isInitialized { get => IsInitialized(); }
    public static Stack<long> savedPositions = new();

    public static void Initialize(BinaryReader newReader, dprograms_t newProgs)
    {
        if (isInitialized)
        {
            reader.Dispose();
            reader = null;
        }
        progs = newProgs;
        reader = newReader;
    }

    public static void FillProgs()
    {
        MarkPosition();
        Set(0);
        progs.version = reader.ReadInt32();
        progs.crc = reader.ReadInt32();
        progs.ofs_statements = reader.ReadInt32();
        progs.numstatements = reader.ReadInt32();
        progs.ofs_globaldefs = reader.ReadInt32();
        progs.numglobaldefs = reader.ReadInt32();
        progs.ofs_fielddefs = reader.ReadInt32();
        progs.numfielddefs = reader.ReadInt32();
        progs.ofs_functions = reader.ReadInt32();
        progs.numfunctions = reader.ReadInt32();
        progs.ofs_strings = reader.ReadInt32();
        progs.numstrings = reader.ReadInt32();
        progs.ofs_globals = reader.ReadInt32();
        progs.numglobals = reader.ReadInt32();
        progs.entityfields = reader.ReadInt32();
        ReturnToMark();
    }

    public static int GetPosition()
    {
        if (isInitialized)
        {
            return (int)reader.BaseStream.Position;
        }
        else
        {
            Debug.LogError("Cant get position with uninitialized reader");
            return -1;
        }
    }

    public static void Set(int newPosition)
    {
        if (isInitialized)
        {
            if (newPosition < reader.BaseStream.Length & newPosition >= 0)
            {
                reader.BaseStream.Position = newPosition;
            }
            else
            {
                Debug.LogError("ProgsReader: New position is outside of the buffer");
            }
        }
        else
        {
            Debug.LogError("Cant set position of uninitialized buffer");
        }
    }

    public static void Offset(int offset)
    {
        if (isInitialized)
        {
            var newPosition = reader.BaseStream.Position + offset;
            if (newPosition < reader.BaseStream.Length & newPosition >= 0)
            {
                reader.BaseStream.Position = newPosition;
            }
        }
        else
        {
            Debug.LogError("ReadBuffer not initialized");
        }
    }

    public static void MarkPosition()
    {
        if (isInitialized)
        {
            savedPositions.Push(reader.BaseStream.Position);
        }
        else
        {
            Debug.LogError("ProgsReader not initialized");
        }
    }

    public static void ReturnToMark()
    {
        if (savedPositions.Count > 0)
        {
            reader.BaseStream.Position = savedPositions.Pop();
        }
    }

    static bool IsInitialized()
    {
        if (reader != null & progs != null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
#endregion
