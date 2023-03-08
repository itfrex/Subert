using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFish
{
    bool IsPassive();
    string GetFishName();
    string GetFishType();
    void Turn();

}