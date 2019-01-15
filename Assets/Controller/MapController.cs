using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MapController : MonoBehaviour
{
    public Sprite floorSprite; // Default floor sprite

    public Sprite selectionSprite; // Mouse selection sprite
    public Sprite backgroundSprite;
    Sprite[] wallSprites;
    Dictionary<string, Sprite> wallSpriteDict;

    public Map map; // Current world map

    GameObject selectionTile; // Game object for selection display
    Text textbox;

    int mapWidth;
    int mapHeight;

    public int MapWidth { get => mapWidth; }
    public int MapHeight { get => mapHeight; }

    void Start()
    {
        mapWidth = 50;
        mapHeight = 50;

        Camera.main.transform.position = new Vector3(mapWidth/2, mapHeight/2,-10);
        // Init map
        map = new Map(mapWidth,mapHeight);
        textbox = GameObject.Find("Text").GetComponent<Text>();

        // Init selection game oject 
        selectionTile = new GameObject();
        selectionTile.name = "Selection tile";
        // Init sprite renderer for a selection tile
        SpriteRenderer selectionSpriter = selectionTile.AddComponent<SpriteRenderer>();
        // Assign specified sprite in the object to renderer
        selectionSpriter.sprite = selectionSprite;

        wallSprites = Resources.LoadAll<Sprite>("Textures/Wall");
        wallSpriteDict = new Dictionary<string, Sprite>();
        foreach(Sprite s in wallSprites)
        {
            Debug.Log(s.name);
            wallSpriteDict[s.name] = s;
        }


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
                GameObject btile = new GameObject();
                btile.name = "background_tile_" + i + "_" + j;
                btile.transform.position = new Vector3(i + 0.5f, j + 0.5f, 5);
                SpriteRenderer bspriter = btile.AddComponent<SpriteRenderer>();
                bspriter.sprite = backgroundSprite;

                // And then the tile itself
                GameObject tileObj = new GameObject();
                SpriteRenderer spriter = tileObj.AddComponent<SpriteRenderer>();
                Tile tileData = map.getTile(i, j);


                // Init all tile game object positions with +0.5 offsets in both x and y directions 
                tileObj.transform.position = new Vector3(i + 0.5f, j + 0.5f, 0);
                tileData.X = i;
                tileData.Y = j;
                tileObj.name = "tile_" + i + "_" + j;
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
        processSelection();
    }

    public void processSelection()
    {
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

    }

    // Tile type update callback
    void onTileTypeChanged(Tile tile, GameObject tileObj)
    {
        if (tile.Type == Tile.TileType.Floor)
            tileObj.GetComponent<SpriteRenderer>().sprite = floorSprite;
        else if (tile.Type == Tile.TileType.Wall)
        {
            int x = tile.X;
            int y = tile.Y;
            tileObj.GetComponent<SpriteRenderer>().sprite = getProperWallSprite(x, y);

            //Update neighbours
            if (y + 1 < mapHeight && map.getTile(x, y + 1).isWall())
            {
                GameObject ntileobj = GameObject.Find("tile_" + x + "_" + (y + 1));
                ntileobj.GetComponent<SpriteRenderer>().sprite = getProperWallSprite(x, y + 1);
            }   
            if (y - 1 >= 0 && map.getTile(x, y - 1).isWall())
            {
                GameObject ntileobj = GameObject.Find("tile_" + x + "_" + (y - 1));
                ntileobj.GetComponent<SpriteRenderer>().sprite = getProperWallSprite(x, y - 1);
            }
            if (x - 1 >= 0 && map.getTile(x - 1, y).isWall())
            {
                GameObject ntileobj = GameObject.Find("tile_" + (x-1) + "_" + y);
                ntileobj.GetComponent<SpriteRenderer>().sprite = getProperWallSprite(x-1, y);
            }
            if (x + 1 < mapWidth && map.getTile(x + 1, y).isWall())
            {
                GameObject ntileobj = GameObject.Find("tile_" + (x+1) + "_" + y);
                ntileobj.GetComponent<SpriteRenderer>().sprite = getProperWallSprite(x+1, y);
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

        Debug.Log(x+" "+ y+" "+spritename);

        return wallSpriteDict[spritename];

    }
}
