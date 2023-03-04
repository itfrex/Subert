using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubController : MonoBehaviour
{
    Camera cam;
    int[,] worldGen;
    int subX, subY;
    float tileSize = 1f;

    private Vector3 subPos;
    private Vector3 startSubPos;

    private Vector3 camStartPos;
    private Vector3 camEndPos;

    public float subLerpLength = 0.25f;
    public float camLerpLength = 0.4f;
    private float elapsedTimeCam;
    private float elapsedTime;

    [SerializeField]
    private AnimationCurve curve;

    // Start is called before the first frame update
    void Start()
    {
        camStartPos = new Vector3(transform.position.x, transform.position.y, -10);
        startSubPos = transform.position;
        cam = FindObjectOfType<Camera>();
        while(World.world.CheckCollision(subX, subY))
        {
            ForceMove(0, 1);
            Debug.Log("Inside a tile!!!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
    }

    private void LateUpdate()
    {
        if (elapsedTime > subLerpLength && elapsedTimeCam > camLerpLength)
            return;

        elapsedTimeCam += Time.deltaTime;
        elapsedTime += Time.deltaTime;

        float percentageComplete = elapsedTime / subLerpLength;
        float percentageCompleteCam = elapsedTimeCam / camLerpLength;

        cam.transform.position = Vector3.Lerp(camStartPos, camEndPos, percentageCompleteCam);
        transform.position = Vector3.Lerp(startSubPos, subPos, curve.Evaluate(percentageComplete));
    }

    void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.W))
            Move(0, 1);
        if (Input.GetKeyDown(KeyCode.A))
            Move(-1, 0);
        if (Input.GetKeyDown(KeyCode.S))
            Move(0, -1);
        if (Input.GetKeyDown(KeyCode.D))
            Move(1, 0);
    }
    void ForceMove(int Xdir, int Ydir)
    {
        subX += Xdir;
        subY += Ydir;
        subPos = new Vector3(subX * tileSize, subY * tileSize, 0);
        transform.position = subPos;
    }

    void Move(int Xdir, int Ydir)
    {
        if (World.world.CheckCollision(subX + Xdir, subY + Ydir)) return;
        startSubPos = transform.position;
        camStartPos = cam.transform.position;

        subX += Xdir;
        subY += Ydir;
        elapsedTime = 0;
        elapsedTimeCam = 0;

        subPos = new Vector3(subX * tileSize, subY * tileSize, 0);

        camEndPos = subPos - new Vector3(0, 0, 10);
        transform.position = subPos;
    }
}
