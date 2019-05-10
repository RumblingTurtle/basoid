using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class MapController : MonoBehaviour
{
    public Sprite floorSprite; // Default floor sprite

    public Sprite selectionSprite; // Mouse selection sprite
    public Sprite backgroundSprite; // Background dirt sprite  
    public Sprite destSprite; // Default character sprite

    //Objects for tile groupings
    GameObject backgroundGroup;
    GameObject tileGroup;
    GameObject characterGroup;

    GameObject pathObj; //Current rendered path

    Character selectedCharacter;

    //Chracter movement command mode
    public static bool commandMode;
    public static bool destroyMode;

    //Sprites for contextual wall placement

    Dictionary<string, Sprite> wallSprites;
    Dictionary<string, Sprite> charSprites;
    Dictionary<string, Sprite> guiSprites;

    //Character entities for rendering and data
    List<Tuple<Character,GameObject>> characters;
    Queue<Job> availableJobs;
    List<Job> pendingJobs;

    Map map; // Current world map

    GameObject selectionTile; // Game object for selection display
    Text textbox; // Tile info text
    GameObject button;
    GameObject destroyButton;

    GameObject selectedCharacterInfo;

    int mapWidth;
    int mapHeight;

    public int MapWidth { get => mapWidth; }
    public int MapHeight { get => mapHeight; }

    public Map Map { get => map; private set => map = value; }

    void Start()
    {
        commandMode = false;
        destroyMode = false;

        mapWidth = 50;
        mapHeight = 50;

        map = new Map(mapWidth, mapHeight);
        availableJobs = new Queue<Job>();
        pendingJobs = new List<Job>();

        textbox = GameObject.Find("TileInfo").GetComponent<Text>();

        characters = new List<Tuple<Character, GameObject>>();

        pathObj = new GameObject();
        pathObj.name = "Path Renderer";
        LineRenderer pathRenderer = pathObj.AddComponent<LineRenderer>();

        pathRenderer.startWidth=0.2f;
        pathRenderer.endWidth=0.2f;

        selectionTile = initObj("Selection tile",new Vector3(0.5f,0.5f,-1), selectionSprite);

        backgroundGroup = new GameObject();
        backgroundGroup.name = "Background tiles";

        tileGroup = new GameObject();
        tileGroup.name = "Foreground tiles";

        characterGroup = new GameObject();
        characterGroup.name = "Characters";

        selectedCharacterInfo = GameObject.Find("CharacterInfo");
        selectedCharacterInfo.SetActive(false);

        wallSprites = getSpritesFromDir("Textures/Wall");
        charSprites = getSpritesFromDir("Textures/Characters");
        guiSprites = getSpritesFromDir("Textures/GUI");
        
        button = GameObject.Find("CommandModeButton");
        button.SetActive(false);
        button.GetComponent<Button>().onClick.AddListener(() => { changeMode("command"); });
        button.GetComponent<Image>().sprite = guiSprites["commandIconUnactive"];

        destroyButton = GameObject.Find("DestroyButton");
        destroyButton.GetComponent<Button>().onClick.AddListener(() => { changeMode("destroy"); });

        initTiles();

        addCharacter(new Character("Ivan", 0, 0),getRandomSprite(charSprites));
        addCharacter(new Character("Oleg", 2, 2), getRandomSprite(charSprites));

        map.randomizeTiles();
        //map.randomizeByWalking();
    }

    public void addCharacter(Character c, Sprite sprite)
    {
        GameObject characterObj = initObj(c.Name, new Vector3(c.X+0.5f, c.Y+0.5f, -4), sprite);
        characters.Add(new Tuple<Character, GameObject>(c,characterObj));
        characterObj.transform.SetParent(characterGroup.transform);
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
        moveAllCharacters();
        updateCharacterInfoPanel();
        processSelection();
        distributeJobs();
        performJobs();
    }

    Dictionary<string, Sprite> getSpritesFromDir(string dir)
    {
        Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>(dir);
        foreach (Sprite s in sprites)
        {
            //Debug.Log(s.name);
            dict[s.name] = s;
        }
        return dict;
    }

    void moveAllCharacters()
    {
        foreach(Tuple<Character,GameObject> ct in characters)
        {
            Character c = ct.Item1;
            GameObject cobj = ct.Item2;

            if(selectedCharacter==c)
                renderPath(c);

            c.move();
            cobj.transform.position = new Vector3(c.X + 0.5f, c.Y + 0.5f, -4);
        }
    }

    void performJobs()
    {
        foreach (Tuple<Character, GameObject> ct in characters)
        {
            Character c = ct.Item1;
            c.doJobs(map);
        }
    }

    void distributeJobs()
    {
        while (availableJobs.Count!=0)
        {
                Character c = characters[Random.Range(0, characters.Count)].Item1;
                Job currentJob = availableJobs.Dequeue();
                if (getPendingJob(currentJob.Tile)==null)
                {
                    c.addJob(currentJob);
                    pendingJobs.Add(currentJob);
                }
        }
    }

    Character getCharacterAt(int x, int y)
    {
        foreach (Tuple<Character, GameObject> ct in characters)
        {
            Character c = ct.Item1;
            if ((int)c.X == x && (int)c.Y == y)
                return c;
        }
        return null;
    }

    GameObject getCharacterObjAt(int x, int y)
    {
        foreach (Tuple<Character, GameObject> ct in characters)
        {
            Character c = ct.Item1;
            if ((int)c.X == x && (int)c.Y == y)
                return ct.Item2;
        }
        return null;
    }

    //Renders current path of a given character
    void renderPath(Character character)
    {
        LineRenderer pathRenderer = pathObj.GetComponent<LineRenderer>();

        if (!character.isMoving() || !commandMode)
        {
            pathRenderer.enabled = false;
            return;
        } else
            pathRenderer.enabled = true;



        Tuple<int, int>[] path = character.CurrentPath.ToArray();

        pathRenderer.positionCount = path.Length + 2;

        Color color = Color.blue;

        pathRenderer.SetPosition(0, new Vector3(character.X + 0.5f, character.Y + 0.5f, -2));
        pathRenderer.SetPosition(1, new Vector3(character.Destx+ 0.5f, character.Desty + 0.5f, -2));
        
        for (int i = 0; i < path.Length; i++)
            pathRenderer.SetPosition(i+2, new Vector3(path[i].Item1 + 0.5f, path[i].Item2 + 0.5f, -2));
    }

    public void processSelection()
    {
        if (CameraController.isMouseOverUI())
            return;

        Vector3 mousepos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        int currentx = (int) mousepos.x;
        int currenty = (int) mousepos.y;

        bool inBoundary = currentx < mapWidth && currentx >= 0 && currenty < mapHeight && currenty >= 0;

        Character c = getCharacterAt(currentx, currenty);

        bool characterIsSelected = selectedCharacter != null;

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

        //If selection is made not in command mode
        if (Input.GetMouseButton(0) && !commandMode && !CameraController.isMouseOverUI())
        {
            if (c != null)
            {
                selectedCharacter = c;
                selectedCharacterInfo.SetActive(true);
                button.SetActive(true);
            }
            else
            {
                selectedCharacterInfo.SetActive(false);
                selectedCharacter = null;
                button.SetActive(false);
            }


        }

        //If in command mode and selection is performed
        if (Input.GetMouseButton(0) && commandMode && characterIsSelected)
        {
            selectedCharacter.commandMovementTo(currentx, currenty, map);
        }


        if (destroyMode && Input.GetMouseButton(0) && !CameraController.isMouseOverUI() && map.isBound(currentx,currenty))
        {
            Tile currentTile = map.getTile(currentx, currenty);
            if (currentTile.isWall())
            {
                Job newDestroyJob = new Job(currentTile, 3.0f, 1.0f);
                newDestroyJob.registerJobDoneCallback(removeWall);
                
                if (!availableJobs.Contains(newDestroyJob) && !pendingJobs.Contains(newDestroyJob))
                {
                    availableJobs.Enqueue(newDestroyJob);
                }
            }

        }
    } 

    Job getPendingJob(Tile tile)
    {
        foreach (Job job in pendingJobs)
        {
            if (job.Tile.X == tile.X && job.Tile.Y == tile.Y)
                return job;
        }
        return null;
    }

    void removeWall(Tile tile)
    {
        Job job = getPendingJob(tile);
        if (job!=null)
            pendingJobs.Remove(job);

        tile.Type = Tile.TileType.Empty;
    }

    void updateCharacterInfoPanel()
    {
        if (selectedCharacterInfo.activeSelf == false)
            return;

        Text name = selectedCharacterInfo.transform.Find("CharacterName").GetComponent<Text>();
        Image picture = selectedCharacterInfo.transform.Find("CharacterImage").GetComponent<Image>();
        Text info = selectedCharacterInfo.transform.Find("InfoText").GetComponent<Text>();

        name.text = selectedCharacter.Name;
        info.text = "X: " + (int)selectedCharacter.X + " Y: " + (int)selectedCharacter.Y;
        info.text += "\nSpeed: " + selectedCharacter.speed;

        SpriteRenderer spriteRenderer = getCharacterObjAt((int)selectedCharacter.X, (int)selectedCharacter.Y).
            GetComponent<SpriteRenderer>();

        picture.sprite = spriteRenderer.sprite;
    }

    // Tile type update callback
    void onTileTypeChanged(Tile tile, GameObject tileObj)
    {
        int x = tile.X;
        int y = tile.Y;

        if (tile.Type == Tile.TileType.Floor)
            tileObj.GetComponent<SpriteRenderer>().sprite = floorSprite;
        else if (tile.isWall())
        {
            tileObj.GetComponent<SpriteRenderer>().sprite = getProperWallSprite(x, y);
        }
        else if (tile.Type == Tile.TileType.Empty)
            tileObj.GetComponent<SpriteRenderer>().sprite = null;
        else
            Debug.Log("Tile types are not matching default ones?!");

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

        return wallSprites[spritename];

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

    Sprite getRandomSprite(Dictionary<string, Sprite> sprites)
    {
        Sprite[] s = new Sprite[sprites.Count];
        sprites.Values.CopyTo(s, 0);

        int selectedSprite = UnityEngine.Random.Range(0, s.Length);

        return s[selectedSprite];
    }

    void changeMode(string mode)
    {

        if(mode=="command")
        {
            if (destroyMode)
                destroyMode = false;

            if (commandMode)
            {
                button.GetComponent<Image>().sprite = guiSprites["commandIconUnactive"];
                commandMode = false;
            }
            else
            {
                button.GetComponent<Image>().sprite = guiSprites["commandIconActive"];
                commandMode = true;
            }

        }
        else if(mode=="destroy")
        {
            button.GetComponent<Image>().sprite = guiSprites["commandIconUnactive"];
            commandMode = false;
            destroyMode = true;
        }

    }
}