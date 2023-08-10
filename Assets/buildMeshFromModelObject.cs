using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using static bspMapReader.bspMapScriptable;
using static MakeMesh;
using UnityEngine.Assertions;

public class BuildMeshFromModelObject : MonoBehaviour
{
    static readonly float SCALE = 40f;

    string textureName;
    public int TotalVerts;
    public int TotalTris;

    public Material baseMaterial;

    public Dictionary<int, int> MapFaceToLM;

    public string fullPathToLM;


    public void BuildMeshFromModel(SubModel subModel, bool rebuildT2DArray = false, bool inEditor = true)
    {
        string modelName = gameObject.name;
        string matPath = "Assets/Resources/Materials/";
        string texPath = "Assets/Resources/Textures/";
        string LMPath = "Assets/Resouces/Textures/LMs/";
        string assetName = modelName;
        string extension = ".asset";
        
        fullPathToLM = LMPath + assetName + extension;

        List<Vector3[]> sM_Vert_List = new();
        List<int[]> sM_Tris_List = new();
        List<Vector2[]> sM_UV_List = new();
        List<Vector2[]> sM_UV_LM_ID_List = new();
        List<Vector2[]> sM_UV_LM_List = new();
        // this.subModel = subModel;
        textureName = subModel.textureName.Replace("*", "-");

        int N_FACES = subModel.faces.Count;

        int LM_COUNT = 0;

        int[] face_LM_index = new int[N_FACES];
        Vector2[] face_UV_LM_scaling = new Vector2[N_FACES];
        Vector2[] face_UV_LM_offset = new Vector2[N_FACES];

        // GET LIGHTMAPS AND BUILD THEM INTO A Texture2DArray
        BuilT2DArray(subModel.faces, ref face_LM_index, ref face_UV_LM_scaling, ref face_UV_LM_offset, rebuildT2DArray);

        // LOOP OVER FACES
        for (int n_face = 0; n_face < N_FACES; n_face++)
        {
            // LOCAL COPY OF FACE DATA FOR CONVENIENCE (REMOVE AT SOME POINT)
            Face face = subModel.faces[n_face];

            bool hasLM = face.hasLM;

            // UV FOR LM ID (x = hasLM, y= LM INDEX)
            Vector2 UV_LM_ID = new()
            {
                x = (int)LM_COUNT,
                y = (int)(face.hasLM? 255 : 0)
            };

            if (hasLM)
            {
                LM_COUNT++;

                //lightmaps.Add(face.light);
            }
            else
            {
                // UV_LM_ID.x = 0;
                // UV_LM_ID.y = 0;
            }

            // INITIALISE PER FACE FIELDS
            List<Edge> edges = face.edges;
            int N_EDGES = face.edges.Count;

            FaceUVData fUV = new(N_EDGES);

            // GET TRI ARRAY WITH ORIGINAL VERT INDICES
            fUV.CalculateTriIndexes();

            // POPULATE LIST OF VERT LOCATIONS AND UV COORDS (BEFORE DUPLICATION)
            fUV.CalculateFaceVertexAndUVs(face);

            fUV.CalculateFaceLMapUVs(face, face_UV_LM_scaling[n_face], face_UV_LM_offset[n_face]); 

            // DUPLICATE ALL VALUES SO THAT NO VERTS ARE SHARED
            for (int n_vert = 0; n_vert < fUV.sub_Tris.Length; n_vert++)
            {
                fUV.sub_Tris[n_vert] = n_vert;

                fUV.sub_Verts[n_vert] = fUV.faceVerts[fUV.faceTriIdxes[n_vert]];
                fUV.sub_UV[n_vert] = fUV.face_UV[fUV.faceTriIdxes[n_vert]];
                fUV.sub_UV_LM[n_vert] = fUV.face_UV_LM[fUV.faceTriIdxes[n_vert]];
                fUV.sub_UV_LM_ID[n_vert] = UV_LM_ID;
            }

            // POPULATE SUBMODEL LISTS
            TotalVerts += fUV.sub_Verts.Length;
            TotalTris += fUV.sub_Tris.Length;

            sM_Vert_List.Add(fUV.sub_Verts);
            sM_Tris_List.Add(fUV.sub_Tris);
            sM_UV_List.Add(fUV.sub_UV);
            sM_UV_LM_ID_List.Add(fUV.sub_UV_LM_ID);
            sM_UV_LM_List.Add(fUV.sub_UV_LM);
        } // END FACE LOOP


        // INITIALISE FLATTENED ARRAYS ONCE NUMBER OF VERTS/TRIS IS KNOWN
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
        Mesh mesh = new()
        {
            vertices = model_Verts,
            triangles = model_Tris,
            uv = model_UV,
            uv2 = model_UV_FACE_ID,
            uv3 = model_UV_LM_ID,
            uv4 = model_UV_LM
        };

        mesh.RecalculateNormals();
        var normals = mesh.normals;

        Color[] vertColors = new Color[normals.Length];

        Vector3 aNormal;

        for (int n_normal = 0; n_normal < normals.Length; n_normal++) {
            aNormal = normals[n_normal];

            aNormal.Scale(new Vector3(0.5f, 0.5f, 0.5f));
            aNormal += new Vector3(0.5f, 0.5f, 0.5f);

            vertColors[n_normal] = new Color(aNormal.x, aNormal.y, aNormal.z);
        }

        mesh.colors = vertColors;
        
        // ASSIGN MESH
        GetComponent<MeshFilter>().mesh = mesh;

        // ASSIGN MATERIAL AND LIGHTMAP TEXTURES
        // Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath + textureName + ".asset");
        Renderer thisRenderer = GetComponent<Renderer>();

        Material material; 



        int numberOfAnimationFrames = 1;
        Texture2DArray texture = AssetDatabase.LoadAssetAtPath<Texture2DArray>(texPath + textureName + ".asset");
        if (textureName.StartsWith("sky")) 
        {
            material = AssetDatabase.LoadAssetAtPath<Material>(matPath + textureName + ".mat");
        } else if (textureName.StartsWith("-")) 
        {
            material = AssetDatabase.LoadAssetAtPath<Material>(matPath + textureName + ".mat");
        } else 
        {
            material = AssetDatabase.LoadAssetAtPath<Material>(matPath + textureName + ".mat");
            numberOfAnimationFrames = texture.depth;
            
        }

        if (inEditor) {
            thisRenderer.sharedMaterial = material;
        } else {
            // thisRenderer.EnsureComponent<MaterialInstance>().Material = material;
            thisRenderer.material = material;
        }

        if (LM_COUNT > 0) {
            // SHOULD ALWAYS EXIST AT THIS POINT, BUT CHECK ANYWAY
            Assert.IsTrue(File.Exists(fullPathToLM));
            Texture2DArray thisT2D = AssetDatabase.LoadAssetAtPath<Texture2DArray>(fullPathToLM);
            material.SetTexture("_LM_1", thisT2D);
        }

        material.SetTexture("_MainTex", texture);

        


        if (numberOfAnimationFrames > 1) {
            thisRenderer.sharedMaterial.SetFloat("_AnimationFrames", numberOfAnimationFrames);
        }

        if (textureName == "trigger")
        {
            thisRenderer.enabled = false;
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            DestroyImmediate(meshCollider);
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
        } else
        {
            MeshCollider meshCollider = GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }

        #if UNITY_EDITOR
            CreateMeshAsset();
        #endif
    } // END METHOD

