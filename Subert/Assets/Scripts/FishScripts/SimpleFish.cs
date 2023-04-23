using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SimpleFish : MonoBehaviour, IFish
{
    const float MOVEMENT_TIME = 0.2f;

    float tileSize;

    public FishData Data { get; }
    [SerializeField]
    private MovementPattern movementPattern;
    private TileCollider col;
    private BSpline path;

    private Coroutine movementCoroutine;
    private float movementProgress;

    SubController sub;
    Vector2Int currSubPos;
    Vector2Int currFishPos;
    public void Start()
    {
        Initialize((int)transform.position.x, (int)transform.position.y);
        path = new BSpline(currFishPos);
    }

    public void Initialize(int x, int y)
    {
        currFishPos = new Vector2Int(x, y);

        sub = FindObjectOfType<SubController>();
        tileSize = sub.TILESIZE;

        TurnEventManager.current.TurnEvent += Turn;

        transform.position = new Vector3(x * tileSize, y * tileSize, 0);
        col = GetComponent<TileCollider>();
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
    public Vector2Int GetFishPosition()
    {
        return currFishPos;
    }
    public void Turn()
    {
        currSubPos = sub.GetSubPosition();
        if(movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            path.RemoveToEvalPoint(movementProgress);
        }
        UnsafeMove(movementPattern.Move(this));
        
    }

    void SafeMove(Vector2 direction)
    {
        if(World.world.CheckCollision(col, currFishPos.x + (int)direction.x, currFishPos.y + (int)direction.y))
        {
            currFishPos += Vector2Int.RoundToInt(direction);

            transform.position = new Vector3(currFishPos.x * tileSize, currFishPos.y * tileSize, 0);
        }
    }
    void UnsafeMove(List<Vector2> moves)
    {
        Vector2 newPos = currFishPos;
        foreach(Vector2 move in moves)
        {
            currFishPos += Vector2Int.RoundToInt(move);
            newPos += move;
            path.AddPoint(newPos);
        }
        movementCoroutine = StartCoroutine(FollowPath(MOVEMENT_TIME));
        col.UpdateCollider(currFishPos);
    }
    private IEnumerator FollowPath(float seconds)
    {
        float time = 0;
        while(time < seconds)
        {
            time += Time.deltaTime;
            movementProgress = time / seconds;
            transform.position = path.Evaluate(movementProgress);
            yield return new WaitForEndOfFrame();
        }
        path.RemoveToEvalPoint(1);
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
                Bounce,
                TurnAround
            }
            [SerializeField]
            public CollideAction collideAction;
            [SerializeField]
            public int damageOnCollide;
        }

        public List<Vector2> Move(SimpleFish fishObj, int offsetX = 0, int offsetY = 0)
        {
            List<Vector2> finalDeviation = new List<Vector2>();
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
                Vector2 dist = fishObj.currSubPos-new Vector2Int((int)fishObj.transform.position.x, (int)fishObj.transform.position.y)+new Vector2(offsetX,offsetY);

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
            if(World.world.CheckCollision(fishObj.col, (int)(fishObj.currFishPos + moveDir).x + offsetX, (int)(fishObj.currFishPos + moveDir).y + offsetY)){
                TileCollider collision = World.world.GetCollider((int)(fishObj.currFishPos + moveDir).x + offsetX, (int)(fishObj.currFishPos + moveDir).y + offsetY);
                if (moves[moveNumber].damageOnCollide > 0 && collision != null && collision.gameObject == fishObj.sub.gameObject)
                {
                    fishObj.sub.HitSub(moves[moveNumber].damageOnCollide);
                }
                switch (moves[moveNumber].collideAction)
                {
                    case MovementAction.CollideAction.DoNothing:
                        moveDir = Vector2.zero;
                        break;
                    case MovementAction.CollideAction.Bounce:
                        facingRight = !facingRight;
                        moveDir = -moveDir;
                        break;
                    case MovementAction.CollideAction.TurnAround:
                        facingRight = !facingRight;
                        moveDir = Vector2.zero;
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
            finalDeviation.Add(moveDir);
            if (toChain)
            {
                moveNumber = (moveNumber + 1) % moves.Length;
                finalDeviation.AddRange(Move(fishObj, (int)(offsetX + moveDir.x), (int)(offsetY + moveDir.y)));
            }
            else
            {
                moveNumber = (moveNumber + 1) % moves.Length;
            }
            return finalDeviation;
        }
    }
}
