using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Mathematics;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using Random = UnityEngine.Random;

public class GCNT : MonoBehaviour
{

    public static GCNT  instance{get; set;}
    float maxMotorSpeed = 100;
    int featureCount = 9;

    float episodeTime = 10;
    double sigma0 = 0.3;
    double decayRate = 0.9995;
    int globalStep = 0;
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
    float currentTime;

    float startTime;

    Vector3 startPos;

    bool training;
    double lastPosX;

    public bool episodeComp = false;
    Trajectory trainingInfo;
    // int[,] deg;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public class Trajectory
    {
        public List<double[,]> states = new();
        public List<double[]> actions = new();
        public List<double> logProbs = new();
        public List<double> rewards = new();
        public List<double> sigs = new();
    }
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
        init();
    }

    public void init()
    { 
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

    public void Train(hingeInfo[] creatureHinges, GameObject creature, int id)
    {
        if(currentTime - startTime < episodeTime)
        {
            Step(creatureHinges, trainingInfo, id, creature);
            currentTime = Time.time;
        } else
        {
            UpdateWeights(trainingInfo, id, 1e-4, 0.99);
            episodeComp = true;
            SaveWeights();
        }
    }

    public void InitWeights() //chatgpt generation
    {
            double XavierScale(int fanIn, int fanOut)
        {
            return Math.Sqrt(2.0 / (fanIn + fanOut));
        }

        // --- Gaussian sampler ---
        double NextGaussian()
        {
            double u1 = 1.0 - Random.value;
            double u2 = 1.0 - Random.value;
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        // -------- Initialize W1: (featureCount × hidden) --------
        w1 = new double[featureCount, hidden];
        double scale1 = XavierScale(featureCount, hidden);

        for (int i = 0; i < featureCount; i++)
            for (int j = 0; j < hidden; j++)
                w1[i, j] = NextGaussian() * scale1;

        // -------- Initialize W2: (hidden × featureCount) --------
        w2 = new double[hidden, 1];
        double scale2 = XavierScale(hidden, 1);

        for (int i = 0; i < hidden; i++)
            for (int j = 0; j < 1; j++)
                w2[i, j] = NextGaussian() * scale2;
        SaveWeights();
    }
    public void initTraining(Vector3 newStartPos, hingeInfo[] inf, int id)
    {
        print("Initialized values");
        LoadWeights();
        initCreature(inf, id);
        startPos = newStartPos;
        trainingInfo = new();
        startTime = Time.time;
        currentTime = Time.time;
        globalStep = 0;
        episodeComp = false;
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
        if(layer == 1)
            interior = interior.Map(x => Math.Max(0,x)); // activation function - ReLU
        return interior.ToArray();
    }

    double exploreActions(double decision, double variance) //Generated by ChatGPT, names changed so I can read it. uses the Box Mueller Transform to go from uniform distribution to normal/gaussian distribution. This is a statistics thing and I'm not entirely sure how it helps but supposedly it does.
    {
        double val1 = Random.value; //given a decision made by the GCN, vary it by some random value. Use Box mueler to change the value's distribution from uniform to gaussian
        double val2 = Random.value;
        double v = Math.Sqrt(-2.0 * Math.Log(val1)) * Math.Sin(2.0 * Math.PI * val2);
        return decision + v * variance;
    }

    double LogProbGaussian(double action, double mean, double sigma) //Generated by ChatGPT.
    {
        double var = sigma * sigma;
        double logScale = Math.Log(sigma * Math.Sqrt(2 * Math.PI));
        double diff = action - mean;
        return - (diff * diff) / (2 * var) - logScale;
    }

    double[] getActions(hingeInfo[] inf, int id) //ChatGPT took this from my code now I'm taking it back. So credit to ChatGPT then to me. This is essentially just the eval function, but written specifically for this case so that I can debug easier
    {
        double[,] features = getFeatures(inf);

        double[,] v1 = layer(features, id, 1);
        double[,] v2 = layer(v1, id, 2);
        
        double[] final = new double[inf.Length];

        for (int i = 0; i < final.Length; i++)
        {
            final[i] = v2[i, 0]; //the only thing the algorithm will be changing is the motorspeed. So this is the only relevant value
        }
        return final;
    }

    public void Step(hingeInfo[] info, Trajectory traj, int creatureID, GameObject center) //generated by chatGPT, adapted very slightly
    {
        double[] means = getActions(info, creatureID); //get the actions the current model wants to take
        double sigma = sigma0 * Math.Pow(decayRate, globalStep);
        globalStep++;; 

        double[] actions = new double[means.Length]; //create an empty array to represent the actions taken here
        double[,] initState = getFeatures(info);
        double totalLogProb = 0;

        for (int i = 0; i < means.Length; i++)
        {
            double a = exploreActions(means[i], sigma);
            actions[i] = a;

            double lp = LogProbGaussian(a, means[i], sigma);
            totalLogProb += lp;
            a = Math.Max(Math.Min(a, maxMotorSpeed), -maxMotorSpeed);
            var motor = info[i].hinge.motor;
            motor.motorSpeed = (float)a;
            info[i].hinge.motor = motor;
        }

        traj.states.Add(initState);
        traj.actions.Add(actions);
        traj.logProbs.Add(totalLogProb);
        traj.sigs.Add(sigma);

        // reward for this step
        double r = ComputeReward(center);
        traj.rewards.Add(r);
    }

    double ComputeReward(GameObject center) 
    {
        double vx = (center.transform.position.x - lastPosX) / Time.deltaTime;
        lastPosX = center.transform.position.x;
        return vx;
    }

    public void UpdateWeights(Trajectory t, int creatureID, double lr, double gamma)
    {
        int T = t.states.Count;
        // ---- 1. Compute return R ----
        double[] G = new double[T];
        double running = 0;

        for (int i = T - 1; i >= 0; i--)
        {
            running = t.rewards[i] + gamma * running;
            G[i] = running;
        }

        //Fancy comments are chatgpt, these are me. Okay so my understanding is that storing a shit ton of info about the state is bad(expensive and slow), so this bypasses that by reconstructing the values after the fact
        Matrix<double> A = DenseMatrix.OfArray(graphInfo[creatureID]); //initial adjascency matrix, a hat
        Matrix<double> W1m = DenseMatrix.OfArray(w1); // weights numba uno
        Matrix<double> W2m = DenseMatrix.OfArray(w2); // weights numba two
        Matrix<double> dW1_total = DenseMatrix.Create(W1m.RowCount, W1m.ColumnCount, 0.0);
        Matrix<double> dW2_total = DenseMatrix.Create(W2m.RowCount, W2m.ColumnCount, 0.0);
        // ---- 3. Loop over every time step ----
        for (int tstep = 0; tstep < T; tstep++)
        {
            double[,] F = t.states[tstep];
            Matrix<double> Fm = DenseMatrix.OfArray(F);

            double[] actions = t.actions[tstep];
            double sig = t.sigs[tstep];

            // ---- Forward pass (to get means for this step) ----
            Matrix<double> H1pre = A * (Fm * W1m);
            Matrix<double> H1 = H1pre.Map(x => x > 0 ? x : 0); // ReLU

            Matrix<double> H2pre = A * (H1 * W2m);
            Matrix<double> H2 = H2pre; // Means (Gaussian μ)

            int n = H2.RowCount;

            // ---- 4. d(log π)/d(mu) for Gaussian ----
            Matrix<double> dH2 = DenseMatrix.Create(n, 1, 0);

            for (int i = 0; i < n; i++)
            {
                double mu = H2[i, 0];
                double a = actions[i];
                double dLogP_dMu = (a - mu) / (sig * sig);

                // REINFORCE full-trajectory gradient: −G_t * derivative
                dH2[i, 0] = -G[tstep] * dLogP_dMu;
            }

            // ---- 5. Backprop into W2 ----
            Matrix<double> dW2 = H1.Transpose() * (A.Transpose() * dH2);

            // ---- 6. Backprop to H1 ----
            Matrix<double> dH1 = (A.Transpose() * dH2) * W2m.Transpose();

            // apply ReLU derivative
            Matrix<double> reluMask1 = H1pre.Map(x => x > 0 ? 1.0 : 0.0);
            dH1 = dH1.PointwiseMultiply(reluMask1);

            // ---- 7. Backprop into W1 ----
            Matrix<double> dW1 = Fm.Transpose() * (A.Transpose() * dH1);

            // ---- 8. Accumulate grads
            dW1_total += dW1;
            dW2_total += dW2;
        }
        // // ---- 9. Normalize accumulated gradients by episode length ----
        // double scale = 1.0 / T;
        // dW1_total = dW1_total * scale;
        // dW2_total = dW2_total * scale;

        // // ---- 10. Global gradient norm clipping ----
        // // Compute global L2 norm of all parameters
        // double normW1 = dW1_total.PointwisePower(2).RowSums().Sum();
        // double normW2 = dW2_total.PointwisePower(2).RowSums().Sum();
        // double globalNorm = Math.Sqrt(normW1 + normW2);

        // // Threshold
        // double maxNorm = 5.0;  // typical value: 1, 5, or 10

        // if (globalNorm > maxNorm)
        // {
        //     double clipScale = maxNorm / globalNorm;

        //     dW1_total = dW1_total * clipScale;
        //     dW2_total = dW2_total * clipScale;
        // }
        W1m = W1m - dW1_total * lr;
        W2m = W2m - dW2_total * lr;

        w1 = W1m.ToArray();
        w2 = W2m.ToArray();
    }

    public void SaveWeights()
    {
        string savew1 = MatrixToString(w1);
        string savew2 = MatrixToString(w2);
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
