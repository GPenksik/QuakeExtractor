using System.IO;
using System.Text;
using UnityEngine;

namespace UnityQuake.Utils
{

/**----------------------
 **    SET GLOBAL PATHS
 *------------------------**/
public static class BspPaths {

        static public string Root =  "Assets/";
        static public string BspFiles =  "Assets/";
        static public string Textures =  "Assets/Resources/Textures/";
        static public string RawTextures =  "Assets/Resources/Textures/Raw/";
        static public string Materials =  "Assets/Resources/Materials/";
        static public string Meshes =  "Assets/Resources/Meshes/";
        static public string ColorMaps =  "Assets/Resources/ColorMaps/";
        static public string Progs =  "Assets/Resources/Progs/";
}

/**----------------------
 **    VARIOUS FILE UTILITIES
 *------------------------**/
public static class ReadBinary {

    public static BinaryReader GetBinaryReader(string binaryFilename, string binaryPath = ""){
        if (binaryPath == "") {
            binaryPath = BspPaths.Root;
        }

        string binaryFullPath = binaryPath + binaryFilename;

        if (File.Exists(binaryFullPath))
        {   
            var stream = File.Open(binaryFullPath, FileMode.Open);
            return new BinaryReader(stream, Encoding.UTF8, false);
        }
        else
        {
            Debug.LogError("BSP FILE NOT FOUND");
            return null;
        }
    }
    // public static MemoryStream GetByteArrayAsMS(string binaryFilename, string binaryPath = "")
    // {
    //     if (binaryPath == "") {
    //         binaryPath = BspPaths.Root;
    //     }

    //     string binaryFullPath = binaryPath + binaryFilename;

    //     if (File.Exists(binaryFullPath))
    //     {
    //         using (var stream = File.Open(binaryFullPath, FileMode.Open))
    //         {
    //             using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
    //             {
    //                 const int bufferSize = 4096;
    //                 using (var ms = new MemoryStream())
    //                 {
    //                     byte[] buffer = new byte[bufferSize];
    //                     int count;
    //                     while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
    //                         ms.Write(buffer, 0, count);
    //                     return ms;
    //                 }
    //             }
    //         }
    //     }
    //     else
    //     {
    //         Debug.LogError("BSP FILE NOT FOUND");
    //         return null;
    //     }
    // }

    public static byte[] GetByteArray(string binaryFilename, string binaryPath = "") 
    {
        if (binaryPath == "") 
        {
            binaryPath = BspPaths.Root;
        }

        string binaryFullPath = binaryPath + binaryFilename;

        if (File.Exists(binaryFullPath))
        {
            using (var stream = File.Open(binaryFullPath, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
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
        }
        else
        {
            Debug.LogError("BSP FILE NOT FOUND");
            return null;
        }
    }

    // public static MemoryStream ReadAllBytesAsMS(BinaryReader reader) {
    //     return WriteBuffer(reader);
    // }

    // public static MemoryStream WriteBuffer(BinaryReader reader) {
    //     const int bufferSize = 4096;
    //     using (var ms = new MemoryStream())
    //     {
    //         byte[] buffer = new byte[bufferSize];
    //         int count;
    //         while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
    //             ms.Write(buffer, 0, count);
    //         return ms;
    //     }
    // }
}
}