using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CreatureControl : MonoBehaviour
{
    public hingeInfo[] hinges;

    Dictionary<string, partType> convInfo = new Dictionary<string, partType>
    {
        {"Hub", partType.Hub},
        {"Limb", partType.Limb},
        {"Null", partType.Null}
    };
    int hingeCount = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hinges = getHingeInfo(gameObject, 0, -1);
        
        GCNT.instance.initCreature(hinges, 1);
        moveCycle();
    }

    // void initHinges()
    // {
    //     Queue<GameObject> objs = new Queue<GameObject>{};
    //     objs.Enqueue(gameObject);
    //     while(objs.Count > 0)
    //     {
    //         GameObject currentObj = objs.Dequeue();
    //         hinges.Append(new hingeInfo()){
                
    //         }
    //     }
    // }


    hingeInfo[] getHingeInfo(GameObject obj, int depth, int parentID)
    {
        print("Getting hinge info");
        HingeJoint2D[] baseHinge = obj.GetComponents<HingeJoint2D>();
        if(baseHinge.Length > 0)
        {
            print("BaseHinge");
            List<hingeInfo> final = new List<hingeInfo>();
            print(baseHinge.Length);
            for (int i = 0; i < baseHinge.Length; i++)
            {
                HingeJoint2D inf = baseHinge[i];
                GameObject oBody = inf.connectedBody.gameObject;

                hingeInfo[] descendants = getHingeInfo(oBody, depth+1, hingeCount);
                final.Add(new hingeInfo(hingeCount, inf, depth, descendants.Length, descendants, (int)convInfo[oBody.tag]));
                hingeCount += 1;
                final.AddRange(descendants);
            }
            return final.ToArray();
        }
        else
        {
            return new hingeInfo[]{};
        }
    }
    // Update is called once per frame
    void Update()
    {
    }

    void moveCycle()
    {
        for (int i = 0; i < hinges.Length; i++)
        {
            HingeJoint2D hinge = hinges[i].hinge;
            hinge.motor = new JointMotor2D {motorSpeed = 100, maxMotorTorque = 500000};
        }
    }
    
    void moveCGN()
    {
        double[,] moveInfo = GCNT.instance.getFeatures(hinges);
    }
    public partType parsePart(string name)
    {
        return convInfo[name];
    }    
    
}
public class hingeInfo
    {
        public int id;
        public HingeJoint2D hinge;
        public int height;
        public int ancestry; //How many subordinates  hinge joints does this have? (Like how many hinge joints will be affected by this hinge articulating)
        public hingeInfo[] descendants; //ref to all it's direct descendants
        
        public int parent;

        public float size;


        public int type;
        public hingeInfo(int idInf, HingeJoint2D hingein, int heightinf, int ancest, hingeInfo[] desc, int attachedType)
        {
            id = idInf;
            hinge = hingein;
            height = heightinf;
            ancestry = ancest;
            descendants = desc;
            type = attachedType;
        }
        // public void addDesc(hingeInfo newDesc)
        // {
        //     descendants.Append(newDesc);
        //     ancestry += 1;
        // }
    }
