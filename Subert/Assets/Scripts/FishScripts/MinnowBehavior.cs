using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinnowBehavior : MonoBehaviour, IFish
{
    int turnCount;
    float tileSize;

    bool passive = true;
    string fishName = "Minnow";
    string fishType = "Sonar";

    public int xPos;
    public int yPos;

    SubController sub;
    Vector2 currSubPos;

    private void Start()
    {
        xPos = -3;
        yPos = -3;

        

        sub = FindObjectOfType<SubController>();
        tileSize = sub.TILESIZE;
        currSubPos = sub.GetSubPosition();

        turnCount = 0;
        TurnEventManager.current.TurnEvent += Turn;

        transform.position = new Vector3(xPos * tileSize, yPos * tileSize, 0);
    }
    public bool IsPassive()
    {
        return passive;
    }

    public string GetFishName()
    {
        return fishName;
    }

    public string GetFishType()
    {
        return fishType;
    }

    public void Turn()
    {
        currSubPos = sub.GetSubPosition();
        turnCount += 1;
        Move();
    }

    void Move()
    {
        int moveX = 0;
        int moveY = 0;

        if (turnCount % 4 == 0)
            moveX = 1;
        else if (turnCount % 4 == 1)
            moveY = 1;
        else if (turnCount % 4 == 2)
            moveX = -1;
        else if (turnCount % 4 == 3)
            moveY = -1;

        if(World.world.CheckCollision(xPos + moveX, yPos + moveY))
        {
            moveX = 0;
            moveY = 0;
        }

        if(currSubPos == new Vector2(xPos + moveX, yPos + moveY))
        {
            moveX = 0;
            moveY = 0;
            sub.HitSub(2);
        }

        xPos += moveX;
        yPos += moveY;

        transform.position = new Vector3(xPos * tileSize, yPos * tileSize, 0);
    }
}
