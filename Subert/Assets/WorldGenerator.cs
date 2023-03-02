using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using Random = UnityEngine.Random;

public class WorldGenerator : MonoBehaviour
{
    public int resolution = 256;
    public int seed;
    private Texture2D texture;
    [SerializeField]
    private Octave[] noiseOctaves;
    [SerializeField]
    private Gradient colorGrad;

    void OnEnable()
    {
        texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
        texture.name = "CaveMap";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;
        //GetComponent<SpriteRenderer>().material.mainTexture = texture;
        GetComponent<MeshRenderer>().material.mainTexture = texture;
        Random.InitState(seed);
        FillTexture();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    float GenerateNoise(int x, int y, Octave[] octaves)
    {
        float result = 0;
        foreach(Octave o in octaves)
        {
            if (!o.noodleMode)
            {
                result += Mathf.Pow(Mathf.PerlinNoise(x*o.scale+seed, y*o.scale+seed), o.degree) * o.amplitude;
            } 
            else
            {
                result += Mathf.Pow(1 - Mathf.Abs(1 - Mathf.PerlinNoise(x * o.scale + seed, y * o.scale + seed)*2), o.degree) * o.amplitude;
            }
            
        }
        return result;
    }

    public void FillTexture()
    {
        if (texture.width != resolution)
        {
            texture.Reinitialize(resolution, resolution);
        }
        float stepSize = 1f / resolution;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                texture.SetPixel(x, y, colorGrad.Evaluate(GenerateNoise(x, y, noiseOctaves)));
            }
        }
        texture.Apply();
    }
    [Serializable]
    private class Octave
    {
        [Range(0.001f, 0.5f)]
        public float scale;
        [Range(-2f, 2f)]
        public float amplitude;
        [Range(1, 8)]
        public int degree;
        public bool noodleMode;
    }
}
