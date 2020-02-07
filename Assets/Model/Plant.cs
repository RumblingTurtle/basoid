using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Plant
{

    public enum PlantType { None, Tree, Bush, Grass};
    //Default tile types


    //Current tile type
    private PlantType type;
    private Tile.BiomeType biome;
        
    private int x, y;

    public int X { get => x; set => x = value; }
    public int Y { get => y; set => y = value; }
    public Tile.BiomeType Biome { get => biome; set => biome = value; }
    internal PlantType Type { get => type; set => type = value; }

    public static int typeCount()
    {
        return Enum.GetValues(typeof(PlantType)).Length;
    }

    public float getPlantEncoding()
    {
        int xoffset = (int) type;
        int yoffset = 0;
        if (Biome == Tile.BiomeType.Tundra)
            yoffset = 0;
        else if (Biome == Tile.BiomeType.Desert)
            yoffset = 1;
        if (Biome == Tile.BiomeType.Ice)
            yoffset = 2;

        return (xoffset + typeCount() * yoffset) / 255.0f;
    }


    public float getPlantIntEncoding()
    {
        int xoffset = (int)type;
        int yoffset = 0;
        if (Biome == Tile.BiomeType.Tundra)
            yoffset = 0;
        else if (Biome == Tile.BiomeType.Desert)
            yoffset = 1;
        if (Biome == Tile.BiomeType.Ice)
            yoffset = 2;

        return xoffset + typeCount() * yoffset;
    }
}