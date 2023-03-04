using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class World : MonoBehaviour
{
    private const int RENDER_DISTANCE = 50;
    public static World world;
    [SerializeField]
    private Octave[] noiseOctaves;
    [SerializeField]
    private int seed;
    [SerializeField]
    private GameObject spriteObj;

    void Awake()
    {
        if(world != null)
        {
            Destroy(world);
        }
        else
        {
            world = this;
        }
        DontDestroyOnLoad(this);
        for(int x = 0; x < RENDER_DISTANCE; x++) 
        {
            for(int y = 0; y< RENDER_DISTANCE; y++)
            {
                Generate(x, y);
            }
        }
    }
    float Generate(int posX, int posY)
    {
        float result = 0;
        foreach (Octave o in noiseOctaves)
        {
            result += Mathf.Pow(1 - Mathf.Abs(o.offset - Mathf.PerlinNoise(posX * o.scale + seed, posY * o.scale + seed)), o.degree) * o.amplitude;
            result = Mathf.Clamp01(result);
        }
        if (Mathf.Round(result) == 0)
        {
            GameObject sprite = Instantiate(spriteObj, new Vector3(posX, posY, 0), Quaternion.identity);
        }

        return result;
    }
    public bool CheckCollision(int posX, int posY)
    {
        float result = 0;
        foreach (Octave o in noiseOctaves)
        {
            result += Mathf.Pow(1 - Mathf.Abs(o.offset - Mathf.PerlinNoise(posX * o.scale + seed, posY * o.scale + seed)), o.degree) * o.amplitude;
            result = Mathf.Clamp01(result);
        }
        return 0 == Mathf.Round(result);
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
        [Range(0, .5f)]
        public float offset;
    }
}
