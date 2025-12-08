
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using System.Text;
using UnityEngine.UI;

public class DrawCreature : MonoBehaviour
{
    [SerializeField] Vector3[] newVertices;
    [SerializeField] Vector2[] newUV;
    [SerializeField] int[] newTriangles;

    public GameObject creatureHolder;

    GameObject[] creatures;

    public Material[] materials;

    public Vector2 offset;

    public Vector2 dimensions;

    public Vector2 startPos;

    Creature currentCreature = new Creature();

    public int currentMeshInd = 0;

    public int id;
    void Start()
    {
        // // Create a new Mesh and set its data properties (vertices, UV coordinates, and triangles).
        // Mesh mesh = circle(10, 3, Vector3.zero);
        Mesh mesh2 = circle(10, 3, Vector3.right);
        creatures = new GameObject[(int)(dimensions.x * dimensions.y)];
        int index = 0;
        for (int x = 0; x < dimensions.x; x++)
        {
            for (int y = 0; y < dimensions.y; y++)
            {
                creatures[index] = Instantiate(creatureHolder);
                creatures[index].transform.position = new Vector3(startPos.x + x * offset.x, startPos.y - y * offset.y);
                index += 1;
            }
        }
        // // After updating the Mesh data, recalculate the normals. 
        // // If the Mesh uses shaders with normal maps, also call RecalculateTangents for proper lighting.
        // mesh.RecalculateNormals();

        // // This assignment is temporary and will reset to the initial Mesh when exiting Play mode.
        // mesh.triangles.ToList().ForEach(i => print(i.ToString()));
        // print((string.Join(", ", mesh.triangles.ToList())));
        // print((string.Join(", ", mesh.vertices.ToList())));
        // GetComponent<MeshFilter>().mesh = mesh2;


        currentCreature = new Creature(1, new Part(partType.Hub, 5, 10, new Connection[10] { new Connection(0, new Part(partType.Limb)), new Connection(0, new Part(partType.Limb)), new Connection(0, new Part(partType.Limb)), new Connection(0, new Part(partType.Limb)), new Connection(0, new Part(partType.Limb)), new Connection(0, new Part(partType.Limb)), new Connection(0, new Part(partType.Limb)), new Connection(0, new Part(partType.Limb)), new Connection(0, new Part(partType.Limb)), new Connection(0, new Part(partType.Limb)) }), "");
        sendPart();
    }


