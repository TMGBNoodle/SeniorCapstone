using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DNAInterp : MonoBehaviour
{
    public int maxSize = 5;
    public int minSize = 1;
    public int minAnchors = 1;
    public int maxAnchors = 20;

    public bool wrapPointer = false;

    public TMP_InputField input;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // print(interpNum(new char[] { 'A' }));
        // print(interpNum(new char[] { 'B', 'A' }));
        // print(interpNum(new char[] { 'B', 'B' }));
        // print(interpNum(new char[] { 'Z', 'Z' }));
        // print(interpNum(new char[] { 'B', 'A', 'A' }));
        // interpDNA("ABCDF");
        input = GetComponent<TMP_InputField>();
    }

    public void interpDNA(string val)
    {
        char[] vals = val.ToUpper().ToCharArray();
        char id = vals[0];
        Creature creature = new Creature();
        switch (id)
        {
            case 'A':
                (int, Part) info = interpPart(vals, 0);
                creature = new Creature(0, info.Item2); //create a new creature with an initial hub part. Move pointer to 1
                print(info.Item2);
                print(creature);
                break;
            default:
                break;
        }
        if (creature.id == -1)
        {
            //didn't work, L
        } else
        {
            
        }
    }

    public bool findEnd()
    {
        return false;
    }
    public (int, Part) interpPart(char[] vals, int pointer)
    {
        if (pointer < vals.Length)
        {
            char id = vals[pointer];
            switch (id)
            {
                case 'A':
                    print("Creating hub");
                    return interpHub(vals, pointer + 1);
                case 'B':
                    print("Creating limb");
                    return interpLimb(vals, pointer + 1);
                default:
                    return (pointer, new Part(partType.Null));
            }
        }
        else
        {
            return (pointer, new Part(partType.Null));
        }
        
    }

    // public Creature interp(Creature creature, char[] vals, int pointer)
    // {
    //     char id = vals[pointer];
    //     switch (id)
    //     {
    //         case 'A':
    //             return interpHub(creature, vals, pointer);
    //         case 'B':
    //             return interpLimb(creature, vals, pointer);
    //         default:
    //             return creature;
    //     }
    // }

    // Create hub 
    public (int, Part) interpHub(char[] vals, int pointer)
    {
        //I want to do all this when I know I have enough numbers left. I should check that first.
        //int baseNumsReq = 4;//2 for size, 2 for total anchors.
        int remaining = vals.Length - pointer;
        if (pointer > 0 && remaining > 4)
        {
            print("Valid Pointer" + pointer);
            (int, int) sizeRaw = interpNum(vals[pointer..(pointer + 1)]);
            float sizeActual = minSize + (maxSize * ((float)sizeRaw.Item1 / (float)sizeRaw.Item2));
            pointer = incrPointer(pointer, 2, vals.Length);
            (int, int) anchorsRaw = interpNum(vals[pointer..(pointer + 1)]);
            int anchorsActual = minAnchors + (int)(maxAnchors * ((float)anchorsRaw.Item1 / (float)anchorsRaw.Item2));
            pointer = incrPointer(pointer, 2, vals.Length);
            (int, int) anchorsUtRaw = interpNum(vals[pointer..(pointer + 1)]);
            int usedActual = (int)(anchorsActual * ((float)anchorsUtRaw.Item1 / (float)anchorsUtRaw.Item2));
            pointer = incrPointer(pointer, 2, vals.Length);
            //float sizeActual = 1;
            Part final = new Part(partType.Hub, sizeActual, anchorsActual);
            Dictionary<char[], int> lookup = new Dictionary<char[], int>();
            print(pointer);
            if (pointer == -1 || findEnd())
            {
                print("Exit halfway");
                return (-1, final);
            }
            for (int i = 0; i < usedActual; i++)
            {
                print("Adding Connection");
                int result = 0;
                if (!lookup.TryGetValue(vals[pointer..(pointer + 1)], out result))
                {
                    lookup[vals[pointer..(pointer + 1)]] = 1;
                    (int, int) idRaw = interpNum(vals[pointer..(pointer + 1)]);
                    int idAct = (int)(anchorsActual * ((float)idRaw.Item1 / (float)idRaw.Item2));
                    (int, Part) newVal = interpPart(vals, pointer);
                    Part otherPart = newVal.Item2;
                    pointer = newVal.Item1;
                    final.connections[idAct] = new Connection(idAct, otherPart);
                }
            }
            print("Next Part");
            print(pointer);
            return (pointer, final);
        } else
        {
            print("nullPart " + pointer);
            print("ASDASDKLASDFKASKDLFASKDFLASKDFLKASDFLKSADFLKa");


            
            return (1, new Part(partType.Null));
        }
    }

    public (int, Part) interpLimb(char[] vals, int pointer)
    {
        print("Do limb");
        return (pointer, new Part(partType.Limb));
    }

    public int incrPointer(int pointer, int add, int max) //basic function increments pointer with wrapparound if current value is above a threshhold
    {
        print("P: " + pointer);
        print("A: " + add);
        print("M: " + max);
                        
        if (pointer == -1)
        {
            return -1;
        }
        else if (pointer + add > max)
        {
            if (wrapPointer)
                return pointer + add - max;
            else
                return -1;
        }
        else
        {
            return pointer + add;
        }
    }

    //recieve two alphabetical numbers and convert them into a number
    public (int, int) interpNum(char[] nums)
    {
        int[] digits = nums.Select((c) => char.ToUpper(c) - 'A').ToArray<int>();//index == 0
        int final = digits.Last();
        int max = 25;
        int decPlace = 1;
        for (int i = digits.Length - 2; i > -1; i--)
        {
            final = final + digits[i] * (int)Mathf.Pow(26, decPlace); //BA = 26. B = 1. A = 0.
            max += 25 * (int)Mathf.Pow(26, decPlace);
            decPlace += 1;
        }
        return (final, max);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
