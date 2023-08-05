using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bspMapReader;
using static bspMapReader.bspMapScriptable;
using System;
using UnityEngine.XR;
using System.Linq;
using static MakeMesh;
using UnityEditor;

public class MakeMesh : MonoBehaviour
{
    public bspMapScriptable mapScriptable;
    public GameObject modelPrefab;
    //Mesh mesh;

    //List<string> texturesInModel = new List<string>();

    public int modelIndex = 1;
    public string textureName = "None";

    [SerializeField]
    model_t qmodel;
    [SerializeField]
    public List<Model> umodels = new List<Model>();

    [System.Serializable]
    public struct Model
    {
        public List<SubModel> subModels;
        public int TotalTris;
        public int TotalVerts;
        public List<string> texturesInModel;
        public Model(int a = 0)
        {
            subModels = new List<SubModel>();
            TotalTris = 0;
            TotalVerts = 0;
            texturesInModel = new List<string>();
        }
        public void calcTotals()
        {
            TotalTris = 0;
            TotalVerts = 0;
            foreach (SubModel subModel in subModels)
            {
                TotalTris += subModel.N_SUBFACES;
            }
            TotalVerts = TotalTris * 3;
        }
    }

    [System.Serializable]
    public struct Vert
    {
        public float x;
        public float y;
        public float z;
        public float u;
        public float v;
        //public string textureName;
    }

    //[System.Serializable]
    public struct Edge
    {
        public string textureName;
        public Vert[] verts;
        public Edge(int a = 2)
        {
            textureName = "";
            verts = new Vert[a];
        }
    }
    [System.Serializable]
    public struct Face
    {
        public int N_EDGES;
        public int N_SUBFACES;
        public string textureName;
        public List<Edge> edges;
        public int lightmap;
        public bool hasLM;
        public Byte typelight;            // type of lighting, for the face
        public Byte baselight;
        public Face(int a = 10)
        {
            N_EDGES = 0;
            N_SUBFACES = 0;
            textureName = "";
            edges = new List<Edge>();
            hasLM = false;
            lightmap = -1;
            typelight = 0;
            baselight = 255;
        }
        public void addEdge(Edge edge)
        {
            N_EDGES++;
            N_SUBFACES = N_EDGES - 2;
            edges.Add(edge);
        }
    }
    [System.Serializable]
    public struct SubModel
    {
        public int N_FACES;
        public int N_SUBFACES;
        public int N_LM;
        public string textureName;
        public List<Face> faces;


        public SubModel(int a = 10)
        {
            N_FACES = 0;
            N_SUBFACES = 0;
            textureName = "";
            faces = new List<Face>();
            N_LM = 0;
        }
        public void addFace(Face face)
        {
            N_FACES++;
            N_SUBFACES += face.N_SUBFACES;
            if (face.hasLM) { N_LM++; }
            faces.Add(face);
        }
    }

