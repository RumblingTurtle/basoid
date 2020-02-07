using System.Collections;
using UnityEngine;
using System;

public class Tile
{
    //Default tile types
    public enum TileType { Empty, Wall ,Floor,  Sand, Water, None };
    public enum BiomeType { Desert, Tundra, Ice };

    //Current tile type
    private TileType type;
    private BiomeType biome;
    
    private int x, y;

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }

    //Tile type change callback delegate
    private Action<Tile> typeChangeCallbacks;

    public static int typeCount()
    {
        return Enum.GetValues(typeof(Tile.TileType)).Length;
    }

    public static Tile.TileType getType(float tileHeight)
    {
        switch (Mathf.Floor(tileHeight))
        {
            case 4.0f:
                return Tile.TileType.Empty;
            case 3.0f:
                return Tile.TileType.Wall;
            case 2.0f:
                return Tile.TileType.Floor;
            case 1.0f:
                return Tile.TileType.Sand;
            case 0.0f:
                return Tile.TileType.Water;
            default:
                return Tile.TileType.None;
        }
    }

    public static Tile.TileType getTypeNormalized(float tileHeight)
    {
        float height = Mathf.Round(tileHeight * (typeCount() - 2));
        return getType(height);
    }

    public static int getHeight(Tile.TileType type)
    {
        switch (type)
        {
            case Tile.TileType.Empty:
                return 4;
            case Tile.TileType.Wall:
                return 3;
            case Tile.TileType.Floor:
                return 2;
            case Tile.TileType.Sand:
                return 1;
            case Tile.TileType.Water:
                return 0;
            default:
                return -1;
        }
    }

    public int getHeight()
    {
        return getHeight(type);
    }

    public float getHeightNormalized()
    {
        return (float)getHeight(type)/(typeCount()-2);
    }

    //Type setter and getter
    public TileType Type
    {   get => type;

        set
        {
            TileType oldType = type;
            type = value;
            //Invoke callback on change
            if (oldType != value && typeChangeCallbacks != null)
                typeChangeCallbacks(this);
        }
    }

    public BiomeType Biome { get => biome; set => biome = value; }

    public float getTileEncoding()
    {
        int xoffset = getHeight();
        int yoffset = 0;

        if (Biome == BiomeType.Tundra)
            yoffset = 0;
        if (Biome == BiomeType.Desert)
            yoffset = 1;
        if (Biome == BiomeType.Ice)
            yoffset = 2;
        
        return (xoffset+typeCount()*yoffset)/255.0f;
    }

    public int getTileIntEncoding()
    {
        int xoffset = getHeight();
        int yoffset = 0;
        if (Biome == BiomeType.Tundra)
            yoffset = 0;
        else if (Biome == BiomeType.Desert)
            yoffset = 1;
        if (Biome == BiomeType.Ice)
            yoffset = 2;

        return xoffset + typeCount() * yoffset;
    }

    public Tile(TileType type)
    {
        this.Type = type;
    }

    public Tile()
    {
        Type = TileType.None;
    }

    //Add delegate to set of callbacks
    public void registerCallback(Action<Tile> callback)
    {
        typeChangeCallbacks += callback;
    }
    //Remove delegate from set of callbacks
    public void unRegisterCallback(Action<Tile> callback)
    {
        typeChangeCallbacks -= callback;
    }

    public bool isWall()
    {
        return type == TileType.Wall;
    }

    public bool isWater()
    {
        return type == TileType.Water;
    }

    public bool isWalkable()
    {
        return !isWall() && !isWater();
    }

    public bool isSand()
    {
        return type == TileType.Sand;
    }

    public bool isNone()
    {
        return type == TileType.None;
    }

    public bool isEmpty()
    {
        return type == TileType.Empty;
    }

}
