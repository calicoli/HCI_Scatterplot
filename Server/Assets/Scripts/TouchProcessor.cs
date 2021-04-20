using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchProcessor : MonoBehaviour
{
    public Text debugText;

	public GameObject sender;
	public GameObject obj;
    public Text modeText;
    public GameObject cancelSelectButton;
    public GameObject cancelFilterButton;
    public GameObject cancelAidshapeButton;
    public GameObject xSlider, ySlider;

    public GameObject orthCamera;

    public GameObject ballController;
    public GameObject selectVisualizer;
    public GameObject filterProcessor;
    public GameObject selectProcessor;
    public Text pointText;
    public GameObject snapProcessor;

    private NaviPhase nPhase;
    private float sendTimer = -1;

    [HideInInspector]
    public char charMode;
    private Mode currentMode;
    [HideInInspector]
    public bool existSelection;
    [HideInInspector]
    public bool isFilteringInServer, isFilteringInClient;
    private float minFilterInClient, maxFilterInClient;
    [HideInInspector]
    public bool infoResetZSlider;
    [HideInInspector]
    public Vector3 vertex1, vertex2;
    [HideInInspector]
    public int cntSelectingInServer, cntSelectingInClient;
    [HideInInspector]
    public Vector2 tpSelectInClient1, tpSelectInClient2;

    [HideInInspector]
	public bool isPanning, isRotating, isLocked;

	private float panTimer, rotateTimer;
    private float panDelay = 0.1f;

	[HideInInspector]
	public Vector3 pos;
    [HideInInspector]
    public Quaternion rot;
    [HideInInspector]
    public Vector3 sca;
	private Vector3 defaultPos;
    private Quaternion defaultRot;
    private Vector3 defaultScale;
	[HideInInspector]
	public float angle;
 
	private const float panRatio = 1;
	private const float minPanDistance = 0;
    private Vector3 panDelta;

    private const float rotateRatio = 0.1f;
    private const float minRotateAngle = 0f;
    private Vector3 rotateDelta;

    private Camera cameraMain;
    private float cameraOrthSize;
    private float cameraWidth, cameraHeight;

    private float screenWidth, screenHeight;

    private enum NaviPhase
    {
        rotate,
        move
    }

    public enum Mode
    {
        navigate,
        filter1,
        filter2,
        selectR,
        selectP,
        selectT,
        selectD,
        selectA,
        selectF,
        focus
    }

    void Start()
	{
        angle = - Mathf.PI / 2;

        screenWidth = Screen.width;
        screenHeight = Screen.height;
        //Debug.Log("ScreenHeight: " + screenHeight + "; ScreenWidth: " + screenWidth);

        pos = defaultPos = new Vector3(0, 0, 0);
        //rot = defaultRot = Quaternion.Euler(Vector3.zero);
        rot = defaultRot = obj.transform.Find("Scatterplot").transform.localRotation;
        sca = defaultScale = obj.transform.Find("Scatterplot").transform.localScale;

        isLocked = false;
        isPanning = false;
        isRotating = false;

        modeText.text = "Mode: Navigation";
        pointText.text = null;

        charMode = 'n';
        currentMode = Mode.navigate;
        existSelection = false;
        isFilteringInServer = isFilteringInClient = false;
        infoResetZSlider = false;
        cntSelectingInServer = cntSelectingInClient = 0;
    }

	// Update is called once per frame
	void Update()
	{
		if(charMode == 'n' && currentMode == Mode.navigate)
        {
            calculate();
            freemoving();

            if (sendTimer < 0)
            {
                sender.GetComponent<ServerController>().sendMessage();
                sendTimer = 0.05f;
            }
            else
            {
                sendTimer -= Time.deltaTime;
            }
            sca = obj.transform.Find("Scatterplot").transform.localScale;
            isLocked = isPanning || isRotating;
        }
        else if(charMode == '1' && currentMode == Mode.filter1)
        {
            detectFilter1();
            sender.GetComponent<ServerController>().sendMessage();
        }
        else if (charMode == '2' && currentMode == Mode.filter2)
        {
            detectFilter2();
            sender.GetComponent<ServerController>().sendMessage();
            //debugText.text = "filtering s/c: " + isFilteringInServer.ToString() + "/" + isFilteringInClient.ToString();
        }
        else if (charMode == 'r' && currentMode == Mode.selectR)
        {
            //detectSelect();

        }
        else if (charMode == 'p' && currentMode == Mode.selectP)
        {
            detectPointSelect();
            //detectRenderedSelect();

        }
        else if (charMode == 't' && currentMode == Mode.selectT)
        {
            // here tetra must be a convex-trigger;
            detectTetraSelect();

        }
        else if (charMode == 'a' && currentMode == Mode.selectA)
        {
            detectAngleTetraSelect();

        }
        else if (charMode == 'd' && currentMode == Mode.selectD)
        {
            detectDiamondSelect();

        }
        else if (charMode == 'f' && currentMode == Mode.selectF)
        {
            // here tetra must be a non-convex;
            detectFaceSelect();
        }
        else if (charMode == 'o' && currentMode == Mode.focus)
        {
            snapProcessor.GetComponent<SnapProcessor>().focus();
        }
    }

    void detectFaceSelect()
    {
        selectProcessor.GetComponent<SelectProcessor>().updateTetraConvexProperty(false);
        if (Input.GetMouseButtonDown(0))
        {
            snapProcessor.GetComponent<SnapProcessor>().
                ProcessFaceSelect(Input.mousePosition);
        } else if(Input.touchCount == 1)
        {
            snapProcessor.GetComponent<SnapProcessor>().
                ProcessFaceSelect(Input.touches[0].position);
        }
        
    }

    #region DiamondSelection
    void detectDiamondSelect()
    {
        cntSelectingInServer = Input.touchCount;
        //cntSelectingInClient = 3 - cntSelectingInServer;
        if (cntSelectingInServer > 0
            && cntSelectingInServer + cntSelectingInClient == 3)
        {
            if (cntSelectingInServer == 1)
            {
                Touch touch = Input.touches[0];
                if (touch.position.y > 400 && touch.position.y < screenHeight - 400)
                {
                    selectProcessor.GetComponent<SelectProcessor>().
                        ProcessDiamondSelect(1,
                        touch.position,
                        tpSelectInClient1,
                        tpSelectInClient2);
                    selectProcessor.GetComponent<SelectProcessor>().showDiamond(true);
                }
            }
            else if (cntSelectingInServer == 2)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                if (touch1.position.y > 400 && touch1.position.y < screenHeight - 400 &&
                   touch2.position.y > 400 && touch2.position.y < screenHeight - 400)
                {
                    selectProcessor.GetComponent<SelectProcessor>().
                        ProcessDiamondSelect(2,
                        touch1.position,
                        touch2.position,
                        tpSelectInClient1);
                    selectProcessor.GetComponent<SelectProcessor>().showDiamond(true);
                }

            }
            else if (cntSelectingInServer == 3)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                Touch touch3 = Input.touches[2];
                if (touch1.position.y > 400 && touch2.position.y > 400 && touch3.position.y > 400)
                {
                    selectProcessor.GetComponent<SelectProcessor>().
                        ProcessDiamondSelect(3, touch1.position, touch2.position, touch3.position);
                    selectProcessor.GetComponent<SelectProcessor>().showDiamond(true);
                }

            }

        }
    }
    #endregion

    #region TetraSelection
    void detectTetraSelect()
    {
        selectProcessor.GetComponent<SelectProcessor>().updateTetraConvexProperty(true);
        cntSelectingInServer = Input.touchCount;
        //cntTetraSelectingInClient = 3 - cntTetraSelectingInServer;
        if(cntSelectingInServer > 0
            && cntSelectingInServer + cntSelectingInClient == 3)
        {
            if(cntSelectingInServer == 1)
            {
                Touch touch = Input.touches[0];
                if (touch.position.y > 400 && touch.position.y < screenHeight - 400)
                {
                    selectProcessor.GetComponent<SelectProcessor>().
                        ProcessTetraSelect(1,
                        touch.position,
                        tpSelectInClient1,
                        tpSelectInClient2);
                        //new Vector2(screenWidth / 3, 2 * screenHeight / 3), 
                        //new Vector2(3 * screenWidth / 4, screenHeight / 2));

                    selectProcessor.GetComponent<SelectProcessor>().showTetra(true);
                }
            }
            else if(cntSelectingInServer == 2)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                if(touch1.position.y > 400 && touch1.position.y < screenHeight - 400 &&
                   touch2.position.y > 400 && touch2.position.y < screenHeight - 400)
                {
                    selectProcessor.GetComponent<SelectProcessor>().
                        ProcessTetraSelect(2,
                        touch1.position,
                        touch2.position,
                        tpSelectInClient1);
                        //new Vector2(screenWidth - 150, screenHeight / 2 - 50));
                    selectProcessor.GetComponent<SelectProcessor>().showTetra(true);
                }
                
            }
            // only test
            else if(cntSelectingInServer == 3)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                Touch touch3 = Input.touches[2];
                if (touch1.position.y > 400 && touch2.position.y > 400 && touch3.position.y > 400)
                {
                    selectProcessor.GetComponent<SelectProcessor>().
                        ProcessTetraSelect(3, touch1.position, touch2.position, touch3.position);
                    selectProcessor.GetComponent<SelectProcessor>().showTetra(true);
                }
                
            }
            
        }
    }

    public void processClientTetraTouch(int cntClient, Vector2 tp1, Vector2 tp2)
    {
        cntSelectingInClient = cntClient;
        tpSelectInClient1 = tp1;
        tpSelectInClient2 = tp2;
    }
    #endregion

    #region AngleTetraSelection
    void detectAngleTetraSelect()
    {
        cntSelectingInServer = Input.touchCount;
        //cntTetraSelectingInClient = 3 - cntTetraSelectingInServer;
        if (cntSelectingInServer > 0
            && cntSelectingInServer + cntSelectingInClient == 3)
        {
            if (cntSelectingInServer == 1)
            {
                Touch touch = Input.touches[0];
                if (touch.position.y > 400 && touch.position.y < screenHeight - 400)
                {
                    selectProcessor.GetComponent<SelectProcessor>().
                        ProcessAngleTetraSelect(1,
                        touch.position,
                        tpSelectInClient1,
                        tpSelectInClient2);

                    selectProcessor.GetComponent<SelectProcessor>().showTetra(true);
                }
            }
            else if (cntSelectingInServer == 2)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                if (touch1.position.y > 400 && touch1.position.y < screenHeight - 400 &&
                   touch2.position.y > 400 && touch2.position.y < screenHeight - 400)
                {
                    selectProcessor.GetComponent<SelectProcessor>().
                        ProcessAngleTetraSelect(2,
                        touch1.position,
                        touch2.position,
                        tpSelectInClient1);
                    //new Vector2(screenWidth - 150, screenHeight / 2 - 50));
                    selectProcessor.GetComponent<SelectProcessor>().showTetra(true);
                }

            }
            // only test
            else if (cntSelectingInServer == 3)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                Touch touch3 = Input.touches[2];
                if (touch1.position.y > 400 && touch2.position.y > 400 && touch3.position.y > 400)
                {
                    selectProcessor.GetComponent<SelectProcessor>().
                        ProcessAngleTetraSelect(3, touch1.position, touch2.position, touch3.position);
                    selectProcessor.GetComponent<SelectProcessor>().showTetra(true);
                }

            }

        }
    }

    public void processClientAngleTetraTouch(int cntClient, Vector2 tp1, Vector2 tp2)
    {
        cntSelectingInClient = cntClient;
        tpSelectInClient1 = tp1;
        tpSelectInClient2 = tp2;
    }
    #endregion

    #region CastRaySelect includng detectRenderedSelect()

    public void detectRenderedSelect()
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.touches[0];
            Vector3 tp = t.position;
            castRay(tp);
        }
    }

    private void castRay(Vector3 pos)
    {
        Camera cam = Camera.main;
        Vector3 tp = new Vector3(10000, 10000, 10000);
        Vector3 mousePos1 = tp, mousePos2 = tp;
        if (pos.magnitude < 10000)
        {
            mousePos1 = mousePos2 = pos;
            float cameraHeight = orthCamera.GetComponent<CameraController>().curCameraHeight;
            Debug.Log("dy3- tp1: " + processTP(mousePos1) + "orthSize: " + Camera.main.orthographicSize*2);
            Debug.Log("dy3- tp2: " + processTouchPoint(mousePos2) + "orthSize: " + cameraHeight);
            mousePos1 = processTP(mousePos1);
            mousePos2 = processTouchPoint(mousePos2);
            Debug.DrawLine(
                cam.gameObject.transform.position,
                cam.gameObject.transform.position + Camera.main.orthographicSize * 2 * (mousePos1 - cam.gameObject.transform.position),
                Color.blue,
                0.01f,
                true
            );
            
            Debug.DrawLine(
                cam.gameObject.transform.position,
                cam.gameObject.transform.position + cameraHeight * (mousePos2 - cam.gameObject.transform.position),
                Color.green,
                0.01f,
                true
            );
            RaycastHit hit;
            bool isHit; GameObject objj;
            if (!Physics.Raycast(cam.gameObject.transform.position, mousePos1 - cam.gameObject.transform.position, out hit))
            {
                isHit = false;
                Debug.Log("dy3- Hit1? " + isHit);
            }
            else
            {
                objj = hit.collider.gameObject;
                isHit = true;
                Debug.Log("Hit1? " + isHit + "target: " + objj.name);
            }
            //Debug.DrawRay(pos, Vector3.forward, Color.green);
            if (!Physics.Raycast(cam.gameObject.transform.position, mousePos2 - cam.gameObject.transform.position, out hit))
            {
                isHit = false;
                Debug.Log("dy3- Hit2? " + isHit);
            }
            else
            {
                objj = hit.collider.gameObject;
                isHit = true;
                Debug.Log("Hit2? " + isHit + "target: " + objj.name);
            }
        }
        
        
    }
    #endregion

    #region PointSelection
    public void detectPointSelect()
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.touches[0];
            if (t.phase == TouchPhase.Began)
            {
                //selectProcessor.GetComponent<SelectProcessor>().DetectRaycastOnBallwithNativeRaycast(1, t.position, Vector2.zero);
                selectProcessor.GetComponent<SelectProcessor>().
                    DetectRaycastOnBallwithNewRaycast(1, t.position, Vector2.zero);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch t1 = Input.touches[0], t2 = Input.touches[1];
            if (t1.phase == TouchPhase.Began || t2.phase == TouchPhase.Began)
            {
                //selectProcessor.GetComponent<SelectProcessor>().DetectRaycastOnBallwithNativeRaycast(2, t1.position, t2.position);
                selectProcessor.GetComponent<SelectProcessor>().
                    DetectRaycastOnBallwithNewRaycast(2, t1.position, t2.position);
            }
        }
    }
    #endregion

    #region Filtering
    private void detectFilter2()
    {
        if (Input.touchCount == 2)
        {
            isFilteringInServer = true;
            Touch touch1 = Input.touches[0];
            Touch touch2 = Input.touches[1];
            filterProcessor.GetComponent<FilterProcessor>().ProcessServerBothRange(touch1, touch2);
            
        } else
        {
            filterProcessor.GetComponent<FilterProcessor>().    DisableParamsInCase();
            isFilteringInServer = false;
            filterProcessor.GetComponent<FilterProcessor>().
                    ProcessClientRange(isFilteringInClient, minFilterInClient, maxFilterInClient);
        }
    }

    public void processClientFilterRange(bool clientFiltering, float minn, float maxx)
    {
        isFilteringInClient = clientFiltering;
        minFilterInClient = minn;
        maxFilterInClient = maxx;
    }

    private void detectFilter1()
    {
        if (Input.touchCount == 1)
        {
            isFilteringInServer = true;
            Touch touch = Input.touches[0];
            filterProcessor.GetComponent<FilterProcessor>().ProcessServerOneRange(touch);
        }
        else
        {
            isFilteringInServer = false;
        }
        
    }

    public void resetZSliderFromServer()
    {
        /*
        bool tmp = isFilteringInServer;
        isFilteringInServer = true;
        sender.GetComponent<ServerController>().sendMessage();
        isFilteringInServer = tmp;
        */
        infoResetZSlider = true;
        sender.GetComponent<ServerController>().sendMessage();
    }
    #endregion

    #region RectSelection - abondon
    private void detectSelect()
    {
        if(Input.touchCount < 2)
        {
            selectVisualizer.GetComponent<SelectVisualizer>().unselect();
        }
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.touches[0];
            Touch touch2 = Input.touches[1];
            //Vector3 pos1 = new Vector3(touch1.position.x, touch1.position.y, 0);
            //Vector3 pos2 = new Vector3(touch2.position.x, touch2.position.y, 0);
            vertex1 = processTouchPoint(touch1.position);
            vertex2 = processTouchPoint(touch2.position);
            selectVisualizer.GetComponent<SelectVisualizer>().select();
        }
    }
    #endregion

    # region Pan & Rotate
    private void freemoving() {
        if (panDelta.magnitude > minPanDistance) {
            pos += panDelta;
            isPanning = true;
            panTimer = panDelay;
        }

        if (rotateDelta.magnitude > minRotateAngle)
        {
            rot *= Quaternion.Euler(rotateDelta);
            isRotating = true;
            rotateTimer = panDelay;
        }

        isPanning = (panTimer > 0);
        panTimer -= Time.deltaTime;
        isRotating = (rotateTimer > 0);
        rotateTimer -= Time.deltaTime;
    }
 
	private void calculate () {
		panDelta = new Vector3(0, 0, 0);
        rotateDelta = new Vector3(0, 0, 0);
 
		// if two fingers are touching the screen at the same time ...
		if (Input.touchCount == 2) {
			Touch touch1 = Input.touches[0];
            Touch touch2 = Input.touches[1];
            if (touch1.phase == TouchPhase.Began)
            {
                isPanning = true;
            }
            else if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved) {
                //panDelta = touch1.deltaPosition;

                Vector3 panStart = (touch1.position - touch1.deltaPosition + touch2.position - touch2.deltaPosition) / 2;
                Vector3 panEnd = (touch1.position + touch2.position) / 2;

                panDelta = panEnd - panStart;

                if (panDelta.magnitude > minPanDistance) {
					panDelta *= Camera.main.orthographicSize / 772;
				}
				else {
					panDelta = new Vector3(0, 0, 0);
				}
			}
			else if (touch1.phase == TouchPhase.Ended) {
				isPanning = false;
			}
		} else if (Input.touchCount == 1)
        {
            Touch touch1 = Input.touches[0];
            if (touch1.phase == TouchPhase.Began)
            {
                isRotating = true;
                //rot = Quaternion.Euler(Vector3.zero);
                rotateTimer = panDelay;
            }
            else if (touch1.phase == TouchPhase.Moved)
            {
                /*
                rotateDelta = Vector3.down * touch1.deltaPosition.x
                            + Vector3.right * touch1.deltaPosition.y;
                            */
                rotateDelta = Vector3.down * touch1.deltaPosition.x;
                if (rotateDelta.magnitude > minRotateAngle)
                {
                    rotateDelta *= rotateRatio;
                }
                else
                {
                    rotateDelta = Vector3.zero ;
                }
            }
            else if (touch1.phase == TouchPhase.Ended)
            {
                isRotating = false;
            }
        }
		
	}
    #endregion

    // abandon
    private Vector3 processTP(Vector3 v)
    {
        /*
        v.x -= 360;
		v.y -= 772;
		v *= Camera.main.orthographicSize / 772;
        return v;
        */
        v.x -= Screen.width / 4;
        v.y -= Screen.height / 4;
        v.z = 0f;
        v *= Camera.main.orthographicSize / (Screen.height / 4);
        return v;
    }

    private Vector3 processTouchPoint(Vector3 v) {
        //cameraOrthSize = orthCamera.GetComponent<CameraController>().curCameraOrthSize;
        cameraHeight = orthCamera.GetComponent<CameraController>().curCameraHeight;
        //cameraWidth = orthCamera.GetComponent<CameraController>().curCameraWidth;
        Vector3 v1 = Vector3.zero;
        v1.x = v.x - screenWidth / 2;
        v1.y = v.y - screenHeight / 2;
        // v1 = v1 * (cameraOrthSize / (screenHeight / 2));
        v1 = v1 * (cameraHeight / Screen.height);
        return v1;
	}

	public void resetAll() {
		pos = defaultPos;
        rot = defaultRot;
        obj.transform.Find("Scatterplot").transform.localScale = defaultScale;
        sca = defaultScale;
        sender.GetComponent<ServerController>().sendMessage();
    }

    public Mode getCurrentMode()
    {
        return currentMode;
    }

    #region Enter filtering/selection/navigation Mode
    public void enterFiltering1Mode()
    {
        charMode = '1';
        modeText.text = "Mode: Filtering 1";
        currentMode = Mode.filter1;
        isFilteringInServer = isFilteringInClient = false;
        sender.GetComponent<ServerController>().sendMessage();


        setIrrelevantOptionInactive();
        cancelFilterButton.SetActive(true);
        xSlider.SetActive(true);
        ySlider.SetActive(true);
    }

    public void enterFiltering2Mode()
    {
        charMode = '2';
        modeText.text = "Mode: Filtering 2";
        currentMode = Mode.filter2;
        isFilteringInServer = isFilteringInClient = false;
        sender.GetComponent<ServerController>().sendMessage();

        setIrrelevantOptionInactive();
        cancelFilterButton.SetActive(true);
        xSlider.SetActive(true);
        ySlider.SetActive(true);
    }

    public void enterFocusMode()
    {
        charMode = 'o';
        modeText.text = "Mode: Focus";
        currentMode = Mode.focus;
        sender.GetComponent<ServerController>().sendMessage();

        setIrrelevantOptionInactive();
        selectProcessor.GetComponent<SelectProcessor>().resetPublicParams();
    }

    public void enterSelectionPMode(char ch)
    {
        if(ch == 'f')
        {
            charMode = 'f';
            modeText.text = "Mode: Face Selection";
            currentMode = Mode.selectF;
        }
        else if(ch == 'p')
        {
            charMode = 'p';
            modeText.text = "Mode: Point Selection";
            currentMode = Mode.selectP;
        }
        sender.GetComponent<ServerController>().sendMessage();

        setIrrelevantOptionInactive();
        cancelSelectButton.SetActive(true);
    }

    public void enterSelectionPMode()
    {
        if (charMode == 'p' && currentMode == Mode.selectP)
        {
            charMode = 'f';
            modeText.text = "Mode: Face Selection";
            currentMode = Mode.selectF;
        } else
        {
            charMode = 'p';
            modeText.text = "Mode: Point Selection";
            currentMode = Mode.selectP;
        }
        sender.GetComponent<ServerController>().sendMessage();

        setIrrelevantOptionInactive();
        cancelSelectButton.SetActive(true);
    }

    public void enterSelectionTMode()
    {
        charMode = 't';
        modeText.text = "Mode: T Selection";
        currentMode = Mode.selectT;
        sender.GetComponent<ServerController>().sendMessage();

        setIrrelevantOptionInactive();
        cancelSelectButton.SetActive(true);
        selectProcessor.GetComponent<SelectProcessor>().resetPublicParams();
        selectProcessor.GetComponent<SelectProcessor>().showDiamond(false);
        selectProcessor.GetComponent<SelectProcessor>().showTetra(false);
    }

    public void enterSelectionAMode()
    {
        charMode = 'a';
        modeText.text = "Mode: A Selection";
        currentMode = Mode.selectA;
        sender.GetComponent<ServerController>().sendMessage();

        setIrrelevantOptionInactive();
        cancelSelectButton.SetActive(true);
        selectProcessor.GetComponent<SelectProcessor>().resetPublicParams();
        selectProcessor.GetComponent<SelectProcessor>().showDiamond(false);
        selectProcessor.GetComponent<SelectProcessor>().showTetra(false);
    }

    public void enterSelectionDMode()
    {
        charMode = 'd';
        modeText.text = "Mode: D Selection";
        currentMode = Mode.selectD;
        sender.GetComponent<ServerController>().sendMessage();

        setIrrelevantOptionInactive();
        cancelSelectButton.SetActive(true);
        selectProcessor.GetComponent<SelectProcessor>().resetPublicParams();
        selectProcessor.GetComponent<SelectProcessor>().showDiamond(false);
        selectProcessor.GetComponent<SelectProcessor>().showTetra(false);
    }

    public void enterNavigationMode()
    {
        charMode = 'n';
        modeText.text = "Mode: Navigation";
        currentMode = Mode.navigate;
        sender.GetComponent<ServerController>().sendMessage();

        setIrrelevantOptionInactive();
        GameObject pa = this.transform.parent.gameObject;
        GameObject lt = pa.transform.Find("LeanTouch").gameObject;
        lt.SetActive(true);

    }

    void setIrrelevantOptionInactive()
    {
        GameObject pa = this.transform.parent.gameObject;
        GameObject lt = pa.transform.Find("LeanTouch").gameObject;
        lt.SetActive(false);
        cancelFilterButton.SetActive(false);
        cancelSelectButton.SetActive(false);
        xSlider.SetActive(false);
        ySlider.SetActive(false);
        ballController.GetComponent<BallController>().UpdateInteractBallScript(false);
    }

    public void clearAidshape()
    {
        snapProcessor.GetComponent<SnapProcessor>().clearHighlight();
    }
    #endregion
}