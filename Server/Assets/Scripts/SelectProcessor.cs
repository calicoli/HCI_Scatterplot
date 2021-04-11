using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SelectProcessor : MonoBehaviour
{
    public GameObject ballController;
    public GameObject pointContainer;
    public GameObject renderProcessor;
    public Text pointText;
    public GameObject orthCamera;
    public GameObject frameThisDevice, frameOtherDevice;

    public GameObject testCube;

    private GameObject connectingPointsLine;
    private LineRenderer lrLine;
    private const float lineWidth = 0.1f;

    public GameObject tetraContainer;
    private GameObject[] triTetra = new GameObject[4];
    private MeshRenderer[] mrTetra = new MeshRenderer[4];
    private MeshFilter[] mfTetra = new MeshFilter[4];
    private GameObject edgeTetra;
    private LineRenderer lrEdgeTetra;
    private Bounds boundTetra;
    private int cntBallTetraSelect;
    private bool ballEnableInteractScript;

    public GameObject diamondContainer;
    private GameObject quadDiamond;
    private MeshRenderer mrDiamond;
    private MeshFilter mfDiamond;

    public GameObject objOrigin, objXEnd, objYEnd, objZEnd;
    private Vector3 posOrigin, posXEnd, posYEnd, posZEnd;
    private Vector3 upposSharedEdge, dnposSharedEdge,
                    upposThisFrameEdge, dnposThisFrameEdge,
                    upposOtherFrameEdge, dnposOtherFrameEdge;

    private Vector2 tpPrevTs1, tpPrevTs2, tpPrevTs3;

    // Start is called before the first frame update
    void Start()
    {
        resetPublicParams();
        updateAnchorPosition();
        // pointSelect
        initConnectingLine();
        // tetraSelect
        initTetra();
        cntBallTetraSelect = 0;
        ballEnableInteractScript = false;
        // diamSelect
        initDiamond();
        
    }

    // Update is called once per frame
    void Update()
    {
        updateAnchorPosition();
    }

    void updateAnchorPosition()
    {
        posOrigin = objOrigin.transform.position;
        posXEnd = objXEnd.transform.position;
        posYEnd = objYEnd.transform.position;
        posZEnd = objZEnd.transform.position;
        upposSharedEdge = frameOtherDevice.GetComponent<FrameController>().lineRenderer.GetPosition(3);
        dnposSharedEdge = frameOtherDevice.GetComponent<FrameController>().lineRenderer.GetPosition(0);
        upposOtherFrameEdge = frameOtherDevice.GetComponent<FrameController>().lineRenderer.GetPosition(2);
        dnposOtherFrameEdge = frameOtherDevice.GetComponent<FrameController>().lineRenderer.GetPosition(1);
        upposThisFrameEdge = frameThisDevice.GetComponent<FrameController>().lineRenderer.GetPosition(1);
        dnposThisFrameEdge = frameThisDevice.GetComponent<FrameController>().lineRenderer.GetPosition(0);
    }

    public void resetPublicParams()
    {
        tpPrevTs1 = tpPrevTs2 = tpPrevTs3 = Vector2.zero;
        pointText.text = null;
    }

    void initConnectingLine()
    {
        connectingPointsLine = new GameObject("ConnetingLine");
        connectingPointsLine.transform.SetParent(this.transform);
        lrLine = connectingPointsLine.AddComponent<LineRenderer>();
        lrLine.startWidth = lrLine.endWidth = lineWidth;
        //lrLine.useWorldSpace = false;
        lrLine.material = new Material(Shader.Find("Sprites/Default"));
        lrLine.material.color = new Color(1, 1, 1, 0.6f);
        lrLine.enabled = false;
    }

    void initTetra()
    {
        // tris
        for(int i = 0; i < 4; i++)
        {
            triTetra[i] = tetraContainer.transform.GetChild(i).gameObject;
            mrTetra[i] = triTetra[i].GetComponent<MeshRenderer>();
            mfTetra[i] = triTetra[i].GetComponent<MeshFilter>();

            renderProcessor.GetComponent<RenderProcessor>().initMeshRenderer(mrTetra[i]);
            mrTetra[i].material.color = new Color32(0, 0, 255, 100);
        }
        // edge
        edgeTetra = tetraContainer.transform.GetChild(4).gameObject;
        lrEdgeTetra = edgeTetra.GetComponent<LineRenderer>();
        renderProcessor.GetComponent<RenderProcessor>().initLineRenderer(lrEdgeTetra);
        lrEdgeTetra.material.color = new Color32(0, 0, 255, 100);
        lrEdgeTetra.startWidth = lrEdgeTetra.endWidth = 0.05f;

        // tetra
        tetraContainer.GetComponent<MeshRenderer>().enabled = false;
        tetraContainer.GetComponent<MeshRenderer>().material.color = new Color32(0, 0, 255, 100);
        RedrawTetra(posOrigin, posOrigin+Vector3.left, posOrigin + Vector3.back);
        int cntBall = ballController.GetComponent<BallController>().ballCount;
        for(int i=0; i< cntBall; i++)
        {
            pointContainer.transform.GetChild(i).GetComponent<BoundsIntersectExample>().
                meshCollider = tetraContainer.GetComponent<MeshCollider>();
        }
    }

    void initDiamond()
    {
        quadDiamond = diamondContainer.transform.GetChild(0).gameObject;
        mrDiamond = quadDiamond.GetComponent<MeshRenderer>();
        mfDiamond = quadDiamond.GetComponent<MeshFilter>();

        renderProcessor.GetComponent<RenderProcessor>().initMeshRenderer(mrDiamond);
        mrDiamond.material.color = new Color32(0, 0, 255, 100);
    }

    public void ProcessDiamondSelect(int cntServer, Vector2 tp1, Vector2 tp2, Vector2 tp3)
    {
        if (tp1 == tpPrevTs1 && tp2 == tpPrevTs2 && tp3 == tpPrevTs3)
        {
            return;
        }
        else
        {
            tpPrevTs1 = tp1;
            tpPrevTs2 = tp2;
            tpPrevTs3 = tp3;
        }
        Vector3 p1, p2, p3;
        if (cntServer == 1)
        {
            p1 = processServerTouchPoint(tp1);
            p2 = processClientTouchPoint(tp2);
            p3 = processClientTouchPoint(tp3);
        }
        else if (cntServer == 2)
        {
            p1 = processServerTouchPoint(tp1);
            p2 = processServerTouchPoint(tp2);
            p3 = processClientTouchPoint(tp3);
        }
        else if (cntServer == 3)
        {
            p1 = processServerTouchPoint(tp1);
            p2 = processServerTouchPoint(tp2);
            p3 = processServerTouchPoint(tp3);
        }
        else
        {
            p1 = p2 = p3 = Vector3.zero;
        }
        RedrawDiamond(cntServer, p1, p2, p3);
    }

    public void ProcessTetraSelect(int cntServer, Vector2 tp1, Vector2 tp2, Vector2 tp3)
    {
        if(tp1 == tpPrevTs1 && tp2 == tpPrevTs2 && tp3 == tpPrevTs3)
        {
            return;
        } else
        {
            tpPrevTs1 = tp1;
            tpPrevTs2 = tp2;
            tpPrevTs3 = tp3;
        }
        Vector3 p1, p2, p3;
        if(cntServer == 1)
        {
            p1 = processServerTouchPoint(tp1);
            p2 = processClientTouchPoint(tp2);
            p3 = processClientTouchPoint(tp3);
        }
        else if(cntServer == 2)
        {
            p1 = processServerTouchPoint(tp1);
            p2 = processServerTouchPoint(tp2);
            p3 = processClientTouchPoint(tp3);
        }
        else if(cntServer == 3)
        {
            p1 = processServerTouchPoint(tp1);
            p2 = processServerTouchPoint(tp2);
            p3 = processServerTouchPoint(tp3);
        }
        else
        {
            p1 = p2 = p3 = Vector3.zero;
        }
        RedrawTetra(p1, p2, p3);
        RedrawTetraEdge(p1, p2, p3);
        //DetectTetraIntersecting();
        if(!ballEnableInteractScript)
        {
            AddBoundsIntersectScriptonBall();
        }
        DetectTetraRayIntersecting();

    }

    void AddBoundsIntersectScriptonBall()
    {
        ballController.GetComponent<BallController>().UpdateInteractBallScript(true);
    }

    void DetectTetraRayIntersecting()
    {
        int cntBall = ballController.GetComponent<BallController>().ballCount;
        int cntInTetra = 0;
        for(int i = 0; i< cntBall; i++)
        {
            bool inTetra = pointContainer.transform.GetChild(i).GetComponent<BoundsIntersectExample>().In;
            bool valid;
            ballController.GetComponent<BallController>().UpdateTetraBallWithID(i, inTetra, out valid);
            if (valid)
            {
                cntInTetra++;
            } else
            {
                cntInTetra--;
            }
        }
        ballController.GetComponent<BallController>().UpdateBallVisulization();
        pointText.text = "Tetra select point number: " + cntBallTetraSelect;          
    }

    // abondon
    public void DetectTetraTrigger(bool enterTrigger, int idx) 
    {
        bool valid;
        if (enterTrigger)
        {
            ballController.GetComponent<BallController>().UpdateTetraBallWithID(idx, true, out valid);
            if(valid)
            {
                cntBallTetraSelect++;
            }
        } else
        {
            Debug.Log("exit following1 function");
            ballController.GetComponent<BallController>().UpdateTetraBallWithID(idx, false, out valid);
            cntBallTetraSelect--;
        }
        //ballController.GetComponent<BallController>().UpdateBallVisulization();
        pointText.text = "Tetra select point number: " + cntBallTetraSelect;
    }

    // abondon
    void DetectTetraIntersecting()
    {
        //boundTetra = tetraContainer.GetComponent<MeshFilter>().mesh.bounds;
        boundTetra = tetraContainer.GetComponent<MeshRenderer>().bounds;
        Bounds boundBall;
        int cntBall = ballController.GetComponent<BallController>().ballCount;
        bool isIntersecting;
        int cntTetraSelect = 0;
        bool cntValid = false;
        for (int idx = 0; idx < cntBall; idx++)
        {
            boundBall = ballController.GetComponent<BallController>().balls[idx].bound;
            //isIntersecting = boundBall.Intersects(boundTetra);
            isIntersecting = boundTetra.Intersects(boundBall);
            if(isIntersecting)
            {
                ballController.GetComponent<BallController>().UpdateTetraBallWithID(idx, true, out cntValid);
                cntTetraSelect = cntValid ? cntTetraSelect + 1 : cntTetraSelect;
            } else
            {
                ballController.GetComponent<BallController>().UpdateTetraBallWithID(idx, false, out cntValid);
            }
        }
        Debug.Log("dy4- ball number in tetraRange: " + cntTetraSelect);
        ballController.GetComponent<BallController>().UpdateBallVisulization();

        //Bounds boundCube = testCube.GetComponent<MeshFilter>().mesh.bounds;
        Bounds boundCube = testCube.GetComponent<MeshRenderer>().bounds;
        if (boundTetra.Intersects(boundCube)) {
            testCube.GetComponent<MeshRenderer>().material.color = Color.red;
        } else
        {
            testCube.GetComponent<MeshRenderer>().material.color = Color.white;
        }
    }


    void RedrawDiamond(int cntServer, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3 p4 = Vector3.zero;
        Vector3[] varr = new Vector3[4];
        // calculate the P4 position 
        if (cntServer == 1)
        {
            if(Mathf.Abs(p2.y - p1.y) < Mathf.Abs(p3.y - p1.y))
            {
                p4 = p3 + p1 - p2;
                Vector3[] temp = { p2, p1, p3, p4 }; varr = temp;
            } else
            {
                p4 = p2 + p1 - p3;
                Vector3[] temp = { p3, p2, p1, p4 }; varr = temp;
            }
        } else if(cntServer == 2)
        {
            if(Mathf.Abs(p1.y - p3.y) < Mathf.Abs(p2.y - p3.y))
            {
                p4 = p2 + p3 - p1;
                Vector3[] temp = { p1, p2, p3, p4 }; varr = temp;
            } else
            {
                p4 = p1 + p3 - p2;
                Vector3[] temp = { p2, p1, p3, p4 }; varr = temp;
            }
        } else if(cntServer == 3)
        {
            p4 = p2 + p3 - p1;
            Vector3[] temp = { p1, p2, p3, p4 }; varr = temp;
        }
        
        Mesh mesh = new Mesh();
        renderProcessor.GetComponent<RenderProcessor>().updateQuad(true, varr, out mesh);
        mesh.name = "diamond-quad";
        mfDiamond.mesh = mesh;
    }
    void RedrawTetraEdge(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3[] vars = {
            p1, p2, p3, p1,
            posOrigin, p2, posOrigin, p3
        };
        //renderProcessor.GetComponent<RenderProcessor>().updateLine(true, vars.Length, vars, out lrEdgeTetra);
        lrEdgeTetra.positionCount = vars.Length;
        for (int i = 0; i < vars.Length; i++)
        {
            lrEdgeTetra.SetPosition(i, vars[i]);
        }
    }

    void RedrawTetra(Vector3 p1, Vector3 p2, Vector3 p3)
    {
        Vector3[] varr0 = { posOrigin, p1, p2 };
        Vector3[] varr1 = { posOrigin, p1, p3 };
        Vector3[] varr2 = { posOrigin, p2, p3 };
        Vector3[] varr3 = { p1, p2, p3 };

        Mesh mesh = new Mesh();
        renderProcessor.GetComponent<RenderProcessor>().updateTri(true, varr0, out mesh);
        mesh.name = "tetra-tri1";
        mfTetra[0].mesh = mesh;
        renderProcessor.GetComponent<RenderProcessor>().updateTri(true, varr1, out mesh);
        mesh.name = "tetra-tri2";
        mfTetra[1].mesh = mesh;
        renderProcessor.GetComponent<RenderProcessor>().updateTri(true, varr2, out mesh);
        mesh.name = "tetra-tri3";
        mfTetra[2].mesh = mesh;
        renderProcessor.GetComponent<RenderProcessor>().updateTri(true, varr3, out mesh);
        mesh.name = "tetra-tri4";
        mfTetra[3].mesh = mesh;

        CombineInstance[] combineInstances = new CombineInstance[4];
        for (int i = 0; i < 4; i++) 
        {
            combineInstances[i].mesh = mfTetra[i].sharedMesh;                        //将共享mesh，赋值
            combineInstances[i].transform = mfTetra[i].transform.localToWorldMatrix; //本地坐标转矩阵，赋值
        }
        Mesh tetraMesh = new Mesh();
        tetraMesh.CombineMeshes(combineInstances);
        tetraMesh.name = "tetra";
        if(tetraMesh)
        {
            tetraContainer.GetComponent<MeshFilter>().mesh = tetraMesh;
            tetraContainer.GetComponent<MeshCollider>().sharedMesh = tetraMesh;
        }
    }

    public void showDiamond(bool flag)
    {
        diamondContainer.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = flag;
        if (!flag)
        {
            pointText.text = null;
        }
    }

    public void showTetra(bool flag)
    {
        /*
        for(int i = 0; i < 4; i++)
        {
            mrTetra[i].enabled = flag;
        }
        */
        lrEdgeTetra.enabled = flag;
        tetraContainer.GetComponent<MeshRenderer>().enabled = flag;
        if(!flag)
        {
            pointText.text = null;
        }
    }

    public void DetectRaycastOnBallwithNewRaycast(int num, Vector2 v1, Vector2 v2)
    {
        bool flagRenderLine = false;
        Vector3 vstart = Vector3.zero, vend = Vector3.zero;
        Vector3[] result = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        string ballInfo;
        Vector3 p1 = processServerTouchPoint(v1), p2 = processServerTouchPoint(v2);
        Vector3 posCamera = Camera.main.gameObject.transform.position;
        float cameraHeight = orthCamera.GetComponent<CameraController>().curCameraHeight;
        if (num == 1)
        {
            lrLine.enabled = false;
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(posCamera, p1 - posCamera, out hit))
            {
                GameObject target = hit.collider.gameObject;
                if (target && target.transform.parent.name == "PointContainer")
                {
                    int idx = System.Convert.ToInt32(target.name);
                    ballController.GetComponent<BallController>().UpdateBallWithID(idx, out ballInfo);
                    pointText.text = ballInfo;
                }
                Debug.Log("Hit1? YES: " + target.name);
            }
            Debug.DrawLine(
                posCamera,
                posCamera + cameraHeight * (p1 - posCamera),
                Color.green,
                0.01f,
                true
            );
        }
        else if (num == 2)
        {
            RaycastHit hit1, hit2;
            GameObject target1, target2;
            int idx1, idx2;
            if (Physics.Raycast(posCamera, p1 - posCamera, out hit1) &&
                Physics.Raycast(posCamera, p2 - posCamera, out hit2))
            {
                target1 = hit1.collider.gameObject;
                target2 = hit2.collider.gameObject;
                if (target1 && target1.transform.parent.name == "PointContainer" &&
                    target2 && target2.transform.parent.name == "PointContainer")
                {
                    idx1 = System.Convert.ToInt32(target1.name);
                    idx2 = System.Convert.ToInt32(target2.name);
                    ballController.GetComponent<BallController>().
                        UpdateBallWithIDs(idx1, idx2, out flagRenderLine, out vstart, out vend, out ballInfo);
                    pointText.text = ballInfo;
                }
            }
        }
        UpdateConnectingLine(flagRenderLine, vstart, vend);
    }

    public void DetectRaycastOnBallwithNativeRaycast(int num, Vector2 v1, Vector2 v2)
    {
        bool flagRenderLine = false;
        Vector3 vstart = Vector3.zero, vend = Vector3.zero;
        pointText.text = null;
        string ballInfo;
        if (num == 1)
        {
            lrLine.enabled = false;
            Ray ray = Camera.main.ScreenPointToRay(v1);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                GameObject target = hit.collider.gameObject;
                if (target && target.transform.parent.name == "PointContainer")
                {
                    int idx = System.Convert.ToInt32(target.name);
                    ballController.GetComponent<BallController>().UpdateBallWithID(idx, out ballInfo);
                    pointText.text = ballInfo;
                }
            }

        }
        else if (num == 2)
        {
            RaycastHit hit1, hit2;
            Ray ray1 = Camera.main.ScreenPointToRay(v1);
            Ray ray2 = Camera.main.ScreenPointToRay(v2);
            pointText.text = null;
            GameObject target1, target2;
            int idx1, idx2;
            if (Physics.Raycast(ray1, out hit1) && Physics.Raycast(ray2, out hit2))
            {
                target1 = hit1.collider.gameObject;
                target2 = hit2.collider.gameObject;
                if (target1 && target1.transform.parent.name == "PointContainer" &&
                    target2 && target2.transform.parent.name == "PointContainer")
                {
                    idx1 = System.Convert.ToInt32(target1.name);
                    idx2 = System.Convert.ToInt32(target2.name);
                    ballController.GetComponent<BallController>().
                        UpdateBallWithIDs(idx1, idx2, out flagRenderLine, out vstart, out vend, out ballInfo);
                    pointText.text = ballInfo;
                }
            }
        }
        UpdateConnectingLine(flagRenderLine, vstart, vend);
    }

    private Vector3 processServerTouchPoint(Vector2 v)
    {
        //float cameraOrthSize = orthCamera.GetComponent<CameraController>().curCameraOrthSize;
        float cameraHeight = orthCamera.GetComponent<CameraController>().curCameraHeight;
        Vector3 rv = Vector3.zero;
        rv.x = v.x - Screen.width / 2;
        rv.y = v.y - Screen.height / 2;
        rv.z = posOrigin.z;
        //Debug.Log("dy4 - v: " + v);
        //Debug.Log("dy4 - rv0: " + rv);
        //rv = rv * (cameraOrthSize / (Screen.height / 2));
        rv = rv * (cameraHeight / Screen.height);
        //Debug.Log("dy4 - rv1: " + rv);
        return rv;
    }

    private Vector3 processClientTouchPoint(Vector2 v)
    {
        Vector3 tv = processServerTouchPoint(v);
        Vector3 rv = Vector3.zero;
        rv.x = posOrigin.x;
        //rv.y = v.y - Screen.height / 2;
        //rv.z = posOrigin.z + v.x * (posZEnd.z - posOrigin.z) - Screen.width;
        rv.y = tv.y;
        rv.z = upposSharedEdge.z + (tv.x - upposThisFrameEdge.x);
        return rv;
    }

    public void UpdateConnectingLine(bool flag, Vector3 v1, Vector3 v2)
    {
        lrLine.enabled = flag;
        if (flag)
        {
            lrLine.SetPosition(0, v1);
            lrLine.SetPosition(1, v2);
        }
    }

}
