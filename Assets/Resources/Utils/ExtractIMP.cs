using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using UnityQuake.Progs;


// namespace UnityQuake.Utils 
// {

// public class ExtractIMP : MonoBehaviour {

//     public string bspPath = BspPaths.BspFiles + "start.bsp";

//     public string imageFilename = "BACKTILE";

//     public byte[] byteArray;

//     public Color32[] paletteColors;

//     public Texture2DArray texTest;

//     public RenderTexture rtt;

//     void Awake() {
//         ExtractImage(imageFilename, rtt);
//     }


//     void Update() {
//         _gpu_scale(texTest, 1024, 1024, FilterMode.Point, rtt);      
//     }

//     public void ExtractImage(string imageName, RenderTexture rtt)
//     {
//         Texture2DArray palette = AssetDatabase.LoadAssetAtPath<Texture2DArray>(BspPaths.ColorMaps + "palette.asset");
//         paletteColors = palette.GetPixels32(0,0);

//         string imagePath = BspPaths.ColorMaps + imageName + ".lmp";

//         if (File.Exists(imagePath))
//         {
//             using (var stream = File.Open(imagePath, FileMode.Open))
//             {
//                 using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
//                 {
//                     byteArray = ReadAllBytes(reader);
//                 }
//             }
//         }


//         string imageTexturePath = BspPaths.ColorMaps + imageName + ".asset";

//         int width = BitConverter.ToInt32(byteArray, 0);
//         int height = BitConverter.ToInt32(byteArray, 4);

//         int length = width * height;

//         // int SCALE = 2;

//         Texture2DArray imageTexture;
//         imageTexture = new(width, height, 1, TextureFormat.RGBA32, false);
//         // biggerTexture = new(width<<SCALE, height<<SCALE, 1, TextureFormat.RGBA32, false);

//         var imageRawData = imageTexture.GetPixels32(0);

//         for (int n_pixel = 0; n_pixel < length; n_pixel++) {
//             imageRawData[n_pixel] = paletteColors[byteArray[n_pixel + 8]];
//         }

//         imageTexture.filterMode = FilterMode.Point;

//         imageTexture.SetPixels32(imageRawData,0);
//         imageTexture.Apply();

//         texTest = imageTexture;

//         // imageRawData = imageTexture.GetPixels32(0);

//         // Texture2D biggerTexture;
//         // biggerTexture = _gpu_scale(imageTexture, width<<SCALE, height<<SCALE, FilterMode.Point, rtt);

//         // imageRawData = biggerTexture.GetPixels32(0);

//         // AssetDatabase.CreateAsset(biggerTexture, imageTexturePath);

//         // AssetDatabase.SaveAssets();
//     }

//     // Internal utility that renders the source texture into the RTT - the scaling method itself.
//     public void _gpu_scale(Texture2DArray src, int width, int height, FilterMode fmode, RenderTexture rtt)
//     {
//         texTest.filterMode = FilterMode.Point;
//         texTest.Apply(false);
//         //Using RTT for best quality and performance. Thanks, Unity 5
//         // RenderTexture rtt = new RenderTexture(256, 256, 32);
//         //Set the RTT in order to render to it

//         RenderTexture.active = rtt;
//         GL.PushMatrix();
//         GL.LoadPixelMatrix();

//         // Graphics.SetRenderTarget(rtt);

//         //Setup 2D matrix in range 0..1, so nobody needs to care about sized
//         // GL.LoadPixelMatrix(0,1,1,0);

//         //Then clear & draw the texture to fill the entire RTT.
//         // int color = UnityEngine.Random.Range(0, 1);
//         // GL.Clear(true,true,new Color(color,color,color,color));
//         // Graphics.DrawTexture(new Rect(0,0,1,1), texTest);
//         Graphics.Blit(texTest, rtt);  

//         // Texture2D tex2DCopy = new(width, height, TextureFormat.RGBA32, false);
//         // tex2DCopy.ReadPixels(new Rect(0, 0, rtt.width, rtt.height), 0, 0);
//         // tex2DCopy.Apply(false);

