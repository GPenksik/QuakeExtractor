using JetBrains.Annotations;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using UnityEngine;
using UnityEngine.Networking.Types;


public class BasicBinaryRead : MonoBehaviour
{
    // Resources:
    // https://docs.microsoft.com/en-us/dotnet/api/system.io.binarywriter?view=net-5.0
    // https://docs.microsoft.com/en-us/dotnet/api/system.io.binaryreader?view=net-5.0
    // https://docs.microsoft.com/en-us/dotnet/api/system.io.filestream?view=net-5.0
    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/using-statement
    // https://docs.microsoft.com/en-us/dotnet/api/system.io.stream?view=net-5.0


    public enum dheader_t_enum {
        ENTITIES,
        PLANES,
        MIPTEX,
        VERTICES,
        VISILIST,
        NODES,
        TEXINFO,
        FACES,
        LIGHTMAPS,
        CLIPNODES,
        LEAVES,
        LFACE,
        EDGES,
        LEDGES,
        MODELS
    }

    private const string HitCountFile = "Assets/start.bsp"; // 1

    private Byte[] byteArray;

    public int BSPVersion;

    [System.Serializable]
    public struct dentry_t
    {
        public int offset;
        public int size;
    }

    [System.Serializable]
    public struct dheader_t
    {
        public dentry_t[] headers;
    }

    [SerializeField]
    public dheader_t headers = new dheader_t();

    [SerializeField]
    public model_t[] models;

    [SerializeField]
    public model_t model = new model_t();

    [TextAreaAttribute]
    public string Entities;

    [SerializeField]
    public vec3_t[] vertices;

    private void Start() // 7
    {
        if (File.Exists(HitCountFile))
        {
            Byte[] Byte4 = new Byte[4];
            using (var stream = File.Open(HitCountFile, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    byteArray = ReadAllBytes(reader);
                }
            }
            BSPVersion = toInt(byteArray,0);
            headers = ParseHeaders(byteArray,4);
            //models = new model_t[N_MODELS];

            models = ParseModels(byteArray, headers.headers[(int)dheader_t_enum.MODELS].offset);
            Entities = ParseEntities(byteArray, headers.headers[(int)dheader_t_enum.ENTITIES].offset, headers.headers[(int)dheader_t_enum.ENTITIES].size);
            vertices = ParseVectices(byteArray, headers.headers[(int)dheader_t_enum.VERTICES].offset);


        } else
        {
            Debug.Log("File not found");
        }
        
    }

    private byte[] ReadAllBytes(BinaryReader reader)
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

    public int toInt(Byte[] byteArray, int offset)
    {
        return BitConverter.ToInt32(byteArray,(int)offset);
    }

    public dheader_t ParseHeaders(Byte[] byteArray, int offset)
    {
        int N_LUMPS = 15;
        dheader_t headers = new dheader_t();
        headers.headers = new dentry_t[N_LUMPS];

        int i_offset = offset;

        for (int i = 0; i < N_LUMPS; i++)
        {
            headers.headers[i].offset = toInt(byteArray, i_offset);
            i_offset += 4;
            headers.headers[i].size = toInt(byteArray, i_offset);
            i_offset += 4;
        }

        return headers;
    }

    [System.Serializable]
    public struct vec3_t
    {
        public float x;
        public float y;
        public float z;
        public static int n_bytes = sizeof(float)*3;
    }

    [System.Serializable]
    public struct boundbox_t
    {
        public vec3_t min;
        public vec3_t max;
        public static int n_bytes = vec3_t.n_bytes*2;
    }

    [System.Serializable]
    public struct bboxshort_t
    {
        public float min;
        public float max;
    }

    [System.Serializable]
    public struct model_t
    {
        public boundbox_t bound;
        public vec3_t origin;
        public int node_id0_bsp;
        public int node_id1_clip1;
        public int node_id2_clip2;
        public int node_id3_0;
        public int numleafs;
        public int face_id;
        public int face_num;
        public static int n_bytes = boundbox_t.n_bytes + vec3_t.n_bytes + 4*7;
    }

    public model_t[] ParseModels(Byte[] byteArray, int offset)
    {
        int N_MODELS = headers.headers[(int)dheader_t_enum.MODELS].size / model_t.n_bytes;
        model_t[] models = new model_t[N_MODELS];

        int i = 0;
        for (int n_model = 0; n_model < N_MODELS; n_model++)
        {
            model_t model = new model_t();
            model.bound = ParseBoundBox(byteArray, offset + i);
            i += boundbox_t.n_bytes;
            model.origin = ParseVec3(byteArray, offset + i);
            i += vec3_t.n_bytes;
            model.node_id0_bsp = BitConverter.ToInt32(byteArray, offset + i);
            i += 4;
            model.node_id1_clip1 = BitConverter.ToInt32(byteArray, offset + i);
            i += 4;
            model.node_id2_clip2 = BitConverter.ToInt32(byteArray, offset + i);
            i += 4;
            model.node_id3_0 = BitConverter.ToInt32(byteArray, offset + i);
            i += 4;
            model.numleafs = BitConverter.ToInt32(byteArray, offset + i);
            i += 4;
            model.face_id = BitConverter.ToInt32(byteArray, offset + i);
            i += 4;
            model.face_num = BitConverter.ToInt32(byteArray, offset + i);
            i += 4;

            models[n_model] = model;
        }

        return models;
    }

    public boundbox_t ParseBoundBox(Byte[] byteArray, int offset)
    {
        boundbox_t bound = new boundbox_t();
        bound.min = ParseVec3(byteArray, offset);
        bound.max = ParseVec3(byteArray, offset+vec3_t.n_bytes);
        return bound;
    }

    public vec3_t ParseVec3(Byte[] byteArray, int offset)
    {
        vec3_t vec3 = new vec3_t();
        vec3.x = BitConverter.ToSingle(byteArray, offset);
        vec3.y = BitConverter.ToSingle(byteArray, offset+4);
        vec3.z = BitConverter.ToSingle(byteArray, offset+8);

        return vec3;
    }

    public string ParseEntities(Byte[] byteArray, int offset, int length)
    {
        //var dest = byteArray.Skip(100).Take(100).ToArray();
        string entities = System.Text.Encoding.ASCII.GetString(byteArray, offset, length);
        return entities;
    }

    public vec3_t[] ParseVectices(Byte[] byteArray, int offset)
    {
        int N_VERTS = headers.headers[(int)dheader_t_enum.VERTICES].size / vec3_t.n_bytes;
        vec3_t[] vertices = new vec3_t[N_VERTS];

        int i = 0;
        for (int n_vert = 0;  n_vert < N_VERTS;n_vert++)
        {
            vec3_t vert = ParseVec3(byteArray, offset + i);
            i += vec3_t.n_bytes;
            vertices[n_vert] = vert;
        }
        
        return vertices; 
    }

}