using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.CompilerServices;
using UnityEngine;
using Color = UnityEngine.Color;

namespace bspMapReader
{
    public class bspMapScriptable : ScriptableObject
    {
        [SerializeField]
        public Color[] palette;
        // ACTUAL 
        [SerializeField]
        public int maxFaceId = 0;

        [System.Serializable]
        public struct dheader_t
        {
            public dentry_t[] headers;
        }

        [SerializeField]
        public dheader_t headers = new dheader_t();

        [SerializeField]
        public model_t[] models;

        //[TextAreaAttribute]
        [System.NonSerialized]
        public string Entities;

        //[SerializeField]
        public Vector3[] vertices;

        [SerializeField]
        public face_t[] faces;

        //[SerializeField]
        public edge_t[] edges;

        //[SerializeField]
        public surface_t[] surfaces;

        //[SerializeField]
        public mipheader_t mipheader;
        //[SerializeField]
        public miptex_t[] miptexs;

        [SerializeField]
        public lightmap_t[] lightmaps;
        // CUSTOM TYPE DEFINITIONS
        [System.Serializable]
        public enum dheader_t_enum
        {
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

        [System.Serializable]
        public struct dentry_t
        {
            public int offset;
            public int size;
        }
        //[System.Serializable]
        public struct vec3_t
        {
            //public float x;
            //public float y;
            //public float z;
            public static int n_bytes = sizeof(float) * 3;
        }


        //[System.Serializable]
        public struct boundbox_t
        {
            public Vector3 min;
            public Vector3 max;
            public static int n_bytes = vec3_t.n_bytes * 2;
        }

        //[System.Serializable]
        public struct bboxshort_t
        {
            public float min;
            public float max;
        }

        [System.Serializable]
        public struct model_t
        {
            public boundbox_t bound;
            public Vector3 origin;
            public int node_id0_bsp;
            public int node_id1_clip1;
            public int node_id2_clip2;
            public int node_id3_0;
            public int numleafs;
            public int face_id;
            public int face_num;
            public static int n_bytes = boundbox_t.n_bytes + vec3_t.n_bytes + 4 * 7;
        }

        [System.Serializable]
        public struct face_t
        { 
            public ushort plane_id;            // The plane id (NOT USED)
            public ushort side;                // 0 if in front of the plane, 1 if behind the plane
            public int ledge_id;               // id to edge_t
            public ushort ledge_num;           // number of edges in the List of edges
            public ushort texinfo_id;          // id to surface_t 
            public Byte typelight;            // type of lighting, for the face
            public Byte baselight;            // from 0xFF (dark) to 0 (bright)
            public Byte[] light;             // two additional LMData models  
            public int lightmap;               // Pointer inside the general LMData map, or -1
                                               // this define the start of the face LMData map
            public int lightmap_index;

            public static int n_bytes = 4 * sizeof(ushort) + 2*sizeof(int) + 4;
        }


        //[System.Serializable]
        public struct edge_t
        { 
            public short vertex0;             // index of the start vertex
                                       //  must be in [0,numvertices[
            public short vertex1;             // index of the end vertex
                                         //  must be in [0,numvertices
            public static int n_bytes = 4;
        }

        [SerializeField]
        public short[] lstedges;

        [SerializeField]
        public ushort[] lface;

        //[System.Serializable]
        public struct surface_t 
        { 
            public Vector3 vectorS;            // S vector, horizontal in texture space)
            public float distS;              // horizontal offset in texture space
            public Vector3 vectorT;            // T vector, vertical in texture space
            public float distT;              // vertical offset in texture space
            public uint texture_id;         // Index of Mip Texture
                                            //           must be in [0,numtex[
            public uint animated;           // 0 for ordinary textures, 1 for water 
            public static int n_bytes = 2 * 12 + 4 * 4;
        }

        //[System.Serializable]
        public struct mipheader_t
        {
            public int numtex;
            public int[] offset;
            public int n_bytes;
        }
        //[System.Serializable]
        public struct miptex_t
        {
            public string nameStr;
            public Byte[] name;
            public uint width;
            public uint height;
            public uint offset1;
            public uint offset2;
            public uint offset4;
            public uint offset8;
            public int n_bytes;
            public int tex_offset;
        }
        [System.Serializable]
        public struct lightmap_t
        {
            // Lightmap specific
            public Byte[] LMData;
            public int LMSamples;
            public int LMHeight;
            public int LMWidth;
            public Vector2 LMSpan;         // SIZE OF FACE IN LM UNITS

            // Associated Face Data
            public int faceID;              // ORIGINAL FACE INDEX
            public Vector3 faceMin;
            public Vector3 faceMax;
            public Vector3 faceSpan;
            
            public Vector3 vectorS;
            public Vector3 vectorT;
            public List<edge_t> edges;
            public List<Vector3> verts;

            public List<Vector2> vertTextureSpace;
            public List<Vector2> LM_UVs;

            public int lightmapID;          // INDEX INTO LIGHTMAP DATA


        }
    }
}