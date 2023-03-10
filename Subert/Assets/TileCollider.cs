using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileCollider : MonoBehaviour
{
    [SerializeField]
    private int xSize = 1;
    [SerializeField]
    private int ySize = 1;
    public int xPos { get; private set; }
    public int yPos { get; private set; }

    public bool CheckInBounds(int x, int y)
    {
        xPos = (int)transform.position.x;
        yPos = (int)transform.position.y;
        return (xPos <= x && x < xPos + xSize && yPos <= y && y < yPos + ySize);
    }
    private void Start()
    {
        World.world.AddCollider(this);
        xPos = (int)transform.position.x;
        yPos = (int)transform.position.y;
    }
    private void OnDisable()
    {
        World.world.RemoveCollider(this);
    }
}
