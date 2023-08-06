using System;
using System.IO;
using System.Text;
using UnityEngine;
using bspMapReader;
using static bspMapReader.bspMapScriptable;
using Color = UnityEngine.Color;
using UnityEditor;
using System.Collections.Generic;
using System.Threading;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class ReadBSPtoScriptable
{
    public int BSPVersion;

    public string bspFilename;

    public int maxFaceId = 0;
    public Material baseMaterial;
    public Material skyMaterial;
    private Color[] colorPalette = new Color[256];

    public bspMapScriptable ReadBSP(string bspFilename, string paletteFilename, bspMapScriptable mapScriptable)
    {
        Byte[] byteArray;
        this.bspFilename = bspFilename;
        mapScriptable.palette = loadPalette(paletteFilename);
        colorPalette = mapScriptable.palette;

        byteArray = getByteArray();

        if (byteArray == null )
        {
            Debug.LogError("ERROR PARSING BYTEARRAY");
            return mapScriptable;
        }

        BSPVersion = toInt(byteArray, 0);
        mapScriptable.headers = ParseHeaders(byteArray, 4);

        int N_MODELS = mapScriptable.headers.headers[(int)dheader_t_enum.MODELS].size / model_t.n_bytes;
        int MODELS_OFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.MODELS].offset;
        mapScriptable.models = ParseModels(byteArray, MODELS_OFFSET, N_MODELS);

        int ENTITIES_OFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.ENTITIES].offset;
        int ENTITIES_SIZE = mapScriptable.headers.headers[(int)dheader_t_enum.ENTITIES].size;
        mapScriptable.Entities = ParseEntities(byteArray, ENTITIES_OFFSET, ENTITIES_SIZE);


        int VERTS_OFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.VERTICES].offset;
        int VERTS_SIZE = mapScriptable.headers.headers[(int)dheader_t_enum.VERTICES].size;
        mapScriptable.vertices = ParseVectices(byteArray, VERTS_OFFSET, VERTS_SIZE);

        int FACES_OFFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.FACES].offset;
        int FACES_SIZE = mapScriptable.headers.headers[(int)dheader_t_enum.FACES].size;
        mapScriptable.faces = ParseFaces(byteArray, FACES_OFFFSET, FACES_SIZE);

        int EDGES_OFFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.EDGES].offset;
        int EDGES_SIZE = mapScriptable.headers.headers[(int)dheader_t_enum.EDGES].size;
        mapScriptable.edges = ParseEdges(byteArray, EDGES_OFFFSET, EDGES_SIZE);

        int LEDGES_OFFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.LEDGES].offset;
        int LEDGES_SIZE = mapScriptable.headers.headers[(int)dheader_t_enum.LEDGES].size;
        mapScriptable.lstedges = ParseLstEdges(byteArray, LEDGES_OFFFSET, LEDGES_SIZE);

        int LFACES_OFFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.LFACE].offset;
        int LFACES_SIZE = mapScriptable.headers.headers[(int)dheader_t_enum.LFACE].size;
        mapScriptable.lface = ParseLFaces(byteArray, LFACES_OFFFSET, LFACES_SIZE);

        int TEX_OFFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.TEXINFO].offset;
        int TEX_SIZE = mapScriptable.headers.headers[(int)dheader_t_enum.TEXINFO].size;
        mapScriptable.surfaces = ParseTexInfos(byteArray, TEX_OFFFSET, TEX_SIZE);

        int MIP_OFFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.MIPTEX].offset;
        int MIP_SIZE = mapScriptable.headers.headers[(int)dheader_t_enum.MIPTEX].size;

        mapScriptable.mipheader = ParseMipHeader(byteArray, MIP_OFFFSET);
        mapScriptable.miptexs = ParseTextures(byteArray, MIP_OFFFSET, MIP_SIZE, mapScriptable.mipheader);


        mapScriptable.maxFaceId = maxFaceId;


        return mapScriptable;

    }

    private Byte[] getByteArray()
    {
        string bspPath = "Assets/" + this.bspFilename;


        if (File.Exists(bspPath))
        {
            using (var stream = File.Open(bspPath, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
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

    public bspMapScriptable getLightMaps(bspMapScriptable mapScriptable)
    {
        int LM_OFFFSET = mapScriptable.headers.headers[(int)dheader_t_enum.LIGHTMAPS].offset;
        int LM_SIZE = mapScriptable.headers.headers[(int)dheader_t_enum.LIGHTMAPS].size;

        mapScriptable = ParseLightmaps(getByteArray(), LM_OFFFSET, LM_SIZE, mapScriptable);

        return mapScriptable;
    }

    private bspMapScriptable ParseLightmaps(byte[] byteArray, int LM_OFFFSET, int LM_SIZE, bspMapScriptable mapScriptable)
    {
        //face_t[] faces = mapScriptable.faces;
        List<face_t> lm_faces = new List<face_t>();
        List<int> lm_face_ids = new List<int>();
        int LM_COUNT = 0;
        for (int i_face = 0; i_face < mapScriptable.faces.Length; i_face++)
        {
            if (mapScriptable.faces[i_face].lightmap >= 0) 
            { 
                lm_faces.Add(mapScriptable.faces[i_face]);
                mapScriptable.faces[i_face].lightmap_index = LM_COUNT;
                lm_face_ids.Add(i_face);
                LM_COUNT++;
            } else
            {
                mapScriptable.faces[i_face].lightmap_index = -1;
            }
        }

        //List<lm_faceData> lm_Face_Datas = new List<lm_faceData>();

        short[] lstedges = mapScriptable.lstedges;

        int n_face = 0;
        int n_indivisible_faces = 0;

        lightmap_t[] lightmaps = new lightmap_t[LM_COUNT];

        foreach (face_t face in lm_faces)
        {
            // INITIALISE NEW LIGHTMAP
            lightmap_t lightmap = new lightmap_t();
            lightmap.lightmapID = face.lightmap;

            int lm_offset = face.lightmap;
            int ledge_num = face.ledge_num;
            int ledge_id = face.ledge_id;
            int texinfo_id = face.texinfo_id;
            
            surface_t surface = mapScriptable.surfaces[texinfo_id];
            miptex_t miptex = mapScriptable.miptexs[surface.texture_id];
            string textureName = miptex.nameStr;



            // INITIALISE MIN AND MAX VALUES
            float fMax = float.MaxValue;
            float fMin = float.MinValue;
            lightmap.faceMin = new Vector3(fMax,fMax,fMax);
            lightmap.faceMax = new Vector3(fMin,fMin,fMin);

            // GET VECTORS FOR ORIENTATION
            lightmap.vectorS = surface.vectorS;
            lightmap.vectorT = surface.vectorT;

            // INITIALISE EDGES AND VERTS
            // TODO: DONT STORE THESE IN LIGHTMAP?
            lightmap.edges = new List<edge_t>();
            lightmap.verts = new List<Vector3>();


            for (int n_edge = 0; n_edge < ledge_num; n_edge++)
            {
                int signed_edge_id = lstedges[ledge_id + n_edge];
                int edge_id = Math.Abs(signed_edge_id);
                edge_t edge = mapScriptable.edges[edge_id];
                lightmap.edges.Add(edge);
                int vert0_id = edge.vertex0;
                int vert1_id = edge.vertex1;

                if (signed_edge_id > 0)
                {
                    lightmap.verts.Add(mapScriptable.vertices[vert0_id]);
                } else
                {
                    lightmap.verts.Add(mapScriptable.vertices[vert1_id]);
                }
            }


            // CALCULATE BOUNDS
            foreach (Vector3 vert in lightmap.verts)
            {
                lightmap.faceMin.x = Math.Min(lightmap.faceMin.x, vert.x);
                lightmap.faceMin.y = Math.Min(lightmap.faceMin.y, vert.y);
                lightmap.faceMin.z = Math.Min(lightmap.faceMin.z, vert.z);

                lightmap.faceMax.x = Math.Max(lightmap.faceMax.x, vert.x);
                lightmap.faceMax.y = Math.Max(lightmap.faceMax.y, vert.y);
                lightmap.faceMax.z = Math.Max(lightmap.faceMax.z, vert.z);
            }

            lightmap.faceSpan = lightmap.faceMax - lightmap.faceMin;

            lightmap.LMSpan = new Vector2();

            lightmap.LMSpan.x = Math.Abs(Vector3.Dot(lightmap.faceSpan, lightmap.vectorS));
            lightmap.LMSpan.y = Math.Abs(Vector3.Dot(lightmap.faceSpan, lightmap.vectorT));

            if (lightmap.LMSpan.x % 16 != 0 || lightmap.LMSpan.y % 16 != 0) 
            {
                //Debug.LogWarning("Face number " + n_face + " is not divisible by 16");
                n_indivisible_faces++;
            }


            // CALCULATE VERT COORDINATES IN TEXTURE SPACE AND LIGHTMAP UVs
            lightmap.LM_UVs = new List<Vector2>();
            lightmap.vertTextureSpace = new List<Vector2>();
            float U_TS, V_TS;
            float min_U_TS, max_U_TS, min_V_TS, max_V_TS;
            float span_U_TS, span_V_TS;
            float U_LM, V_LM;

            min_U_TS = Vector3.Dot(lightmap.faceMin, lightmap.vectorS);
            max_U_TS = Vector3.Dot(lightmap.faceMax, lightmap.vectorS);

            min_V_TS = Vector3.Dot(lightmap.faceMin, lightmap.vectorT);
            max_V_TS = Vector3.Dot(lightmap.faceMax, lightmap.vectorT);

            span_U_TS = max_U_TS - min_U_TS;
            span_V_TS = max_V_TS - min_V_TS;

            foreach (Vector3 vert in lightmap.verts)
            {

                U_TS = Vector3.Dot(vert, lightmap.vectorS);
                V_TS = Vector3.Dot(vert, lightmap.vectorT);
                lightmap.vertTextureSpace.Add(new Vector2(U_TS, V_TS));

                U_LM = (U_TS - min_U_TS) / span_U_TS;
                V_LM = (V_TS - min_V_TS) / span_V_TS;

                lightmap.LM_UVs.Add(new Vector2(U_LM, V_LM));
            }

            // CALCULATE SIZE OF LIGHTMAP
            lightmap.LMWidth = (int)Mathf.Ceil(lightmap.LMSpan.x / 16f) + 1;
            lightmap.LMHeight = (int)Mathf.Ceil(lightmap.LMSpan.y / 16f) + 1;
            lightmap.LMSamples = lightmap.LMWidth * lightmap.LMHeight;


            // INITIALISE AND POPULATE LIGHTMAP DATA
            lightmap.LMData = new byte[lightmap.LMSamples];

            int counter = 0;
            int index = LM_OFFFSET + lightmap.lightmapID;
            for (int h = 0; h < lightmap.LMHeight; h++)
            {
                for (int w = 0; w < lightmap.LMWidth; w++)
                {
                    lightmap.LMData[counter] = byteArray[index + counter];
                    counter++;
                }
            }

            lightmaps[n_face] = lightmap;

            n_face++;
        }

        mapScriptable.lightmaps = lightmaps;

        return mapScriptable;
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

    public uint toUInt(Byte[] byteArray, int offset)
    {
        return BitConverter.ToUInt32(byteArray, (int)offset);
    }

    public short toShort(Byte[] byteArray, int offset)
    {
        return BitConverter.ToInt16(byteArray,(int)offset);
    }

    public ushort toUShort(Byte[] byteArray, int offset)
    {
        return BitConverter.ToUInt16(byteArray, (int)offset);
    }


    // PARSE DATA
    public mipheader_t ParseMipHeader(Byte[] byteArray, int offset)
    {
        mipheader_t mipheader = new mipheader_t();

        int i = 0;
        mipheader.numtex = toInt(byteArray, offset + i);
        i += 4;
        mipheader.offset = new int[mipheader.numtex];
        for (int n_offset = 0;  n_offset < mipheader.numtex;n_offset++)
        {
            mipheader.offset[n_offset] = toInt(byteArray, offset + i);
            i += 4;
        }
        mipheader.n_bytes = 4 + 4*mipheader.numtex;

        return mipheader;
    }
    
    public miptex_t ParseMiptex(Byte[] byteArray, int offset)
    {
        miptex_t miptex = new miptex_t();
        miptex.tex_offset = offset;
        int i = 0;
        miptex.name = new Byte[16];
        int N_CHARS = 0;
        for (int n_char = 0; n_char < 16; n_char++)
        {
            if (byteArray[offset + n_char] == 0)
            {
                break;
            }
            miptex.name[n_char] = byteArray[offset + n_char];
            N_CHARS++;
        }
        i += 16;
        miptex.nameStr = Encoding.UTF8.GetString(miptex.name).Substring(0,N_CHARS);
        miptex.width = toUInt(byteArray, offset + i);
        i += 4;
        miptex.height = toUInt(byteArray, offset + i);
        i += 4;
        miptex.offset1 = toUInt(byteArray, offset + i);
        i += 4;
        miptex.offset2 = toUInt(byteArray, offset + i);
        i += 4;
        miptex.offset4 = toUInt(byteArray, offset + i);
        i += 4;
        miptex.offset8 = toUInt(byteArray, offset + i);

        return miptex;
    }

    public miptex_t[] ParseTextures(Byte[] byteArray, int offset, int size, mipheader_t mipheader)
    {
        int n_mips = mipheader.numtex;
        miptex_t[] miptexs = new miptex_t[n_mips];

        int i = 0;
        for (int n_mip = 0; n_mip < n_mips; n_mip++)
        {
            i = mipheader.offset[n_mip];
            miptexs[n_mip] = ParseMiptex(byteArray, offset+i);
            //generateTexture(byteArray, miptexs[n_mip]);
        }

        return miptexs;
    }

    public surface_t ParseSurface(Byte[] byteArray, int offset)
    {
        surface_t surface = new surface_t();

        int i = 0;

        surface.vectorS = ParseVec3(byteArray, offset + i);
        i += vec3_t.n_bytes;
        surface.distS = BitConverter.ToSingle(byteArray, offset + i);
        i += sizeof(float);
        surface.vectorT = ParseVec3(byteArray, offset + i);
        i += vec3_t.n_bytes;
        surface.distT = BitConverter.ToSingle(byteArray, offset + i);
        i += sizeof(float);
        surface.texture_id = toUInt(byteArray, offset + i);
        i += sizeof(uint);
        surface.animated = toUInt(byteArray, offset + i);


        return surface;

    }
    
    public surface_t[] ParseTexInfos(Byte[] byteArray, int offset, int size)
    {
        int N_SURFS = size / surface_t.n_bytes;
        surface_t[] surfaces = new surface_t[N_SURFS];

        int i = 0;
        for (int n_surf = 0; n_surf < N_SURFS; n_surf++)
        {
            surfaces[n_surf] = ParseSurface(byteArray, offset + i); ;
            i += surface_t.n_bytes;
        }

        return surfaces;
    }
    
    public short[] ParseLstEdges(Byte[] byteArray, int offset, int size)
    {
        int N_LEDGES = size / 4;
        short[] ledges = new short[N_LEDGES];

        int i = 0;
        for (int n_ledge = 0; n_ledge < N_LEDGES; n_ledge++)
        {
            ledges[n_ledge] = (short)toInt(byteArray, offset + i);
            i += 4;
        }

        return ledges;
    }

    public ushort[] ParseLFaces(Byte[] byteArray, int offset, int size)
    {
        int N_LFACES = size / 2;
        ushort[] lfaces = new ushort[N_LFACES];

        int i = 0;
        for (int n_lface = 0; n_lface < N_LFACES; n_lface++)
        {
            lfaces[n_lface] = toUShort(byteArray, offset + i);
            i += 2;
        }

        return lfaces;
    }

    public edge_t ParseEdge(Byte[] byteArray, int offset)
    {
        int i = 0;
        edge_t edge = new edge_t();
        edge.vertex0 = toShort(byteArray, offset + i);
        i += 2;
        edge.vertex1 = toShort(byteArray, offset + i);

        return edge;
    }

    public edge_t[] ParseEdges(Byte[] byteArray, int offset, int size)
    {
        int N_EDGES = size / edge_t.n_bytes;
        edge_t[] faces = new edge_t[N_EDGES];

        int i = 0;
        for (int n_edge = 0; n_edge < N_EDGES; n_edge++)
        {
            faces[n_edge] = ParseEdge(byteArray, offset + i); ;
            i += edge_t.n_bytes;
        }

        return faces;
    }

    public face_t ParseFace(Byte[] byteArray, int offset)
    {
        int i = 0;
        face_t face = new face_t();
        face.plane_id = toUShort(byteArray, offset + i);
        i += 2;
        face.side = toUShort(byteArray, offset + i);
        i += 2;
        face.ledge_id = toInt(byteArray, offset + i);
        i += 4;
        face.ledge_num = toUShort(byteArray, offset + i);
        i += 2;
        face.texinfo_id = toUShort(byteArray, offset + i);
        i += 2;
        face.typelight = byteArray[offset + i];
        i += 1;
        face.baselight = byteArray[offset + i];
        i += 1;
        face.light = new byte[2];
        face.light[0] = byteArray[offset + i];
        i += 1;
        face.light[1] = byteArray[offset + i];
        i += 1;
        face.lightmap = toInt(byteArray, offset + i);

        return face;
    }

    public face_t[] ParseFaces(Byte[] byteArray, int offset, int size)
    {
        int N_FACES = size / face_t.n_bytes;
        face_t[] faces = new face_t[N_FACES];

        int i = 0;
        for (int n_face = 0; n_face < N_FACES; n_face++)
        {
            faces[n_face] = ParseFace(byteArray, offset + i); ;
            i += face_t.n_bytes;
        }

        return faces;
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

    public model_t ParseModel(Byte[] byteArray, int offset)
    {
        int i = 0;
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

        return model;
    }
    
    public model_t[] ParseModels(Byte[] byteArray, int MODELS_OFFSET, int N_MODELS)
    {
        model_t[] models = new model_t[N_MODELS];

        int i = 0;
        for (int n_model = 0; n_model < N_MODELS; n_model++)
        {
            models[n_model] = ParseModel(byteArray, MODELS_OFFSET + i); ;
            i += model_t.n_bytes;

            if (models[n_model].face_id > maxFaceId)
            {
                maxFaceId = models[n_model].face_id;
            }
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

    public Vector3 ParseVec3(Byte[] byteArray, int offset)
    {
        Vector3 vec3 = new Vector3();
        vec3.x = BitConverter.ToSingle(byteArray, offset);
        vec3.y = BitConverter.ToSingle(byteArray, offset+4);
        vec3.z = BitConverter.ToSingle(byteArray, offset+8);

        return vec3;
    }

    public string ParseEntities(Byte[] byteArray, int ENTITIES_OFFSET, int ENTITIES_SIZE)
    {
        string entities = System.Text.Encoding.ASCII.GetString(byteArray, ENTITIES_OFFSET, ENTITIES_SIZE);
        return entities;
    }

    public Vector3[] ParseVectices(Byte[] byteArray, int VERTS_OFFSET, int VERTS_SIZE)
    {
        int N_VERTS = VERTS_SIZE / vec3_t.n_bytes;
        Vector3[] vertices = new Vector3[N_VERTS];

        int i = 0;
        for (int n_vert = 0;  n_vert < N_VERTS;n_vert++)
        {
            Vector3 vert = ParseVec3(byteArray, VERTS_OFFSET + i);
            i += vec3_t.n_bytes;
            vertices[n_vert] = vert;
        }
        
        return vertices; 
    }

    // TEXTURE METHODS
    private Color[] loadPalette(string paletteFilename)
    {
        Color[] palette = new Color[256];
        string palettePath = "Assets/" + paletteFilename;
        Byte[] byteArray = new byte[768];
        if (File.Exists(palettePath))
        {
            using (var stream = File.Open(palettePath, FileMode.Open))
            {
                using (var reader = new BinaryReader(stream, Encoding.UTF8, false))
                {
                    byteArray = ReadAllBytes(reader);
                }
            }
        }

        for (int n_color = 0; n_color < byteArray.Length / 3; n_color++)
        {
            int offset = n_color * 3;
            float r, g, b;
            r = (float)byteArray[offset] / 255f;
            g = (float)byteArray[offset + 1] / 255f;
            b = (float)byteArray[offset + 2] / 255f;
            palette[n_color] = new Color(r, g, b, 1f);
        }

        return palette;
    }
    
    private Texture2D generateTexture(Byte[] byteArray, miptex_t miptex)
    {
        uint width = miptex.width;
        uint height = miptex.height;
        int offset = miptex.tex_offset;
        Texture2D texture = new Texture2D((int)width, (int)height);

        Byte colorIndex;
        Color32 color = new UnityEngine.Color(0, 0, 0, 255);
        int i = (int)miptex.offset1;
        for (int h = 0; h < height; h++)
        {
            for (int w = 0; w < width; w++)
            {
                colorIndex = byteArray[offset + i];
                i++;
                texture.SetPixel(w, h, getColor(colorIndex));
            }
        }

        texture.name = miptex.nameStr.Replace("*", "");

        byte[] bytes = texture.EncodeToPNG();
        var dirPath = "Assets/Textures/";
        if (!Directory.Exists(dirPath))
        {
            Directory.CreateDirectory(dirPath);
        }
        var texturePath = dirPath + texture.name + ".png";
        File.WriteAllBytes(texturePath, bytes);

        createMaterialAsset(texture.name);

        return texture;
    }

    private void createMaterialAsset(string textureFilename)
    {
        string matPath = "Assets/Materials/";
        Material material = getNewMaterialFromTexture(textureFilename);
        AssetDatabase.CreateAsset(material, matPath + textureFilename+".mat");
    }

    private Material getNewMaterialFromTexture(string textureName)
    {
        string texPath = "Assets/Textures/";
        string fullTexPath = texPath + textureName + ".png";
        if (File.Exists(fullTexPath))
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(fullTexPath);
            Material material;
            if (textureName == "sky1")
            {
                material = new Material(skyMaterial);
            } else
            {
                material = new Material(baseMaterial);
            }
            material.mainTexture = texture;
            return material;
        } else
        {
            Debug.LogWarning("Material created without Texture: " + textureName);
            return new Material(baseMaterial);
        }
    }

    public void rebuildMaterials(miptex_t[] miptexs)
    {
        string texPath = "Assets/Textures/";
        string matPath = "Assets/Materials/";
        string textureName;
        foreach (miptex_t miptex in miptexs)
        {
            textureName = miptex.nameStr.Replace("*", "");
            string texturePath = texPath + textureName + ".png";
            string materialPath = matPath + textureName + ".mat";
            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture == null)
            {
                Debug.LogWarning("Texture " + textureName + " not found");
                continue;
            }

            Material material = (Material)AssetDatabase.LoadMainAssetAtPath(materialPath);
            if (material == null)
            {
                createMaterialAsset(textureName);
            } else
            {
                EditorUtility.CopySerialized(getNewMaterialFromTexture(textureName), material);
                material.name = textureName;
                AssetDatabase.SaveAssets();
                //AssetDatabase.ImportAsset(materialPath, ImportAssetOptions.ImportRecursive);
            }
        }
    }

    private Color getColor(Byte colorIndex) { 
    
        return colorPalette[colorIndex];

    }

    // MISC METHODS
    //static Vector3 vec3Convert(vec3_t vec3)
    //{
    //    Vector3 newVec = new Vector3();

    //    newVec.x = vec3.x;
    //    newVec.y = vec3.y;
    //    newVec.z = vec3.z;

    //    return newVec;
    //}
}