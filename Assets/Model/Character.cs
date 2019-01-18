using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    string name;

    public float speed;

    //Next path point
    float destx;
    float desty;

    Stack<Tuple<int, int>> currentPath;

    float x;
    float y;

    public float X { get => x; set => x = value; }
    public float Y { get => y; set => y = value; }

    public string Name { get => name; set => name = value; }
    public float Destx { get => destx; set => destx = value; }
    public float Desty { get => desty; set => desty = value; }


    public Stack<Tuple<int, int>> CurrentPath { get => currentPath; }

    public bool isMoving()
    {
        if ((int)X == (int)destx && (int)Y == (int)desty)
            return false;
        else
            return true;
    }


    public Character(string name, float x, float y)
    {
        speed = 2.0f;
        Name = name;
        X = x;
        Y = y;
        currentPath = new Stack<Tuple<int, int>>();
        destx = X;
        desty = Y;
    }

    public void move()
    {
        //If current path point is reached and it is not final
        if(!isMoving() && currentPath.Count!=0)
        {
            //Get next path point
            Tuple<int, int> next = currentPath.Pop();
            destx = next.Item1;
            desty = next.Item2;
        }

        X = Mathf.MoveTowards(x, destx, speed * Time.deltaTime);
        Y = Mathf.MoveTowards(y, desty, speed * Time.deltaTime);
    }

    public void commandMovementTo(int x, int y, Map currentMap)
    {
        //Stop at a current closest tile in case of pending movement
        destx = (int)X;
        desty = (int)Y;

        //Recalculate path
        calculatePath(x, y, currentMap);
    }

    public struct PathTile
    {
        public int x;
        public int y;
        public float h; //Heuristic cost
        public float g; //Calculated cost to reach this node from starting point
        public Tuple<int, int> camefrom;
    }

    
    //A* implementation
    void calculatePath(int x, int y, Map currentMap)
    {
        //If destination is not a walkable object
        if (!currentMap.getTile(x, y).isWalkable())
            return;

        int width = currentMap.Width;
        int height = currentMap.Height;

        //Path tile accounting matrix
        PathTile[,] tileMat = new PathTile[width, height];

        List<PathTile> closedSet = new List<PathTile>();
        List<PathTile> openedSet = new List<PathTile>();

        //Checks if coordinates are within map bounds
        bool isBound(int a, int b)
        {
            return Mathf.Clamp(a, 0, width-1) == a && Mathf.Clamp(b, 0, height-1) == b;
        }

        //Adds unchecked accessible neighbours of a given tile
        void addNeighbours(PathTile tile)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    bool isCurrentOrDiagonal = i==j;

                    if (isCurrentOrDiagonal || 
                        !isBound(tile.x + i, tile.y + j) || 
                        currentMap.getTile(tile.x + i, tile.y + j).isWalkable()||
                        closedSet.Contains(tileMat[tile.x + i, tile.y + j])||
                        openedSet.Contains(tileMat[tile.x + i, tile.y + j]))
                        continue;


                    PathTile neighbour = new PathTile();
                    neighbour.x = tile.x+i;
                    neighbour.y = tile.y+j;

                    //Euclidian distance as heuristic metric
                    neighbour.h = Mathf.Sqrt(Mathf.Pow(x - neighbour.x, 2) + Mathf.Pow(y - neighbour.y, 2));
                    //A step from tile to tile costs 1 unit
                    neighbour.g = tile.g+1;

                    neighbour.camefrom = new Tuple<int,int>(tile.x,tile.y);

                    //Update tile matrix
                    tileMat[tile.x + i, tile.y + j] = neighbour;
                    //And add new tile to the opened set

                    openedSet.Add(tileMat[tile.x + i, tile.y + j]);
                    //Debug.Log("Added " + neighbour.x+" "+neighbour.y);
                }
            }
        };

        //Starting point initialization
        PathTile start = new PathTile();
        start.g = 0;
        start.h = Mathf.Sqrt(Mathf.Pow(x - (int)X, 2) + Mathf.Pow(y - (int)Y, 2));
        start.x = (int)X;
        start.y = (int)Y;
        tileMat[(int)X, (int)Y] = start;
        closedSet.Add(tileMat[(int)X, (int)Y]);

        addNeighbours(start);
        
        PathTile bestTile = new PathTile(); //Best tile from the opened set

        while (openedSet.Count != 0)
        {
            float bestScore = float.PositiveInfinity;

            foreach (PathTile tile in openedSet)
            {
                if (bestScore > tile.g + tile.h)
                {
                    bestTile = tile;
                    bestScore = tile.g + tile.h;
                }
            }

            //Debug.Log("Best tile: " + bestTile.x + " " + bestTile.y);

            addNeighbours(bestTile);
            openedSet.Remove(bestTile);
            closedSet.Add(bestTile);

            //If destination is reached
            if (bestTile.x == x && bestTile.y == y)
                break;
        }

        //If no route exists then return
        if (bestTile.x != x && bestTile.y != y)
            return;

        //Clear the stack
        currentPath.Clear();

        //Push the last best tile == destination
        currentPath.Push(new Tuple<int,int>(bestTile.x,bestTile.y));

        //Until current position is reached
        while (bestTile.x!=(int)X && bestTile.y!= (int)Y)
        {
            //Update stack and traverse backwards from the current bestTile value
            currentPath.Push(bestTile.camefrom);
            bestTile = tileMat[currentPath.Peek().Item1, currentPath.Peek().Item2];
        }

        //Get next path point from the stack
        Tuple<int,int> next = currentPath.Pop();
        destx = next.Item1;
        desty = next.Item2;
    }
}
