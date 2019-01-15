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
        resetTiles();
    }
    
    void resetTiles()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                tiles[i, j] = new Tile();
            }
        }
    }

    //Reassign tile type with preling noize and value ranges
    public void randomizeTiles()
    {
        int maxValue = 6;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float tileType = Mathf.PerlinNoise(i-0.5f,j-0.5f)*maxValue;

                if (0 <= tileType && tileType < maxValue/3)
                    tiles[i, j].Type = Tile.TileType.Empty;
                else if(maxValue / 3 <= tileType && tileType < 1 * maxValue / 2.3f)
                    tiles[i, j].Type = Tile.TileType.Wall;
                else
                    tiles[i, j].Type = Tile.TileType.Floor;
            }
        }
    }

    //Bad drunkards walk implementation
    public void randomizeByWalking()
    {
        int x = Random.Range(0, Width);
        int y = Random.Range(0, Height);

        tiles[x, y].Type = Tile.TileType.Wall;

        while (true)
        {
            int direction = Random.Range(0, 4);
            int newx = 0;
            int newy = 0;

            Debug.Log("New wall at: " + x + " " + y);
            //0 <= x && x < width && 0 <= y && y < height

            if (0 <= y + 1 && y + 1 < height && direction == 0)
                newy = y + 1;
            else if (0 <= y - 1 && y - 1 < height && direction == 1)
                newy = y - 1;
            else if (0 <= x + 1 && x + 1 < width && direction == 2)
                newx = x + 1;
            else if (0 <= x - 1 && x - 1 < width && direction == 3)
                newx = x - 1;
            else
                break;

            x = newx;
            y = newy;

            tiles[x, y].Type = Tile.TileType.Wall;
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

