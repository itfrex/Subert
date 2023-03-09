using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SpawnBehavior
{
    public GameObject fishPrefab;

    // must be between 0 and 1
    public float rarity;
    
    // width and height REQUIRED for a fish to spawn, aka how big room has to be
    public int width;
    public int height;
}
