using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneratedMapRenderer : MonoBehaviour
{
    /// <summary>
    /// Marchings square algorithm
    /// give each point a random size, when computing add that size and check between the offset points to see if you place a pixel
    /// when moving, generate only new pixels by shifting the texture and then computing the new ungenerated pixels.
    /// only check empty tiles as full tiles with never have differeing shapes. therefore diagonal cases only ever use the unfilled case.
    /// </summary>
    public GameObject chunk;
    public SubController sub;
    public int resolution = 64;
    private int pixelsPerTile = 32;
    public int maxVariance = 8;
    public float roughness;
    int arraySize;
    private bool[][] cells;
    private bool[][] edges;
    private int[][] corners;

    private List<Vector2> generatedChunks;

    private void Start()
    {
        generatedChunks = new List<Vector2>();
        arraySize = resolution / pixelsPerTile;
        UpdateChunks(0, 0);
    }
    public void UpdateChunks(int posX, int posY)
    {
        Vector2 chunkPos = new Vector2((posX-arraySize/2) / arraySize, (posY-arraySize/2) / arraySize);
        for(int i = -1; i <=1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                bool needChunk = true;
                    foreach(Vector2 v in generatedChunks)
                {
                    if (v.Equals(new Vector2(chunkPos.x+i, chunkPos.y+j)))
                    {
                        needChunk = false;
                    }
                }
                if (needChunk)
                {
                    Instantiate(chunk, new Vector2((chunkPos.x+i)*arraySize + arraySize/2 - 0.5f, (chunkPos.y+j)*arraySize+ arraySize / 2 - 0.5f), Quaternion.identity, this.transform).GetComponent<MeshRenderer>().material.mainTexture = GenerateChunk((int)chunkPos.x+i, (int)chunkPos.y+j);
                    generatedChunks.Add(new Vector2(chunkPos.x + i, chunkPos.y + j));
                }
            }
        }
    }
    private Texture2D GenerateChunk(int chunkX, int chunkY)
    {
        generatedChunks.Add(new Vector2(chunkX, chunkY));
        Texture2D texture;
        texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true)
        {
            name = "CaveMap " + chunkX + "," + chunkY,
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Point
        };

        int noiseOffsetX = arraySize * chunkX;
        int noiseOffsetY = arraySize * chunkY;


        corners = new int[arraySize + 1][];
        for (int i = 0; i <= arraySize; i++)
        {
            corners[i] = new int[arraySize + 1];
        }
        edges = new bool[arraySize + 1][];
        for (int i = 0; i <= arraySize; i++)
        {
            edges[i] = new bool[2 * arraySize + 1];
        }
        cells = new bool[arraySize][];
        for (int i = 0; i < arraySize; i++)
        {
            cells[i] = new bool[arraySize];
        }
        FillCells();
        GenerateCorners();
        GenerateTexture();
        texture.Apply();
        return texture;

        void FillCells()
        {
            for (int y = 0; y < arraySize; y++)
            {
                for (int x = 0; x < arraySize; x++)
                {
                    cells[x][y] = World.world.CheckCollision(x + noiseOffsetX, y + noiseOffsetY);
                }
            }
        }
        void GenerateCorners()
        {
            for (int y = 0; y < arraySize; y++)
            {
                for (int x = 0; x < arraySize; x++)
                {
                    if (cells[x][y])
                    {
                        corners[x][y] = Mathf.RoundToInt(Mathf.PerlinNoise((x + noiseOffsetX) * roughness, (y + noiseOffsetY) * roughness) * maxVariance);
                        corners[x + 1][y] = Mathf.RoundToInt(Mathf.PerlinNoise((x + 1 + noiseOffsetX) * roughness, (y + noiseOffsetY) * roughness) * maxVariance);
                        corners[x][y + 1] = Mathf.RoundToInt(Mathf.PerlinNoise((x + noiseOffsetX) * roughness, (y + noiseOffsetY + 1) * roughness) * maxVariance);
                        corners[x + 1][y + 1] = Mathf.RoundToInt(Mathf.PerlinNoise((x + noiseOffsetX + 1) * roughness, (y + noiseOffsetY + 1) * roughness) * maxVariance);
                        edges[x][2 * y] = true;
                        edges[x][2 * y + 1] = true;
                        edges[x][2 * y + 2] = true;
                        edges[x + 1][2 * y + 1] = true;
                    }
                }
            }
        }
        void GenerateTexture()
        {
            for (int y = 0; y < arraySize; y++)
            {
                int texPosY = pixelsPerTile * y;
                for (int x = 0; x < arraySize; x++)
                {
                    int texPosX = pixelsPerTile * x;
                    if (cells[x][y])
                    {
                        FillSquare(texPosX, texPosY);
                    }
                    else
                    {
                        int numCorners = 0;
                        //Compare number of corners to know what case to use
                        if (corners[x][y] != 0) numCorners++;
                        if (corners[x + 1][y] != 0) numCorners++;
                        if (corners[x][y + 1] != 0) numCorners++;
                        if (corners[x + 1][y + 1] != 0) numCorners++;

                        if (numCorners == 0)
                        {
                            continue;
                        }

                        else if (numCorners == 1)
                        {
                            DrawCorners(texPosX, texPosY, x, y);
                        }

                        else if (numCorners == 2)
                        {
                            if (edges[x][2 * y])
                            {
                                DrawOneEdge(texPosX, texPosY, 2, corners[x][y], corners[x + 1][y]);
                            }
                            else if (edges[x][2 * y + 1])
                            {
                                DrawOneEdge(texPosX, texPosY, 1, corners[x][y], corners[x][y + 1]);
                            }
                            else if (edges[x][2 * y + 2])
                            {
                                DrawOneEdge(texPosX, texPosY, 0, corners[x][y + 1], corners[x + 1][y + 1]);
                            }
                            else if (edges[x + 1][2 * y + 1])
                            {
                                DrawOneEdge(texPosX, texPosY, 3, corners[x + 1][y], corners[x + 1][y + 1]);
                            }
                            else
                            {
                                DrawCorners(texPosX, texPosY, x, y);
                            }
                        }
                        else if (numCorners == 3)
                        {
                            //DL
                            if (edges[x][2 * y] && edges[x][2 * y + 1])
                            {
                                DrawCurve(texPosX, texPosY, 0, 0, corners[x][y + 1], corners[x + 1][y]);
                            }
                            //DR
                            else if (edges[x][2 * y] && edges[x + 1][2 * y + 1])
                            {
                                DrawCurve(texPosX, texPosY, pixelsPerTile - 1, 0, corners[x + 1][y + 1], corners[x][y]);
                            }
                            //UL
                            else if (edges[x][2 * y + 2] && edges[x][2 * y + 1])
                            {
                                DrawCurve(texPosX, texPosY, 0, pixelsPerTile - 1, corners[x][y], corners[x + 1][y + 1]);
                            }
                            //UR
                            else if (edges[x][2 * y + 2] && edges[x + 1][2 * y + 1])
                            {
                                DrawCurve(texPosX, texPosY, pixelsPerTile - 1, pixelsPerTile - 1, corners[x + 1][y], corners[x][y + 1]);
                            }
                            //D
                            else if (edges[x][2 * y])
                            {
                                DrawOneEdge(texPosX, texPosY, 2, corners[x][y], corners[x + 1][y]);
                                DrawCorners(texPosX, texPosY, x, y);
                            }
                            //L
                            else if (edges[x][2 * y + 1])
                            {
                                DrawOneEdge(texPosX, texPosY, 1, corners[x][y], corners[x][y + 1]);
                                DrawCorners(texPosX, texPosY, x, y);
                            }
                            //U
                            else if (edges[x][2 * y + 2])
                            {
                                DrawOneEdge(texPosX, texPosY, 0, corners[x][y + 1], corners[x + 1][y + 1]);
                                DrawCorners(texPosX, texPosY, x, y);
                            }
                            //R
                            else if (edges[x + 1][2 * y + 1])
                            {
                                DrawOneEdge(texPosX, texPosY, 3, corners[x + 1][y], corners[x + 1][y + 1]);
                                DrawCorners(texPosX, texPosY, x, y);
                            }
                            else
                            {
                                DrawCorners(texPosX, texPosY, x, y);
                            }

                        }
                        else if (numCorners == 4)
                        {
                            //d edges[x][ 2 * y]
                            //l edges[x][ 2 * y + 1]
                            //u edges[x][ 2 * y + 2]
                            //r edges[x+1][ 2 * y + 1]
                            //All
                            if (edges[x][2 * y + 2] && edges[x + 1][2 * y + 1] && edges[x][2 * y] && edges[x][2 * y + 1])
                            {
                                DrawCurve(texPosX, texPosY, 0, pixelsPerTile - 1, corners[x][y], corners[x + 1][y + 1]);
                                DrawCurve(texPosX, texPosY, pixelsPerTile - 1, pixelsPerTile - 1, corners[x + 1][y], corners[x][y + 1]);
                            }
                            //UD
                            else if (edges[x][2 * y + 2] && edges[x][2 * y])
                            {
                                //OR
                                if (edges[x][2 * y + 1])
                                {
                                    DrawCurve(texPosX, texPosY, 0, 0, corners[x][y + 1], corners[x + 1][y]);
                                    DrawCurve(texPosX, texPosY, 0, pixelsPerTile - 1, corners[x][y], corners[x + 1][y + 1]);
                                }
                                //OL
                                else if (edges[x + 1][2 * y + 1])
                                {
                                    DrawCurve(texPosX, texPosY, pixelsPerTile - 1, 0, corners[x][y], corners[x][y + 1]);
                                    DrawCurve(texPosX, texPosY, pixelsPerTile - 1, pixelsPerTile - 1, corners[x + 1][y], corners[x][y + 1]);
                                }
                                //OLOR
                                else
                                {
                                    DrawOneEdge(texPosX, texPosY, 0, corners[x][y + 1], corners[x + 1][y + 1]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                    DrawOneEdge(texPosX, texPosY, 2, corners[x][y], corners[x + 1][y]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                }
                            }
                            //LR
                            else if (edges[x][2 * y + 1] && edges[x + 1][2 * y + 1])
                            {
                                //OU
                                if (edges[x][2 * y + 2])
                                {
                                    DrawCurve(texPosX, texPosY, 0, pixelsPerTile - 1, corners[x][y], corners[x + 1][y + 1]);
                                    DrawCurve(texPosX, texPosY, pixelsPerTile - 1, pixelsPerTile - 1, corners[x + 1][y], corners[x][y + 1]);
                                }
                                //OD
                                else if (edges[x][2 * y])
                                {
                                    DrawCurve(texPosX, texPosY, 0, 0, corners[x][y + 1], corners[x + 1][y]);
                                    DrawCurve(texPosX, texPosY, pixelsPerTile - 1, 0, corners[x + 1][y + 1], corners[x][y]);
                                }
                                //OUOD
                                else
                                {
                                    DrawOneEdge(texPosX, texPosY, 1, corners[x][y], corners[x][y + 1]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                    DrawOneEdge(texPosX, texPosY, 3, corners[x + 1][y], corners[x + 1][y + 1]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                }
                            }
                            //d edges[x][ 2 * y]
                            //l edges[x][ 2 * y + 1]
                            //u edges[x][ 2 * y + 2]
                            //r edges[x+1][ 2 * y + 1]

                            //D
                            else if (edges[x][2 * y])
                            {
                                //DL
                                if (edges[x][2 * y + 1])
                                {
                                    DrawCurve(texPosX, texPosY, 0, 0, corners[x][y + 1], corners[x + 1][y]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                }

                                //DR
                                else if (edges[x + 1][2 * y + 1])
                                {
                                    DrawCurve(texPosX, texPosY, pixelsPerTile - 1, 0, corners[x + 1][y + 1], corners[x][y]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                }
                                //D
                                else
                                {
                                    DrawOneEdge(texPosX, texPosY, 0, corners[x][y + 1], corners[x + 1][y + 1]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                }
                            }
                            //U
                            else if (edges[x][2 * y + 2])
                            {
                                //UL
                                if (edges[x][2 * y + 1])
                                {
                                    DrawCurve(texPosX, texPosY, 0, pixelsPerTile - 1, corners[x][y], corners[x + 1][y + 1]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                }
                                //UR
                                else if (edges[x + 1][2 * y + 1])
                                {
                                    DrawCurve(texPosX, texPosY, pixelsPerTile - 1, pixelsPerTile - 1, corners[x + 1][y], corners[x][y + 1]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                }
                                //U
                                else
                                {
                                    DrawOneEdge(texPosX, texPosY, 2, corners[x][y], corners[x + 1][y]);
                                    DrawCorners(texPosX, texPosY, x, y);
                                }
                            }
                            //L
                            else if (edges[x][2 * y + 1])
                            {
                                DrawOneEdge(texPosX, texPosY, 1, corners[x][y], corners[x][y + 1]);
                                DrawCorners(texPosX, texPosY, x, y);
                            }
                            //R
                            else if (edges[x + 1][2 * y + 1])
                            {
                                DrawOneEdge(texPosX, texPosY, 2, corners[x][y], corners[x + 1][y]);
                                DrawCorners(texPosX, texPosY, x, y);
                            }
                            else
                            {
                                DrawCorners(texPosX, texPosY, x, y);
                            }
                        }
                    }
                }
            }
        }
        void FillSquare(int offsetX, int offsetY)
        {
            int upperX = offsetX + pixelsPerTile;
            int upperY = offsetY + pixelsPerTile;
            for (int i = offsetY; i < upperY; i++)
            {
                for (int j = offsetX; j < upperX; j++)
                {
                    texture.SetPixel(j, i, Color.black);
                }
            }
        }
        void DrawCorners(int offSetX, int offSetY, int cellPosX, int cellPosY)
        {
            int r = corners[cellPosX][cellPosY];
            int ox = offSetX, oy = offSetY;
            if (r != 0)
            {
                for (int x = r; x >= 0; x--)
                {
                    for (int y = (int)Mathf.Sqrt(r * r - x * x); y >= 0; y--)
                    {
                        texture.SetPixel(x + ox, y + oy, Color.black);
                    }
                }
            }
            r = corners[cellPosX + 1][cellPosY];
            ox += pixelsPerTile - 1;
            if (r != 0)
            {
                for (int x = -r; x <= 0; x++)
                {
                    for (int y = (int)Mathf.Sqrt(r * r - x * x); y >= 0; y--)
                    {
                        texture.SetPixel(x + ox, y + oy, Color.black);
                    }
                }
            }
            r = corners[cellPosX][cellPosY + 1];
            ox = offSetX;
            oy += pixelsPerTile - 1;
            if (r != 0)
            {
                for (int x = r; x >= 0; x--)
                {
                    for (int y = -(int)Mathf.Sqrt(r * r - x * x); y <= 0; y++)
                    {
                        texture.SetPixel(x + ox, y + oy, Color.black);
                    }
                }
            }
            r = corners[cellPosX + 1][cellPosY + 1];
            ox += pixelsPerTile - 1;
            if (r != 0)
            {
                for (int x = -r; x <= 0; x++)
                {
                    for (int y = -(int)Mathf.Sqrt(r * r - x * x); y <= 0; y++)
                    {
                        texture.SetPixel(x + ox, y + oy, Color.black);
                    }
                }
            }
        }
        void DrawOneEdge(int offsetX, int offsetY, int direction, int d1, int d2)
        {
            //up
            if (direction == 0)
            {
                for (int x = 0; x < pixelsPerTile; x++)
                {
                    int difference = d2 - d1;
                    float slope = (float)difference / (float)pixelsPerTile;
                    for (int y = 0; y <= d1 + x * slope; y++)
                    {
                        texture.SetPixel(offsetX + x, offsetY + pixelsPerTile - 1 - y, Color.black);
                    }
                }
            }
            //left
            else if (direction == 1)
            {
                for (int y = 0; y < pixelsPerTile; y++)
                {
                    int difference = d2 - d1;
                    float slope = (float)difference / (float)pixelsPerTile;
                    for (int x = 0; x <= d1 + y * slope; x++)
                    {
                        texture.SetPixel(offsetX + x, y + offsetY, Color.black);
                    }
                }
            }
            //down
            else if (direction == 2)
            {
                for (int x = 0; x < pixelsPerTile; x++)
                {
                    int difference = d2 - d1;
                    float slope = (float)difference / (float)pixelsPerTile;
                    for (int y = 0; y <= d1 + x * slope; y++)
                    {
                        texture.SetPixel(offsetX + x, offsetY + y, Color.black);
                    }
                }
            }
            //right
            else if (direction == 3)
            {
                for (int y = 0; y < pixelsPerTile; y++)
                {
                    int difference = d2 - d1;
                    float slope = (float)difference / (float)pixelsPerTile;
                    for (int x = 0; x <= d1 + y * slope; x++)
                    {
                        texture.SetPixel(offsetX + pixelsPerTile - 1 - x, y + offsetY, Color.black);
                    }
                }
            }
        }
        void DrawCurve(int offsetX, int offsetY, int focusX, int focusY, int d1, int d2)
        {
            int d1diff = (int)Mathf.Pow(pixelsPerTile - d1, 4);
            int d2diff = (int)Mathf.Pow(pixelsPerTile - d2, 4);

            for (int y = 0; y < pixelsPerTile; y++)
            {
                for (int x = 0; Mathf.Pow(pixelsPerTile - x, 4) / d1diff + Mathf.Pow(pixelsPerTile - y, 4) / d2diff >= 1 && x < pixelsPerTile; x++)
                {
                    texture.SetPixel(offsetX + Mathf.Abs(focusX - x), Mathf.Abs(focusY - y) + offsetY, Color.black);
                }
            }
        }
        
    }
}
