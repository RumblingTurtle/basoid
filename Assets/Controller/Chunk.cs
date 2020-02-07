using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

struct BiomeCenter
{
    public int x;
    public int y;
    public Tile.BiomeType biome;
    public Chunk chunk;
}

public class Chunk
{
    GameObject groupObj;
    GameObject terrainObj;
    GameObject plantObj;

    public Map map;

    int x;
    int y;

    BiomeCenter[,] biomes;

    internal BiomeCenter[,] Biomes { get => biomes; set => biomes = value; }

    public int Y { get => y; set => y = value; }
    public int X { get => x; set => x = value; }

    public Chunk(int width,int height,int x, int y)
    {
        X = x;
        Y = y;

        float namePostfix = Mathf.Floor(Random.value * 1000);


        groupObj = new GameObject();
        groupObj.name = "Chunk " + namePostfix;

        terrainObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        terrainObj.GetComponent<MeshFilter>().mesh = createPlane();
        terrainObj.name = "Terrain";

        plantObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plantObj.GetComponent<MeshFilter>().mesh = createPlane();
        plantObj.name = "Plants";

        terrainObj.transform.SetParent(groupObj.transform);
        plantObj.transform.SetParent(groupObj.transform);

        map = new Map(width, height);

        biomes = new BiomeCenter[2,2];

        for (int i = 0;i<2;i++)
        {
            for (int j = 0; j < 2; j++)
            {
                BiomeCenter newCenter;
                newCenter.x = Random.Range(i*map.Height/2,(i+1)*map.Height/2);
                newCenter.y = Random.Range(j * map.Height /2, (j + 1) * map.Height / 2);
                newCenter.biome = (Tile.BiomeType)Mathf.Floor(Random.Range(0.0f, 3.0f));
                newCenter.chunk = this;
                biomes[i, j] = newCenter;
            }
        }


        Renderer terrainRenderer = terrainObj.GetComponent<Renderer>();
        Renderer plantRenderer = plantObj.GetComponent<Renderer>();

        terrainRenderer.material = Resources.Load<Material>("Resources/mapper.mat");
        terrainRenderer.material.shader = Shader.Find("Custom/MapRenderer");

        plantRenderer.material = Resources.Load<Material>("Resources/plants.mat");
        plantRenderer.material.shader = Shader.Find("Custom/PlantRenderer");

        plantRenderer.material.SetFloat("_DataTexRes", height);
        terrainRenderer.material.SetFloat("_DataTexRes", height);

        terrainObj.transform.position = new Vector3(x * width, y * height, 0);
        terrainObj.transform.localScale = new Vector3(height, width,1);

        plantObj.transform.position = new Vector3(x * width, y * height, 0);
        plantObj.transform.localScale = new Vector3(height, width, 1);

        terrainObj.GetComponent<Renderer>().material.SetTexture("_DataTex", TextureGenerator.genTexture(width));
        plantObj.GetComponent<Renderer>().material.SetTexture("_DataTex", TextureGenerator.genTexture(width));

    }

    public void destroy()
    {
        GameObject.Destroy(terrainObj);
        GameObject.Destroy(plantObj);
        GameObject.Destroy(groupObj);
    }

    
    public void syncWithMap()
    {
        
        int texSize = map.Height+1;

        Material plantMaterial = plantObj.GetComponent<Renderer>().material;
        Material terrMaterial = terrainObj.GetComponent<Renderer>().material;

        Texture2D plantTexture = plantMaterial.GetTexture("_DataTex") as Texture2D;
        Texture2D terrTexture = terrMaterial.GetTexture("_DataTex") as Texture2D;

        Color[] newPlant = plantTexture.GetPixels();
        Color[] newTerrain = terrTexture.GetPixels();

        for (int i = 0; i < texSize-1; i++)
        {
            for (int j = 0; j < texSize-1; j++)
            {
                int index = j * texSize + i;

                Tile tile = map.getTile(i, j);
                Plant plant = map.getPlant(i, j);

                newTerrain[index].b = tile.getTileEncoding();
                newPlant[index].b = plant.getPlantEncoding();
                
            }
        }

        plantTexture.SetPixels(newPlant);
        terrTexture.SetPixels(newTerrain);

        plantTexture.Apply();
        terrTexture.Apply();
        
        terrMaterial.SetTexture("_DataTex", terrTexture);
        plantMaterial.SetTexture("_DataTex", plantTexture);
        
    }

