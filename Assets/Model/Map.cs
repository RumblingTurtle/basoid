using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map
{
    //Tile mesh
    Tile[,] tiles;
    Plant[,] plants;

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

    public Map(float[,] tileValues)
    {
        width = tileValues.GetLength(0);
        height = tileValues.GetLength(1);
        
        //Init mesh matrix
        tiles = new Tile[width, height];
        plants = new Plant[width, height];

        //Initialize all tiles in a mesh
        resetTiles(tileValues, true);
        resetPlants();
    }

    void resetTiles()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Tile t = new Tile();
                t.X = i;
                t.Y = j;
                tiles[i, j] = t;
            }
        }
    }

    void resetPlants()
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Plant p = new Plant();
                p.Type = Plant.PlantType.None;

                p.X = i;
                p.Y = j;
                plants[i, j] = p;
            }
        }
    }

    void resetTiles(float[,] tileValues, bool normalized)
    {
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Tile currentTile = new Tile();
                currentTile.X = i;
                currentTile.Y = j;
                
                if (normalized)
                    currentTile.Type = Tile.getTypeNormalized(tileValues[i, j]);
                else
                    currentTile.Type = Tile.getType(tileValues[i, j]);

                tiles[i, j] = currentTile;
            }
        }
    }

    public void generateTilemapPerlin()
    {
        float[,] heights = MapGenerators.perlinGenerator(width, height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].Type = getTileType(heights[i,j]);
            }
        }
    }

    public void generateTilemapCelluar(int fillPecent, int epochs)
    {
        float[,] heights = MapGenerators.celluarGenerator(width, height, fillPecent, epochs);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].Type = getTileType(heights[i, j]);
            }
        }
    }


    public void generateTilemapDiamondSquare(int grain)
    {
        float[,] heights = MapGenerators.diamondSquareGenerator(width, height, grain);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].Type = getTileType(heights[i, j]);
            }
        }
    }

    public void setTile(int i, int j , Tile tile)
    {
        tiles[i, j] = tile;
    }
    

    public static Tile.TileType getTileType(float tileHeight)
    {
        switch (tileHeight)
        {
            case 4.0f:
                return Tile.TileType.Empty;
            case 3.0f:
                return Tile.TileType.Wall;
            case 2.0f:
                return Tile.TileType.Floor;
            case 1.0f:
                return Tile.TileType.Water;
            default:
                return Tile.TileType.None;
        }
    }

    public static int getTileHeight(Tile.TileType type)
    {
        switch (type)
        {
            case Tile.TileType.Empty:
                return 4;
            case Tile.TileType.Wall:
                return 3;
            case Tile.TileType.Floor:
                return 2;
            case Tile.TileType.Water:
                return 1;
            default:
                return 1;
        }
    }

    public Tile getRandomWalkable(int prob)
    {
        Tile candidate = null;
        bool candidateSelected = false;
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                if(tiles[i,j].isWalkable())
                {
                    if(candidateSelected)
                    {
                        if (Random.Range(0, 100) < prob)
                            return tiles[i, j];
                    }
                    else
                    {
                        candidate = tiles[i, j];
                        candidateSelected = true;
                    }
                }
            }
        }

        return candidate;
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

    public Plant getPlant(int x, int y)
    {
        if (0 <= x && x < width && 0 <= y && y < height)
        {
            return plants[x, y];
        }
        else
        {
            Debug.LogError("Invalid plant index: [" + x + ", " + y + "]");
            return null;
        }
    }


    static bool inRegion(List<Vector2Int> region, Vector2Int cell)
    {
        foreach(Vector2Int c in region)
        {
            if (c.x == cell.x && c.y == cell.y)
                return true;
        }
        return false;
    }

    static bool inRegions(List<List<Vector2Int>> regions, Vector2Int cell)
    {
        if (regions.Capacity == 0)
            return false;

        foreach (List<Vector2Int> region in regions)
        {
            if (inRegion(region, cell))
                return true;
        }
        return false;
    }

    List<Vector2Int> floodfill(int x, int y, Tile.TileType type)
    {
        List<Vector2Int> coveredCells = new List<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        

        if (tiles[x, y].Type != type)
            return coveredCells;

        queue.Enqueue(new Vector2Int(x, y));
        while (queue.Count!=0)
        {
            Vector2Int currentCell = queue.Dequeue();
            if (inRegion(coveredCells, currentCell))
                continue;

            for (int i = currentCell.x - 1; i <= currentCell.x + 1; i++)
            {
                for (int j = currentCell.y - 1; j <= currentCell.y + 1; j++)
                {
                    if (i >= 0 && i < width && j >= 0 && j < height)
                    {
                        Vector2Int neighbour = new Vector2Int(i, j);

                        if (tiles[i, j].Type != type)
                            continue;

                        if(inRegion(coveredCells, neighbour))
                            continue;

                        if ((i != currentCell.x && j != currentCell.y) ||
                            j != i ||
                            (i != currentCell.x + 1 && j != currentCell.y - 1) ||
                            (i != currentCell.x - 1 && j != currentCell.y + 1) )
                            queue.Enqueue(neighbour);
                    }
                }
            }

            
            coveredCells.Add(currentCell);

        }
        return coveredCells;
    }

    public List<List<Vector2Int>> getStructures(Tile.TileType type)
    {
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();

        float[,] heights = MapGenerators.perlinGenerator(width, height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if(tiles[i, j].Type == type)
                {
                    if(!inRegions(regions, new Vector2Int(i, j)))
                    {
                        regions.Add(floodfill(i, j,type));
                    }

                }
            }
        }

        return regions;
    }

    public static int structureTotalArea(List<List<Vector2Int>> structures)
    {
        int result = 0;
        foreach (List<Vector2Int> structure in structures)
        {
            result += structure.Count;
        }
        return result;
    }

}

