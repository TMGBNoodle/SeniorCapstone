using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class GCNT : MonoBehaviour
{

    public static GCNT  instance{get; set;}

    int featureCount = 8;
    // Joint type, 2 options so takes 2 spaces
    // Features:
    // Size
    // Depth
    // Subordinates
    // CurrentAngle
    // CurrentSpeed
    // CurrentTorque

    Dictionary<int, double[,]> graphInfo;
    // int[,] adj;
    // int[,] identity;
    int hidden = 32;
    double[,] w1; // featurecount, hidden
    double[,] w2; //hidden, featurecount
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
        w1 = new double[featureCount,hidden];
        w2 = new double[hidden,featureCount];
        graphInfo = new Dictionary<int, double[,]>();
    }

    public double[,] eval(hingeInfo[] inf, int id)
    {
        double[,] features = getFeatures(inf);

        double[,] v1 = layer(features, id, 1);
        double[,] v2 = layer(v1, id, 2);
        return v2;
    }

    public double[,] getFeatures(hingeInfo[] info)
    {
        double[,] final = new double[info.Length, featureCount];
        foreach(hingeInfo data in info)
        {
            HingeJoint2D hinge = data.hinge;
            final[data.id, data.type] = 1; //2 (0 indexed this is counting how many I have)
            final[data.id, 3] = data.size; // 3
            final[data.id, 4] = data.height; // 4
            final[data.id, 5] = data.ancestry; // 5
            final[data.id, 6] = hinge.jointAngle;// 6
            final[data.id, 7] = hinge.motor.motorSpeed; // 7
            final[data.id, 8] = hinge.GetMotorTorque(Time.time); //8
        }
        return final;
    }
    public void initCreature(hingeInfo[] info, int id)
    {
        int n = info.Length;
        int[,] adj = new int[n,n];
        int[,] identity = new int[n,n];
        int[] deg = new int[n];
        int[,] at = new int[n,n];
        double[] degCalc = new double[n];
        double[,] aHat = new double[n,n];
        int connCount = 0;

        for (int i = 0; i < info.Length; i++)
        {   
            hingeInfo current = info[i];
            int crrID = current.id;
            print(crrID);
            identity[crrID,crrID] = 1;
            at[crrID, crrID] = 1;
            hingeInfo[] descs = current.descendants;
            deg[crrID] += 1;
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

    public void Train(int Time)
    {
        
    }

    double[,] layer(double[,] features, int creatureID, int layer)
    {
        Matrix<double> aHat = DenseMatrix.OfArray(graphInfo[creatureID]);
        Matrix<double> fMatrix = DenseMatrix.OfArray(features);
        double[,] w;
        if(layer == 1)
                w = w1;
        else
                w = w2;
        Matrix<double> wMatrix = DenseMatrix.OfArray(w);
        Matrix<double> interior = aHat * (fMatrix * wMatrix);
        
        interior = interior.Map(x => Math.Max(0,x)); // activation function - ReLU
        return interior.ToArray();
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
        w1 = StringTodoubleMatrix(System.IO.File.ReadAllText(Application.persistentDataPath + "/w1.txt"));
        w2 = StringTodoubleMatrix(System.IO.File.ReadAllText(Application.persistentDataPath + "/w2.txt"));
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

    // public double[,] MultdoubleMatrix(double[,])
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
    public static double[,] StringTodoubleMatrix(string data)
    {
        string[] lines = data.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        int rows = int.Parse(lines[0]);
        int cols = int.Parse(lines[1]);

        double[,] matrix = new double[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            string[] values = lines[r + 2].Split(',');

            for (int c = 0; c < cols; c++)
            {
                matrix[r, c] = double.Parse(values[c]);
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
