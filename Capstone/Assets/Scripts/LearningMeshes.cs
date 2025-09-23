using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class LearningMeshes : MonoBehaviour
{
    [SerializeField] Vector3[] newVertices;
    [SerializeField] Vector2[] newUV;
    [SerializeField] int[] newTriangles;

    void Start()
    {
        // Create a new Mesh and set its data properties (vertices, UV coordinates, and triangles).
        Mesh mesh = circle(28, 5);

        // After updating the Mesh data, recalculate the normals. 
        // If the Mesh uses shaders with normal maps, also call RecalculateTangents for proper lighting.
        mesh.RecalculateNormals();

        // This assignment is temporary and will reset to the initial Mesh when exiting Play mode.
        mesh.triangles.ToList().ForEach(i => print(i.ToString()));
        print((string.Join(", ", mesh.triangles.ToList())));
        print((string.Join(", ", mesh.vertices.ToList())));
        GetComponent<MeshFilter>().mesh = mesh;
    }


    public void Redraw(Slider val)
    {
        float complexity = val.value;
        Mesh mesh = circle((int)complexity, 5);
        print((string.Join(", ", mesh.triangles.ToList())));
        print((string.Join(", ", mesh.vertices.ToList())));
        GetComponent<MeshFilter>().mesh = mesh;
    }
    //Triangle array is constructed by taking 3 indices from the triangle array at a time and using them to create a triangle. 
    //Vertice count represents the number of vertices the outer circle should have. Have to add one to include the inner vertice, 
    //as all triangles will be constructed using that centerpoint. 
    //For a circle of N outer vertices, there will be n Triangles. IE a circle of 4 outer vertices will require 4 triangles.
    //Step determines the angle difference between each vertice for calculating actual coordinate. IE for a circle of 4 outer vertices,
    //the calculation will step 90 degrees for each point calculation
    Mesh circle(int verticeCount, int size)
    {
        Vector3[] vertices = new Vector3[verticeCount+1];
        Vector2[] uv = new Vector2[verticeCount+1];
        int[] triangles = new int[verticeCount * 3];
        vertices[0] = new Vector3(0, 0, 0);
        uv[0] = new Vector2(0, 0);
        float step = (2 * Mathf.PI) / verticeCount;
        int triInd = 0;
        for (int i = 1; i < verticeCount + 1; i++)
        { //sin(theta) = opp/hyp cos(theta) = adj/hyp. size = hyp, opp = y, adj = x,
            int stepC = i - 1;
            float posX = size * Mathf.Sin((stepC * step));
            float posY = size * Mathf.Cos((stepC * step));
            vertices[i] = new Vector3(posX, posY);
            uv[i] = new Vector2(0, 0);
            //Triangles will be 3 times as long as vertices, so must assign 3 points in each iteration
            // An example triangles array for a circle of 4 vertices would be |0,1,2|,|0,2,3|,|0,3,4|,|0,5,6| (| | deliniates between a triangle, purely aesthetic)
            //for i = 1, append 0 1 and 2
            // for i =  2, append 0 2 and 3
            triangles[triInd] = 0;
            triInd += 1;
            triangles[triInd] = i;
            triInd += 1;
            if (i+1 > verticeCount)
            {
                triangles[triInd] = 1;
                triInd += 1;
            }
            else
            {
                triangles[triInd] = i+1;
                triInd += 1;
            }
        }
        return new Mesh
        {
            vertices = vertices,
            uv = uv,
            triangles = triangles
        };
    }
}