    static void updateLightmapAsset(string fullPathToLM, Texture2DArray T2DArray)
    {
        Texture2DArray oldT2D = AssetDatabase.LoadAssetAtPath<Texture2DArray>(fullPathToLM);
        EditorUtility.SetDirty(oldT2D);
        T2DArray.name = oldT2D.name;
        EditorUtility.CopySerialized(T2DArray, oldT2D);
        oldT2D.Apply();
        AssetDatabase.SaveAssets();
    }

    private void BuilT2DArray(List<Face> faces, ref int[] face_LM_index, ref Vector2[] face_LM_UV_scaling, ref Vector2[] face_LM_UV_offset, bool rebuildT2DArray = false)
    {
        
        Texture2DArray T2DArray = null;

        List<lightmap_t> lightmaps = new();

        int max_LM_width = 0;
        int max_LM_height = 0;

        List<int> originalIDs = new();

        if (!rebuildT2DArray) {
            if(!File.Exists(fullPathToLM)) {
                // Override rebuilding of array
                rebuildT2DArray = true;
            }
        }

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
                face_LM_UV_scaling[n_face] = new(1f,1f);
                face_LM_UV_offset[n_face] = new(0,0);
            }
        }

        if (LM_Count < 1) {
            return;
        }

        // Get nearest power of two square size
        int maxLMDimension = math.max(max_LM_width,max_LM_height);
        int patchWidthInPixels = Mathf.NextPowerOfTwo(maxLMDimension);

        int numberOfPatchesPerSide;
        float sqrtLMCount = Mathf.Sqrt((float)LM_Count+1);

        if (sqrtLMCount % 1 < 0.0000001) {
            numberOfPatchesPerSide = Mathf.RoundToInt(sqrtLMCount);
        } else {
            numberOfPatchesPerSide = Mathf.CeilToInt(sqrtLMCount);
        }
        
        // Calculate UV Offsets and scaling, and optionally fill new texture grid
        TexGrid textureGrid = new(patchWidthInPixels, numberOfPatchesPerSide, rebuildT2DArray);
        Raveller patchRavel;
        for (int n_lm = 0; n_lm < LM_Count; n_lm++) {
            lightmap_t lightmap = lightmaps[n_lm];

            var invNumberOfPatches = 1/(float)numberOfPatchesPerSide;

            int o_ID = originalIDs[n_lm];
            float scalingU = (float)lightmap.LMWidth / patchWidthInPixels;
            float scalingV = (float)lightmap.LMHeight / patchWidthInPixels;
            scalingU *= invNumberOfPatches;
            scalingV *= invNumberOfPatches;

            face_LM_UV_scaling[o_ID] = new(scalingU,scalingV); 

            (int patchOffsetU, int patchOffsetV) = textureGrid.gridRavel.xy(n_lm+1);
            float offsetU = invNumberOfPatches * patchOffsetU;
            float offsetV = invNumberOfPatches * patchOffsetV;

            face_LM_UV_offset[o_ID] = new(offsetU,offsetV);

            patchRavel = new(lightmap.LMWidth, lightmap.LMHeight);

            if (rebuildT2DArray) {
                textureGrid.putPatchAtIndex(n_lm+1, lightmap.LMData, patchRavel);
            }
        }

        if (rebuildT2DArray) {
            Byte[] textureGridData = textureGrid.getFlatArray();
            if (textureGrid.gridWidthInPixels > 2048) {
                Debug.Log("TOO BIG");
            }
            T2DArray = new(textureGrid.gridWidthInPixels, textureGrid.gridWidthInPixels, 1, TextureFormat.Alpha8, false, true);
            T2DArray.SetPixelData(textureGridData, 0, 0);
            T2DArray.Apply();
        }

        if (rebuildT2DArray) {
            if (File.Exists(fullPathToLM) && T2DArray != null) {
                updateLightmapAsset(fullPathToLM, T2DArray);
            }
            else
            {
                AssetDatabase.CreateAsset(T2DArray, fullPathToLM);
                AssetDatabase.SaveAssets();
            }
        } else {
            if (!File.Exists(fullPathToLM) && T2DArray != null)
            {
                updateLightmapAsset(fullPathToLM, T2DArray);
            } else {
                // DO NOTING
            }
        }
    }

    /// <summary>
    /// Converts between x y and flat indexing
    /// </summary>
    public struct Raveller {
        public readonly int  width; // 16
        public readonly int height; // 16
        public readonly int length; // 256
        public readonly bool widthFirst;

        public Raveller(int width, int height, bool widthFirst = true) {
            this.width = width;
            this.height = height;
            this.widthFirst = widthFirst;
            this.length = width*height;
        }
        internal int index(int x, int y) {
            Assert.IsTrue(x <= width);
            Assert.IsTrue(y <= height);
            if (widthFirst) {
                return y*width + x;
            } else {
                return x*height + y;
            }
        }
        internal (int x, int y) xy(int index) {
            Assert.IsTrue(index < length);
            if (widthFirst)
            {
                return (
                    index % width,
                    Mathf.FloorToInt((float)index / width)
                );
            } else {
                return (
                    index % height,
                    Mathf.FloorToInt((float)index / height)
                );
            }
        }
        internal bool isInside(int x, int y) {
            if (x <= width && y <= height)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        internal bool isInside(int index) {
            (int x, int y) = xy(index);
            return isInside(x,y);
        }

        /// <summary>
        /// Given x, y coords into "other" Raveller, returns the equivalent index into this one.
        /// </summary>
        internal int getEquivalentIndexFromOther(int indexOther, Raveller other)
        {
            Assert.IsTrue(other.isInside(indexOther));
            (int xOther, int yOther) = other.xy(indexOther);
            Assert.IsTrue(isInside(xOther, yOther));

            return index(xOther, yOther);
        }
    }

    public struct TexGrid {
        public Byte[,] pixelArray;
        public Byte[] pixelFlat;
        public readonly int gridWidthInPixels;
        public readonly int totalNumberOfPixels;
        public readonly int patchWidthInPixels;
        public readonly int numberOfPatchesPerSide;
        public readonly int totalNumberOfPatches;
        public Raveller gridRavel;
        public Raveller pixelRavel;

        // Constructor
        public TexGrid (int patchWidthInPixels, int numberOfPatchesPerSide, bool initialisePixelArrays = true) {
            gridWidthInPixels = patchWidthInPixels * numberOfPatchesPerSide;
            totalNumberOfPixels = gridWidthInPixels*gridWidthInPixels;

            this.patchWidthInPixels = patchWidthInPixels;
            this.numberOfPatchesPerSide = numberOfPatchesPerSide;
            totalNumberOfPatches = numberOfPatchesPerSide*numberOfPatchesPerSide;
            
            pixelRavel = new(gridWidthInPixels, gridWidthInPixels);
            gridRavel = new(numberOfPatchesPerSide, numberOfPatchesPerSide);

            if (initialisePixelArrays) {
                pixelArray = new Byte[this.gridWidthInPixels, this.gridWidthInPixels];
                pixelFlat = new byte[this.totalNumberOfPixels]; 
            } else {
                pixelArray = null;
                pixelFlat = null;
            }
        }

        // Methods
        internal (int xOffset, int yOffset) getPixelOffset(int patchIndex) {
            (int xOffset, int yOffset) = gridRavel.xy(patchIndex);
            return (xOffset * patchWidthInPixels, yOffset * patchWidthInPixels);
        }

        internal Byte[] getFlatArray(bool reverseXY = false) {
            for (int n_pixel = 0; n_pixel < totalNumberOfPixels; n_pixel++) {
                int x, y;
                if (reverseXY) {
                    (y, x) = pixelRavel.xy(n_pixel);
                } else {
                    (x, y) = pixelRavel.xy(n_pixel);
                }
                pixelFlat[n_pixel] = pixelArray[x,y];
            }

            return pixelFlat;
        }

        internal void putPatchAtIndex(int patchIndex, Byte[] patchData, Raveller patchRavel, bool reverseXY = false) {
            Assert.IsTrue(patchIndex < totalNumberOfPatches);
            Assert.IsTrue(patchRavel.height <= patchWidthInPixels && patchRavel.width <= patchWidthInPixels);

            (int xOffset, int yOffset) = getPixelOffset(patchIndex);
            int xLast = xOffset + patchRavel.width;
            int yLast = yOffset + patchRavel.height;

            Assert.IsTrue(xLast <= gridWidthInPixels && yLast <= gridWidthInPixels);

            int xPatch, yPatch, indexInPatch;
            for (int x = xOffset; x < xLast; x++) {
                for (int y = yOffset; y < yLast; y++) {
                    xPatch = x - xOffset;
                    yPatch = y - yOffset;

                    indexInPatch = patchRavel.index(xPatch,yPatch);

                    pixelArray[x, y] = patchData[indexInPatch];
                }
            }
        }

    }

    public struct FaceUVData {
        readonly int N_EDGES;
        readonly int N_SUB_FACES;
        public int[] faceTriIdxes;
        public Vector3[] faceVerts;
        public Vector2[] face_UV;
        public Vector2[] face_UV_LM;
        
        public int[] sub_Tris;
        public Vector3[] sub_Verts;
        public Vector2[] sub_UV;
        public Vector2[] sub_UV_LM_ID;
        public Vector2[] sub_UV_LM;

        public FaceUVData(int N_EDGES)
        {
            this.N_EDGES = N_EDGES;
            N_SUB_FACES = this.N_EDGES-2;
            faceTriIdxes = new int[N_SUB_FACES * 3];
            faceVerts = new Vector3[this.N_EDGES];
            face_UV = new Vector2[this.N_EDGES];
            face_UV_LM = new Vector2[this.N_EDGES];

            sub_Tris = new int[N_SUB_FACES * 3];
            sub_Verts = new Vector3[N_SUB_FACES * 3];
            sub_UV = new Vector2[N_SUB_FACES * 3];
            sub_UV_LM_ID = new Vector2[N_SUB_FACES * 3];
            sub_UV_LM = new Vector2[N_SUB_FACES * 3];
        }    

        internal readonly void CalculateTriIndexes()
        {
            for (int i_edge = 0; i_edge < N_EDGES - 2; i_edge++)
            {
                int edgeStart = i_edge * 3;
                faceTriIdxes[edgeStart] = 0;
                faceTriIdxes[edgeStart + 1] = i_edge + 1;
                faceTriIdxes[edgeStart + 2] = i_edge + 2;
            }
        }

        internal void CalculateFaceVertexAndUVs(Face face)
        {
            var edges = face.edges;
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

            }
        }

        internal void CalculateFaceLMapUVs(Face face, Vector2 face_UV_LM_scaling, Vector2 face_UV_LM_offset)
        {
            bool hasLM = face.hasLM;
            for (int n_edge = 0; n_edge < N_EDGES; n_edge++) {
                if (hasLM)
                {
                    // HAVE TO FLIP THE Y/V COORDINATE (NOT SURE WHY YET)
                    face_UV_LM[n_edge] = face.light.LM_UVs[n_edge];
                    face_UV_LM[n_edge].y = 1 - face_UV_LM[n_edge].y;
                    face_UV_LM[n_edge].x *= face_UV_LM_scaling.x;
                    face_UV_LM[n_edge].y *= face_UV_LM_scaling.y;
                    face_UV_LM[n_edge].x += face_UV_LM_offset.x;
                    face_UV_LM[n_edge].y += face_UV_LM_offset.y;
                }
                else
                {
                    face_UV_LM[n_edge] = new Vector2(0, 0);
                }        
            }
        }
    }




#if UNITY_EDITOR
    public void CreateMeshAsset()
    {
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;
        string meshPath = BspPaths.Meshes + gameObject.name + ".asset";
        AssetDatabase.CreateAsset(mesh, meshPath);
    }
#endif
}
