using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileMesh : MonoBehaviour
{

    Mesh mesh;
    MeshFilter mf;
    MeshCollider mc;
    Dictionary<string, Sprite> tileAtlas;

    Mesh generateTile(int resolution)
    {
        int tileNum = resolution * resolution;
        int vertexNum = tileNum * 4;
        int indexNum = tileNum * 6;

        Vector3[] vertices = new Vector3[vertexNum];
        Vector2[] uvs = new Vector2[vertexNum];
        int[] indices = new int[indexNum];
        Color[] colors = new Color[vertexNum];

        //Init vertices for quads
        int vertOffset = 0; int idxOffset = 0;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                //Set vertex coordinates in a world space
                vertices[vertOffset + 0] = new Vector3(i, 0, j);
                vertices[vertOffset + 1] = new Vector3(i + 1.0f, 0, j);
                vertices[vertOffset + 2] = new Vector3(i + 1.0f, 0, j + 1.0f);
                vertices[vertOffset + 3] = new Vector3(i, 0, j + 1.0f);

                colors[vertOffset + 0] = new Color(0.0f, 1.0f, 0.0f);
                colors[vertOffset + 1] = new Color(0.0f, 1.0f, 0.0f);
                colors[vertOffset + 2] = new Color(0.0f, 1.0f, 0.0f);
                colors[vertOffset + 3] = new Color(0.0f, 1.0f, 0.0f);

                //Set uv coordinates per vertex in a normalized sprite coordinates
                uvs[vertOffset + 0] = new Vector2(0.0f, 0.0f);
                uvs[vertOffset + 1] = new Vector2(0.25f, 0.0f);
                uvs[vertOffset + 2] = new Vector2(0.25f, 0.25f);
                uvs[vertOffset + 3] = new Vector2(0.0f, 0.25f);

                //Set rendering indices
                indices[idxOffset + 0] = vertOffset + 0;
                indices[idxOffset + 1] = vertOffset + 2;
                indices[idxOffset + 2] = vertOffset + 1;
                indices[idxOffset + 3] = vertOffset + 0;
                indices[idxOffset + 4] = vertOffset + 3;
                indices[idxOffset + 5] = vertOffset + 2;

                vertOffset += 4; //Per 4 vertices
                idxOffset += 6; //2 triangles with 3 vertices each
            }
        }

        Mesh mesh = new Mesh
        {
            vertices = vertices,
            uv = uvs,
            colors = colors,
            triangles = indices
        };

        return mesh;
    }


    void buildMesh(int width, int height)
    {
        int tileNum = width * height;
        int vertexNum = tileNum * 4;
        int indexNum = tileNum * 6;

        Vector3[] vertices = new Vector3[vertexNum];
        Vector2[] uvs = new Vector2[vertexNum];
        int[] indices = new int[indexNum];
        Color[] colors = new Color[vertexNum];

        //Init vertices for quads
        int vertOffset = 0; int idxOffset = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                //Set vertex coordinates in a world space
                vertices[vertOffset + 0] = new Vector3(0, i, j);
                vertices[vertOffset + 1] = new Vector3(0, i + 1.0f, j);
                vertices[vertOffset + 2] = new Vector3(0, i + 1.0f, j + 1.0f);
                vertices[vertOffset + 3] = new Vector3(0, i, j + 1.0f);

                colors[vertOffset + 0] = new Color(0.0f, 0.0f, 0.0f);
                colors[vertOffset + 1] = new Color(0.0f, 1.0f, 0.0f);
                colors[vertOffset + 2] = new Color(0.0f, 0.0f, 1.0f);
                colors[vertOffset + 3] = new Color(1.0f, 0.0f, 0.0f);

                //Set uv coordinates per vertex in a normalized sprite coordinates
                uvs[vertOffset + 0] = new Vector2(0.0f, 0.0f);
                uvs[vertOffset + 1] = new Vector2(0.25f, 0.0f);
                uvs[vertOffset + 2] = new Vector2(0.25f, 0.25f);
                uvs[vertOffset + 3] = new Vector2(0.0f, 0.25f);

                //Set rendering indices
                indices[idxOffset + 0] = vertOffset + 0;
                indices[idxOffset + 1] = vertOffset + 2;
                indices[idxOffset + 2] = vertOffset + 1;
                indices[idxOffset + 3] = vertOffset + 0;
                indices[idxOffset + 4] = vertOffset + 3;
                indices[idxOffset + 5] = vertOffset + 2;

                vertOffset += 4; //Per 4 vertices
                idxOffset += 6; //2 triangles with 3 vertices each
            }
        }

        mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = indices;
    }

    public void setSprite(int x, int y, string name)
    {
        Sprite sprite = tileAtlas[name];
        Vector2[] uv = mf.mesh.uv;
        uv[4 * (x + y) + 0] = new Vector2(sprite.rect.xMin / 256, sprite.rect.yMin / 256);
        uv[4 * (x + y) + 1] = new Vector2(sprite.rect.xMax / 256, sprite.rect.yMin / 256);
        uv[4 * (x + y) + 2] = new Vector2(sprite.rect.xMax / 256, sprite.rect.yMax / 256);
        uv[4 * (x + y) + 3] = new Vector2(sprite.rect.xMin / 256, sprite.rect.yMax / 256);
        mf.mesh.uv = uv;

    }

    void loadTileAtlas()
    {
        Dictionary<string, Sprite> dict = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Textures/Wall");
        foreach (Sprite s in sprites)
        {
            //Debug.Log(s.name);
            dict[s.name] = s;
        }
        tileAtlas = dict;
    }

    // Start is called before the first frame update
    void Start()
    {
        mf = GetComponent<MeshFilter>();
        Mesh m = generateTile(10);
        mf.mesh = m;
        mc.sharedMesh = m;
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Transform>().position.Set(Mathf.Sin(Time.frameCount),0.0f, Mathf.Cos(Time.frameCount));

    }
}
