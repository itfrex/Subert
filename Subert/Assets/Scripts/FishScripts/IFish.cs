using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFish
{
    bool IsPassive();
    string GetFishName();
    string GetFishType();

    Vector2Int GetFishPosition();
    void Turn();

}
