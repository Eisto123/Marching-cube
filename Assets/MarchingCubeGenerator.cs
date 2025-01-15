using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MarchingCubeGenerator : MonoBehaviour
{
    [SerializeField]private int gridSizeX = 15;
    [SerializeField]private int gridSizeY = 15;
    [SerializeField]private int gridSizeZ = 15;
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
                scalarField[x, y, z] = radius - distance;
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

    // Determine the cube configuration
    for (int i = 0; i < 8; i++) {
        if (scalarField[x + MarchingCubesTables.vertexOffsets[i].x, y + MarchingCubesTables.vertexOffsets[i].y, z + MarchingCubesTables.vertexOffsets[i].z] < isolevel)
            cubeIndex |= 1 << i;
    }

    // Use edgeTable to find intersecting edges
    int[] edges = MarchingCubesTables.triTable[cubeIndex];

    for (int i = 0; i < 12; i++) {
        if ((edges[i]) == -1) {
            return;
        }
        else{
            Vector3 edgeStart = new Vector3Int(x,y,z) + MarchingCubesTables.Edges[edges[i],0];
            Vector3 edgeEnd = new Vector3Int(x,y,z) + MarchingCubesTables.Edges[edges[i],1];
            Vector3 vertex = (edgeStart + edgeEnd)/2; // if interpolation, change this
            vertices.Add(vertex);
            triangles.Add(vertices.Count - 1);
        }
        
    }


    // // Create triangles
    // for (int i = 0; MarchingCubesTables.triTable[cubeIndex][i] != -1; i += 3) {
    //     AddTriangle(edgeVertices[triangleTable[cubeIndex][i]],
    //                 edgeVertices[triangleTable[cubeIndex][i + 1]],
    //                 edgeVertices[triangleTable[cubeIndex][i + 2]]);
    // }
}
private void SetMesh()
    {
        Mesh mesh = new Mesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
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

}
