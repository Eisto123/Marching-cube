using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubeGenerator : MonoBehaviour
{
    [SerializeField]private int gridSizeX = 15;
    [SerializeField]private int gridSizeY = 15;
    [SerializeField]private int gridSizeZ = 15;
    [SerializeField] private float noiseScale;
    private Vector3 sphereCenter;

    [SerializeField]private float radius = 5;
    public bool visualizing;
    public float isolevel;
    private float[,,] scalarField;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private MeshFilter meshFilter;
    

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        
        sphereCenter = new Vector3(gridSizeX/2,gridSizeY/2,gridSizeZ/2);
        scalarField = new float[gridSizeX, gridSizeY, gridSizeZ];

    // Example: Fill the field with values based on distance to a sphere
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                for (int z = 0; z < gridSizeZ; z++) {
                float distance = Vector3.Distance(new Vector3(x, y, z), sphereCenter);
                scalarField[x, y, z] = (radius - distance);
                //scalarField[x, y, z] = radius - Mathf.Sqrt((x - sphereCenter.x) * (x - sphereCenter.x) + (y - sphereCenter.y) * (y - sphereCenter.y) + (z - sphereCenter.z) * (z - sphereCenter.z));
                }
            }
        }
        MarchingCubes();
        SetMesh();
    }

    void MarchingCubes() {
        vertices.Clear();
        triangles.Clear();
        for (int x = 0; x < gridSizeX - 1; x++) {
            for (int y = 0; y < gridSizeY - 1; y++) {
                for (int z = 0; z < gridSizeZ - 1; z++) {
                ProcessCube(x, y, z);
                }
            }
        }
    }

void ProcessCube(int x, int y, int z) {
    int cubeIndex = 0;
    float[] cubeValues = new float[] {
		scalarField[x, y, z + 1],
		scalarField[x + 1, y, z + 1],
		scalarField[x + 1, y, z],
		scalarField[x, y, z],
		scalarField[x, y + 1, z + 1],
		scalarField[x + 1, y + 1, z + 1],
		scalarField[x + 1, y + 1, z],
		scalarField[x, y + 1, z]
	};

    for (int i = 0; i < 8; i++) {
        if (scalarField[x + MarchingCubesTables.vertexOffsets[i].x, y + MarchingCubesTables.vertexOffsets[i].y, z + MarchingCubesTables.vertexOffsets[i].z] < isolevel)
            cubeIndex |= 1 << i;
    }

    // Use edgeTable to find intersecting edges
    int[] edges = MarchingCubesTables.triTable[cubeIndex];
    Vector3 worldPos = new Vector3(x, y, z);
    for (int i = 0; edges[i] != -1; i += 3) {
        int v00 = MarchingCubesTables.edgeConnections[edges[i]][0];
		int v01 = MarchingCubesTables.edgeConnections[edges[i]][1];

        Vector3 v1 = CulculateVertexPosition(MarchingCubesTables.vertexOffsets[v00],cubeValues[v00],MarchingCubesTables.vertexOffsets[v01],cubeValues[v01])+worldPos;


		int v10 = MarchingCubesTables.edgeConnections[edges[i + 1]][0];
		int v11 = MarchingCubesTables.edgeConnections[edges[i + 1]][1];
        Vector3 v2 = CulculateVertexPosition(MarchingCubesTables.vertexOffsets[v10],cubeValues[v10],MarchingCubesTables.vertexOffsets[v11],cubeValues[v11])+worldPos;

		int v20 = MarchingCubesTables.edgeConnections[edges[i + 2]][0];
		int v21 = MarchingCubesTables.edgeConnections[edges[i + 2]][1];
        Vector3 v3 = CulculateVertexPosition(MarchingCubesTables.vertexOffsets[v20],cubeValues[v20],MarchingCubesTables.vertexOffsets[v21],cubeValues[v21])+worldPos;

        AddTriangle(v1,v2,v3);
    }
}
    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3) {
        int triIndex = triangles.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);

        triangles.Add(triIndex);
        triangles.Add(triIndex + 1);
        triangles.Add(triIndex + 2);
    }

    private void SetMesh(){
        if (vertices.Count > 0 && triangles.Count % 3 == 0) {
            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles,0);
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;
        } 
        else {
            Debug.LogWarning("Mesh generation failed: Invalid vertices or triangles.");
        }
    }


    private void OnDrawGizmos()
    {
        if(!visualizing){
            return;
        }
        for (int x = 0; x < gridSizeX; x++) {
            for (int y = 0; y < gridSizeY; y++) {
                for (int z = 0; z < gridSizeZ; z++) {
                    Gizmos.color = new Color(scalarField[x,y,z],scalarField[x,y,z],scalarField[x,y,z],1);
                    Gizmos.DrawSphere(new Vector3(x,y,z),0.2f);
                }
            }
        } 
    }



    private Vector3 CulculateVertexPosition(Vector3 vertex1, float value1, Vector3 vertex2, float value2){
        if (Mathf.Abs(isolevel-value1) < 0.0001){
            return(vertex1);
        }
      
        if (Mathf.Abs(isolevel-value2) < 0.0001){
            return(vertex2);
        }
      
        if (Mathf.Abs(value1-value2) < 0.0001){
            return(vertex1);
        }
        return (vertex1 + (isolevel - value1) * (vertex2 - vertex1)  / (value2 - value1));
    }

}
