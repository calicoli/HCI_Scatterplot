using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SelectProcessor : MonoBehaviour
{
    public GameObject ballController;
    public Text pointText;
    public GameObject orthCamera;

    private GameObject objLine;
    private LineRenderer lrLine;
    private const float lineWidth = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        objLine = new GameObject("ConnetingLine");
        objLine.transform.SetParent(this.transform);
        lrLine = objLine.AddComponent<LineRenderer>();
        lrLine.startWidth = lrLine.endWidth = lineWidth;
        //lrLine.useWorldSpace = false;
        lrLine.material = new Material(Shader.Find("Sprites/Default"));
        lrLine.material.color = new Color(1, 1, 1, 0.6f);
        lrLine.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void DetectRaycastOnBallwithNewRaycast(int num, Vector2 v1, Vector2 v2)
    {
        bool flagRenderLine = false;
        Vector3 vstart = Vector3.zero, vend = Vector3.zero;
        Vector3[] result = { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };
        string ballInfo;
        Vector3 p1 = processTouchPoint(v1), p2 = processTouchPoint(v2);
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

    private Vector3 processTouchPoint(Vector2 v)
    {
        float cameraOrthSize = orthCamera.GetComponent<CameraController>().curCameraOrthSize;
        Vector3 v1 = Vector3.zero;
        v1.x = v.x - Screen.width / 2;
        v1.y = v.y - Screen.height / 2;
        v1 = v1 * (cameraOrthSize / (Screen.height / 2));
        return v1;
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
