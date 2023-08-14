using System.Globalization;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityQuake.Progs
{
    public class ProgsBuffer : MonoBehaviour
    {
        public int testValue;

    }

    #region ByteBuffer
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

        public static void InitializeBufferFromByteArray(byte[] byteArrayIn)
        {
            buffer = byteArrayIn;
            isInitialized = true;

        }


        internal static void Clear()
        {
            buffer = null;
            isInitialized = false;
        }
        // public static void Add(SerializableObject SO, int dest) 
        // {
        //     if (isInitialized) 
        //     {
        //         SO.Serialize(dest);
        //     }
        // }
    }
    #endregion


}