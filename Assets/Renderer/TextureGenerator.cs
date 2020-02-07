using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TextureGenerator : MonoBehaviour
{
    int tileTypes = 5;

    public static Texture2D genTexture(Map map)
    {
        int size = map.Height;
        Texture2D texture = new Texture2D(size+1, size+1, TextureFormat.RGBA32, true);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float colorCode = 0.0f;
                Tile.TileType type = map.getTile(i, j).Type;

                if (type == Tile.TileType.Floor)
                    colorCode = 2.0f / 255.0f;
                else if (type == Tile.TileType.Water)
                    colorCode = 3.0f / 255.0f;
                else if (type == Tile.TileType.Wall)
                    colorCode = 1.0f / 255.0f;
                else if (type == Tile.TileType.Empty)
                    colorCode = 0.0f / 255.0f;
                else
                {
                    Color pinky = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                    texture.SetPixel(i, j, pinky);
                    continue;
                }
                
                Color col = new Color(Random.Range(1,255) / 255.0f, Random.Range(1, 255) / 255.0f, colorCode , Random.Range(1, 255) / 255.0f);
                texture.SetPixel(i, j, col);
            }
        }

        texture.wrapMode = TextureWrapMode.Mirror;
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }

    public static Texture2D genTexture(int size)
    {
        Texture2D texture = new Texture2D(size+1, size+1, TextureFormat.RGBA32, true);

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                Color col = new Color(Random.Range(1, 255) / 255.0f, Random.Range(1, 255) / 255.0f, 2.0f/255.0f, Random.Range(1, 255) / 255.0f);
                texture.SetPixel(i, j, col);
            }
        }

        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return texture;
    }

    // Start is called before the first frame update
    void Start()
    {
        Map map = new Map(256,256);
        map.generateTilemapDiamondSquare(10);
        setupMap(map);
    }

    public void setupMap(Map map)
    {
        GetComponent<Renderer>().material.SetTexture("_MainTex", genTexture(map));
        GetComponent<Renderer>().material.SetInt("_TileTypeNum", tileTypes);
        GetComponent<Renderer>().material.SetFloat("_DataTexRes", map.Height);
        transform.localScale = new Vector3(map.Height/10, 1, map.Width/10);
        transform.position = new Vector3((map.Width - 8)/ 2, (map.Height-8) / 2, 0);
        transform.rotation = Quaternion.Euler(-90.0f,0.0f,0.0f);

    }

    // Update is called once per frame
    void Update()
    {
    }
}
