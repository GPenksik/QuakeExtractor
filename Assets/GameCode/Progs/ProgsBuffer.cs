using System.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityQuake.Progs.ProgDefs;

namespace UnityQuake.Progs
{
public class ProgsBuffer : MonoBehaviour
{

    public int testValue;

    public void TestTheBuffer(int n) {
        ByteBuffer.InitializeBuffer(4096);

        testDataType td1 = new(n++, n++);
        testDataType td2 = new(n++, n++);
        testContainer tc1 = new(td1, td2);

        ByteBuffer.Add(tc1, 0);

        for (int i = 0; i < tc1.objBytes; i++) {
            Debug.Log(i + " = " + ByteBuffer.buffer[i]);
        }


        for (int i = 0; i < 256; i++) {
            ByteBuffer.buffer[i] = (byte)(ByteBuffer.buffer[i]<<1);
        }

        tc1.DeSerialize();
        Debug.Log("testInt1 = " + tc1.dt1.value1);
    }

    void Awake() {
        TestTheBuffer((int)Time.time);
    }
}

public static class ByteBuffer
{
    public static byte[] buffer;

    public static int bufferSize { get => buffer.Length; }

    public static bool isInitialized;

    public static void InitializeBuffer(int length)
    {
        buffer = new byte[length];
        isInitialized = true;
    }

    public static void InitializeBufferFromByteArray(byte[] byteArrayIn) {
        buffer = byteArrayIn;
        isInitialized = true;

    }

    public static void Add(SerializableObject SO, int dest) 
    {
        if (isInitialized) 
        {
            SO.Serialize(dest);
        }
    }

        internal static void Clear()
        {
            buffer = null;
            isInitialized = false;
        }
    }

public static class ProgsReader {
    public static dprograms_t progs;
    public static BinaryReader reader;
    static long savedPosition;
    public static bool isInitialized { get => IsInitialized(); }

    public static void Initialize(BinaryReader newReader, dprograms_t newProgs) {
        if (isInitialized) {
            reader.Dispose();
            reader = null;
        }
        progs = newProgs;
        reader = newReader;
        savedPosition = 0;
    }

    public static void FillProgs() 
    {
        MarkPosition();
        Set(0);
        progs.version =         reader.ReadInt32();
        progs.crc =             reader.ReadInt32();
        progs.ofs_statements =  reader.ReadInt32();
        progs.numstatements =   reader.ReadInt32();
        progs.ofs_globaldefs =  reader.ReadInt32();
        progs.numglobaldefs =   reader.ReadInt32();
        progs.ofs_fielddefs =   reader.ReadInt32();
        progs.numfielddefs =    reader.ReadInt32();
        progs.ofs_functions =   reader.ReadInt32();
        progs.numfunctions =    reader.ReadInt32();
        progs.ofs_strings =     reader.ReadInt32();
        progs.numstrings =      reader.ReadInt32();
        progs.ofs_globals =     reader.ReadInt32();
        progs.numglobals =      reader.ReadInt32();
        progs.entityfields =    reader.ReadInt32();
        ReturnToMark();
    }

    public static int GetPosition()
    {
        if (isInitialized)
        {
            return (int)reader.BaseStream.Position;
        } else {
            Debug.LogError("Cant get position with uninitialized reader");
            return -1;
        }
    }

    public static void Set(int newPosition) 
    {
        if (isInitialized)
        {
            if (newPosition < reader.BaseStream.Length & newPosition >= 0) {
                reader.BaseStream.Position = newPosition;
            } else {
                Debug.LogError("ProgsReader: New position is outside of the buffer");
            }
        } else {
            Debug.LogError("Cant set position of uninitialized buffer");
        }
    }

    public static void Offset(int offset) 
    {   
        if (isInitialized)
        {
            var newPosition = reader.BaseStream.Position + offset;
            if (newPosition < reader.BaseStream.Length & newPosition >= 0) {
                reader.BaseStream.Position = newPosition;
            }
        } else {
            Debug.LogError("ReadBuffer not initialized");
        }
    }

    public static void MarkPosition()
    {
        if (isInitialized) {
            savedPosition = reader.BaseStream.Position;
        } else {
            Debug.LogError("ProgsReader not initialized");
        }
    }

    public static void ReturnToMark()
    {
        reader.BaseStream.Position = savedPosition;
    }

    static bool IsInitialized() 
    {
        if (reader != null & progs != null) {
            return true;
        } else {
            return false;
        }
    }
}

public class testDataType : SerializableObject {

    public int value1;
    public int value2;

    public testDataType(int first, int second)
    {
        objBytes += 8;
        value1 = first;
        s.Add(new SInt(value1));
        value2 = second;
        s.Add(new SInt(value2));
    }

    public override void DeSerialize()
    {
        s.Update();
        value1 = ((SInt)s.serializableObjects[0]).value;
        value2 = ((SInt)s.serializableObjects[1]).value;
    }

    public override byte[] Serialize(int pointer)
    {
        this.pointer = pointer;
        return s.GetBytes(pointer);
    }
}

public class testContainer : SerializableObject {

    public testDataType dt1;
    public testDataType dt2;

    public testContainer(testDataType dt1, testDataType dt2) {
        objBytes += dt1.objBytes;
        this.dt1 = dt1;
        s.Add(dt1);
        objBytes += dt2.objBytes;
        this.dt2 = dt2;
        s.Add(dt2);
    }

    public override void DeSerialize()
    {
        s.Update();
    }

    public override byte[] Serialize(int pointer)
    {
        this.pointer = pointer;
        return s.GetBytes(pointer);
    }
}

public class Serializer 
{
    public readonly List<SerializableObject> serializableObjects = new();

    public int sBytes = 0;

    public void Add(SerializableObject obj) 
    {
        Add(obj, obj.objBytes);
    }

    public void Add(SerializableObject obj, int bytes) {
        serializableObjects.Add(obj);
        sBytes += bytes;
    }

    public byte[] GetBytes(int pointer) 
    {
        byte[] byteArray = new byte[sBytes];
        int byteOffset = 0;
        int nextLength;
        foreach (SerializableObject obj in serializableObjects)
        {
            nextLength = obj.objBytes;
            if (byteOffset + nextLength <= sBytes)
            {
                obj.Serialize(pointer);
                byteOffset += nextLength;
                pointer += nextLength;
            }
            else
            {
                Debug.LogError("Serialize failed. Returned data too big for expected buffer returning incomplete array");
                return byteArray;
            }
        }
        return byteArray;
    }

    public void Update() 
    {
        foreach (SerializableObject obj in serializableObjects)
        {
            obj.DeSerialize();
        }
    }
}

public abstract class SerializableObject 
{
    protected Serializer s = new();

    protected int pointer;

    public int objBytes;

    public abstract byte[] Serialize(int pointer);
    public abstract void DeSerialize();

}

public class SInt : SerializableObject
{
    public int value;

    public SInt(int value) {
        objBytes = 4;
        this.value = value;
        s.Add(this);
    }

    override public byte[] Serialize(int pointer) {
        this.pointer = pointer;
        Array.Copy(BitConverter.GetBytes(value),0,ByteBuffer.buffer,pointer, objBytes);
        return new byte[0];
    }

    public override void DeSerialize()
    {
        value = BitConverter.ToInt32(ByteBuffer.buffer, pointer);
    }
}
}