    public void Redraw(Slider val)
    {
        float complexity = val.value;
        Mesh mesh = circle((int)complexity, 5, new Vector3(0, 0));
        print((string.Join(", ", mesh.triangles.ToList())));
        print((string.Join(", ", mesh.vertices.ToList())));
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void sendPart()
    {
        Part part = currentCreature.hub;
        GameObject fullCreature = hub(part.connections, part.size, new Vector3(0, 0));
        fullCreature.AddComponent<CreatureControl>();
        fullCreature.transform.position = new Vector3(0, 0, 0);
        // fullCreature.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void drawCreature(Creature creature)
    {
        currentCreature = creature;
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

    // public Mesh drawHub(Part part)
    // {
    //     return hub(part.connections, part.size, new Vector3(0, 0));
    // }
    //Triangle array is constructed by taking 3 indices from the triangle array at a time and using them to create a triangle. 
    //Vertice count represents the number of vertices the outer circle should have. Have to add one to include the inner vertice, 
    //as all triangles will be constructed using that centerpoint. 
    //For a circle of N outer vertices, there will be n Triangles. IE a circle of 4 outer vertices will require 4 triangles.
    //Step determines the angle difference between each vertice for calculating actual coordinate. IE for a circle of 4 outer vertices,
    //the calculation will step 90 degrees for each point calculation
    GameObject limb(Vector3 start, Vector3 end, float angleInfX, float angleInfY, float thiccness, Connection con)
    {
        print("Draw limb");
        float widthMag = thiccness / 2;
        Vector3 displacement1 = widthMag * new Vector3(-(angleInfY + 0.05f), angleInfX + 0.05f);
        Vector3 displacement2 = widthMag * new Vector3(angleInfY + 0.05f, -(angleInfX + 0.05f));
        Vector3[] vertices = new Vector3[4] { start + displacement1, start + displacement2, end + displacement1, end + displacement2 };
        Vector2[] uv = new Vector2[4] { start + displacement1, start + displacement2, end + displacement1, end + displacement2 };
        int[] triangles = new int[6] { 0, 2, 3, 0, 1, 3 };
        List<Mesh> meshes = new List<Mesh>();
        GameObject thisObject = new GameObject();
        thisObject.tag = "Limb";
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = uv,
            triangles = triangles
        };
        Rigidbody2D rb = thisObject.AddComponent<Rigidbody2D>();
        // rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        PolygonCollider2D collider = thisObject.AddComponent<PolygonCollider2D>();
        collider.SetPath(0, fromV3(vertices));
        MeshFilter meshFilter = thisObject.AddComponent<MeshFilter>();
        MeshRenderer rend = thisObject.AddComponent<MeshRenderer>();
        rend.material = materials[0];
        meshFilter.mesh = mesh;
        // meshes.Append(mesh);
        // print((string.Join(", ", mesh.triangles.ToList())));
        // print((string.Join(", ", mesh.vertices.ToList())));
        GameObject oElement;
        if (con != null && con.otherPart.type != partType.Null)
        {
            Part oPart = con.otherPart;
            if (oPart.type == partType.Hub)
            {
                float eposX = end.x + (oPart.size) * angleInfX;
                float eposY = end.y + (oPart.size) * angleInfY;
                oElement = hub(oPart.connections, oPart.size, new Vector3(eposX, eposY));
            }
            else if (oPart.type == partType.Limb)
            {
                float eposX = end.x + (oPart.size) * angleInfX;
                float eposY = end.y + (oPart.size) * angleInfY;
                oElement = limb(end, new Vector3(eposX, eposY), angleInfX, angleInfY, 0.5f, oPart.connections[0]);
            }
            else
            {
                print("How did we get here?");
                oElement = new GameObject();
                thisObject.tag = "Null";
            }
            oElement.transform.SetParent(thisObject.transform);
            HingeJoint2D hinge = thisObject.AddComponent<HingeJoint2D>();
            hinge.anchor = end;
            hinge.connectedBody = oElement.GetComponent<Rigidbody2D>();
            return thisObject;
        }
        else
        {
            print("Return base mesh/No attachments");
            return thisObject;
        }
    }
    Vector2[] fromV3(Vector3[] vec3)
    {
        Vector2[] vec2 = new Vector2[vec3.Length];
        for (int i = 0; i < vec3.Length; i++)
        {
            vec2[i] = (Vector2)vec3[i];
        }
        return vec2;
    }

    Mesh combineMeshes(Mesh mesh1, Mesh mesh2)
    {
        int meshStartId = mesh1.vertices.Length;
        Vector3[] vertices = mesh1.vertices.Concat(mesh2.vertices).ToArray();
        Vector2[] uv = mesh1.uv.Concat(mesh2.uv).ToArray();
        int[] triangles2 = new int[mesh2.triangles.Length];
        for (int i = 0; i < mesh2.triangles.Length; i++)
        {
            triangles2[i] = mesh2.triangles[i] + meshStartId;
        }
        int[] triangles = mesh1.triangles.Concat(triangles2).ToArray();
        Mesh final = new Mesh
        {
            vertices = vertices,
            uv = uv,
            triangles = triangles
        };
        return final;
    }
    GameObject hub(Connection[] verticeinfo, float size, Vector3 center)
    {
        print("Making hub");
        int verticeCount = verticeinfo.Length;
        Vector3[] vertices = new Vector3[verticeCount + 1];
        Vector2[] uv = new Vector2[verticeCount + 1];
        int[] triangles = new int[verticeCount * 3];
        vertices[0] = center;
        uv[0] = new Vector2(0, 0);
        float step = (2 * Mathf.PI) / verticeCount;
        int triInd = 0;
        GameObject thisObject = new GameObject();
        thisObject.tag = "Hub";
        Rigidbody2D rb = thisObject.AddComponent<Rigidbody2D>();
        MeshRenderer rend = thisObject.AddComponent<MeshRenderer>();
        rend.material = materials[0];
        // rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        PolygonCollider2D collider = thisObject.AddComponent<PolygonCollider2D>();
        for (int i = 1; i < verticeCount + 1; i++)
        { //sin(theta) = opp/hyp cos(theta) = adj/hyp. size = hyp, opp = y, adj = x,
            int stepC = i - 1;
            float xAngle = Mathf.Sin((stepC * step)); // this is not angle this is the normalized height of the x
            float yAngle = Mathf.Cos((stepC * step)); // height of the y
            float posX = center.x + size * xAngle; // center + (normalized height * size)
            float posY = center.y + size * yAngle;
            vertices[i] = new Vector3(posX, posY);
            uv[i] = new Vector2(0, 0);
            if (verticeinfo[stepC] != null && verticeinfo[stepC].otherPart.type != partType.Null)
            {
                print("Other connected");
                Part oPart = verticeinfo[stepC].otherPart;
                GameObject oElement;
                if (oPart.type == partType.Hub)
                {
                    float eposX = center.x + (size + oPart.size) * xAngle;
                    float eposY = center.y + (size + oPart.size) * yAngle;
                    oElement = hub(oPart.connections, oPart.size, new Vector3(eposX, eposY));
                }
                else if (oPart.type == partType.Limb)
                {
                    float eposX = center.x + (size + oPart.size) * xAngle;
                    float eposY = center.y + (size + oPart.size) * yAngle;
                    oElement = limb(new Vector3(posX, posY), new Vector3(eposX, eposY), xAngle, yAngle, 0.5f, oPart.connections[0]);
                }
                else
                {
                    print("How did we get here?");
                    oElement = new GameObject();
                    oElement.AddComponent<Rigidbody2D>();
                    thisObject.tag = "Null";
                }
                oElement.transform.SetParent(thisObject.transform);
                HingeJoint2D hinge = thisObject.AddComponent<HingeJoint2D>();
                hinge.anchor = new Vector2(posX, posY);
                hinge.connectedBody = oElement.GetComponent<Rigidbody2D>();
            }
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
        print("These are the vertices: " + SerializeVector3Array(vertices));
        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = uv,
            triangles = triangles
        };
        collider.SetPath(0, fromV3(vertices));
        MeshFilter meshFilter = thisObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        return thisObject;
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
    public static string SerializeVector3Array(Vector3[] aVectors)
    {
        string final = "";
        foreach (Vector3 v in aVectors)
        {
            final += v.ToString();
        }
        return final;
    }
}