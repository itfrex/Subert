using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSpawnManager : MonoBehaviour
{

    const int SPAWNCAP = 20; // max amount of spawned fish

    public SpawnBehavior[] fishList;
    private IFish[] spawnedFish;
    void Start()
    {
        spawnedFish = new IFish[SPAWNCAP];
        TurnEventManager.current.TurnEvent += Turn;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Turn()
    {

    }

    void Spawn()
    {

    }

    public IFish CheckFish(Vector2Int coordinates)
    {
        for (int i = 0; i < SPAWNCAP; i++)
        {
            if (spawnedFish[i] != null)
            {
                if (spawnedFish[i].GetFishPosition() == coordinates)
                {
                    return spawnedFish[i];
                }
            }
        }

        return null;
    }
}
