using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
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
    public Creature()
    {
        this.id = -1;
        this.hub = new Part(partType.Null);
    }
    public override string ToString()
    {
        return string.Format("ID: {0}, Part Info: {1}", id, hub.ToString());
    }
}
public enum partType
{
    Hub,
    Limb,
    Null
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
                this.type = type;
                this.connections = new Connection[4]{new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb))};
                this.size = 5;
                break;
            case partType.Limb:
                this.type = type;
                this.connections = new Connection[1];
                this.size = 2;
                break;
            case partType.Null:
                this.type = type;
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
    public Part(partType type, float size, int anchors, Connection[] connections)
    {
        switch (type)
        {
            case partType.Hub:
                this.connections = connections;
                this.size = size;
                break;
            case partType.Limb:
                this.connections = connections;
                this.size = size;
                break;
        }
    }

    public Part()
    {
        this.type = partType.Null;
        // this.connections = new Connection[1];
        // this.connections = new Connection[4]{new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb)),new Connection(0, new Part(partType.Limb))};
        // this.size = 5;
    }

    public void addConn(int anchorID, Part otherPart)
    {
        if (anchorID < connections.Length)
        {
            this.connections[anchorID] = new Connection(anchorID, otherPart);
            UnityEngine.Debug.Log(this.connections[anchorID]);
        }
        else
            UnityEngine.Debug.Log("Anchor ID out of bounds of connection");
    }
    String printConn()
    {
        Boolean hasVals = false;
        String final = "";
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i] != null)
            {
                hasVals = true;
                final = final + (" (" + connections[i] + ") ");
            }
        }
        if (hasVals)
            return final;
        else
            return "No Connections.";
    }
    public override string ToString()
    {
        if (this.type != partType.Null)
        {
            return string.Format("Type: {0}, Size: {1}, AnchorCount: {2}, Connections: {3}", type, size, connections.Length, printConn());
        }
        else
        {
            return "Null Part";
        }
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