    // Start is called before the first frame update
    public void buildMesh(bool buildAll = false)
    {
        if (mapScriptable == null)
        {
            Debug.LogWarning("NO MAP LOADED");
            return;
        }

        int modelIndexStart, modelIndexEnd;
        if (buildAll)
        {
            modelIndexStart = 0;
            modelIndexEnd = mapScriptable.models.Length-1;
        } else
        {
            modelIndexStart = modelIndex;
            modelIndexEnd = modelIndex;
        }

        umodels.Clear();
        int N_MODELS = 0;

        // FOR EACH MODEL
        //for (int modelIndex = 0; modelIndex < mapScriptable.models.Length; modelIndex++)
        for (int modelIndexLocal = modelIndexStart; modelIndexLocal <= modelIndexEnd; modelIndexLocal++)
        {
            N_MODELS++;
            int iSubmodel = 0;
            Model currentuModel = new Model(1);
            //umodels.Add(currentuModel);
            umodels.Add(currentuModel);
            SubModel currentSubModel;

            qmodel = mapScriptable.models[modelIndexLocal];

            int N_FACES = qmodel.face_num;

            int offsetToFacesInModel = qmodel.face_id;

            face_t[] qfaces = new face_t[N_FACES];

            // FOR LOOP Build list of submodels
            for (int n_face = 0; n_face < N_FACES; n_face++)
            {
                // Save face_t array
                qfaces[n_face] = mapScriptable.faces[offsetToFacesInModel + n_face];

                string textureName = getTextureName(qfaces[n_face]);
                // IF NEW TEXTURE, ADD TO LIST.
                if (!currentuModel.texturesInModel.Contains(textureName))
                {
                    currentuModel.texturesInModel.Add(textureName);
                    SubModel subModel = new SubModel(1);
                    subModel.textureName = textureName;
                    //subModel.faces = new Face[N_FACES];
                    currentuModel.subModels.Add(subModel);

                    iSubmodel = currentuModel.texturesInModel.FindIndex(a => a.Contains(textureName));
                } else
                {
                    iSubmodel = currentuModel.texturesInModel.FindIndex(a => a.Contains(textureName));
                    currentSubModel = currentuModel.subModels[iSubmodel];
                    currentuModel.subModels[iSubmodel] = currentSubModel;
                }
            } // END BUILD LIST OF SUBMODELS


            // FOR EACH FACE
            for (int n_face = 0; n_face < N_FACES; n_face++)
            {
                // TEXTURE NAME
                surface_t qsurface = getSurface(qfaces[n_face]);
                string textureName = getTextureName(qfaces[n_face]);
                miptex_t miptex = getMiptex(qfaces[n_face]);

                int lightmap = qfaces[n_face].lightmap;

                // GET APPROPRIATE SUBMODEL
                iSubmodel = currentuModel.texturesInModel.FindIndex(a => a.Contains(textureName));
                currentSubModel = currentuModel.subModels[iSubmodel];

                // SAVE TEXTURE SIZE
                int tWidth = (int)miptex.width;
                int tHeight = (int)miptex.height;

                // SAVE TEXTURE FOR THIS FACE
                Face currentFace = new Face(1);
                currentFace.textureName = textureName;

                // GET DETAILS FOR MAPPING
                Vector3 vectorS = vec3Convert(qsurface.vectorS);
                Vector3 vectorT = vec3Convert(qsurface.vectorT);


                float distS = qsurface.distS;
                float distT = qsurface.distT;

                // GET NUMBER OF EDGES AND OFFSET
                int N_EDGES = qfaces[n_face].ledge_num;
                int edge_offset = qfaces[n_face].ledge_id;

                // INITIALISE Q EDGES
                short[] qledge = new short[N_EDGES];
                edge_t[] qedge = new edge_t[N_EDGES];

                // GET LIGHT PROPERTIES
                if (lightmap >= 0) { 
                    currentFace.hasLM = true;
                    currentFace.lightmap = lightmap;
                }
                currentFace.baselight = qfaces[n_face].baselight;
                currentFace.typelight = qfaces[n_face].typelight;

                // FOR EACH EDGE ADD VERTEX
                for (int n_edge = 0; n_edge < N_EDGES; n_edge++)
                {
                    Edge currentEdge = new Edge(2);
                    // SAVE TEXTURE TO EDGE
                    currentEdge.textureName = textureName;

                    // GET EDGE INDEX
                    qledge[n_edge] = mapScriptable.lstedges[edge_offset + n_edge];

                    // SAVE THIS EDGE INDEX
                    int thisqledge = qledge[n_edge];

                    // DETERMINE WHICH WAY THE EDGE SHOULD BE READ
                    if (thisqledge > 0)
                    {
                        qedge[n_edge] = new edge_t
                        {
                            vertex0 = mapScriptable.edges[thisqledge].vertex1,
                            vertex1 = mapScriptable.edges[thisqledge].vertex0
                        };
                    }
                    else
                    {
                        thisqledge = Math.Abs(thisqledge);
                        qedge[n_edge] = mapScriptable.edges[thisqledge];
                    }

                    // CREATE NEW VERTICES
                    //currentEdge.verts = new Vert[2];

                    vec3_t vert0 = mapScriptable.vertices[qedge[n_edge].vertex0];
                    vec3_t vert1 = mapScriptable.vertices[qedge[n_edge].vertex1];

                    // SAVE TEXTURE NAME TO VERTEX
                    //currentEdge.verts[0].textureName = textureName;
                    //currentEdge.verts[1].textureName = textureName;

                    // CREATE NEW MODEL.VERT ENTRIES
                    currentEdge.verts[0] = new Vert
                    {
                        x = vert0.x,
                        y = vert0.z,
                        z = vert0.y,
                        u = ((Vector3.Dot(vec3Convert(vert0), vectorS) + distS) / tWidth),
                        v = ((Vector3.Dot(vec3Convert(vert0), vectorT) + distT) / tHeight),
                    };

                    currentEdge.verts[1] = new Vert
                    {
                        x = vert1.x,
                        y = vert1.z,
                        z = vert1.y,
                        u = ((Vector3.Dot(vec3Convert(vert1), vectorS) + distS) / tWidth),
                        v = ((Vector3.Dot(vec3Convert(vert1), vectorT) + distT) / tHeight),
                    };
                    currentFace.addEdge(currentEdge);
                } // FINISH EDGE LOOP
                currentSubModel.addFace(currentFace);
                // SAVE SUBMODEL BACK INTO LIST
                currentuModel.subModels[iSubmodel] = currentSubModel;
            } // FINISH FACE LOOP


            //ADD UMODEL TO LIST
            currentuModel.calcTotals();
            umodels[N_MODELS-1] = currentuModel;
        } // FINISH MODEL LOOP

    } // FINISH MAKE MESH METHOD

    public void buildPrefabs(List<Model> models)
    {
        GameObject modelParentGO = DestroyOldModels();

        int modelCount = 0;
        foreach (Model model in models)
        {
            for (int n_subModel = 0; n_subModel < model.subModels.Count; n_subModel++)
            {
                SubModel subModel = model.subModels[n_subModel];
                GameObject newModel = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(modelPrefab);
                newModel.transform.parent = modelParentGO.transform;
                newModel.name = "Model_" + modelCount + "_" + n_subModel;

                buildMeshFromModelObject builder = newModel.GetComponent<buildMeshFromModelObject>();
                builder.buildMeshFromModel(subModel);
            }
            modelCount++;
        }
    }



    public GameObject DestroyOldModels()
    {
        GameObject modelParentGO = GameObject.Find("MODELS");
        while (modelParentGO.transform.childCount > 0)
        {
            DestroyImmediate(modelParentGO.transform.GetChild(0).gameObject);
        }

        return modelParentGO;
    }

    public surface_t getSurface(face_t qface)
    {
        // Save surface description to array
        return mapScriptable.surfaces[qface.texinfo_id];
    }

    private string getTextureName(face_t qface)
    {
        // TEXTURE NAME
        return getMiptex(qface).nameStr;
    }
    private miptex_t getMiptex(face_t qface)
    {
        uint current_texture_id = getSurface(qface).texture_id;
        return mapScriptable.miptexs[current_texture_id];
    }

    static Vector3 vec3Convert(vec3_t vec3)
    {
        Vector3 newVec = new Vector3();

        newVec.x = vec3.x;
        newVec.y = vec3.y;
        newVec.z = vec3.z;

        return newVec;
    }

}