    Texture2D bakeTexture(Material mat, int shadowResolution = 128)
    {
        RenderTexture renderTexture = new RenderTexture(shadowResolution, shadowResolution, 0, RenderTextureFormat.ARGB32); 
        
        renderTexture.filterMode = FilterMode.Point;
        Texture2D texture = new Texture2D(shadowResolution, shadowResolution, TextureFormat.RGBA32, false);
        RenderTexture.active = renderTexture;
        Graphics.Blit(renderTexture, mat);
        texture.ReadPixels(new Rect(Vector2.zero, new Vector2(shadowResolution, shadowResolution)), 0, 0, false);

        texture.wrapMode = TextureWrapMode.Mirror;
        texture.filterMode = FilterMode.Point;

        texture.Apply();

        RenderTexture.active = null;
        renderTexture.Release();
        return texture;
    }

    public void bakeShadows()
    {

        Texture2D bakedTerrain = bakeTexture(terrainObj.GetComponent<Renderer>().material);
        Texture2D plantsBaked = bakeTexture(plantObj.GetComponent<Renderer>().material);

        terrainObj.GetComponent<Renderer>().material.SetTexture("_BakedPlants", plantsBaked);
        terrainObj.GetComponent<Renderer>().material.SetTexture("_BakedTerrain", bakedTerrain);

        plantObj.GetComponent<Renderer>().material.SetFloat("_Baked", 1.0f);
        terrainObj.GetComponent<Renderer>().material.SetFloat("_Baked", 1.0f);
    }

    public void syncWithMap(int x,int y)
    {
        Tile tile = map.getTile(x, y);
        Plant plant = map.getPlant(x, y);
        setTileType(x,y, tile.Type, tile.Biome);
        setPlantType(x, y, plant.Type);
    }

    public Tuple<int,int> getTileWorldPos(int tX, int tY)
    {
        return new Tuple<int, int>(map.Height * X + tX, map.Height * Y + tY);
    }

    public void setTileType(int x, int y,Tile.TileType type, Tile.BiomeType biome)
    {
        Tile tile = map.getTile(x, y);
        tile.Type = type;
        tile.Biome = biome;

        Material material = terrainObj.GetComponent<Renderer>().material;

        Texture2D texture = material.GetTexture("_DataTex") as Texture2D;

        Color pixelValue = texture.GetPixel(x, y);

        pixelValue.b = tile.getTileEncoding();
        texture.SetPixel(x, y,pixelValue);
        texture.Apply();

        material.SetTexture("_DataTex", texture);
    }

    public void setPlantType(int x, int y, Plant.PlantType type)
    {
        Plant plant = map.getPlant(x, y);
        plant.Biome = map.getTile(x, y).Biome;
        plant.Type = type;

        Material material = plantObj.GetComponent<Renderer>().material;

        Texture2D texture = material.GetTexture("_DataTex") as Texture2D;

        Color pixelValue = texture.GetPixel(x, y);

        pixelValue.b = plant.getPlantEncoding();
        texture.SetPixel(x, y, pixelValue);
        texture.Apply();

        material.SetTexture("_DataTex", texture);
    }


    public static Mesh createPlane()
    {
        Mesh mesh = new Mesh();
        
        var vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };
        mesh.vertices = vertices;

        var tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        mesh.triangles = tris;

        var normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        var uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;

        return mesh;
    }
}
