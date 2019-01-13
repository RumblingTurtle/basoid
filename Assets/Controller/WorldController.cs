using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class WorldController : MonoBehaviour
{
    public Sprite floorSprite; // Default floor sprite
    public Sprite selectionSprite; // Mouse selection sprite

    public Map map; // Current world map

    GameObject selectionTile; // Game object for selection display
    Text textbox;

    int mapWidth;
    int mapHeight;

    void Start()
    {
        mapHeight = 50;
        mapWidth = 50;
        Camera.main.transform.position = new Vector3(mapWidth/2, mapHeight/2,-10);
        // Init map
        map = new Map(mapWidth,mapHeight);
        textbox = GameObject.Find("Text").GetComponent<Text>();

       // Init selection game oject 
       selectionTile = new GameObject();
        // Init sprite renderer for a selection tile
        SpriteRenderer selectionSpriter = selectionTile.AddComponent<SpriteRenderer>();
        // Assign specified sprite in the object to renderer
        selectionSpriter.sprite = selectionSprite;

        for (int i = 0; i < map.Width; i++)
        {
            for (int j = 0; j < map.Height; j++)
            {
                GameObject tileObj = new GameObject();
                SpriteRenderer spriter = tileObj.AddComponent<SpriteRenderer>();
                Tile tileData = map.getTile(i, j);


                // Init all tile game object positions with +0.5 offsets in both x and y directions 
                tileObj.transform.position = new Vector3(i+0.5f, j+0.5f, 0);

                tileObj.name = "tile_" + i + "_" + j;
                // Set onchange callback
                tileData.registerCallback((tile) => { onTileTypeChanged(tile, tileObj); });
            }
        }

        map.randomizeTiles();
        Debug.Log("Created a map with "+ map.Height*map.Width+" tiles");
    }

    int lastx;
    int lasty;

    // Update is called once per frame
    void Update()
    {
        Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int currentx = (int) mousepos.x;
        int currenty = (int) mousepos.y;

        if (lastx != currentx || lasty != currenty)
        {
            selectionTile.transform.position = new Vector3(currentx + 0.5f, currenty + 0.5f, -1);
        }

        textbox.text = map.getTile(currentx, currenty).Type.ToString();
        textbox.text += " " + currentx + " " + currenty;
        lastx = currentx;
        lasty = currenty;
    }

    void onTileTypeChanged(Tile tile, GameObject tileObj)
    {
        if (tile.Type == Tile.TileType.Floor)
            tileObj.GetComponent<SpriteRenderer>().sprite = floorSprite;
        else if (tile.Type == Tile.TileType.Empty)
            tileObj.GetComponent<SpriteRenderer>().sprite = null;
        else
            Debug.Log("Tile types are not matching default ones?!");
    }
}
