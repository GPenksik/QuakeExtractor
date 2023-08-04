using System.Collections;
using System.Collections.Generic;
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
            
            int[] newTriangles = new int[(N_EDGES - 2) * 3];
            Vector3[] newVertices = new Vector3[N_EDGES];
            Vector2[] newUVs = new Vector2[N_EDGES];

            for (int i_edge = 0; i_edge < N_EDGES - 2; i_edge++)
            {
                int edgeStart = i_edge * 3;
                newTriangles[edgeStart] = 0;
                newTriangles[edgeStart + 1] = i_edge + 1;
                newTriangles[edgeStart + 2] = i_edge + 2;
            }

            for (int i = 0; i < N_EDGES; i++)
            {
                newVertices[i] = new Vector3
                {
                    x = edges[i].verts[1].x / SCALE,
                    y = edges[i].verts[1].y / SCALE,
                    z = edges[i].verts[1].z / SCALE
                };
                newUVs[i] = new Vector2
                {
                    x = edges[i].verts[1].u,
                    y = edges[i].verts[1].v
                };
            }

            TotalVerts += newVertices.Length;
            TotalTris += newTriangles.Length;

            VertList.Add(newVertices);
            TriangleList.Add(newTriangles);
            UVList.Add(newUVs);
        } // END FACE LOOP

        int[] AllNewTriangles = new int[TotalTris];
        Vector3[] AllNewVertices = new Vector3[TotalVerts];
        Vector2[] AllNewUVs = new Vector2[TotalVerts];

        int vertCount = 0;
        foreach (Vector3[] verts in VertList)
        {
            foreach (Vector3 vert in verts)
            {
                AllNewVertices[vertCount++] = vert;
            }
        }

        int UVCount = 0;
        foreach (Vector2[] UVs in UVList)
        {
            foreach (Vector2 UV in UVs)
            {
                AllNewUVs[UVCount++] = UV;
            }
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
        mesh.RecalculateNormals();

        mesh.normals = mesh.normals;
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
        //gameObject.transform.localScale = new Vector3(-1f,1f,1f);


    } // END METHOD

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
