using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
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
    public List<Vector3[]> VertList = new List<Vector3[]>();
    public List<int[]> TriangleList = new List<int[]>();
    public List<Vector2[]> UVList = new List<Vector2[]>();

    public void buildMeshFromModel(SubModel subModel)
    {
        this.subModel = subModel;
        textureName = subModel.textureName.Replace("*", "");

        int N_FACES = subModel.faces.Count;

        for (int n_face = 0; n_face < N_FACES; n_face++)
        {


            List<Edge> edges = subModel.faces[n_face].edges;
            int N_EDGES = subModel.faces[n_face].edges.Count;
            int N_SUB_FACES = N_EDGES - 2;
            
            int[] newTrianglesWhole = new int[N_SUB_FACES * 3];
            int[] newTrianglesSub = new int[N_SUB_FACES * 3];
            Vector3[] newVerticesWhole = new Vector3[N_EDGES];
            Vector3[] newVerticesSub = new Vector3[N_SUB_FACES * 3];
            Vector2[] newUVsWhole = new Vector2[N_EDGES];
            Vector2[] newUVsSub = new Vector2[N_SUB_FACES * 3];

            for (int i_edge = 0; i_edge < N_EDGES - 2; i_edge++)
            {
                int edgeStart = i_edge * 3;
                newTrianglesWhole[edgeStart] = 0;
                newTrianglesWhole[edgeStart + 1] = i_edge + 1;
                newTrianglesWhole[edgeStart + 2] = i_edge + 2;
            }

            for (int i = 0; i < N_EDGES; i++)
            {
                newVerticesWhole[i] = new Vector3
                {
                    x = edges[i].verts[1].x / SCALE,
                    y = edges[i].verts[1].y / SCALE,
                    z = edges[i].verts[1].z / SCALE
                };
                newUVsWhole[i] = new Vector2
                {
                    x = edges[i].verts[1].u,
                    y = edges[i].verts[1].v
                };
            }

            for (int n_vert = 0; n_vert < newTrianglesSub.Length; n_vert++)
            {
                newTrianglesSub[n_vert] = n_vert;

                newVerticesSub[n_vert] = newVerticesWhole[newTrianglesWhole[n_vert]];
                newUVsSub[n_vert] = newUVsWhole[newTrianglesWhole[n_vert]];
            }

            TotalVerts += newVerticesSub.Length;
            TotalTris += newTrianglesSub.Length;

            VertList.Add(newVerticesSub);
            TriangleList.Add(newTrianglesSub);
            UVList.Add(newUVsSub);
        } // END FACE LOOP

        int[] AllNewTriangles = new int[TotalTris];
        Vector3[] AllNewVertices = new Vector3[TotalVerts];
        Vector2[] AllNewUVs = new Vector2[TotalVerts];
        Vector2[] AllNewUVFaceID = new Vector2[TotalVerts];

        int vertCount = 0;
        foreach (Vector3[] verts in VertList)
        {
            foreach (Vector3 vert in verts)
            {
                AllNewVertices[vertCount++] = vert;
            }
        }

        int UVCount = 0;
        int n_face_count = 0;
        foreach (Vector2[] UVs in UVList)
        {
            int n_uv_in_face = 0;
            foreach (Vector2 UV in UVs)
            {
                int countOfTris = (int)math.floor(UVCount / 3);
                int n_sub = (int)math.floor(n_uv_in_face / 3);
                AllNewUVs[UVCount] = UV;
                AllNewUVFaceID[UVCount] = new Vector2(n_face_count, n_sub);
                UVCount++;
                n_uv_in_face++;
            }
            n_face_count++;
        }

        int triCount = 0;
        int triOffset = 0;
        int faceCount = 0;
        foreach (int[] tris in TriangleList)
        {
            foreach (int tri in tris)
            {
                AllNewTriangles[triCount++] = tri + triOffset;
            }
            triOffset += VertList[faceCount++].Length;
        }

        Mesh mesh = new Mesh();

        mesh.vertices = AllNewVertices;
        mesh.triangles = AllNewTriangles;
        mesh.uv = AllNewUVs;
        mesh.uv2 = AllNewUVFaceID;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;

        Renderer m_Renderer = GetComponent<Renderer>();
        string matPath = "Assets/Materials/";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(matPath + textureName + ".mat");
        m_Renderer.sharedMaterial = material;

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

        //gameObject.transform.localScale = new Vector3(-1f,1f,1f);


    } // END METHOD

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
