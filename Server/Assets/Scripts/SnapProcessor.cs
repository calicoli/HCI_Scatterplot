using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SnapProcessor : MonoBehaviour
{
    public Camera cam;
    public Text debugText;
    public GameObject touchProcessor;
    public GameObject obj;
    //public GameObject objHighlight;

    //private Vector3 touchPosition;
    private Vector3 prevTouchPosition;

    [HideInInspector]
    public GameObject hitObj;
    private Mesh hitMesh;
    private Vector3[] hitVertices;
    private Vector2[] hituv;
    private int[] hitTriangles;
    private Transform hitTransform;
    private int hitVerticesNum;
    private int hitTrianglesNum;
    private Renderer hitRenderer;
    private Vector3 centerToHitFace;

    private Mesh highlight;
    private MeshRenderer mr;

    private bool isHit = false;

    //selection
    private List<int> selected = new List<int>();
    private int selectedFaceIndex;
    private int faceNum;
    private Vector3 selectedNormal;
    private Vector3[] selectedVertices = new Vector3[3];

    private List<int> edgeVertices;
    private int edgeLength;

    //focus
    private Vector3 axisToFocus;
    private float angleToFocus;
    private Vector3 posToFocus;
    private const float focusSpeed = 25;

    // Start is called before the first frame update
    void Start()
    {
        highlight = new Mesh();
        highlight.name = "HL";
        GetComponent<MeshFilter>().mesh = highlight;
        //objHighlight.GetComponent<MeshFilter>().mesh = highlight;
        mr = GetComponent<MeshRenderer>();
        //mr = objHighlight.GetComponent<MeshRenderer>();

        highlight.vertices = new Vector3[3];
        highlight.uv = new Vector2[3];
        highlight.triangles = new int[] { 0, 1, 2 };

        prevTouchPosition = new Vector3(10000, 10000, 10000);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ProcessFaceSelect(Vector3 tp)
    {
        //Debug.Log("dy6- mousPosistion: " + tp);
        castRay(tp);
        //focus();
    }

    /* #region Construct highlight */
    private void castRay(Vector3 tp)
    {
        Vector3 touchPos = tp;
        touchPos.x -= Screen.width / 2;
        touchPos.y -= Screen.height / 2;
        touchPos.z = 0;
        touchPos *= Camera.main.orthographicSize / (Screen.height / 2);

        Debug.DrawLine(
            cam.gameObject.transform.position,
            cam.gameObject.transform.position + 10 * (touchPos - cam.gameObject.transform.position),
            Color.blue,
            0.01f,
            true
        );

        RaycastHit hit;
        if (!Physics.Raycast(cam.gameObject.transform.position, touchPos - cam.gameObject.transform.position, out hit))
        {
            isHit = false;
        }
        else
        {
            MeshCollider meshCollider = hit.collider as MeshCollider;
            if (meshCollider != null && meshCollider.sharedMesh != null)
            {
                hitObj = meshCollider.gameObject;
                hitMesh = meshCollider.sharedMesh;
                selectedFaceIndex = hit.triangleIndex;
                selectedNormal = hit.normal;
                hitVertices = hitMesh.vertices;
                hitVerticesNum = hitVertices.Length;
                hituv = hitMesh.uv;
                hitTriangles = hitMesh.triangles;
                hitTrianglesNum = hitTriangles.Length;
                hitTransform = hit.collider.transform;
                hitRenderer = hitObj.GetComponent<Renderer>();
                Debug.Log("dy6- name: " + meshCollider.gameObject.name);
                Debug.Log("dy6- idx: " + selectedFaceIndex + " num: " + hitTrianglesNum);
                
                int ta = hitTriangles[selectedFaceIndex * 3 + 0];
                Debug.Log("dy6- hitTriangles Length: " + hitTriangles.Length + " ta: " + ta);
                Vector3 tb = hitVertices[hitTriangles[selectedFaceIndex * 3 + 0]];
                selectedVertices[0] = hitVertices[hitTriangles[selectedFaceIndex * 3 + 0]];
                selectedVertices[1] = hitVertices[hitTriangles[selectedFaceIndex * 3 + 1]];
                selectedVertices[2] = hitVertices[hitTriangles[selectedFaceIndex * 3 + 2]];

                findPosition();
                findCoplanar();
                constructHighlight();
                
            }
            isHit = true;
        }
    }

    private void findPosition()
    {
        centerToHitFace =
            hitTransform.InverseTransformPoint(
                    selectedNormal + hitObj.transform.position
                ).normalized *
            dotProduct(
                hitTransform.InverseTransformPoint(
                    selectedNormal + hitObj.transform.position
                ).normalized,
                (hitVertices[hitTriangles[selectedFaceIndex * 3 + 0]])
            );
        posToFocus = new Vector3(0, 0, (hitTransform.TransformPoint(centerToHitFace) - hitObj.transform.position).magnitude + 0.1f);
        Debug.Log("dy6- posToFocs: " + posToFocus);
    }

    private void findCoplanar()
    {
        int n = hitTrianglesNum / 3;
        Vector3 localNormal = crossProduct(selectedVertices[0] - selectedVertices[1], selectedVertices[0] - selectedVertices[2]);

        selected.Clear();
        bool[] isVisited = new bool[n];
        for (int i = 0; i < n; i++)
        {
            isVisited[i] = false;
        }
        Queue<int> bs = new Queue<int>();
        bs.Enqueue(selectedFaceIndex);
        while (bs.Count > 0)
        {
            int idx = bs.Peek();
            isVisited[idx] = true;
            bs.Dequeue();
            Vector3 currentNormal = crossProduct(
                hitVertices[hitTriangles[idx * 3 + 0]] - hitVertices[hitTriangles[idx * 3 + 1]],
                hitVertices[hitTriangles[idx * 3 + 0]] - hitVertices[hitTriangles[idx * 3 + 2]]
            );
            if (localNormal.normalized == currentNormal.normalized)
            {
                selected.Add(idx);
                for (int i = 0; i < n; i++)
                {
                    if (!isVisited[i])
                    {
                        int matchCount = 0;
                        for (int v1 = 0; v1 < 3; v1++)
                        {
                            for (int v2 = 0; v2 < 3; v2++)
                            {
                                if (hitVertices[hitTriangles[idx * 3 + v1]] == hitVertices[hitTriangles[i * 3 + v2]])
                                {
                                    matchCount++;
                                }
                            }
                        }
                        if (matchCount >= 2)
                        {
                            bs.Enqueue(i);
                        }
                    }
                }
            }
        }
        faceNum = selected.Count;
        axisToFocus = crossProduct(selectedNormal, new Vector3(0, 0, -1));
        angleToFocus = Vector3.Angle(selectedNormal, new Vector3(0, 0, -1));
    }

    private void findEdge()
    {

        edgeVertices = new List<int>();

        int[] edgeLeft = new int[faceNum * 3];
        int[] edgeRight = new int[faceNum * 3];
        bool[] isDup = new bool[faceNum * 3];
        for (int i = 0; i < faceNum * 3; i++)
        {
            isDup[i] = false;
        }

        for (int i = 0; i < faceNum; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                edgeLeft[i * 3 + j] = hitTriangles[selected[i] * 3 + j];
                edgeRight[i * 3 + j] = hitTriangles[selected[i] * 3 + ((j + 1) % 3)];
            }
        }

        for (int i = 0; i < faceNum * 3; i++)
        {
            for (int j = i + 1; j < faceNum * 3; j++)
            {
                if (!isDup[i] && !isDup[j] &&
                    ( (hitVertices[edgeLeft[i]] == hitVertices[edgeLeft[j]] && hitVertices[edgeRight[i]] == hitVertices[edgeRight[j]]) ||
                      (hitVertices[edgeLeft[i]] == hitVertices[edgeRight[j]] && hitVertices[edgeRight[i]] == hitVertices[edgeLeft[j]]) )
                )
                {
                    isDup[i] = true;
                    isDup[j] = true;
                }
            }
        }

        int startVertex = 0;
        for (int i = 0; i < faceNum * 3; i++)
        {
            if (!isDup[i])
            {
                startVertex = i;
                break;
            }
        }

        edgeVertices.Add(edgeLeft[startVertex]);
        edgeVertices.Add(edgeRight[startVertex]);
        isDup[startVertex] = true;
        bool done = false;
        while (!done)
        {
            done = true;
            for (int i = 0; i < faceNum * 3; i++)
            {
                if (!isDup[i])
                {
                    if (hitVertices[edgeLeft[i]] == hitVertices[edgeVertices[edgeVertices.Count - 1]])
                    {
                        edgeVertices.Add(edgeRight[i]);
                        isDup[i] = true;
                        done = false;
                        break;
                    }
                    else if (hitVertices[edgeRight[i]] == hitVertices[edgeVertices[edgeVertices.Count - 1]])
                    {
                        edgeVertices.Add(edgeLeft[i]);
                        isDup[i] = true;
                        done = false;
                        break;
                    }
                }
            }
        }
        edgeVertices.RemoveAt(edgeVertices.Count - 1);
        edgeLength = edgeVertices.Count;

    }

    public void focus()
    {
        float deltaAngle = angleToFocus - Mathf.Lerp(angleToFocus, 0, focusSpeed * Time.deltaTime);
        angleToFocus -= deltaAngle;
        //hitObj.transform.rotation = Quaternion.AngleAxis(deltaAngle, axisToFocus) * hitObj.transform.rotation;
        //obj.transform.rotation = Quaternion.AngleAxis(deltaAngle, axisToFocus) * obj.transform.rotation;
        touchProcessor.GetComponent<TouchProcessor>().rot = Quaternion.AngleAxis(deltaAngle, axisToFocus) * obj.transform.rotation;
        constructHighlight();
        if (Mathf.Abs(angleToFocus) < 0.01f)
        {
            angleToFocus = 0;
            touchProcessor.GetComponent<TouchProcessor>().enterSelectionPMode('f');
            highlight.Clear();
        }
        //hitObj.transform.position = Vector3.Lerp(hitObj.transform.position, posToFocus, focusSpeed * Time.deltaTime);
        //Vector3 deltaPos = Vector3.Lerp(hitObj.transform.position, posToFocus, focusSpeed * Time.deltaTime) - hitObj.transform.position;
        //touchProcessor.GetComponent<TouchProcessor>().pos += deltaPos;
        /*
        if (!hitObj.GetComponent<ObjectController>())
        {
            hitObj.GetComponent<ObjectController>().isTransformUpdated = true;
        }*/
    }

    private void constructHighlight()
    {
        highlight.Clear();

        highlight.vertices = new Vector3[faceNum * 3];
        highlight.triangles = new int[faceNum * 3];
        Vector3[] vertexTemps = highlight.vertices;
        int[] triangleTemps = highlight.triangles;
        for (int i = 0; i < faceNum * 3; i++)
        {
            triangleTemps[i] = i;
        }
        for (int i = 0; i < faceNum; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                vertexTemps[i * 3 + j] = hitVertices[hitTriangles[selected[i] * 3 + j]];
                vertexTemps[i * 3 + j] = hitTransform.TransformPoint(vertexTemps[i * 3 + j]) + 0.01f * selectedNormal.normalized;
            }
        }
        highlight.vertices = vertexTemps;
        highlight.triangles = triangleTemps;
        highlight.MarkModified();
        highlight.RecalculateNormals();
    }

    public void clearHighlight()
    {
        highlight.Clear();
    }
    /* #endregion */

    /* #region Calculator */
    private Vector3 crossProduct(Vector3 a, Vector3 b)
    {
        return new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
    }

    private float dotProduct(Vector3 a, Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }

    private Vector3 intersectLinePlane(Vector3 a, Vector3 b, Vector3 p, Vector3 n)
    {   //line passes a and b, plane passes p with normal n
        float t = (dotProduct(p, n) - dotProduct(a, n)) / dotProduct(n, a - b);
        return a + t * (a - b);
    }

    private bool areLinesIntersect(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2)
    {
        bool result = true;
        if (crossProduct(a2 - a1, b2 - a2).magnitude > 0 &&
            crossProduct(a2 - a1, b1 - a2).magnitude > 0 &&
            Vector3.Angle(crossProduct(a2 - a1, b2 - a2), crossProduct(a2 - a1, b1 - a2)) < 0.001f
        )
        {
            result = false;
        }
        if (crossProduct(b2 - b1, a1 - b2).magnitude > 0 &&
            crossProduct(b2 - b1, a2 - b2).magnitude > 0 &&
            Vector3.Angle(crossProduct(b2 - b1, a1 - b2), crossProduct(b2 - b1, a2 - b2)) < 0.001f
        )
        {
            result = false;
        }
        return result;
    }
    /* #endregion */

}
