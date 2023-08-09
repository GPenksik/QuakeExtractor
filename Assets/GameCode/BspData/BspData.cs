using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;


public static class BspData {

    public const string bspPath = "Assets/start.bsp";
    // public public Data dataStruct;

    public ref struct Data {
        public Span<byte> rawData;
    }


    // public static bool NullCheck() {

    //     if (Data == null) {
    //         Data = ReadBytes();
    //     }

    //     return true;

    // } 



    public static byte[] ReadBytes() 
    {
        if (File.Exists(bspPath))
        {
            using (var stream = File.Open(bspPath, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, System.Text.Encoding.UTF8, false))
                {
                    return ReadAllBytes(reader);
                }
            }
        }
        else
        {
            Debug.LogError("BSP FILE NOT FOUND");
            return null;
        }
    }

    private static byte[] ReadAllBytes(BinaryReader reader)
    {
        const int bufferSize = 4096;
        using (var ms = new MemoryStream())
        {
            byte[] buffer = new byte[bufferSize];
            int count;
            while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
                ms.Write(buffer, 0, count);
            return ms.ToArray();
        }
    }

}

