using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SimpleFish : MonoBehaviour, IFish
{
    float tileSize;

    public FishData Data { get; }
    [SerializeField]
    private MovementPattern movementPattern;

    public int xPos { get; private set; }
    public int yPos { get; private set; }

    SubController sub;
    Vector2 currSubPos;
    public void Start()
    {
        Initialize((int)transform.position.x, (int)transform.position.y);
    }

    public void Initialize(int x, int y)
    {
        xPos = x;
        yPos = y;

        sub = FindObjectOfType<SubController>();
        tileSize = sub.TILESIZE;

        TurnEventManager.current.TurnEvent += Turn;

        transform.position = new Vector3(xPos * tileSize, yPos * tileSize, 0);
    }
    public void Kill()
    {
        TurnEventManager.current.TurnEvent -= Turn;
    }
    public bool IsPassive()
    {
        return Data.passive;
    }
    public string GetFishName()
    {
        return Data.fishName;
    }
    public string GetFishType()
    {
        return Data.fishType;
    }
    public void Turn()
    {
        currSubPos = sub.GetSubPosition();
        UnsafeMove(movementPattern.Move(new Vector2(xPos, yPos), currSubPos));
    }

    void SafeMove(Vector2 direction)
    {
        if(World.world.CheckCollision(xPos + (int)direction.x, yPos + (int)direction.y))
        {
            xPos += (int)direction.x;
            yPos += (int)direction.y;

            transform.position = new Vector3(xPos * tileSize, yPos * tileSize, 0);
        }
    }
    void UnsafeMove(Vector2 direction)
    {
        xPos += (int)direction.x;
        yPos += (int)direction.y;

        transform.position = new Vector3(xPos * tileSize, yPos * tileSize, 0);
    }
    [Serializable]
    private class MovementPattern
    {
        [SerializeField]
        private MovementAction[] moves;
        private int moveNumber;
        private bool facingRight = true;
        [Serializable]
        public class MovementAction
        {
            public enum Direction
            {
                Forward,
                Backward,
                Upward,
                Downward,
                None
            }
            [SerializeField]
            public Direction direction;
            [SerializeField]
            public bool targetPlayer;
            [SerializeField]
            public bool chainNextMove;
            public bool cancelChainOnCollision;
            public enum CollideAction
            {
                DoNothing,
                Damage,
                Bounce,
                TurnAround
            }
            [SerializeField]
            public CollideAction collideAction;
        }

        public Vector2 Move(Vector2 fishPos, Vector2 subPos, float offsetX = 0, float offsetY = 0)
        {
            Vector2 finalDeviation = Vector2.zero;
            //set direction
            Vector2 moveDir = Vector2.zero;
            switch (moves[moveNumber].direction)
            {
                case MovementAction.Direction.Forward:
                    moveDir = Vector2.right;
                    break;
                case MovementAction.Direction.Backward:
                    moveDir = Vector2.left;
                    break;
                case MovementAction.Direction.Upward:
                    moveDir = Vector2.up;
                    break;
                case MovementAction.Direction.Downward:
                    moveDir = Vector2.down;
                    break;
                case MovementAction.Direction.None:
                    moveDir = Vector2.zero;
                    break;
            }
            //rotate direction so that fish moves towards player
            if (moves[moveNumber].targetPlayer)
            {
                Vector2 dist = subPos-fishPos+new Vector2(offsetX,offsetY);

                if (MathF.Abs(dist.x) > MathF.Abs(dist.y))
                {
                    moveDir = moveDir * (Vector2.right*dist.x).normalized.x;
                }
                else
                {
                    moveDir = new Vector2(moveDir.y, moveDir.x) * (Vector2.up*dist.y).normalized.y;
                }
            }
            //forwards is left if facing left
            if (!facingRight && moveDir.x != 0)
            {
                moveDir = -moveDir;
            }
            bool toChain = moves[moveNumber].chainNextMove;
            //check for collision and do action if true
            if(World.world.CheckCollision((int)(fishPos + moveDir).x, (int)(fishPos + moveDir).y)){
                switch (moves[moveNumber].collideAction)
                {
                    case MovementAction.CollideAction.DoNothing:
                        moveDir = Vector2.zero;
                        break;
                    case MovementAction.CollideAction.Bounce:
                        moveDir = -moveDir;
                        break;
                    case MovementAction.CollideAction.TurnAround:
                        facingRight = !facingRight;
                        moveDir = -moveDir;
                        break;                    
                }
                if (moves[moveNumber].cancelChainOnCollision)
                {
                    //skip all chained moves
                    while (moves[moveNumber].chainNextMove)
                    {
                        moveNumber = (moveNumber + 1) % moves.Length;
                    }
                    toChain = false;
                }
            }
            finalDeviation += moveDir;
            if (toChain)
            {
                moveNumber = (moveNumber + 1) % moves.Length;
                finalDeviation += Move(fishPos+moveDir, subPos, offsetX + moveDir.x, offsetY + moveDir.y);
            }
            else
            {
                moveNumber = (moveNumber + 1) % moves.Length;
            }
            return finalDeviation;
        }
    }
}
