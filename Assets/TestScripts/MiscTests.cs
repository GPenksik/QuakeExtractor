// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.IO;
// using System.Text;
// using UnityQuake.Utils;

// namespace TestScripts
// {
//     public class MiscTests : MonoBehaviour
//     {
//         public string bspFilename = "start.bsp";
//         public const int MAX_STYLESTRING = 64;
//         public Color32 testColor = new(0,0,0,255);
//         public lightstyle_t lightstyle;
//         public byte[] byteArray;

//         public struct lightstyle_t
//         {
//             public int		length;
//             public byte[]	map; //* size 64 Maps a string to a light value in animation sequence

//             public lightstyle_t(bool x) : this() { 
//                 map = new byte[MAX_STYLESTRING];
//             }

//             public void SetMap(string newMap) {
//                 length = 0;
//                 byte offset = (byte)96;
//                 foreach (char val in newMap) {
//                     map[length] = (Convert.ToByte(val));
//                     map[length++] -= offset;
//                 }
//             }
//         }

//         // public struct headers

//         private byte[] ReadAllBytes(BinaryReader reader)
//         {
//             const int bufferSize = 4096;
//             using (var ms = new MemoryStream())
//             {
//                 byte[] buffer = new byte[bufferSize];
//                 int count;
//                 while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
//                     ms.Write(buffer, 0, count);
//                 return ms.ToArray();
//             }
//         }

//     private byte[] getByteArray()
//     {
//         string bspPath = BspPaths.BspFiles + this.bspFilename;


//         if (File.Exists(bspPath))
//         {
//             using (var stream = File.Open(bspPath, FileMode.Open))
//             {
//                 using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
//                 {
//                     return ReadAllBytes(reader);
//                 }
//             }
//         }
//         else
//         {
//             Debug.LogError("BSP FILE NOT FOUND");
//             return null;
//         }
//     }

//         public byte GetLightValueFromStyle(int indexIntoLightstyle) {
            
//             return lightstyle.map[indexIntoLightstyle];

//         }

//         void Awake() 
//         {
//             byteArray = getByteArray();
//             lightstyle = new lightstyle_t(1==1);
//             string vals = "amz";
//             foreach (char val in vals) {
//                 Debug.Log(Convert.ToByte(val)); ;
//             }

//             lightstyle.SetMap("mmamammmmammamamaaamammma");

//         }

//         void Update()
//         {
//             float time = Time.time * 10f;
//             int frame = (int)(time % lightstyle.length);
//             byte lightLevel =  GetLightValueFromStyle(frame);
//             if (testColor.r != lightLevel) {
//                 testColor.r = (byte)(lightLevel<<2);
//                 testColor.g = (byte)(lightLevel<<2);
//                 testColor.b = (byte)(lightLevel<<2);
//             }
//         }
//     }
// }
