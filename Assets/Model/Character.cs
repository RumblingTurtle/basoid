using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character
{
    string name;

    public float speed;

    Queue<Job> jobs;

    bool busy;
    float timer;
    //Next path point
    float destx;
    float desty;

    //End of the path
    int endx;
    int endy;

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
        Vector2 currentPos = new Vector2(X, Y);
        Vector2 destination = new Vector2(destx, desty);
        if (Vector2.Distance(currentPos,destination)==0)
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
        busy = false;
        jobs = new Queue<Job>();
    }

    public void addJob(Job job)
    {
        jobs.Enqueue(job);
    }

    public bool isMovingTo(Tile tile)
    {
        return tile.X != endx || tile.Y != endy;
    }


    public void doJobs(Map currentMap)
    {
        if (jobs.Count != 0)
        {
            busy = true;
            Job currentJob = jobs.Peek();
            Tile jobTile = currentMap.getClosestWalkable((int)currentJob.Tile.X, (int)currentJob.Tile.Y, X, Y);
            if (jobTile == null)
            {
                jobs.Dequeue();
                return;
            }
                

            if (!currentJob.Done)
            {
                if (currentMap.getTile((int)X,(int)Y)!=jobTile && currentMap.getTile((int)destx, (int)desty)!=jobTile)
                {
                    if(isMovingTo(jobTile))
                        commandMovementTo(jobTile.X, jobTile.Y, currentMap);    
                }
                else
                {
                    currentJob.performJob();
                }
            }
            else
            {
                jobs.Dequeue();
            }
        }
        else
        {
            //DO a random walk every 4 seconds if not busy
            timer += Time.deltaTime;
            if (busy==false && timer>4.0f)
            {
                if (UnityEngine.Random.Range(0, 11)/10 >0.5)
                    return;
                int xOffset = UnityEngine.Random.Range(-5, 5);
                int yOffset = UnityEngine.Random.Range(-5, 5);
                if (currentMap.isBound((int)X + xOffset, (int)Y + yOffset))
                    commandMovementTo((int)X + xOffset, (int)Y + yOffset, currentMap);
                timer = 0.0f;
            }
        }
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
        if (x == (int)destx && y == (int)desty)
            return;

        float lastTileDist = Vector2.Distance(new Vector2((int)X, (int)Y), new Vector2(x, y));
        float nextTileDitst = Vector2.Distance(new Vector2(destx, desty), new Vector2(x, y));

        //Stop at a current closest tile to the destination
        if (lastTileDist <= nextTileDitst)
        {
            destx = (int)X;
            desty = (int)Y;
        }

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

        //Adds unchecked accessible neighbours of a given tile
        void addNeighbours(PathTile tile)
        {
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    bool isCurrentOrDiagonal = Mathf.Abs(i) == Mathf.Abs(j);

                    if (isCurrentOrDiagonal || 
                        !currentMap.isBound(tile.x + i, tile.y + j) || 
                        !currentMap.getTile(tile.x + i, tile.y + j).isWalkable()||
                        closedSet.Contains(tileMat[tile.x + i, tile.y + j])||
                        openedSet.Contains(tileMat[tile.x + i, tile.y + j]))
                        continue;


                    PathTile neighbour = new PathTile();
                    neighbour.x = tile.x+i;
                    neighbour.y = tile.y+j;

                    //Euclidian distance as heuristic metric
                    neighbour.h = Vector2.Distance(new Vector2(x, y), new Vector2(neighbour.x, neighbour.y));
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
        start.h = Vector2.Distance(new Vector2(x, y), new Vector2(destx, desty));
        start.x = (int)destx;
        start.y = (int)desty;
        tileMat[(int)destx, (int)desty] = start;
        closedSet.Add(tileMat[(int)destx, (int)desty]);

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

            //If destination is reached
            if (bestTile.x == x && bestTile.y == y)
                break;

            //Debug.Log("Best tile: " + bestTile.x + " " + bestTile.y);
            openedSet.Remove(bestTile);
            closedSet.Add(bestTile);

            addNeighbours(bestTile);

        }

        //If no route exists then return
        if (bestTile.x != x && bestTile.y != y)
            return;

        //Clear the stack
        currentPath.Clear();
        //Until current position is reached
        while (bestTile.x!= (int)destx || bestTile.y != (int)desty)
        {
            //Update stack and traverse backwards from the current bestTile value
            currentPath.Push(new Tuple<int,int>(bestTile.x, bestTile.y));
            bestTile = tileMat[bestTile.camefrom.Item1, bestTile.camefrom.Item2];
        }

        endx = x;
        endy = y;
    }
}
