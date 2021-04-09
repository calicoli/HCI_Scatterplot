//Attach this script to an empty GameObject. Create 2 more GameObjects and attach a Collider component on each. Choose these as the "My Object" and "New Object" in the Inspector.
//This script allows you to move your main GameObject left to right. If it intersects with the other, it outputs the message to the Console.

/*
using UnityEngine;

public class BoundsIntersectExample : MonoBehaviour
{
    public GameObject m_MyObject, m_NewObject;
    Collider m_Collider, m_Collider2;

    void Start()
    {
        //Check that the first GameObject exists in the Inspector and fetch the Collider
        if (m_MyObject != null)
            m_Collider = m_MyObject.GetComponent<Collider>();

        //Check that the second GameObject exists in the Inspector and fetch the Collider
        if (m_NewObject != null)
            m_Collider2 = m_NewObject.GetComponent<Collider>();
    }

    void Update()
    {
        //If the first GameObject's Bounds enters the second GameObject's Bounds, output the message
        if (m_Collider.bounds.Intersects(m_Collider2.bounds))
        {
            Debug.Log("dy5- Bounds intersecting");
        }
        if (m_Collider.bounds.Contains(m_Collider2.bounds.min) &&
            m_Collider.bounds.Contains(m_Collider2.bounds.max))
        {566666666666666666666666666666
            Debug.Log("dy5- Bounds Containing");
        }
    }
}
*/

using UnityEngine;
using System.Collections;

public class BoundsIntersectExample : MonoBehaviour
{

    public MeshCollider meshCollider;
    public bool In;
    public bool concaveHull;
    public float distance = 100f;

    Ray right, left, up, down, forward, back, tempRay;
    bool r, l, u, d, f, b;

    RaycastHit rightHit = new RaycastHit();
    RaycastHit leftHit = new RaycastHit();
    RaycastHit upHit = new RaycastHit();
    RaycastHit downHit = new RaycastHit();
    RaycastHit forwardHit = new RaycastHit();
    RaycastHit backHit = new RaycastHit();
    RaycastHit tempHit = new RaycastHit();

    void Start()
    {

        right = new Ray(Vector3.zero, -Vector3.right);
        left = new Ray(Vector3.zero, -Vector3.left);
        up = new Ray(Vector3.zero, -Vector3.up);
        down = new Ray(Vector3.zero, -Vector3.down);
        forward = new Ray(Vector3.zero, -Vector3.forward);
        back = new Ray(Vector3.zero, -Vector3.back);
        tempRay = new Ray();

    }

    bool ConcaveHull(Ray ray, RaycastHit hit)
    {


        tempRay.origin = transform.position;
        tempRay.direction = -ray.direction;
        float customDistance = distance - hit.distance;
        int lastPoint = hit.triangleIndex;

        while (meshCollider.Raycast(tempRay, out tempHit, customDistance))
        {

            if (tempHit.triangleIndex == lastPoint) break;
            lastPoint = tempHit.triangleIndex;
            customDistance = tempHit.distance;
            ray.origin = -ray.direction * customDistance + transform.position;

            if (!meshCollider.Raycast(ray, out tempHit, customDistance))
            {

                concaveHull = true;
                return true;

            }

            if (tempHit.triangleIndex == lastPoint) break;
            lastPoint = tempHit.triangleIndex;
            customDistance -= tempHit.distance;

        }

        return false;

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        right.origin = -right.direction * distance + transform.position;
        left.origin = -left.direction * distance + transform.position;
        up.origin = -up.direction * distance + transform.position;
        down.origin = -down.direction * distance + transform.position;
        forward.origin = -forward.direction * distance + transform.position;
        back.origin = -back.direction * distance + transform.position;

        r = meshCollider.Raycast(right, out rightHit, distance);
        l = meshCollider.Raycast(left, out leftHit, distance);
        u = meshCollider.Raycast(up, out upHit, distance);
        d = meshCollider.Raycast(down, out downHit, distance);
        f = meshCollider.Raycast(forward, out forwardHit, distance);
        b = meshCollider.Raycast(back, out backHit, distance);

        if (r && l && u && d && f && b)
        {

            if (ConcaveHull(right, rightHit)) In = false;
            else if (ConcaveHull(left, leftHit)) In = false;
            else if (ConcaveHull(up, upHit)) In = false;
            else if (ConcaveHull(down, downHit)) In = false;
            else if (ConcaveHull(forward, forwardHit)) In = false;
            else if (ConcaveHull(back, backHit)) In = false;
            else { In = true; concaveHull = false; }

        }
        else In = false;

    }

}