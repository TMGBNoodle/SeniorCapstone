using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;

public class GCNT : MonoBehaviour
{

    public static GCNT  instance{get; set;}

    int featureCount = 15;
    // Joint type, 2 options so takes 2 spaces
    // Features:
    // Size
    // Depth
    // Subordinates
    // CurrentAngle
    // CurrentSpeed
    // CurrentTorque

    Dictionary<int, float[,]> graphInfo;
    // int[,] adj;
    // int[,] identity;

    float[,] w1;
    float[,] w2;
    // int[,] deg;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake ()
    {
        if (instance)
        {
            DestroyImmediate(gameObject);
            return;
        } else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void init()
    {
        w1 = new float[3,3];
        w2 = new float[3,3];
        graphInfo = new Dictionary<int, float[,]>();
    }
    public void initCreature(hingeInfo[] info, int id)
    {
        int n = info.Length;
        int[,] adj = new int[n,n];
        int[,] identity = new int[n,n];
        int[] deg = new int[n];
        int[,] at = new int[n,n];
        float[] degCalc = new float[n];
        float[,] aHat = new float[n,n];
        int connCount = 0;

        for (int i = 0; i < info.Length; i++)
        {   
            hingeInfo current = info[i];
            current.
            int crrID = current.id;
            print(crrID);
            identity[crrID,crrID] = 1;
            at[crrID, crrID] = 1;
            hingeInfo[] descs = current.descendants;
            deg[crrID] = 1;
            for (int j = 0; j < descs.Length; j++)
            {
                connCount += 1;
                hingeInfo desc = descs[j];
                adj[crrID,desc.id] = 1;
                adj[desc.id, crrID] = 1;
                at[crrID, desc.id] = 1;
                at[desc.id, crrID] = 1;
                deg[crrID] += 1;
                deg[desc.id] += 1;
            }

            
        }
        for (int i = 0; i < n; i++)
            degCalc[i] = 1f / Mathf.Sqrt(deg[i]);
        for (int i = 0; i < at.GetLength(0); i++)
        {
            for (int k = 0; k < at.GetLength(1); k++)
            {
                aHat[i,k] = degCalc[i] * at[i,k] * degCalc[k];
            }
        }

        graphInfo[id] = aHat;
        PrintMatrix(adj);
        // PrintArr(degCalc);
        PrintMatrix(identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveWeights()
    {
        string savew1 = MatrixToString(w1);
        string savew2 = MatrixToString(w1);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/w2.txt", savew2);
        System.IO.File.WriteAllText(Application.persistentDataPath + "/w1.txt", savew1);
    }

    public void LoadWeights()
    {
        w1 = StringToFloatMatrix(System.IO.File.ReadAllText(Application.persistentDataPath + "/w1.txt"));
        w2 = StringToFloatMatrix(System.IO.File.ReadAllText(Application.persistentDataPath + "/w2.txt"));
    }
    public int[,] AddIntMatrix(int[,] matrix1, int[,] matrix2) //Does not check whether the matrixes are the same size
    {
        int rows = matrix1.GetLength(0);
        int cols = matrix1.GetLength(1);
        int[,] final = new int[rows,cols];
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                final[r,c] = matrix1[r,c] + matrix2[r,c];
            }
        }
        return final;
    }
    public static void PrintMatrix<T>(T[,] matrix) //generated using OpenAI ChatGPT
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                sb.Append(matrix[r, c]);
                sb.Append("\t");
            }
            sb.AppendLine();
        }

        Debug.Log(sb.ToString());
    }

    public static int[,] StringToIntMatrix(string data)
    {
        string[] lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        int rows = int.Parse(lines[0]);
        int cols = int.Parse(lines[1]);

        int[,] matrix = new int[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            string[] values = lines[r + 2].Split(',');

            for (int c = 0; c < cols; c++)
            {
                matrix[r, c] = int.Parse(values[c]);
            }
        }

        return matrix;
    }
    public static float[,] StringToFloatMatrix(string data)
    {
        string[] lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        int rows = int.Parse(lines[0]);
        int cols = int.Parse(lines[1]);

        float[,] matrix = new float[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            string[] values = lines[r + 2].Split(',');

            for (int c = 0; c < cols; c++)
            {
                matrix[r, c] = float.Parse(values[c]);
            }
        }

        return matrix;
    }

    public static string MatrixToString<T>(T[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);

        StringBuilder sb = new StringBuilder();

        // Store dimensions first
        sb.AppendLine(rows.ToString());
        sb.AppendLine(cols.ToString());

        // Store matrix data row by row
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                sb.Append(matrix[r, c]);
                if (c < cols - 1)
                    sb.Append(",");     // comma-separated values
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

}
