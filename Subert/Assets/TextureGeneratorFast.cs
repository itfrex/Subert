using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureGeneratorFast : MonoBehaviour
{
    public int resolution = 512;
    private int pixelsPerTile = 32;
    private int maxVariance = 8;
    private float falloffAmt = 0.5f;
    int arraySize;
    private bool[,] cells;
    private int[,] corners;
    private Texture2D texture;
    void OnEnable()
    {
        texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
        texture.name = "CaveMap";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        GetComponent<MeshRenderer>().material.mainTexture = texture;
        arraySize = resolution / pixelsPerTile;
        corners = new int[arraySize + 1, arraySize + 1];
        cells = new bool[arraySize, arraySize];
        //FillSquare(0, 0);
        for(int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                float toDraw = 0;
                toDraw = 1 - Mathf.Round(World.world.Generate(i / pixelsPerTile, j / pixelsPerTile));
                toDraw += World.world.Generate(i / (float)pixelsPerTile, j / (float)pixelsPerTile) * falloffAmt;
                texture.SetPixel(i, j, Mathf.Clamp01(toDraw) * Color.white);
            }
        }
        texture.Apply();
    }
}
