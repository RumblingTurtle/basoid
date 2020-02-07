using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evaluator : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for(int i=0;i<10;i++)
        {   
            evalPerlin();
            evalCelluar();
            evalDiamondSquare();
        }
    }

    void evalPerlin(int size = 128)
    {
        Map map = new Map(size, size);

        System.DateTime startTime = System.DateTime.UtcNow;
        map.generateTilemapPerlin();
        System.TimeSpan timePassed = System.DateTime.UtcNow - startTime;

        List<List<Vector2Int>> emptyStructures = map.getStructures(Tile.TileType.Empty);
        List<List<Vector2Int>> wallStructures = map.getStructures(Tile.TileType.Wall);

        float areaRatio = (float)Map.structureTotalArea(emptyStructures) / (float)Map.structureTotalArea(wallStructures);
        float countRatio = (float)emptyStructures.Count / (float)wallStructures.Count;
        Debug.Log("Evaluating Perlin noise generation\n"+
            "Time passed " + timePassed.Milliseconds.ToString() + "ms\n"+
            "Cavity to formation area ratio " + areaRatio.ToString()+"\n"+
            "Cavity to formation count ratio " + countRatio.ToString());
    }

    void evalCelluar(int size = 128)
    {
        Map map = new Map(size, size);

        System.DateTime startTime = System.DateTime.UtcNow;
        map.generateTilemapCelluar(50, 1);
        System.TimeSpan timePassed = System.DateTime.UtcNow - startTime;

        List<List<Vector2Int>> emptyStructures = map.getStructures(Tile.TileType.Empty);
        List<List<Vector2Int>> wallStructures = map.getStructures(Tile.TileType.Wall);

        float areaRatio = (float)Map.structureTotalArea(emptyStructures) / (float)Map.structureTotalArea(wallStructures);
        float countRatio = (float)emptyStructures.Count / (float)wallStructures.Count;
        Debug.Log("Evaluating Celluar Automation generation\n" +
            "Time passed " + timePassed.Milliseconds.ToString() + "ms\n" +
            "Cavity to formation area ratio " + areaRatio.ToString() + "\n" +
            "Cavity to formation count ratio " + countRatio.ToString());
    }

    void evalDiamondSquare(int size = 128)
    {
        Map map = new Map(size, size);

        System.DateTime startTime = System.DateTime.UtcNow;
        map.generateTilemapDiamondSquare(10);
        System.TimeSpan timePassed = System.DateTime.UtcNow - startTime;

        List<List<Vector2Int>> emptyStructures = map.getStructures(Tile.TileType.Empty);
        List<List<Vector2Int>> wallStructures = map.getStructures(Tile.TileType.Wall);

        float areaRatio = (float)Map.structureTotalArea(emptyStructures) / (float)Map.structureTotalArea(wallStructures);
        float countRatio = (float)emptyStructures.Count / (float)wallStructures.Count;
        Debug.Log("Evaluating Diamond Square generation\n" +
            "Time passed " + timePassed.Milliseconds.ToString() + "ms\n" +
            "Cavity to formation area ratio " + areaRatio.ToString() + "\n" +
            "Cavity to formation count ratio " + countRatio.ToString());
    }
}
