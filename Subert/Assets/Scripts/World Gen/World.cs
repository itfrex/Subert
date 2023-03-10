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
    private List<TileCollider> colliders;

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
        colliders = new List<TileCollider>();
    }
    public void AddCollider(TileCollider col)
    {
        colliders.Add(col);
    }
    public void RemoveCollider(TileCollider col)
    {
        colliders.Remove(col);
    }
    public float Generate(float posX, float posY)
    {
        float result = 0;
        foreach (Octave o in noiseOctaves)
        {
            result += Mathf.Pow(1 - Mathf.Abs(o.offset - Mathf.PerlinNoise(posX * o.scale + seed, posY * o.scale + seed)), o.degree) * o.amplitude;
            result = Mathf.Clamp01(result);
        }
        return result;
    }
    //Checks the tilemap if there is a tile at specified point
    public bool CheckTile(int posX, int posY)
    {
        float result = 0;
        foreach (Octave o in noiseOctaves)
        {
            result += Mathf.Pow(1 - Mathf.Abs(o.offset - Mathf.PerlinNoise(posX * o.scale + seed, posY * o.scale + seed)), o.degree) * o.amplitude;
            result = Mathf.Clamp01(result);
        }
        return 0 == Mathf.Round(result);
    }
    //Checks all colliders/the tilemap at a point (excluding the given object) and returns true if there is a collision there.
    public bool CheckCollision(TileCollider exclude, int posX, int posY)
    {
        float result = 0;
        foreach (Octave o in noiseOctaves)
        {
            result += Mathf.Pow(1 - Mathf.Abs(o.offset - Mathf.PerlinNoise(posX * o.scale + seed, posY * o.scale + seed)), o.degree) * o.amplitude;
            result = Mathf.Clamp01(result);
        }
        if(Mathf.Round(result) > 0)
        {
            foreach(TileCollider col in colliders)
            {
                if(col.CheckInBounds(posX, posY) && col != exclude)
                {
                    return true;
                }
            }
        }
        
        return 0 == Mathf.Round(result);
    }
    //Returns the TileCollider of the object found a point
    public TileCollider GetCollider(int posX, int posY)
    {
        foreach (TileCollider col in colliders)
        {
            if (col.CheckInBounds(posX, posY))
            {
                return col;
            }
        }
        return null;
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
