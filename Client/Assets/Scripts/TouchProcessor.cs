using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchProcessor : MonoBehaviour
{
	public GameObject sender;
	public GameObject obj;
    public Text modeText;

    public GameObject orthCamera;
    public GameObject filterProcessor;
    public GameObject zSlider;

    private float sendTimer = -1;

    [HideInInspector]
    public char charMode;
    [HideInInspector]
    public bool modeFilter1, modeFilter2;
    [HideInInspector]
    public float lowerFilterDelta, upperFilterDelta;
    [HideInInspector]
    public bool modeNavigate, modeSelect;
    [HideInInspector]
    public float selectOffset, selectDepth;
    private Vector3 vertex3, vertex4;

    [HideInInspector]
	public bool isPanning;
    [HideInInspector]
    public bool isRotating;
    [HideInInspector]
    public bool isLocked;
    [HideInInspector]
    public bool isClientSelecting;
    [HideInInspector]
    public bool isClientFiltering, isServerFiltering;
    [HideInInspector]
    public int cntTetraOrDiamSelectingInClient;
    [HideInInspector]
    public Vector2 tpTetraOrDiamSelect1, tpTetraOrDiamSelect2;

    private float panTimer;
    private float rotateTimer;
    private float panDelay = 0.1f;

	[HideInInspector]
	public Vector3 pos;
    [HideInInspector]
    public Quaternion rot;
    [HideInInspector]
    public Vector3 sca;
    private Vector3 defaultPos;
    [HideInInspector]
    public Quaternion defaultRot;
    private Vector3 defaultScale;

    private const float panRatio = 1;
	private const float minPanDistance = 0;
	private Vector3 panDelta;

    private const float rotateRatio = 0.1f;
    private const float minRotateAngle = 0f;
    private Vector3 rotateDelta;

    private float cameraWidth;
    private float cameraHeight;
    private float cameraOrthSize;

    private float screenWidth;
    private float screenHeight;

    void Start()
	{
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        pos = new Vector3(0, 0, 0);
		defaultPos = new Vector3(0, 0, 0);

        rot = Quaternion.Euler(Vector3.zero);           // update in time
        defaultRot = Quaternion.Euler(Vector3.zero);    // update in time
        rot = Quaternion.Euler(0f, -90f, 0f);
        defaultRot = Quaternion.Euler(0f, -90f, 0f);

        //rot = obj.transform.Find("Scatterplot").transform.rotation;
        //defaultRot = obj.transform.Find("Scatterplot").transform.rotation;

        sca = obj.transform.Find("Scatterplot").transform.localScale;
        defaultScale = obj.transform.Find("Scatterplot").transform.localScale;

        //modeSelect = isClientSelecting = false;
        isClientSelecting = false;
        selectOffset = selectDepth = 0f;

        //modeFilter1 = modeFilter2 = false;
        isClientFiltering = isServerFiltering = false;
        lowerFilterDelta = upperFilterDelta = 0f;

        // tetra select
        cntTetraOrDiamSelectingInClient = 0;
        tpTetraOrDiamSelect1 = tpTetraOrDiamSelect2 = Vector2.zero;

        isLocked = isPanning = isRotating = false;
        modeText.text = "Mode: Navigation";
        charMode = 'n';
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(isLocked.ToString() + isPanning.ToString() + isRotating.ToString());
        if (charMode == 'n')
        {
            calculate();
            freemoving();

            /*
            if (sendTimer < 0)
            {
                sender.GetComponent<ClientController>().sendMessage();
                sendTimer = 0.05f;
            }
            else
            {
                sendTimer -= Time.deltaTime;
            }*/
            isLocked = isPanning || isRotating;
        }
        else if (charMode == 's')
        {
            detectSelect();
        }
        else if (charMode == '1')
        {
            detectFilter1();
        }
        else if (charMode == '2')
        {
            detectFilter2();
        }
        else if (charMode == 't')
        {
            detectTetraSelect();
        }
        else if (charMode == 'd')
        {
            detectDiamondSelect();
        }
        else if (charMode == 'a')
        {
            detectAngleTetraSelect();
        }
    }
    private void detectDiamondSelect()
    {
        int cntValidTouch = 0;
        tpTetraOrDiamSelect1 = tpTetraOrDiamSelect2 = Vector2.zero;
        if (Input.touchCount > -1 && Input.touchCount < 3)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.touches[0];
                if (touch.position.y > 100 && touch.position.y < screenHeight - 100)
                {
                    tpTetraOrDiamSelect1 = touch.position;
                    cntValidTouch = 1;
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                if (touch1.position.y > 100 && touch1.position.y < screenHeight - 100 &&
                   touch2.position.y > 100 && touch2.position.y < screenHeight - 100)
                {
                    tpTetraOrDiamSelect1 = touch1.position;
                    tpTetraOrDiamSelect2 = touch2.position;
                    cntValidTouch = 2;
                }
            }
        }
        cntTetraOrDiamSelectingInClient = cntValidTouch;
        sender.GetComponent<ClientController>().sendMessage();
    }

    private void detectTetraSelect()
    {
        int cntValidTouch = 0;
        tpTetraOrDiamSelect1 = tpTetraOrDiamSelect2 = Vector2.zero;
        if (Input.touchCount > -1 && Input.touchCount < 3)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.touches[0];
                if (touch.position.y > 100 && touch.position.y < screenHeight - 100)
                {
                    tpTetraOrDiamSelect1 = touch.position;
                    cntValidTouch = 1;
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                if (touch1.position.y > 100 && touch1.position.y < screenHeight - 100 &&
                   touch2.position.y > 100 && touch2.position.y < screenHeight - 100)
                {
                    tpTetraOrDiamSelect1 = touch1.position;
                    tpTetraOrDiamSelect2 = touch2.position;
                    cntValidTouch = 2;
                }
            }
        }
        cntTetraOrDiamSelectingInClient = cntValidTouch;
        sender.GetComponent<ClientController>().sendMessage();
    }

    private void detectAngleTetraSelect()
    {
        int cntValidTouch = 0;
        tpTetraOrDiamSelect1 = tpTetraOrDiamSelect2 = Vector2.zero;
        if (Input.touchCount > -1 && Input.touchCount < 3)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.touches[0];
                if (touch.position.y > 100 && touch.position.y < screenHeight - 100)
                {
                    tpTetraOrDiamSelect1 = touch.position;
                    cntValidTouch = 1;
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                if (touch1.position.y > 100 && touch1.position.y < screenHeight - 100 &&
                   touch2.position.y > 100 && touch2.position.y < screenHeight - 100)
                {
                    tpTetraOrDiamSelect1 = touch1.position;
                    tpTetraOrDiamSelect2 = touch2.position;
                    cntValidTouch = 2;
                }
            }
        }
        cntTetraOrDiamSelectingInClient = cntValidTouch;
        sender.GetComponent<ClientController>().sendMessage();
    }

    private void detectFilter1()
    {
        if(!isServerFiltering)
        {
            if (Input.touchCount == 1)
            {
                isClientFiltering = true;
                //filterProcessor.GetComponent<FilterProcessor>().isFilteringInServer = isServerFiltering;
                Touch touch = Input.touches[0];
                filterProcessor.GetComponent<FilterProcessor>().ProcessClientOneRange(touch);
                lowerFilterDelta = filterProcessor.GetComponent<FilterProcessor>().getLowerFilterDelta();
                upperFilterDelta = filterProcessor.GetComponent<FilterProcessor>().getUpperFilterDelta();
            }
            else
            {
                isClientFiltering = false;
            }
            sender.GetComponent<ClientController>().sendMessage();
        }
        else
        {
            //filterProcessor.GetComponent<FilterProcessor>().isFilteringInServer = isServerFiltering;
            filterProcessor.GetComponent<FilterProcessor>().InitSlider('z');
        }
    }

    private void detectFilter2()
    {
        if(!isServerFiltering)
        {
            if (Input.touchCount == 2)
            {
                isClientFiltering = true;
                Touch touch1 = Input.touches[0];
                Touch touch2 = Input.touches[1];
                filterProcessor.GetComponent<FilterProcessor>().ProcessClientBothRange(touch1, touch2);
                lowerFilterDelta = filterProcessor.GetComponent<FilterProcessor>().getLowerFilterDelta();
                upperFilterDelta = filterProcessor.GetComponent<FilterProcessor>().getUpperFilterDelta();
            }
            else
            {
                isClientFiltering = false;
                lowerFilterDelta = 0f;
                upperFilterDelta = 0f;
            }
            sender.GetComponent<ClientController>().sendMessage();
            Debug.Log("dy3-1 lowerFilterDelta: " + lowerFilterDelta + "; upper: " + upperFilterDelta);
        } else
        {
            //filterProcessor.GetComponent<FilterProcessor>().isFilteringInServer = isServerFiltering;
            filterProcessor.GetComponent<FilterProcessor>().InitSlider('z');
        }
        
    }

    private void detectSelect()
    {
        if (Input.touchCount == 2)
        {
            isClientSelecting = true;
            Touch touch3 = Input.touches[0];
            Touch touch4 = Input.touches[1];
            // touch3 is left, touch4 is right
            if(touch3.position.x > touch4.position.x)
            {
                touch3 = Input.touches[1];
                touch4 = Input.touches[0];
            }
            vertex3 = processTouchPoint(touch3.position);
            vertex4 = processTouchPoint(touch4.position);

            if (touch3.phase == TouchPhase.Moved)
            {
                Vector3 delta = touch3.deltaPosition;
                if (delta.magnitude > minPanDistance)
                {
                    cameraOrthSize = orthCamera.GetComponent<CameraController>().curCameraOrthSize;
                    delta *= (cameraOrthSize / (screenHeight / 4));
                    selectOffset = delta.x;
                }
                else
                {
                    selectOffset = 0f;
                }
            }
            //selectVisualizer.GetComponent<SelectVisualizer>().select();
            selectDepth = Mathf.Abs(vertex3.x - vertex4.x);

        } else
        {
            isClientSelecting = false;
        }
        sender.GetComponent<ClientController>().sendMessage();
    }

    private void freemoving() {
		if (panDelta.magnitude > minPanDistance) {
			pos += panDelta;
            isPanning = true;
            panTimer = panDelay;
            sender.GetComponent<ClientController>().sendMessage();
        }

        if (rotateDelta.magnitude > minRotateAngle)
        {
            rot *= Quaternion.Euler(rotateDelta);
            isRotating = true;
            rotateTimer = panDelay;
            sender.GetComponent<ClientController>().sendMessage();
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
			else if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved)
            {
                //panDelta = touch1.deltaPosition;
                Vector3 panStart = (touch1.position - touch1.deltaPosition + touch2.position - touch2.deltaPosition) / 2;
                Vector3 panEnd = (touch1.position + touch2.position) / 2;

                panDelta = panEnd - panStart;

                if (panDelta.magnitude > minPanDistance)
                {
                    panDelta *= Camera.main.orthographicSize / 772;
                }
                else
                {
                    panDelta = new Vector3(0, 0, 0);
                }
            }
			else if (touch1.phase == TouchPhase.Ended) {
				isPanning = false;
			}
		}
        else if (Input.touchCount == 1)
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
                rotateDelta = Vector3.down * touch1.deltaPosition.x
                            + Vector3.right * touch1.deltaPosition.y;
                if (rotateDelta.magnitude > minRotateAngle)
                {
                    rotateDelta *= rotateRatio;
                }
                else
                {
                    rotateDelta = Vector3.zero;
                }
            }
            else if (touch1.phase == TouchPhase.Ended)
            {
                isRotating = false;
            }
        }

    }

	private Vector3 processTouchPoint(Vector3 v) {
        /*
		v.x -= 360;
		v.y -= 772;
		v *= Camera.main.orthographicSize / 772;
		return v;
        */
        cameraOrthSize = orthCamera.GetComponent<CameraController>().curCameraOrthSize;
        //cameraHeight = orthCamera.GetComponent<CameraController>().curCameraHeight;
        //cameraWidth = orthCamera.GetComponent<CameraController>().curCameraWidth;
        //Debug.Log("TP: " + v);
        Vector3 v1 = Vector3.zero;
        v1.x = v.x - screenWidth / 2;
        v1.y = v.y - screenHeight / 2;
        v1 = v1 * (cameraOrthSize / (screenHeight / 4));
        return v1;
    }

	public void resetAll() {
		pos = defaultPos;
        rot = defaultRot;
        obj.transform.Find("Scatterplot").transform.localScale = defaultScale;
        sca = defaultScale;
        sender.GetComponent<ClientController>().sendMessage();
    }

    public void switchtoServerMode(char ch)
    {
        charMode = ch;
        switch (ch)
        {
            case 'n':
                modeText.text = ("Mode: Navigation");
                isClientFiltering = false; isClientSelecting = false;
                zSlider.SetActive(false);
                break;
            case '1':
                modeText.text = ("Mode: Filtering 1");
                isClientFiltering = false; isClientSelecting = false;
                zSlider.SetActive(true);
                break;
            case '2':
                modeText.text = ("Mode: Filtering 2");
                isClientFiltering = false; isClientSelecting = false;
                zSlider.SetActive(true);
                break;
            case 'r':
                modeText.text = ("Mode: Select R");
                isClientFiltering = false; isClientSelecting = false;
                zSlider.SetActive(false);
                break;
            case 'p':
                modeText.text = ("Mode: Selection P");
                isClientFiltering = false; isClientSelecting = false;
                zSlider.SetActive(false);
                break;
            case 'a':
                modeText.text = "Mode: Selection A";
                isClientFiltering = false; isClientSelecting = false;
                zSlider.SetActive(false);
                break;
            case 't':
                modeText.text = "Mode: Selection T";
                isClientFiltering = false; isClientSelecting = false;
                zSlider.SetActive(false);
                break;
            case 'd':
                modeText.text = "Mode: Selection D";
                isClientFiltering = false; isClientSelecting = false;
                zSlider.SetActive(false);
                break;
            default:
                modeText.text = ("Mode: error");
                //modeFilter1 = false; modeNavigate = false;
                //modeFilter2 = false; modeSelect = false;
                isClientFiltering = false; isClientSelecting = false;
                zSlider.SetActive(false);
                break;
        }
    }
}
