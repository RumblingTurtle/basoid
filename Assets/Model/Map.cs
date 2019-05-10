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
                float tileType = Mathf.PerlinNoise(((float)i /width)*10,((float)j /height)*10)*maxValue;
                if (0 <= tileType && tileType < maxValue/3)
                    tiles[i, j].Type = Tile.TileType.Wall;
                else if(maxValue / 3 <= tileType && tileType < 1 * maxValue / 2.3f)
                    tiles[i, j].Type = Tile.TileType.Empty;
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

    public Tile getClosestWalkable(int toX, int toY, float fromX, float fromY)
    {
        Tile result = null;
        float minDist = float.PositiveInfinity;

        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                bool isDiagonal = Mathf.Abs(i) == Mathf.Abs(j) && i != 0;
                bool bound = isBound(toX + i, toY + j);
                if (isDiagonal|| !bound)
                    continue;
                if(tiles[toX + i, toY + j].isWalkable())
                {
                    float dist = Vector2.Distance(new Vector2(toX + i, toY + j), new Vector2(fromX, fromY));
                    if (dist<minDist)
                    {
                        minDist = dist;
                        result = tiles[toX + i, toY + j];
                    }
                }
            }
        }
        return result;
    }
            //Checks if coordinates are within map bounds
            public bool isBound(int a, int b)
    {
        return Mathf.Clamp(a, 0, Width - 1) == a && Mathf.Clamp(b, 0, Height - 1) == b;
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

