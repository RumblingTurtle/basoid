using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MapController : MonoBehaviour
{
    public Sprite floorSprite; // Default floor sprite

    public Sprite selectionSprite; // Mouse selection sprite
    public Sprite backgroundSprite; // Background dirt sprite  
    public Sprite characterSprite; // Default character sprite

    //Object for tile groupings
    GameObject backgroundGroup;
    GameObject tileGroup;

    //Chracter movement command mode
    public static bool commandMode;

    //Sprites for contextual wall placement
    Sprite[] wallSprites;
    Dictionary<string, Sprite> wallSpriteDict;

    //Character entities for rendering and data
    Character c;
    GameObject characterObj;

    Map map; // Current world map

    GameObject selectionTile; // Game object for selection display
    Text textbox; // Tile info text

    int mapWidth;
    int mapHeight;

    public int MapWidth { get => mapWidth; }
    public int MapHeight { get => mapHeight; }

    public Map Map { get => map; private set => map = value; }

    void Start()
    {
        mapWidth = 50;
        mapHeight = 50;
        commandMode = false;
        map = new Map(mapWidth, mapHeight);
        textbox = GameObject.Find("Text").GetComponent<Text>();

        c = new Character("Ivan", 0,0);
        characterObj = initObj("ivan", new Vector3(0.5f, 0.5f, -4), characterSprite);

        Camera.main.transform.position = new Vector3(mapWidth/2, mapHeight/2,-10);

        // Init selection game oject 
        selectionTile = initObj("Selection tile",new Vector3(0,0,0), selectionSprite);

        backgroundGroup = new GameObject();
        backgroundGroup.name = "Background tiles";

        tileGroup = new GameObject();
        tileGroup.name = "Foreground tiles";


        wallSprites = Resources.LoadAll<Sprite>("Textures/Wall");
        wallSpriteDict = new Dictionary<string, Sprite>();
        foreach (Sprite s in wallSprites)
        {
            //Debug.Log(s.name);
            wallSpriteDict[s.name] = s;
        }

        GameObject.Find("Button").GetComponent<Button>().onClick.AddListener(() => { changeMode(); });

        initTiles();

        map.randomizeTiles();
        //map.randomizeByWalking();
    }

    public void initTiles()
    {
        for (int i = 0; i < map.Width; i++)
        {
            for (int j = 0; j < map.Height; j++)
            {
                //Init background tile object 
                GameObject btile = initObj("background_tile_" + i + "_" + j,
                                            new Vector3(i + 0.5f, j + 0.5f, 5),
                                            backgroundSprite);
                btile.transform.SetParent(backgroundGroup.transform);

                // And then the tile itself
                GameObject tileObj = initObj("tile_" + i + "_" + j,
                                            new Vector3(i + 0.5f, j + 0.5f, 0),
                                            null);
                tileObj.transform.SetParent(tileGroup.transform);

                Tile tileData = map.getTile(i, j);
                // Init all tile game object positions with +0.5 offsets in both x and y directions 
                tileData.X = i;
                tileData.Y = j;
                // Set onchange callback
                tileData.registerCallback((tile) => { onTileTypeChanged(tile, tileObj); });
            }
        }

        Debug.Log("Created a map with " + map.Height * map.Width + " tiles");
    }

    int lastx;
    int lasty;

    // Update is called once per frame
    void Update()
    {
        renderPath(c);
        c.move();
        characterObj.transform.position = new Vector3(c.X+0.5f,c.Y+0.5f,-4);
        processSelection();
    }

    //Renders current path of a given character
    void renderPath(Character character)
    {
 
        if (!character.isMoving())
            return;

        Tuple<int, int>[] path = character.CurrentPath.ToArray();

        Color color = Color.blue;

        //Draw ray from current to next tile
        Vector3 start = new Vector3(character.X + 0.5f, character.Y + 0.5f, -8);
        Vector3 direction = new Vector3(character.Destx + 0.5f, character.Desty + 0.5f, -8);
        Debug.DrawLine(start, direction, color);

        //Draw from next point to the point on top of the stack
        if(path.Length!=0)
        {
            start = new Vector3(character.Destx + 0.5f, character.Desty + 0.5f, -8);
            direction = new Vector3(path[0].Item1 + 0.5f, path[0].Item2 + 0.5f, -8);
            Debug.DrawLine(start, direction, color);
        }

        for (int i = 0; i < path.Length - 1; i++)
        {
            start = new Vector3(path[i].Item1 + 0.5f, path[i].Item2 + 0.5f, -8);
            direction = new Vector3(path[i + 1].Item1 + 0.5f, path[i + 1].Item2 + 0.5f, -8);
            Debug.DrawLine(start, direction, color);
        }
    }

    public void processSelection()
    {
        if (CameraController.isMouseOverUI())
            return;

        Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int currentx = (int) mousepos.x;
        int currenty = (int) mousepos.y;

        bool inBoundary = currentx < mapWidth && currentx >= 0 && currenty < mapHeight && currenty >= 0;

        //Check if selected tile location has changed
        if ((lastx != currentx || lasty != currenty) && inBoundary && Input.touchCount != 2)
        {
            selectionTile.transform.position = new Vector3(currentx + 0.5f, currenty + 0.5f, -1);
            //Add text to the info panel
            textbox.text = map.getTile(currentx, currenty).Type.ToString();
            textbox.text += " " + currentx + " " + currenty;
            lastx = currentx;
            lasty = currenty;
        }

        //If in command mode and selection is performed
        if(Input.GetMouseButton(0) && commandMode)
        {
            c.commandMovementTo(lastx, lasty, map);
        }
    } 

    // Tile type update callback
    void onTileTypeChanged(Tile tile, GameObject tileObj)
    {
        if (tile.Type == Tile.TileType.Floor)
            tileObj.GetComponent<SpriteRenderer>().sprite = floorSprite;
        else if (tile.isWall())
        {
            int x = tile.X;
            int y = tile.Y;
            tileObj.GetComponent<SpriteRenderer>().sprite = getProperWallSprite(x, y);


            //Update neighbours
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    bool isCurrentOrDiagonal = i == j;

                    if (!isCurrentOrDiagonal && map.isBound(x + i, y + j) && map.getTile(x + i, y + j).isWall())
                    {
                        GameObject ntileobj = GameObject.Find("tile_" + (x + i) + "_" + (y + j));
                        ntileobj.GetComponent<SpriteRenderer>().sprite = getProperWallSprite(x + i, y + j);
                    }
                }
            }
        }
        else if (tile.Type == Tile.TileType.Empty)
            tileObj.GetComponent<SpriteRenderer>().sprite = null;
        else
            Debug.Log("Tile types are not matching default ones?!");
    }

    // Returns wall sprite that will suit the neighbours around given tile
    public Sprite getProperWallSprite(int x , int y)
    {

        string spritename = "stonewall";
        if (y + 1 < mapHeight && map.getTile(x,y+1).isWall())
            spritename += "_U";
        if (y - 1 >= 0 && map.getTile(x, y-1).isWall())
            spritename += "_D";
        if (x - 1 >= 0 && map.getTile(x-1, y).isWall())
            spritename += "_L";
        if (x + 1< mapWidth && map.getTile(x + 1, y).isWall())
            spritename += "_R";

        //Debug.Log(x+" "+ y+" "+spritename);

        return wallSpriteDict[spritename];

    }

    public GameObject initObj(string name, Vector3 position, Sprite sprite)
    {
        GameObject obj = new GameObject();
        SpriteRenderer spriter = obj.AddComponent<SpriteRenderer>();
        obj.name = name;
        obj.transform.position = position;
        spriter.sprite = sprite;
        return obj;
    }

    void changeMode()
    {
        if (commandMode)
        {
            GameObject.Find("Button").GetComponent<Button>().
                GetComponentInChildren<Text>().text = "Command mode";
            commandMode = false;
        }
        else
        {
            GameObject.Find("Button").GetComponent<Button>().
                GetComponentInChildren<Text>().text = "Movement mode";
            commandMode = true;
        }

    }
}
