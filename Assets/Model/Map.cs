using System.Collections;
using UnityEngine;

public class Map
{
    //Tile mesh
    Tile[,] tiles;

    //Mesh parameters
    int width;
    public int Width{ get => width;}

    int height;
    public int Height{ get => height;}


    public Map(int width, int height)
    {
        this.width = width;
        this.height = height;

        //Init mesh matrix
        tiles = new Tile[width, height];

        //Initialize all tiles in a mesh
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j] = new Tile();
            }
        }
        randomizeTiles();
    }
    
    //Reassign tile type randomly
    public void randomizeTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (Random.Range(0, 2) == 1)
                    tiles[i, j].Type = Tile.TileType.Floor;
                else
                    tiles[i, j].Type = Tile.TileType.Empty;
            }
        }
    }

    //Returns tile at a specific location
    public Tile getTile(int x, int y)
    {
        if (0 <= x && x < width && 0 <= y && y < height)
        {
            return tiles[x, y];
        }
        else
        {
            Debug.LogError("Invalid tile index: ["+x+", "+ y + "]");
            return null;
        }
    }
}

