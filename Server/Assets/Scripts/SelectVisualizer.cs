using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectVisualizer : MonoBehaviour
{
    public GameObject touchProcessor;
    public GameObject ballController;
    public GameObject orthCamera;

    private GameObject[] objs = new GameObject[12];
    private LineRenderer[] lrs = new LineRenderer[12];
    private GameObject lineContainer;
    private LineRenderer lr = new LineRenderer();
    private const float lineWidth = 0.04f;

    [HideInInspector]
    public bool isSelectingInServer;
    [HideInInspector]
    public bool isSelectingInClient;
    private Vector3 pos1;
    private Vector3 pos2;
    [HideInInspector]
    public float selectOffset, selectDepth;
    private float defaultZ, frontZ, backZ;
    private float minBallZ;

    //private float timer = -1;
    private const float tolerance = 0.1f;

    private Camera cam;
    private float camWidth;
    private float camHeight;

    [HideInInspector]
    public float minX, minY, minZ,
                 maxX, maxY, maxZ;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        camHeight = cam.orthographicSize;
        camWidth = camHeight * cam.aspect;

        defaultZ = 2.66f;
        selectOffset = 0f;
        selectDepth = 0f;

        for (int i = 0; i < lrs.Length; i++)
        {
            objs[i] = new GameObject("Edge");
            objs[i].transform.SetParent(this.transform);

            lrs[i] = objs[i].AddComponent<LineRenderer>();
            lrs[i].startWidth = lineWidth;
            lrs[i].useWorldSpace = false;
            lrs[i].material = new Material(Shader.Find("Sprites/Default"));
            lrs[i].material.color = Color.white;
        }

        isSelectingInServer = false;
        isSelectingInClient = false;
    }

    // Update is called once per frame
    void Update()
    {
        //defaultZ = 2.66f;
        //defaultZ = orthCamera.GetComponent<CameraController>().curCameraNearClipPlane;
        if (isSelectingInServer)
        {
            for (int i = 0; i < 4; i++)
            {
                lrs[i].enabled = true;
            }
            for (int i = 4; i < lrs.Length; i++)
            {
                lrs[i].enabled = isSelectingInClient;
            }
            pos1 = touchProcessor.GetComponent<TouchProcessor>().vertex1;
            pos2 = touchProcessor.GetComponent<TouchProcessor>().vertex2;
            

            RedrawRangeAndBall();
        } else
        {
            for (int i = 0; i < lrs.Length; i++)
            {
                lrs[i].enabled = false;
            }
        }
        //Debug.Log("DefaultZ: " + defaultZ);
        //isSelecting = (timer > 0);
        //timer -= Time.deltaTime;
    }

    private void RedrawRangeAndBall()
    {
        if (isSelectingInServer && !isSelectingInClient)
        {
            minX = pos1.x < pos2.x ? pos1.x : pos2.x;
            maxX = pos1.x > pos2.x ? pos1.x : pos2.x;
            minY = pos1.y < pos2.y ? pos1.y : pos2.y;
            maxY = pos1.y > pos2.y ? pos1.y : pos2.y;

            //Debug.Log("Range(minX, maxX): (" + minX + ", " + maxX + ")");
            //Debug.Log("Range(minY, maxY): (" + minY + ", " + maxY + ")");

            RedrawRect();
            ColorBallInRect();
        }
        else if(isSelectingInServer && isSelectingInClient)
        {
            minX = pos1.x < pos2.x ? pos1.x : pos2.x;
            maxX = pos1.x > pos2.x ? pos1.x : pos2.x;
            minY = pos1.y < pos2.y ? pos1.y : pos2.y;
            maxY = pos1.y > pos2.y ? pos1.y : pos2.y;

            frontZ = (defaultZ + selectOffset < 0f) ? 0f : (defaultZ + selectOffset);
            backZ  = frontZ + selectDepth;
            minZ = frontZ;
            maxZ = backZ;

            RedrawCuboid();
            ColorBallInCuboid();
        }
        
    }

    private void RedrawRect()
    {
        Vector3[] fourVec = {
                new Vector3(pos1.x, pos1.y, defaultZ),
                new Vector3(pos1.x, pos2.y, defaultZ),
                new Vector3(pos2.x, pos2.y, defaultZ),
                new Vector3(pos2.x, pos1.y, defaultZ),
            };
        lrs[0].SetPosition(0, fourVec[0]);
        lrs[0].SetPosition(1, fourVec[1]);
        lrs[1].SetPosition(0, fourVec[1]);
        lrs[1].SetPosition(1, fourVec[2]);
        lrs[2].SetPosition(0, fourVec[2]);
        lrs[2].SetPosition(1, fourVec[3]);
        lrs[3].SetPosition(0, fourVec[3]);
        lrs[3].SetPosition(1, fourVec[0]);
    }
    
    private void RedrawCuboid()
    {
        Vector3[] eightVec = {
                new Vector3(pos1.x, pos1.y, frontZ),
                new Vector3(pos1.x, pos2.y, frontZ),
                new Vector3(pos2.x, pos2.y, frontZ),
                new Vector3(pos2.x, pos1.y, frontZ),
                new Vector3(pos1.x, pos1.y, backZ),
                new Vector3(pos1.x, pos2.y, backZ),
                new Vector3(pos2.x, pos2.y, backZ),
                new Vector3(pos2.x, pos1.y, backZ)
            };
        lrs[0].SetPosition(0, eightVec[0]);
        lrs[0].SetPosition(1, eightVec[1]);
        lrs[1].SetPosition(0, eightVec[1]);
        lrs[1].SetPosition(1, eightVec[2]);
        lrs[2].SetPosition(0, eightVec[2]);
        lrs[2].SetPosition(1, eightVec[3]);
        lrs[3].SetPosition(0, eightVec[3]);
        lrs[3].SetPosition(1, eightVec[0]);
        lrs[4].SetPosition(0, eightVec[4]);
        lrs[4].SetPosition(1, eightVec[5]);
        lrs[5].SetPosition(0, eightVec[5]);
        lrs[5].SetPosition(1, eightVec[6]);
        lrs[6].SetPosition(0, eightVec[6]);
        lrs[6].SetPosition(1, eightVec[7]);
        lrs[7].SetPosition(0, eightVec[7]);
        lrs[7].SetPosition(1, eightVec[4]);
        lrs[8].SetPosition(0, eightVec[0]);
        lrs[8].SetPosition(1, eightVec[4]);
        lrs[9].SetPosition(0, eightVec[1]);
        lrs[9].SetPosition(1, eightVec[5]);
        lrs[10].SetPosition(0, eightVec[2]);
        lrs[10].SetPosition(1, eightVec[6]);
        lrs[11].SetPosition(0, eightVec[3]);
        lrs[11].SetPosition(1, eightVec[7]);
    }

    private void ColorBallInRect()
    {
        ballController.GetComponent<BallController>().UpdateBallWithRect(minX, maxX, minY, maxY);
    }

    private void ColorBallInCuboid()
    {
        ballController.GetComponent<BallController>().UpdateBallWithCuboid(minX, maxX, minY, maxY, minZ, maxZ);
    }

    public void UpdateMinZ(float m)
    {
        defaultZ = (m > 0f) ? m : 0f;
    }

    public void select()
    {
        isSelectingInServer = true;
        //timer = tolerance;
    }

    public void unselect()
    {
        defaultZ = 2.66f;
        isSelectingInServer = false;
    }

}
