using System.Collections;
using UnityEngine;
using System;

public class Tile
{
    //Default tile types
    public enum TileType { Empty, Floor, Wall };

    //Current tile type
    [SerializeField]
    private TileType type;

    int x;
    int y;

    //Tile type change callback delegate
    private Action<Tile> typeChangeCallbacks;

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

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }

    public Tile(TileType type)
    {
        this.Type = type;
    }

    public Tile()
    {
        Type = TileType.Empty;
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
}
