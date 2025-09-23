using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using UnityEngine;

[SerializeField]
public class Creature
{
    public int id;
    public Part hub;

    public Creature(int id, Part part)
    {
        this.id = id;
        this.hub = part;
    }

}
public enum partType
{
    Hub,
    Limb
}

[SerializeField]
public class Part
{
    public partType type;

    public float size;
    public Connection[] connections;
    public Part(partType type)
    {
        switch (type)
        {
            case partType.Hub:
                this.connections = new Connection[4];
                this.size = 5;
                break;
            case partType.Limb:
                this.connections = new Connection[1];
                this.size = 2;
                break;
        }
    }
    public Part(partType type, float size, int anchors)
    {
        switch (type)
        {
            case partType.Hub:
                this.connections = new Connection[anchors];
                this.size = size;
                break;
            case partType.Limb:
                this.connections = new Connection[1];
                this.size = size;
                break;
        }
    }
    public override string ToString()
    {
        return string.Format("Type: {0}, Size: {1}, AnchorCount: {2}, Connected: {3}", type, size, connections.Length, connections);
    }
}

public class Connection
{
    public int anchorID;
    public Part otherPart;
    public Connection(int anchorID, Part otherPart)
    {
        this.anchorID = anchorID;
        this.otherPart = otherPart;
    }

    public override string ToString()
    {
        return string.Format("Anchored At: {0}, Connected to: {1}", anchorID, otherPart);
    }
}
