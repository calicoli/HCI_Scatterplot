using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterProcessor : MonoBehaviour
{
    public GameObject zslider;

    [HideInInspector]
    public bool isFilteringInClient;
    //public bool isFilteringInServer;
    private float lowerFilterDelta, upperFilterDelta;
    private bool hadDirection;
    private const float filterPanRatio = 1.6f;
    private const float minFilter1PanDistance = 40f, minFilter2PanDistance = 40f;
    private enum slideDirection { nullDirection, updown, leftright, frontback };
    private slideDirection dirCurrent;

    private Vector3 posStart1, posCurrent1,
                    posStart2, posCurrent2;

    // value for ball
    private float zlb, zub;
    private float curzlb, curzub;
    private float minRangeValue, maxRangeValue;
    // value for slider
    private float minzSlider,maxzSlider;
    private const float minSliderRange = 0f, maxSliderRange = 10f;

    private float screenWidth, screenHeight;

    // Start is called before the first frame update
    void Start()
    {
        screenWidth = Screen.width;
        screenHeight = Screen.height;

        dirCurrent = slideDirection.nullDirection;
        posStart1 = posStart2 = posCurrent1 = posCurrent2 = Vector3.zero;
        lowerFilterDelta = upperFilterDelta = 0f;

        minzSlider = minSliderRange;
        minzSlider = maxSliderRange;

        hadDirection = false;
        isFilteringInClient = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ProcessClientOneRange(Touch touch)
    {
        
        if (touch.phase == TouchPhase.Began)
        {
            isFilteringInClient = true;
            posStart1 = touch.position;
            dirCurrent = slideDirection.nullDirection;
            hadDirection = false;
            lowerFilterDelta = 0f;
            upperFilterDelta = 0f;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            isFilteringInClient = true;
            posCurrent1 = touch.position;
            if (!hadDirection)
            {
                dirCurrent = slideDirection.frontback;
                //? InitFilterBoundary('z', zlb, zub);
                //InitSlider('z');
                hadDirection = true;
            }
            if (posStart1.x < screenWidth * 0.5f)
            {
                lowerFilterDelta = touch.deltaPosition.x / screenWidth * filterPanRatio;
                upperFilterDelta = 0f;
            }
            else if (posStart1.x >= screenWidth * 0.5f)
            {
                lowerFilterDelta = 0f;
                upperFilterDelta = touch.deltaPosition.x / screenWidth * filterPanRatio;
            }
            else
            {
                lowerFilterDelta = 0f;
                upperFilterDelta = 0f;
            }

            if (Mathf.Abs(posCurrent1.x - posStart1.x) < minFilter1PanDistance)
            {
                lowerFilterDelta = 0f;
                upperFilterDelta = 0f;
            }
            
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            isFilteringInClient = false;
            dirCurrent = slideDirection.nullDirection;
            hadDirection = false;
        }
        // deliver filterDalta to server -> do in TouchProcessor.cs
        DeliverConvertedRangeValue();
    }

    public void ProcessClientBothRange(Touch touch1, Touch touch2)
    {
        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            isFilteringInClient = true;
            posStart1 = touch1.position;
            posStart2 = touch2.position;
            dirCurrent = slideDirection.nullDirection;
            hadDirection = false;
            //lowerFilterDelta = 0f;
            //upperFilterDelta = 0f;
        }
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            isFilteringInClient = true;
            posCurrent1 = touch1.position;
            posCurrent2 = touch2.position;
            Vector2 deltaPos1 = touch1.deltaPosition;
            Vector2 deltaPos2 = touch2.deltaPosition;

            if (!hadDirection)
            {
                dirCurrent = slideDirection.frontback;
                //InitSlider('z');
                //InitFilterBoundary('z', zlb, zub);
                hadDirection = true;
            }

            // touch1 is left/front, touch2 is right/back
            if (posCurrent1.x <= posCurrent2.x)
            {
                lowerFilterDelta = deltaPos1.x / screenWidth * filterPanRatio;
                upperFilterDelta = deltaPos2.x / screenWidth * filterPanRatio;
                if (Mathf.Abs(posCurrent1.x - posStart1.x) < minFilter2PanDistance) lowerFilterDelta = 0f;
                if (Mathf.Abs(posCurrent2.x - posStart2.x) < minFilter2PanDistance) upperFilterDelta = 0f;
            }
            // touch1 is right/back, touch2 is left/front
            else if (posCurrent1.x > posCurrent2.x)
            {
                lowerFilterDelta = deltaPos2.x / screenWidth * filterPanRatio;
                upperFilterDelta = deltaPos1.x / screenWidth * filterPanRatio;
                if (Mathf.Abs(posCurrent1.x - posStart1.x) < minFilter2PanDistance) upperFilterDelta = 0f;
                if (Mathf.Abs(posCurrent2.x - posStart2.x) < minFilter2PanDistance) lowerFilterDelta = 0f;
            }
            // something bad happened
            else
            {
                lowerFilterDelta = 0f;
                upperFilterDelta = 0f;
            }
            //if (Mathf.Abs(lowerFilterDelta) < minFilter2Ratio) lowerFilterDelta = 0f;
            //if (Mathf.Abs(upperFilterDelta) < minFilter2Ratio) upperFilterDelta = 0f;

        }
        else if (touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended)
        {
            isFilteringInClient = false;
            dirCurrent = slideDirection.nullDirection;
            hadDirection = false;
        }
        DeliverConvertedRangeValue();
    }

    void DeliverConvertedRangeValue()
    {
        if (dirCurrent == slideDirection.frontback)
        {
            // slider range
            minzSlider = CountLowerValue(minzSlider, maxzSlider, minSliderRange, maxSliderRange);
            maxzSlider = CountUpperValue(minzSlider, maxzSlider, minSliderRange, maxSliderRange);
            UpdateZSlider(minzSlider, maxSliderRange - maxzSlider);
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

    /*
    public void InitFilterBoundary(char axis, float minn, float maxx)
    {
        if (axis == 'z')
        {
            InitSlider('z');
            zlb = minn; curzlb = minn;
            zub = maxx; curzub = maxx;
            // send message to server
        }
    }*/

    public void InitSlider(char axis)
    {
        if (axis == 'z')
        {
            minzSlider = minSliderRange;
            maxzSlider = maxSliderRange;
            UpdateZSlider(minzSlider, maxSliderRange - maxzSlider);
        }
    }

    void UpdateZSlider(float minn, float maxx)
    {
        zslider.GetComponent<Michsky.UI.ModernUIPack.RangeSlider>().minSlider.Refresh(minn);
        zslider.GetComponent<Michsky.UI.ModernUIPack.RangeSlider>().maxSlider.Refresh(maxx);
    }

    public float getLowerFilterDelta()
    {
        return lowerFilterDelta;
    }
    public float getUpperFilterDelta()
    {
        return upperFilterDelta;
    }
}
