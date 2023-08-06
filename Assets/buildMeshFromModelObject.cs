using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using static bspMapReader.bspMapScriptable;
using static MakeMesh;

public class buildMeshFromModelObject : MonoBehaviour
{
    [SerializeField]
    public SubModel subModel;
    string textureName;
    // Start is called before the first frame update
    static float SCALE = 40f;
    public int TotalVerts;
    public int TotalTris;


    public void buildMeshFromModel(SubModel subModel)
    {
        List<Vector3[]> sM_Vert_List = new List<Vector3[]>();
        List<int[]> sM_Tris_List = new List<int[]>();
        List<Vector2[]> sM_UV_List = new List<Vector2[]>();
        List<Vector2[]> sM_UV_LM_ID_List = new List<Vector2[]>();
        List<Vector2[]> sM_UV_LM_List = new List<Vector2[]>();
        this.subModel = subModel;
        textureName = subModel.textureName.Replace("*", "");

        Texture2DArray T2DArray; // new Texture2DArray(1,1,1,TextureFormat.Alpha8,false,true);

        int N_FACES = subModel.faces.Count;

        int N_LMs = 0;

        int[] face_LM_index = new int[N_FACES];
        Vector2[] face_UV_LM_scaling = new Vector2[N_FACES];

        T2DArray = buildT2DArray(subModel.faces, ref face_LM_index, ref face_UV_LM_scaling);

        for (int n_face = 0; n_face < N_FACES; n_face++)
        {
            // LOCAL COPY OF FACE DATA
            Face face = subModel.faces[n_face];

            bool hasLM = face.hasLM;

            // UV FOR LM ID (x = hasLM, y= LM INDEX)
            Vector2 UV_LM_ID = new Vector2();
            if (hasLM)
            {
                N_LMs++;
                UV_LM_ID.x = 1;
                UV_LM_ID.y = N_LMs;
                //lightmaps.Add(face.light);
            } else
            {
                UV_LM_ID.x = 0;
                UV_LM_ID.y = 0;
            }

            // INITIALISE PER FACE FIELDS
            List<Edge> edges = face.edges;
            int N_EDGES = face.edges.Count;
            int N_SUB_FACES = N_EDGES - 2;
            
            int[] face_Tris = new int[N_SUB_FACES * 3];
            Vector3[] faceVerts = new Vector3[N_EDGES];
            Vector2[] face_UV = new Vector2[N_EDGES];
            Vector2[] face_UV_LM_ID = new Vector2[N_EDGES];
            Vector2[] face_UV_LM = new Vector2[N_EDGES];
            
            int[] sub_Tris = new int[N_SUB_FACES * 3];
            Vector3[] sub_Verts = new Vector3[N_SUB_FACES * 3];
            Vector2[] sub_UV = new Vector2[N_SUB_FACES * 3];
            Vector2[] sub_UV_LM_ID = new Vector2[N_SUB_FACES * 3];
            Vector2[] sub_UV_LM = new Vector2[N_SUB_FACES * 3];

            // GET TRI ARRAY WITH ORIGINAL VERT INDICES
            for (int i_edge = 0; i_edge < N_EDGES - 2; i_edge++)
            {
                int edgeStart = i_edge * 3;
                face_Tris[edgeStart] = 0;
                face_Tris[edgeStart + 1] = i_edge + 1;
                face_Tris[edgeStart + 2] = i_edge + 2;
            }

            // POPULATE LIST OF VERT LOCATIONS AND UV COORDS (BEFORE DUPLICATION)
            for (int i = 0; i < N_EDGES; i++)
            {
                faceVerts[i] = new Vector3
                {
                    x = edges[i].verts[1].x / SCALE,
                    y = edges[i].verts[1].y / SCALE,
                    z = edges[i].verts[1].z / SCALE
                };
                face_UV[i] = new Vector2
                {
                    x = edges[i].verts[1].u,
                    y = edges[i].verts[1].v
                };
                if (hasLM)
                {
                    face_UV_LM[i] = face.light.LM_UVs[i];
                    face_UV_LM[i].x *= face_UV_LM_scaling[n_face].x;
                    face_UV_LM[i].y *= face_UV_LM_scaling[n_face].y;
                } else
                {
                    face_UV_LM[i] = new Vector2 (0, 0);
                }
            }


            // DUPLICATE ALL VALUES SO THAT NO VERTS ARE SHARED
            for (int n_vert = 0; n_vert < sub_Tris.Length; n_vert++)
            {
                sub_Tris[n_vert] = n_vert;

                sub_Verts[n_vert] = faceVerts[face_Tris[n_vert]];
                sub_UV[n_vert] = face_UV[face_Tris[n_vert]];
                sub_UV_LM[n_vert] = face_UV_LM[face_Tris[n_vert]];
                sub_UV_LM_ID[n_vert] = UV_LM_ID;
            }

            // POPULATE SUBMODEL LISTS
            TotalVerts += sub_Verts.Length;
            TotalTris += sub_Tris.Length;

            sM_Vert_List.Add(sub_Verts);
            sM_Tris_List.Add(sub_Tris);
            sM_UV_List.Add(sub_UV);
            sM_UV_LM_ID_List.Add(sub_UV_LM_ID);
            sM_UV_LM_List.Add(sub_UV_LM);
        } // END FACE LOOP

        int[] model_Tris = new int[TotalTris];
        Vector3[] model_Verts = new Vector3[TotalVerts];
        Vector2[] model_UV = new Vector2[TotalVerts];
        Vector2[] model_UV_LM_ID = new Vector2[TotalVerts];
        Vector2[] model_UV_LM = new Vector2[TotalVerts];
        Vector2[] model_UV_FACE_ID = new Vector2[TotalVerts];

        // FLATTEN VERTS TO ONE VECTOR
        int vertCount = 0;
        foreach (Vector3[] verts in sM_Vert_List)
        {
            foreach (Vector3 vert in verts)
            {
                model_Verts[vertCount++] = vert;
            }
        }

        // FLATTEN UVs to ONE VECTOR
        int UVCount = 0;
        int n_face_count = 0;
        foreach (Vector2[] UVs in sM_UV_List)
        {
            int n_uv_in_face = 0;
            foreach (Vector2 UV in UVs)
            {
                model_UV[UVCount] = UV;

                int countOfTris = (int)math.floor(UVCount / 3);
                int n_sub = (int)math.floor(n_uv_in_face / 3);

                model_UV_FACE_ID[UVCount] = new Vector2(n_face_count, n_sub);
                UVCount++;
                n_uv_in_face++;
            }
            n_face_count++;
        }


        // FLATTEN UVs to ONE VECTOR
        UVCount = 0;
        foreach (Vector2[] UVs in sM_UV_LM_List)
        {
            foreach (Vector2 UV in UVs)
            {
                model_UV_LM_ID[UVCount] = UV;
                UVCount++;
            }
        }

        // FLATTEN UVs to ONE VECTOR
        UVCount = 0;
        foreach (Vector2[] UVs in sM_UV_LM_ID_List)
        {
            foreach (Vector2 UV in UVs)
            {
                model_UV_LM[UVCount] = UV;
                UVCount++;
            }
        }

        // FLATTEN VERTEX INDICES/TRIS
        int triCount = 0;
        int triOffset = 0;
        int faceCount = 0;
        foreach (int[] tris in sM_Tris_List)
        {
            foreach (int tri in tris)
            {
                model_Tris[triCount++] = tri + triOffset;
            }
            triOffset += sM_Vert_List[faceCount++].Length;
        }

        // MAKE NEW MESH AND POPULATE
        Mesh mesh = new Mesh();

        mesh.vertices = model_Verts;
        mesh.triangles = model_Tris;
        mesh.uv = model_UV;
        mesh.uv2 = model_UV_FACE_ID;
        mesh.uv3 = model_UV_LM_ID;
        mesh.uv4 = model_UV_LM;
        mesh.RecalculateNormals();

        // ASSIGN MESH
        GetComponent<MeshFilter>().mesh = mesh;


        // GET Texture2DArray FOR LIGHTMAP
        //T2DArray = getT2DArray_Test(lightMapX,lightMapY);

        
        // ASSIGN MATERIAL AND LIGHTMAP TEXTURES
        string matPath = "Assets/Materials/";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(matPath + textureName + ".mat");
        
        Renderer m_Renderer = GetComponent<Renderer>();
        m_Renderer.sharedMaterial = material;
        
        m_Renderer.sharedMaterial.SetTexture("_LM_1", T2DArray);

        // SAVE LIGHTMAP FOR TESTING
        string LMPath = "Assets/Textures/LMs/";
        string assetName = "LM_1";
        string extension = ".asset";
        string fullPath = LMPath + assetName + extension;
        if (File.Exists(fullPath))
        {
            Texture2DArray oldT2D = AssetDatabase.LoadAssetAtPath<Texture2DArray>(fullPath);
            T2DArray.name = oldT2D.name;
            EditorUtility.CopySerialized(T2DArray, oldT2D);
            oldT2D.Apply();
        }
        else
        {
            AssetDatabase.CreateAsset(T2DArray, "Assets/Textures/LMs/LM_1.asset");
            AssetDatabase.SaveAssets();
        }
        if (textureName == "trigger")
        {
            m_Renderer.enabled = false;
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            DestroyImmediate(meshCollider);
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        } else
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }
        createMeshAsset();

    } // END METHOD

    private Texture2DArray buildT2DArray(List<Face> faces, ref int[] face_LM_index, ref Vector2[] face_LM_UV_scaling)
    {
        List<lightmap_t> lightmaps = new List<lightmap_t>();

        int max_LM_width = 0;
        int max_LM_height = 0;

        List<int> originalIDs = new List<int>();

        // Get indices and max values
        int LM_Count = 0;
        for (int n_face = 0; n_face < faces.Count; n_face++ ) { 
            if (faces[n_face].hasLM)
            {
                LM_Count++;
                face_LM_index[n_face] = LM_Count;

                lightmaps.Add(faces[n_face].light);
                
                max_LM_height = math.max(max_LM_height, faces[n_face].light.LMHeight);
                max_LM_width = math.max(max_LM_width, faces[n_face].light.LMWidth);

                originalIDs.Add(n_face);
            } else
            {
                face_LM_index[n_face] = 0;
                face_LM_UV_scaling[n_face].x = 1f;
                face_LM_UV_scaling[n_face].y = 1f;
            }
        }

        // INITIALISE NEW T2DArray
        Texture2DArray T2DArray = new Texture2DArray(max_LM_width, max_LM_height, LM_Count+1, TextureFormat.Alpha8, false, true);

        int LM_length = max_LM_height*max_LM_width;

        // LOOP THROUGH LMs
        for (int n_lm = 0; n_lm < LM_Count; n_lm++)
        {
            int originalID = originalIDs[n_lm];
            lightmap_t lightmap = lightmaps[n_lm];

            face_LM_UV_scaling[originalID].x = lightmap.LMWidth / max_LM_width;
            face_LM_UV_scaling[originalID].y = lightmap.LMHeight / max_LM_height;

            Byte[] LMData = lightmap.LMData;
            Byte[] newLMData = new Byte[LM_length];

            // COPY LM DATA TO NEW BUFFER
            int new_pixel_counter = 0;
            int old_pixel_counter = 0;
            for (int h = 0; h < max_LM_height; h++)
            {
                for (int w = 0;  w < max_LM_width; w++)
                {
                    if (w < lightmap.LMWidth && h < lightmap.LMHeight)
                    {
                        newLMData[new_pixel_counter] = LMData[old_pixel_counter];
                        old_pixel_counter++;
                    } else
                    {
                        newLMData[new_pixel_counter] = 0;
                    }
                    new_pixel_counter++;
                }
            }

            // PUT NEW BUFFER IN T2DA
            T2DArray.SetPixelData(newLMData, 0, n_lm+1);
        }
        T2DArray.Apply();

        return T2DArray;


    }

    private static Texture2DArray getT2DArray_Test(int lightMapX, int lightMapY)
    {
        int scale = 10;
        int n_levels = 10;
        int length = lightMapX * scale * lightMapY * scale;


        Texture2DArray T2D = new Texture2DArray(lightMapX * scale, lightMapY * scale, n_levels, TextureFormat.Alpha8, false, true);


        for (int n_level = 0; n_level < n_levels; n_level++)
        {
            //Color32[] t2data = new Color32[length];
            byte[] t2data = new byte[length];
            for (int j = 0; j < length; j++)
            {
                byte dataPoint = (byte)(((float)n_level / n_levels)*255);
                t2data[j] = dataPoint;
            }
            T2D.SetPixelData(t2data, 0, n_level);   
        }
        T2D.Apply();

        return T2D;
    }

    public void createMeshAsset()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        string meshPath = "Assets/Meshes/" + gameObject.name + ".asset";
        AssetDatabase.CreateAsset(mesh, meshPath);
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