//         GL.PopMatrix();
//         RenderTexture.active = null;

//         // return tex2DCopy;
//     }

//     private byte[] ReadAllBytes(BinaryReader reader)
//     {
//         const int bufferSize = 4096;
//         using (var ms = new MemoryStream())
//         {
//             byte[] buffer = new byte[bufferSize];
//             int count;
//             while ((count = reader.Read(buffer, 0, buffer.Length)) != 0)
//                 ms.Write(buffer, 0, count);
//             return ms.ToArray();
//         }
//     }
// }
// }

// #region OldSerializedObject

// public class testDataType : SerializableObject {
    
//         public int value1;
//         public int value2;
    
//         public testDataType(int first, int second)
//         {
//             objBytes += 8;
//             value1 = first;
//             s.Add(new SInt(value1));
//             value2 = second;
//             s.Add(new SInt(value2));
//         }
    
//         public override void DeSerialize()
//         {
//             s.Update();
//             value1 = ((SInt)s.serializableObjects[0]).value;
//             value2 = ((SInt)s.serializableObjects[1]).value;
//         }
    
//         public override byte[] Serialize(int pointer)
//         {
//             this.pointer = pointer;
//             return s.GetBytes(pointer);
//         }
//     }
    
//     public class testContainer : SerializableObject {
    
//         public testDataType dt1;
//         public testDataType dt2;
    
//         public testContainer(testDataType dt1, testDataType dt2) {
//             objBytes += dt1.objBytes;
//             this.dt1 = dt1;
//             s.Add(dt1);
//             objBytes += dt2.objBytes;
//             this.dt2 = dt2;
//             s.Add(dt2);
//         }
    
//         public override void DeSerialize()
//         {
//             s.Update();
//         }
    
//         public override byte[] Serialize(int pointer)
//         {
//             this.pointer = pointer;
//             return s.GetBytes(pointer);
//         }
//     }
    
//     public class Serializer 
//     {
//         public readonly List<SerializableObject> serializableObjects = new();
    
//         public int sBytes = 0;
    
//         public void Add(SerializableObject obj) 
//         {
//             Add(obj, obj.objBytes);
//         }
    
//         public void Add(SerializableObject obj, int bytes) {
//             serializableObjects.Add(obj);
//             sBytes += bytes;
//         }
    
//         public byte[] GetBytes(int pointer) 
//         {
//             byte[] byteArray = new byte[sBytes];
//             int byteOffset = 0;
//             int nextLength;
//             foreach (SerializableObject obj in serializableObjects)
//             {
//                 nextLength = obj.objBytes;
//                 if (byteOffset + nextLength <= sBytes)
//                 {
//                     obj.Serialize(pointer);
//                     byteOffset += nextLength;
//                     pointer += nextLength;
//                 }
//                 else
//                 {
//                     Debug.LogError("Serialize failed. Returned data too big for expected buffer returning incomplete array");
//                     return byteArray;
//                 }
//             }
//             return byteArray;
//         }
    
//         public void Update() 
//         {
//             foreach (SerializableObject obj in serializableObjects)
//             {
//                 obj.DeSerialize();
//             }
//         }
//     }
    
//     public abstract class SerializableObject 
//     {
//         protected Serializer s = new();
    
//         protected int pointer;
    
//         public int objBytes;
    
//         public abstract byte[] Serialize(int pointer);
//         public abstract void DeSerialize();
    
//     }
    
//     public class SInt : SerializableObject
//     {
//         public int value;
    
//         public SInt(int value) {
//             objBytes = 4;
//             this.value = value;
//             s.Add(this);
//         }
    
//         override public byte[] Serialize(int pointer) {
//             this.pointer = pointer;
//             Array.Copy(BitConverter.GetBytes(value),0,ByteBuffer.buffer,pointer, objBytes);
//             return new byte[0];
//         }
    
//         public override void DeSerialize()
//         {
//             value = BitConverter.ToInt32(ByteBuffer.buffer, pointer);
//         }
//     }
// #endregion