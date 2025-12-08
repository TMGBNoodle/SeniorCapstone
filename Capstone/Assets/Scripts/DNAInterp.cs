using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.UI;
// Idea:
// Overarching string. Initial input, determines actual complexity of the creature. 
// RefDoc - Constructed from initial input, consists of looped input. Overarching string references this. For initial prototype is essentially just input[(pointer % input.length)]
public class DNAInterp : MonoBehaviour
{
    public int maxSize = 5;
    public int minSize = 1;
    public int minAnchors = 1;
    public int maxAnchors = 20;

    public Toggle append;

    public int creatureid;

    public Toggle prepend;
    public bool wrapPointer = false;

    public string[] stopCodons;
    public TMP_InputField input;
    public const int stopCount = 25;
    int baseNum = (char)'A';

    public GameObject mutationSlider;
    Slider mutationIntensity;
    public string currentString = "";
    public GameObject creatureCreator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // print(interpNum(new char[] { 'A' }));
        // print(interpNum(new char[] { 'B', 'A' }));
        // print(interpNum(new char[] { 'B', 'B' }));
        // print(interpNum(new char[] { 'Z', 'Z' }));
        // print(interpNum(new char[] { 'B', 'A', 'A' }));
        // interpDNA("ABCDF");
        stopCodons = new string[stopCount];
        int endNum = baseNum + 25;
        for (int i = 0; i < stopCodons.Length; i++)
        {
            stopCodons[i] = new string(new char[] { (char)endNum, (char)(endNum - i) });
            // print(stopCodons[i]);
        }
        // print((int)'a');
        // print((int)'A');
        // print((int)'Z');
        mutationIntensity = mutationSlider.GetComponent<Slider>(); 
        input = GetComponent<TMP_InputField>();
    }

    public void mutate()
    {
        string val = input.text;
        char[] vals = val.ToCharArray();
        if(vals.Length > 0)
        {
            bool doPrepend = true;
            bool doAppend = true;
            int intensity = (int)mutationIntensity.value;
            int[] usedVals = new int[intensity];
            for (int i = 0; i < intensity; i++)
            {
                int rand = Random.Range(0, vals.Length - 1);
                while (usedVals.Contains(rand))
                {
                    rand = Random.Range(0, vals.Length - 1);
                }
                usedVals.Append(rand);
                vals[rand] = (char)Mathf.Clamp(((int)vals[rand] + Random.Range(-1, 2)), 97, 122);
            }
            string final = (new string(vals)).ToLower();
            if (doPrepend)
            {
                final = (char)Random.Range(97, 122) + final;
            }
            if (doAppend)
            {
                final = final + (char)Random.Range(97, 122);
            }
            input.text = final;
        }   
    }

    public void interpDNA(string val)
    {
        creatureid = Random.Range(1000, 10000);
        if (val.Length <= 0)
        {
            print("Length to short");
            return;
        }
        val = val.ToLower();
        char[] vals = val.ToUpper().ToCharArray();
        Creature creature;
        // (int, Part) info = interpPart(vals, 0);
        // Part main = info.Item2;
        creature = new Creature(creatureid, interpHub(vals, 1).Item2, currentString); //create a new creature with an initial hub part. Move pointer to 1
        if (creature.id == -1)
        {
            print("Bad creature");
            //didn't work, L
        }
        else
        {
            print("Creature at: " + creature);
            creatureCreator.GetComponent<DrawCreature>().drawCreature(creature);
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
            switch ((int)id % 2)//)
            {
                case 1:
                    print("Found hub: " + pointer);
                    return interpHub(vals, pointer + 1);
                case 0:
                    print("Found limb: " + pointer);
                    return interpLimb(vals, pointer + 1);
                default:
                    return interpPart(vals, pointer + 1);
            }
        }
        else
        {
            print("Nullpart, pointer too large" + pointer);
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
        if (pointer > 0 && remaining >= 4)
        {
            // string seq = new string(vals[pointer..(pointer + 4)]);
            // foreach (string stopCodon in stopCodons)
            // {
            //     if (seq.Contains(stopCodon))
            //     {
            //         print("Found stop codon");
            //         return (pointer, new Part(partType.Null));
            //     }
            // }
            print("Valid Pointer: " + pointer + " with remaining: " + remaining);
            (int, int) sizeRaw = interpNum(vals[pointer..(pointer + 1)]);
            float sizeActual = minSize + (maxSize * ((float)sizeRaw.Item1 / (float)sizeRaw.Item2));
            pointer = incrPointer(pointer, 2, vals.Length);
            if (pointer == -1 || vals.Length - pointer <= 1)
            {
                print("Exit halfway");
                return (-1, new Part(partType.Null));
            }
            (int, int) anchorsRaw = interpNum(vals[pointer..(pointer + 1)]);
            int anchorsActual = minAnchors + (int)(maxAnchors * ((float)anchorsRaw.Item1 / (float)anchorsRaw.Item2));
            pointer = incrPointer(pointer, 2, vals.Length);
            print("Actual anchors: " + anchorsActual);
            if (pointer == -1 || vals.Length - pointer <= 1)
            {
                print("Exit with base hub");
                return (-1, new Part(partType.Hub, sizeActual, anchorsActual));
            }
            (int, int) anchorsUtRaw = interpNum(vals[pointer..(pointer + 1)]);
            print("Anchors ut info: " + anchorsUtRaw);
            int usedActual = (int)(anchorsActual * ((float)anchorsUtRaw.Item1 / (float)anchorsUtRaw.Item2));
            print("Anchors used: " + usedActual);
            pointer = incrPointer(pointer, 2, vals.Length);
            if (pointer == -1 || vals.Length - pointer <= 1)
            {
                print("Exit halfway" + creatureid);
                return (-1, new Part(partType.Hub, sizeActual, anchorsActual));
            }
            //float sizeActual = 1;
            Part final = new Part(partType.Hub, sizeActual, anchorsActual);
            Dictionary<char[], int> lookup = new Dictionary<char[], int>();
            print(pointer);
            print("using " + usedActual + " spaces : " + creatureid);
            if (pointer == -1 || vals.Length - pointer <= 1)
            {
                print("Exit halfway" + creatureid);
                return (-1, final);
            }
            for (int i = 0; i < usedActual; i++)
            {
                // print("Adding Connection");
                remaining = vals.Length - pointer;
                if (remaining > 1 && pointer > -1)
                {
                    print("Valid remaining: " + remaining);
                    print("ArrayL: " + vals.Length);
                    print("Pointer at: " + pointer);
                    int result = 0;
                    if (!lookup.TryGetValue(vals[pointer..(pointer + 1)], out result))
                    {
                        print("Has not been used");
                        lookup[vals[pointer..(pointer + 1)]] = 1;
                        (int, int) idRaw = interpNum(vals[pointer..(pointer + 1)]);
                        print("Actual good anchors: " + anchorsActual);
                        int idAct = (int)((anchorsActual-1) * ((float)idRaw.Item1 / (float)idRaw.Item2));
                        print("Actual id: " + idAct);
                        pointer = incrPointer(pointer, 2, vals.Length);
                        (int, Part) newVal = interpPart(vals, pointer);
                        Part otherPart = newVal.Item2;
                        print("OPart type: " + otherPart.type);
                        // pointer = newVal.Item1;
                        final.connections[idAct] = new Connection(idAct, otherPart);
                    }
                } else
                {
                    print("Invalid pointer or not enough vals left : " + creatureid);
                    print("Pointer is " + pointer);
                    print("Remaining" + remaining);
                }
            }
            print("Part ended, Pointer: " + pointer + " : " + creatureid);
            return (pointer, final);
        }
        else
        {
            print("nullPart, pointer : " + pointer + " : " + creatureid);



            return (-1, new Part(partType.Null));
        }
    }

    public (int, Part) interpLimb(char[] vals, int pointer)
    {
        print("building limb");
        int remaining = vals.Length - pointer;
        if (pointer > 0 && remaining >= 2)
        {
        (int, int) sizeRaw = interpNum(vals[pointer..(pointer + 1)]);
        float sizeActual = minSize + (maxSize * ((float)sizeRaw.Item1 / (float)sizeRaw.Item2));
        print("Limbsize" + sizeActual);
        pointer = incrPointer(pointer, 2, vals.Length);
        //minimum data reached to create a limb
        Part part = new Part(partType.Limb, sizeActual, 1);
        if (pointer == -1 || vals.Length - pointer <= 1)
        {
            print("limb with no cons");
            return (pointer, part);
        } else
        {
            print("Limb with conns");
            (int,Part) info = interpPart(vals, pointer);
            // pointer = info.Item1;
            part.connections[0] = new Connection(0, info.Item2);
        }
        return (pointer, part);
        } else
        {
            print("NP: " + pointer + remaining);
            return (pointer, new Part(partType.Null));
        }
    }

    public int incrPointer(int pointer, int add, int max) //basic function increments pointer with wrapparound if current value is above a threshhold
    {
        if (pointer == -1)
        {
            print("Return invalid");
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
        int[] digits = nums.Select((c) => char.ToLower(c) - 'a').ToArray<int>();//index == 0
        int final = digits.Last();
        int max = 25;
        int decPlace = 1;
        for (int i = digits.Length - 1; i > -1; i--)
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
