
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.UI;

public class DrawCreature : MonoBehaviour
{
    [SerializeField] Vector3[] newVertices;
    [SerializeField] Vector2[] newUV;
    [SerializeField] int[] newTriangles;

    void Start()
    {
        // // Create a new Mesh and set its data properties (vertices, UV coordinates, and triangles).
        Mesh mesh = circle(10, 3, Vector3.zero);
        Mesh mesh2 = circle(10, 3, Vector3.right);

        // // After updating the Mesh data, recalculate the normals. 
        // // If the Mesh uses shaders with normal maps, also call RecalculateTangents for proper lighting.
        // mesh.RecalculateNormals();

        // // This assignment is temporary and will reset to the initial Mesh when exiting Play mode.
        mesh.triangles.ToList().ForEach(i => print(i.ToString()));
        print((string.Join(", ", mesh.triangles.ToList())));
        print((string.Join(", ", mesh.vertices.ToList())));
        GetComponent<MeshFilter>().mesh = mesh2;


        // Creature creature = new Creature(1, new Part(partType.Hub, 5, 10)); //new Connection[10]{new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb))}));
        // drawCreature(creature);

    }


    public void Redraw(Slider val)
    {
        float complexity = val.value;
        Mesh mesh = circle((int)complexity, 5, new Vector3(0, 0));
        print((string.Join(", ", mesh.triangles.ToList())));
        print((string.Join(", ", mesh.vertices.ToList())));
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void drawCreature(Creature creature)
    {
        Part part = creature.hub;
        Mesh final = hub(part.connections, part.size, new Vector3(0, 0));
        print(final);
        GetComponent<MeshFilter>().mesh = final;
    }

    // public Mesh addPart(Part oPart)
    // {
    //     switch(oPart.type)
    //     {
    //         case (partType.Hub):
    //             return hub(oPart.connections.Length, oPart.size);
    //         case (partType.Limb):
    //             return Line(oPart.)
    //     }
    // }

    public Mesh drawHub(Part part)
    {
        return hub(part.connections, part.size, new Vector3(0, 0));
    }
    //Triangle array is constructed by taking 3 indices from the triangle array at a time and using them to create a triangle. 
    //Vertice count represents the number of vertices the outer circle should have. Have to add one to include the inner vertice, 
    //as all triangles will be constructed using that centerpoint. 
    //For a circle of N outer vertices, there will be n Triangles. IE a circle of 4 outer vertices will require 4 triangles.
    //Step determines the angle difference between each vertice for calculating actual coordinate. IE for a circle of 4 outer vertices,
    //the calculation will step 90 degrees for each point calculation
    Mesh limb(Vector3 start, Vector3 end, Vector3 angleInf, float thiccness, Connection con)
    {
        Vector3[] vertices = new Vector3[4] { start, start, end, end };
        Vector2[] uv = new Vector2[4] { start, start, end, end };
        int[] triangles = new int[6] {0, 2, 3, 1, 2, 3};
        List<Mesh> meshes = new List<Mesh>();
        meshes.Append(new Mesh
        {
            vertices = vertices,
            uv = uv,
            triangles = triangles
        });
        if (con != null)
        {
            Part oPart = con.otherPart;
            Mesh oMesh;
            if (oPart.type == partType.Hub)
            {
                oMesh = hub(oPart.connections, oPart.size, end);
            }
            else if (oPart.type == partType.Limb)
            {
                float eposX = end.x + (oPart.size) * angleInf.x;
                float eposY = end.y + (oPart.size) * angleInf.y;
                oMesh = limb(end, new Vector3(eposX, eposY), angleInf, 0.3f, oPart.connections[0]);
            }
            else
            {
                oMesh = circle(10, 0.4f, end);
            }
            meshes.Append(oMesh);
        }
        CombineInstance[] mesh = new CombineInstance[meshes.Count];
        for (int i = 0; i < mesh.Length; i++)
        {
            mesh[i] = new CombineInstance
            {
                mesh = meshes[i]
            };
        }
        Mesh final = new Mesh();
        final.CombineMeshes(mesh);
        return final;
    }
    Mesh hub(Connection[] verticeinfo, float size, Vector3 center)
    {
        int verticeCount = verticeinfo.Length;
        Vector3[] vertices = new Vector3[verticeCount + 1];
        Vector2[] uv = new Vector2[verticeCount + 1];
        int[] triangles = new int[verticeCount * 3];
        vertices[0] = center;
        uv[0] = new Vector2(0, 0);
        float step = (2 * Mathf.PI) / verticeCount;
        int triInd = 0;
        List<Mesh> meshes = new List<Mesh>();
        for (int i = 1; i < verticeCount + 1; i++)
        { //sin(theta) = opp/hyp cos(theta) = adj/hyp. size = hyp, opp = y, adj = x,
            int stepC = i - 1;
            float xAngle = Mathf.Sin((stepC * step));
            float yAngle = Mathf.Cos((stepC * step));
            float posX = center.x + size * xAngle;
            float posY = center.y + size * yAngle;
            if (verticeinfo[stepC] != null)
            {
                print("Other connected");
                Part oPart = verticeinfo[stepC].otherPart;
                Mesh oMesh;
                if (oPart.type == partType.Hub)
                {
                    oMesh = hub(oPart.connections, oPart.size, new Vector3(posX, posY));
                }
                else if (oPart.type == partType.Limb)
                {
                    float eposX = center.x + (size + oPart.size) * xAngle;
                    float eposY = center.y + (size + oPart.size) * yAngle;
                    oMesh = limb(new Vector3(posX, posY), new Vector3(eposX, eposY), new Vector3(xAngle, yAngle), 0.3f, oPart.connections[0]);
                }
                else
                {
                    oMesh = circle(10, 0.4f, new Vector3(posX, posY));
                }
                meshes.Append(oMesh);
            }
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
            if (i + 1 > verticeCount)
            {
                triangles[triInd] = 1;
                triInd += 1;
            }
            else
            {
                triangles[triInd] = i + 1;
                triInd += 1;
            }
        }
        meshes.Append(new Mesh
        {
            vertices = vertices,
            uv = uv,
            triangles = triangles
        });
        Mesh final = new Mesh();
        CombineInstance[] mesh = new CombineInstance[meshes.Count];
        for (int i = 0; i < mesh.Length; i++)
        {
            mesh[i] = new CombineInstance
            {
                mesh = meshes[i]
            };
        }
        final.CombineMeshes(mesh);
        return final;
    }
    Mesh circle(int verticeCount, float size, Vector3 center)
    {
        Vector3[] vertices = new Vector3[verticeCount + 1];
        Vector2[] uv = new Vector2[verticeCount + 1];
        int[] triangles = new int[verticeCount * 3];
        vertices[0] = center;
        uv[0] = new Vector2(0, 0);
        float step = (2 * Mathf.PI) / verticeCount;
        int triInd = 0;
        for (int i = 1; i < verticeCount + 1; i++)
        { //sin(theta) = opp/hyp cos(theta) = adj/hyp. size = hyp, opp = y, adj = x,
            int stepC = i - 1;
            float xAngle = Mathf.Sin((stepC * step));
            float yAngle = Mathf.Cos((stepC * step));
            float posX = center.x + size * xAngle;
            float posY = center.y + size * yAngle;
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
            if (i + 1 > verticeCount)
            {
                triangles[triInd] = 1;
                triInd += 1;
            }
            else
            {
                triangles[triInd] = i + 1;
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