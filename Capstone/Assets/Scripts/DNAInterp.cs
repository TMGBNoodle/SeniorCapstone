using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class DNAInterp : MonoBehaviour
{
    public int maxSize = 5;
    public int minSize = 1;
    public int minAnchors = 1;
    public int maxAnchors = 20;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        print(interpNum(new char[] { 'A' }));
        print(interpNum(new char[] { 'B', 'A' }));
        print(interpNum(new char[] { 'B', 'B' }));
        print(interpNum(new char[] { 'Z', 'Z' }));
        print(interpNum(new char[] { 'B', 'A', 'A' }));
        interp("ABCDF");
    }

    public Creature interp(string val)
    {
        char[] vals = val.ToUpper().ToCharArray();
        char id = vals[0];
        Creature creature;
        switch (id)
        {
            case 'A':
                creature = interpHub(new Creature(0, new Part(partType.Hub)), vals, 1);
                break;
            default:
                return null;
                break;
        }
        return creature;
    }

    public Creature interp(Creature creature, char[] vals, int pointer)
    {
        char id = vals[0];
        switch (id)
        {
            case 'A':
                return interpHub(creature, vals, pointer);
            default:
                return creature;
        }
    }

    public Creature interpHub(Creature entity, char[] vals, int pointer)
    {
        //convert first 2 digits into a number representing size
        (int, int) sizeRaw = interpNum(vals[pointer..(pointer + 2)]);
        print(sizeRaw);
        float sizeActual = minSize + (maxSize * ((float)sizeRaw.Item1 / (float)sizeRaw.Item2));
        print(sizeActual);
        pointer += 2;
        (int, int) anchorsRaw = interpNum(vals[pointer..(pointer + 2)]);
        print(anchorsRaw);
        int anchorsActual = minAnchors + (int)(maxAnchors * ((float)anchorsRaw.Item1 / (float)anchorsRaw.Item2));
        print(anchorsActual);
        entity.hub = new Part(partType.Hub, sizeActual, anchorsActual);
        print(entity.hub);
        return entity;
    }

    //recieve two alphabetical numbers and convert them into a number
    public (int,int) interpNum(char[] nums) {
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
