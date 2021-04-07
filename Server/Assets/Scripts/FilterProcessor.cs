using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterProcessor : MonoBehaviour
{
    public GameObject xslider;
    public GameObject yslider;
    public GameObject ballController;
    public GameObject touchProcessor;
    public GameObject filterVisulizer;

    [HideInInspector]
    public bool isFilteringInServer;
    [HideInInspector]
    public bool isFilteringInClient;
    private bool hadDirection;
    private const float filterPanRatio = 1.6f;
    private const float minFilter1PanDistance = 40f, minFilter2PanDistance = 40f;
    //private const float minFilter1Ratio = 0.00f, minFilter2Ratio = 0.00f;  // 0.01f has been tested
    private float lowerFilterDelta, upperFilterDelta;
    private enum slideDirection { nullDirection, updown, leftright, frontback };
    //private enum slideVector { nullVector, up, down, left, right, front, back };
    private slideDirection dirCurrent;
    //private slideVector vecCurrent;
    private Vector3 posStart1, posCurrent1,
                    posStart2, posCurrent2;

    // value for ball with original positions
    private float xlb, ylb, zlb, xub, yub, zub;
    private float curxlb, curylb, curzlb, curxub, curyub, curzub;
    private float minRangeValue, maxRangeValue;
    // value for slider
    private float minxSlider, minySlider, minzSlider,
                  maxxSlider, maxySlider, maxzSlider;
    private const float minSliderRange = 0f, maxSliderRange = 10f;

    // values for ball with realtime positions
    private float rtxlb, rtylb, rtzlb, rtxub, rtyub, rtzub;
    private float crxlb, crylb, crzlb, crxub, cryub, crzub;

    private float screenWidth, screenHeight;

    // Start is called before the first frame update
    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        dirCurrent = slideDirection.nullDirection;
        //vecCurrent = slideVector.nullVector;
        posStart1 = posStart2 = posCurrent1 = posCurrent2 = Vector3.zero;
        lowerFilterDelta = upperFilterDelta = 0f;

        minxSlider = minySlider = minzSlider = minSliderRange;
        maxxSlider = maxySlider = minzSlider = maxSliderRange;

        hadDirection = false;
    }

    // Update is called once per frame
    void Update()
    {
        //DeliverConvertedRangeValue();
    }
    // original position boundaries
    public void InitFilterBoundary(char axis, float minn, float maxx)
    {
        if (axis == 'x')
        {
            InitSlider('x');
            xlb = minn; curxlb = minn;
            xub = maxx; curxub = maxx;
        }
        if (axis == 'y')
        {
            InitSlider('y');
            ylb = minn; curylb = minn;
            yub = maxx; curyub = maxx;
        }
        if(axis == 'z')
        {
            InitSlider('z');
            zlb = minn; curzlb = minn;
            zub = maxx; curzub = maxx;
            // send message to client
            touchProcessor.GetComponent<TouchProcessor>().resetZSliderFromServer();
        }
    }
    // realtime position boundaries
    public void InitQuadBoundary(char axis, float minn, float maxx)
    {
        if (axis == 'x')
        {
            crxlb = rtxlb = minn;
            crxub = rtxub = maxx;
        }
        if (axis == 'y')
        {
            crylb = rtylb = minn;
            cryub = rtyub = maxx;
        }
        if (axis == 'z')
        {
            crzlb = rtzlb = minn;
            crzub = rtzub = maxx;
        }
    }
    public void InitSlider(char axis)
    {
        if(axis == 'x')
        {
            minxSlider = minSliderRange;
            maxxSlider = maxSliderRange;
            UpdateXSlider(minxSlider, maxSliderRange - maxxSlider);
        }
        if(axis == 'y')
        {
            minySlider = minSliderRange;
            maxySlider = maxSliderRange;
            UpdateYSlider(minySlider, maxSliderRange - maxySlider);
        }
        // z donot need here
        if(axis == 'z')
        {
            minzSlider = minSliderRange;
            maxzSlider = maxSliderRange;
            // send message to client
        }
    }

    void DeliverConvertedRangeValue()
    {
        char ch;
        if (dirCurrent == slideDirection.leftright)
        {
            ch = 'x';
            // ball range
            curxlb = CountLowerValue(curxlb, curxub, xlb, xub);
            curxub = CountUpperValue(curxlb, curxub, xlb, xub);
            minRangeValue = curxlb;
            maxRangeValue = curxub;
            // slider range
            minxSlider = CountLowerValue(minxSlider, maxxSlider, minSliderRange, maxSliderRange);
            maxxSlider = CountUpperValue(minxSlider, maxxSlider, minSliderRange, maxSliderRange);
            UpdateXSlider(minxSlider, maxSliderRange - maxxSlider);
            // quad vertics
            crxlb = CountLowerValue(crxlb, crxub, rtxlb, rtxub);
            crxub = CountUpperValue(crxlb, crxub, rtxlb, rtxub);
            Vector3[] ver1 = {
                new Vector3(crxlb, rtylb, rtzlb),
                new Vector3(crxlb, rtylb, rtzub),
                new Vector3(crxlb, rtyub, rtzub),
                new Vector3(crxlb, rtyub, rtzlb)
            };
            Vector3[] ver2 = {
                new Vector3(crxub, rtylb, rtzlb),
                new Vector3(crxub, rtylb, rtzub),
                new Vector3(crxub, rtyub, rtzub),
                new Vector3(crxub, rtyub, rtzlb)
            };
            //filterVisulizer.GetComponent<FilterVisulizer>().updateQuad(true, ver1, ver2);
        }
        else if (dirCurrent == slideDirection.updown)
        {
            ch = 'y';
            // ball range
            curylb = CountLowerValue(curylb, curyub, ylb, yub);
            curyub = CountUpperValue(curylb, curyub, ylb, yub);
            minRangeValue = curylb;
            maxRangeValue = curyub;
            // slider range
            minySlider = CountLowerValue(minySlider, maxySlider, minSliderRange, maxSliderRange);
            maxySlider = CountUpperValue(minySlider, maxySlider, minSliderRange, maxSliderRange);
            UpdateYSlider(minySlider, maxSliderRange - maxySlider);
        }
        else if (dirCurrent == slideDirection.frontback)
        {
            ch = 'z';
            curzlb = CountLowerValue(curzlb, curzub, zlb, zub);
            curzub = CountUpperValue(curzlb, curzub, zlb, zub);
            minRangeValue = curzlb;
            maxRangeValue = curzub;
        }
        else { ch = '0'; }
        if(ch == 'x' || ch =='y' || ch=='z')
        {
            ballController.GetComponent<BallController>().UpdateBallWithRange(ch, minRangeValue, maxRangeValue);
        }
        
    }
    float CountLowerValue(float cl, float cu, float lowerbound, float upperbound)
    {
        cl = cl + lowerFilterDelta * (upperbound - lowerbound);
        cl = cl > lowerbound ? cl : lowerbound;
        cl = cl < cu ? cl : cu;
        return cl;
    }
    
    float CountUpperValue(float cl, float cu, float lowerbound, float upperbound)
    {
        cu = cu + upperFilterDelta * (upperbound - lowerbound);
        cu = cu < upperbound ? cu : upperbound;
        cu = cu > cl ? cu : cl;
        return cu;
    }

    void UpdateXSlider(float minn, float maxx)
    {
        xslider.GetComponent<Michsky.UI.ModernUIPack.RangeSlider>().minSlider.Refresh(minn);
        xslider.GetComponent<Michsky.UI.ModernUIPack.RangeSlider>().maxSlider.Refresh(maxx);
    }
    void UpdateYSlider(float minn, float maxx)
    {
        yslider.GetComponent<Michsky.UI.ModernUIPack.RangeSlider>().minSlider.Refresh(minn);
        yslider.GetComponent<Michsky.UI.ModernUIPack.RangeSlider>().maxSlider.Refresh(maxx);
    }

    public void ProcessorClientRange(bool clientFiltering, float minn, float maxx)
    {
        if(!isFilteringInServer)
        {
            isFilteringInClient = clientFiltering;
            if(isFilteringInClient)
            {
                hadDirection = true;
                dirCurrent = slideDirection.frontback;
                InitFilterBoundary('x', xlb, xub);
                InitFilterBoundary('y', ylb, yub);
                lowerFilterDelta = minn;
                upperFilterDelta = maxx;
            } else
            {
                hadDirection = false;
                dirCurrent = slideDirection.nullDirection;
            }
            DeliverConvertedRangeValue();
        }
        
    }

    public void ProcessServerOneRange(Touch touch)
    {
        if (touch.phase == TouchPhase.Began)
        {
            isFilteringInServer = true;
            posStart1 = touch.position;
            dirCurrent = slideDirection.nullDirection;
            hadDirection = false;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            isFilteringInServer = true;
            posCurrent1 = touch.position;
            if(!hadDirection)
            {
                if (Mathf.Abs(touch.deltaPosition.x) > Mathf.Abs(touch.deltaPosition.y))
                {
                    dirCurrent = slideDirection.leftright;
                    //InitSlider('y');
                    InitFilterBoundary('y', ylb, yub);
                }
                else
                {
                    dirCurrent = slideDirection.updown;
                    //InitSlider('x');
                    InitFilterBoundary('x', xlb, xub);
                }
                hadDirection = true;
            }
            
            if (posStart1.x < screenWidth * 0.5f && dirCurrent == slideDirection.leftright)
            {
                //?
                //cameraOrthSize = orthCamera.GetComponent<CameraController>().curCameraOrthSize;
                //lowerFilterDelta = touch.deltaPosition.x * Camera.main.orthographicSize / (screenHeight / 4);
                lowerFilterDelta = touch.deltaPosition.x / screenWidth * filterPanRatio;
                upperFilterDelta = 0f;
            }
            else if (posStart1.x >= screenWidth * 0.5f && dirCurrent == slideDirection.leftright)
            {
                lowerFilterDelta = 0f;
                //upperFilterDelta = touch.deltaPosition.x * Camera.main.orthographicSize / (screenHeight / 4);
                upperFilterDelta = touch.deltaPosition.x / screenWidth * filterPanRatio;
            }
            else if (posStart1.y < screenHeight * 0.5f && dirCurrent == slideDirection.updown)
            {
                lowerFilterDelta = touch.deltaPosition.y / screenWidth * filterPanRatio;
                upperFilterDelta = 0f;
            }
            else if (posStart1.y > screenHeight * 0.5f && dirCurrent == slideDirection.updown)
            {
                lowerFilterDelta = 0f;
                upperFilterDelta = touch.deltaPosition.y / screenWidth * filterPanRatio;
            } else
            {
                lowerFilterDelta = 0f;
                upperFilterDelta = 0f;
            }
            //if (Mathf.Abs(lowerFilterDelta) < minFilter1Ratio) lowerFilterDelta = 0f;
            //if (Mathf.Abs(upperFilterDelta) < minFilter1Ratio) upperFilterDelta = 0f;

            if (Mathf.Abs(posCurrent1.x - posStart1.x) < minFilter1PanDistance &&
                Mathf.Abs(posCurrent1.y - posStart1.y) < minFilter1PanDistance)
            {
                lowerFilterDelta = 0f;
                upperFilterDelta = 0f;
            }
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            isFilteringInServer = false;
            dirCurrent = slideDirection.nullDirection;
            hadDirection = false;
        }
        DeliverConvertedRangeValue();
    }

    public void ProcessServerBothRange(Touch touch1, Touch touch2)
    {
        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            isFilteringInServer = true;
            posStart1 = touch1.position;
            posStart2 = touch2.position;
            dirCurrent = slideDirection.nullDirection;
            hadDirection = false;
            
        }
        else if(touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            isFilteringInServer = true;
            posCurrent1 = touch1.position;
            posCurrent2 = touch2.position;
            Vector2 deltaPos1 = touch1.deltaPosition;
            Vector2 deltaPos2 = touch2.deltaPosition;

            if(!hadDirection)
            {
                if ((Mathf.Abs(deltaPos1.x) >= Mathf.Abs(deltaPos2.x) &&
                 Mathf.Abs(deltaPos1.x) >= Mathf.Abs(deltaPos1.y) &&
                 Mathf.Abs(deltaPos1.x) >= Mathf.Abs(deltaPos2.y)) ||
                (Mathf.Abs(deltaPos2.x) >= Mathf.Abs(deltaPos1.x) &&
                 Mathf.Abs(deltaPos2.x) >= Mathf.Abs(deltaPos1.y) &&
                 Mathf.Abs(deltaPos2.x) >= Mathf.Abs(deltaPos2.y))
               )
                {
                    dirCurrent = slideDirection.leftright;
                    //InitSlider('y');
                    InitFilterBoundary('y', ylb, yub);
                }
                else
                {
                    dirCurrent = slideDirection.updown;
                    //InitSlider('x');
                    InitFilterBoundary('x', xlb, xub);
                }
                //Debug.Log("dy2- direction: " + dirCurrent);
                hadDirection = true;
            }

            // touch1 is left, touch2 is right
            if (posCurrent1.x <= posCurrent2.x && dirCurrent == slideDirection.leftright)
            {
                lowerFilterDelta = deltaPos1.x / screenWidth * filterPanRatio;
                upperFilterDelta = deltaPos2.x / screenWidth * filterPanRatio;

                if (Mathf.Abs(posCurrent1.x - posStart1.x) < minFilter2PanDistance) lowerFilterDelta = 0f;
                if (Mathf.Abs(posCurrent2.x - posStart2.x) < minFilter2PanDistance) upperFilterDelta = 0f;
            }
            // touch1 is right, touch2 is left
            else if(posCurrent1.x > posCurrent2.x && dirCurrent == slideDirection.leftright)
            {
                lowerFilterDelta = deltaPos2.x / screenWidth * filterPanRatio;
                upperFilterDelta = deltaPos1.x / screenWidth * filterPanRatio;

                if (Mathf.Abs(posCurrent1.x - posStart1.x) < minFilter2PanDistance) upperFilterDelta = 0f;
                if (Mathf.Abs(posCurrent2.x - posStart2.x) < minFilter2PanDistance) lowerFilterDelta = 0f;
            }
            // touch1 is down, touch2 is up
            else if (posCurrent1.y <= posCurrent2.y && dirCurrent == slideDirection.updown)
            {
                lowerFilterDelta = deltaPos1.y / screenWidth * filterPanRatio;
                upperFilterDelta = deltaPos2.y / screenWidth * filterPanRatio;

                if (Mathf.Abs(posCurrent1.y - posStart1.y) < minFilter2PanDistance) lowerFilterDelta = 0f;
                if (Mathf.Abs(posCurrent2.y - posStart2.y) < minFilter2PanDistance) upperFilterDelta = 0f;
            }
            // touch1 is up, touch2 is down
            else if (posCurrent1.y > posCurrent2.y && dirCurrent == slideDirection.updown)
            {
                lowerFilterDelta = deltaPos2.y / screenWidth * filterPanRatio;
                upperFilterDelta = deltaPos1.y / screenWidth * filterPanRatio;

                if (Mathf.Abs(posCurrent1.y - posStart1.y) < minFilter2PanDistance) upperFilterDelta = 0f;
                if (Mathf.Abs(posCurrent2.y - posStart2.y) < minFilter2PanDistance) lowerFilterDelta = 0f;
            }
            // something bad happened
            else
            {
                lowerFilterDelta = 0f;
                upperFilterDelta = 0f;
            }
            //if (Mathf.Abs(lowerFilterDelta) < minFilter2Ratio) lowerFilterDelta = 0f;
            //if (Mathf.Abs(upperFilterDelta) < minFilter2Ratio) upperFilterDelta = 0f;
            //filterVisulizer.GetComponent<FilterVisulizer>().enableQuad(true);

        }
        else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
        {
            isFilteringInServer = false;
            dirCurrent = slideDirection.nullDirection;
            hadDirection = false;
            //filterVisulizer.GetComponent<FilterVisulizer>().enableQuad(false);
        }

        DeliverConvertedRangeValue();
    }
}
