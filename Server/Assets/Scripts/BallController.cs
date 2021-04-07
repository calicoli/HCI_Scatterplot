using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public GameObject pointContainer;
    public GameObject selectVisualizer;
    public GameObject filterProcessor;
    public GameObject selectProcessor;

    [HideInInspector]
    public int ballCount;

    [HideInInspector]
    public struct Ball
    {
        public int id;
        public bool isSelected;
        public bool isFiltered;
        public Vector3 oriPosition;
        public Vector3 curPosition;
        public Vector3 showPosition;
        public Color defaultColor;
        public Color selectedColor;
        public Color filteredColor;
    }

    [HideInInspector]
    public Ball[] balls;

    // original position boundaries
    private float xlowerbound, ylowerbound, zlowerbound, 
                  xupperbound, yupperbound, zupperbound;
    // updated position boundaries
    private float rtxlb, rtylb, rtzlb,
                  rtxub, rtyub, rtzub;

    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //UpdateBallVisulization();
    }    

    public void UpdateBallVisulization()
    {
        for (int i = 0; i < ballCount; i++)
        {
            if (balls[i].isSelected)
            {
                pointContainer.transform.GetChild(i).GetComponent<MeshRenderer>().material.color = balls[i].selectedColor;
            }
            else if (balls[i].isFiltered)
            {
                pointContainer.transform.GetChild(i).GetComponent<MeshRenderer>().material.color = balls[i].filteredColor;
            }
            else
            {
                pointContainer.transform.GetChild(i).GetComponent<MeshRenderer>().material.color = balls[i].defaultColor;
            }
        }
    }

    public void UpdateBallWithID(int idx, out string str)
    {
        if (!balls[idx].isFiltered && !balls[idx].isSelected)
        {
            balls[idx].isSelected = true;
            str = "Point position: " + balls[idx].showPosition; ;
        }
        else if (!balls[idx].isFiltered && balls[idx].isSelected)
        {
            balls[idx].isSelected = false;
            str = null;
        }
        else
        {
            str = null;
        }
        UpdateBallVisulization();
    }

    public void UpdateBallWithIDs(int idx1, int idx2, 
        out bool flagRenderLine, out Vector3 vstart, out Vector3 vend, out string str)
    {
        if (!balls[idx1].isFiltered && !balls[idx2].isFiltered)
        {
            balls[idx1].isSelected = true;
            balls[idx2].isSelected = true;
            float distance = CalculateDistance(balls[idx1].oriPosition, balls[idx2].oriPosition);
            flagRenderLine = true;
            str = "Points Distance: " + distance.ToString("0.00");
            vstart = balls[idx1].curPosition;
            vend = balls[idx2].curPosition;
        }
        else
        {
            flagRenderLine = false;
            str = "Do not detect 2 points.";
            vstart = Vector3.zero;
            vend = Vector3.zero;
        }
        UpdateBallVisulization();
    }

    float CalculateDistance(Vector3 v1, Vector3 v2)
    {
        float d = Mathf.Sqrt(
            (v1.x - v2.x) * (v1.x - v2.x) +
            (v1.y - v2.y) * (v1.y - v2.y) +
            (v1.z - v2.z) * (v1.z - v2.z)
            );
        return d;
    }

    public void UpdateBallWithRange(char axis, float minn, float maxx)
    {

        switch(axis)
        {
            case 'x':
                //Debug.Log("dy-case x: " + minn + ',' + maxx);
                for (int i = 0; i < ballCount; i++)
                {
                    if (balls[i].oriPosition.x < minn || balls[i].oriPosition.x > maxx)
                    {
                        balls[i].isFiltered = true;
                        balls[i].isSelected = false;
                    }
                    else
                    {
                        balls[i].isFiltered = false;
                        balls[i].isSelected = false;
                    }
                }
                break;
            case 'y':
                for (int i = 0; i < ballCount; i++)
                {
                    if (balls[i].oriPosition.y < minn || balls[i].oriPosition.y > maxx)
                    {
                        balls[i].isFiltered = true;
                        balls[i].isSelected = false;
                    }
                    else
                    {
                        balls[i].isFiltered = false;
                        balls[i].isSelected = false;
                    }
                }
                break;
            case 'z':
                for (int i = 0; i < ballCount; i++)
                {
                    if (balls[i].oriPosition.z < minn || balls[i].oriPosition.z > maxx)
                    {
                        balls[i].isFiltered = true;
                        balls[i].isSelected = false;
                    }
                    else
                    {
                        balls[i].isFiltered = false;
                        balls[i].isSelected = false;
                    }
                }
                break;
            default:
                break;
        }
        UpdateBallVisulization();
    }

    public void UpdateBallWithRect( float minx, float maxx, 
                                    float miny, float maxy)
    {
        for(int i=0; i < ballCount; i++)
        {
            if (minx < balls[i].curPosition.x && balls[i].curPosition.x < maxx &&
                miny < balls[i].curPosition.y && balls[i].curPosition.y < maxy )
            {
                balls[i].isSelected = true;
                balls[i].isFiltered = false;
            }
            else
            {
                balls[i].isSelected = false;
                balls[i].isFiltered = false;
            }
        }
        UpdateBallVisulization();
    }

    public void UpdateBallWithCuboid(   float minx, float maxx, 
                                        float miny, float maxy, 
                                        float minz, float maxz)
    {
        for (int i = 0; i < ballCount; i++)
        {
            if (minx < balls[i].curPosition.x && balls[i].curPosition.x < maxx &&
                miny < balls[i].curPosition.y && balls[i].curPosition.y < maxy &&
                minz < balls[i].curPosition.z && balls[i].curPosition.z < maxz ) 
            {
                balls[i].isSelected = true;
                balls[i].isFiltered = false;
            }
            else
            {
                balls[i].isSelected = false;
                balls[i].isFiltered = false;
            }
        }
        UpdateBallVisulization();
    }

    public void ResetBallColorInSelection()
    {
        UpdateBallStatus(true, false);
        UpdateBallVisulization();
        selectProcessor.GetComponent<SelectProcessor>().UpdateConnectingLine(false, Vector3.zero, Vector3.zero);
        selectProcessor.GetComponent<SelectProcessor>().pointText.text = null;
    }

    public void ResetBallColorInFiltering()
    {
        UpdateBallStatus(false, true);
        Debug.Log("Ball(minX, maxX): (" + xlowerbound + ", " + xupperbound + ")");
        Debug.Log("Ball(minY, maxY): (" + ylowerbound + ", " + yupperbound + ")");
        Debug.Log("Ball(minZ, maxZ): (" + zlowerbound + ", " + zupperbound + ")");
        filterProcessor.GetComponent<FilterProcessor>().InitFilterBoundary('x', xlowerbound, xupperbound);
        filterProcessor.GetComponent<FilterProcessor>().InitFilterBoundary('y', ylowerbound, yupperbound);
        filterProcessor.GetComponent<FilterProcessor>().InitFilterBoundary('z', zlowerbound, zupperbound);
        filterProcessor.GetComponent<FilterProcessor>().InitQuadBoundary('x', rtxlb, rtxub);
        filterProcessor.GetComponent<FilterProcessor>().InitQuadBoundary('y', rtylb, rtyub);
        filterProcessor.GetComponent<FilterProcessor>().InitQuadBoundary('z', rtzlb, rtzub);
        UpdateBallVisulization();
    }

    public void InitBall()
    {
        // Init balls property
        // Get ball color
        ballCount = pointContainer.transform.childCount;
        balls = new Ball[ballCount];
        for(int i = 0; i < ballCount; i++)
        {
            balls[i].id = i;
            balls[i].defaultColor = pointContainer.transform.GetChild(i).GetComponent<MeshRenderer>().material.color;
            balls[i].selectedColor = Color.white;
            //balls[i].filteredColor = balls[i].defaultColor;
            //balls[i].filteredColor.a = 0.3f;
            balls[i].filteredColor = new Color(
                balls[i].defaultColor.r, 
                balls[i].defaultColor.g, 
                balls[i].defaultColor.b, 
                0.2f);
            balls[i].isSelected = false;
            balls[i].isFiltered = false;
            balls[i].oriPosition = pointContainer.transform.GetChild(i).transform.localPosition;
            balls[i].showPosition = new Vector3(
                -balls[i].oriPosition.z,
                balls[i].oriPosition.y,
                balls[i].oriPosition.x);
        }
    }

    public void UpdateBallOriginalBoundary(float xi, float yi, float zi,
                                   float xa, float ya, float za)
    {
        xlowerbound = xi; ylowerbound = yi; zlowerbound = zi;
        xupperbound = xa; yupperbound = ya; zupperbound = za;
        //print6float(xi, xa, yi, ya, zi, za);
        
    }

    public void UpdateBallRealtimeBoundary( float xi, float yi, float zi,
                                            float xa, float ya, float za)
    {
        rtxlb = xi; rtylb = yi; rtzlb = zi;
        rtxub = xa; rtyub = ya; rtzub = za;
    }

    public void UpdateBallStatus(bool updateSelected, bool updateFiltered)
    {
        if(updateSelected)
        {
            for (int i = 0; i < ballCount; i++)
            {
                balls[i].isSelected = false;
            }
        }
        if (updateFiltered)
        {
            for (int i = 0; i < ballCount; i++)
            {
                balls[i].isFiltered = false;
            }
        }

    }

    public void UpdateBallPosition()
    {
        float minx, miny, minz, maxx, maxy, maxz;
        minx = miny = minz = float.MaxValue;
        maxx = maxy = maxz = float.MinValue;
        
        for (int i = 0; i < ballCount; i++)
        {
            balls[i].curPosition = pointContainer.transform.GetChild(i).position;
            
            minx = (minx < balls[i].curPosition.x) ? minx : balls[i].curPosition.x;
            miny = (miny < balls[i].curPosition.y) ? miny : balls[i].curPosition.y;
            minz = (minz < balls[i].curPosition.z) ? minz : balls[i].curPosition.z;
            maxx = (maxx > balls[i].curPosition.x) ? maxx : balls[i].curPosition.x;
            maxy = (maxy > balls[i].curPosition.y) ? maxy : balls[i].curPosition.y;
            maxz = (maxz > balls[i].curPosition.z) ? maxz : balls[i].curPosition.z;
        }
        
        UpdateBallRealtimeBoundary(minx, miny, minz, maxx, maxy, maxz);
        filterProcessor.GetComponent<FilterProcessor>().InitQuadBoundary('x', minx, maxx);
        filterProcessor.GetComponent<FilterProcessor>().InitQuadBoundary('y', miny, maxy);
        filterProcessor.GetComponent<FilterProcessor>().InitQuadBoundary('z', minz, maxz);
        /*
        filterProcessor.GetComponent<FilterProcessor>().InitFilterBoundary('x', minx, maxx);
        filterProcessor.GetComponent<FilterProcessor>().InitFilterBoundary('y', miny, maxy);
        filterProcessor.GetComponent<FilterProcessor>().InitFilterBoundary('z', minz, maxz);
        UpdateBallVisulization();
        */
        //print6float(minx, maxx, miny, maxy, minz, maxz); 
        Debug.Log("Quad(minX, maxX): (" + minx + ", " + maxx + ")");
        Debug.Log("Quad(minY, maxY): (" + miny + ", " + maxy + ")");
        Debug.Log("Quad(minZ, maxZ): (" + minz + ", " + maxz + ")");
        
        //selectVisualizer.GetComponent<SelectVisualizer>().minBallZ = minz;
        //selectVisualizer.GetComponent<SelectVisualizer>().UpdateMinZ(minz);
        /*
        Transform frame = pointContainer.transform.parent.Find("Frame").transform;
        Vector3 fx = frame.Find("FrameX").transform.position;
        Debug.Log("Frame X: " + fx);
        */
    }

    public void InitBallPosition()
    {
        float minx, miny, minz, maxx, maxy, maxz;
        minx = miny = minz = float.MaxValue;
        maxx = maxy = maxz = float.MinValue;
        for (int i = 0; i < ballCount; i++)
        {
            balls[i].curPosition = pointContainer.transform.GetChild(i).position;

            minx = (minx < balls[i].oriPosition.x) ? minx : balls[i].oriPosition.x;
            miny = (miny < balls[i].oriPosition.y) ? miny : balls[i].oriPosition.y;
            minz = (minz < balls[i].oriPosition.z) ? minz : balls[i].oriPosition.z;
            maxx = (maxx > balls[i].oriPosition.x) ? maxx : balls[i].oriPosition.x;
            maxy = (maxy > balls[i].oriPosition.y) ? maxy : balls[i].oriPosition.y;
            maxz = (maxz > balls[i].oriPosition.z) ? maxz : balls[i].oriPosition.z;
        }
        UpdateBallOriginalBoundary(minx, miny, minz, maxx, maxy, maxz);
        filterProcessor.GetComponent<FilterProcessor>().InitFilterBoundary('x', minx, maxx);
        filterProcessor.GetComponent<FilterProcessor>().InitFilterBoundary('y', miny, maxy);
        filterProcessor.GetComponent<FilterProcessor>().InitFilterBoundary('z', minz, maxz);
        UpdateBallVisulization();

        //print6float(minx, maxx, miny, maxy, minz, maxz); 
        Debug.Log("Ball(minX, maxX): (" + minx + ", " + maxx + ")");
        Debug.Log("Ball(minY, maxY): (" + miny + ", " + maxy + ")");
        Debug.Log("Ball(minZ, maxZ): (" + minz + ", " + maxz + ")");
    }

    void print6float(float a, float b, float c, float d, float e, float f)
    {
        Debug.Log("(" + a + "," + b +
                ") (" + c + "," + d +
                ") (" + e + "," + f + ")");
    }

    // Filering with curPosistion
    /*
     * public void UpdateBallWithRange(char axis, float minn, float maxx)
    {

        switch(axis)
        {
            case 'x':
                //Debug.Log("dy-case x: " + minn + ',' + maxx);
                for (int i = 0; i < ballCount; i++)
                {
                    if (balls[i].curPosition.x < minn || balls[i].curPosition.x > maxx)
                    {
                        balls[i].isFiltered = true;
                        balls[i].isSelected = false;
                    }
                    else
                    {
                        balls[i].isFiltered = false;
                        balls[i].isSelected = false;
                    }
                }
                break;
            case 'y':
                for (int i = 0; i < ballCount; i++)
                {
                    if (balls[i].curPosition.y < minn || balls[i].curPosition.y > maxx)
                    {
                        balls[i].isFiltered = true;
                        balls[i].isSelected = false;
                    }
                    else
                    {
                        balls[i].isFiltered = false;
                        balls[i].isSelected = false;
                    }
                }
                break;
            case 'z':
                for (int i = 0; i < ballCount; i++)
                {
                    if (balls[i].curPosition.z < minn || balls[i].curPosition.z > maxx)
                    {
                        balls[i].isFiltered = true;
                        balls[i].isSelected = false;
                    }
                    else
                    {
                        balls[i].isFiltered = false;
                        balls[i].isSelected = false;
                    }
                }
                break;
            default:
                break;
        }
        UpdateBallVisulization();
    }*/
}
