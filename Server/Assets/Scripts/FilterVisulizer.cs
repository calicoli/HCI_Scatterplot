using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterVisulizer : MonoBehaviour
{
    //public GameObject lineContainer;
    //public GameObject meshContainer;
    
    private GameObject quad1, quad2;
    private MeshRenderer mr1, mr2;
    private MeshFilter mf1, mf2;

    // Start is called before the first frame update
    void Start()
    {
        initQuad();   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void initQuad()
    {
        quad1 = this.transform.GetChild(0).gameObject;
        quad2 = this.transform.GetChild(1).gameObject;

        mr1 = quad1.GetComponent<MeshRenderer>();
        mr1.material = new Material(Shader.Find("Sprites/Default"));
        mr1.material.color = new Color32(255, 255, 255, 100);
        mr1.enabled = false;

        quad1.AddComponent<MeshFilter>();
        mf1 = quad1.GetComponent<MeshFilter>();

        mr2 = quad2.GetComponent<MeshRenderer>();
        mr2.material = new Material(Shader.Find("Sprites/Default"));
        mr2.material.color = new Color32(255, 255, 255, 100);
        mr2.enabled = false;

        quad2.AddComponent<MeshFilter>();
        mf2 = quad2.GetComponent<MeshFilter>();

    }

    public void enableQuad(bool flag)
    {
        mr1.enabled = mr2.enabled = flag;
    }
    public void updateQuad(bool flag, Vector3[] vertices1, Vector3[] vertics2)
    {
        Vector3[] normals = {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
        Vector2[] uvs = {
                new Vector2(0, 0),
                -new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
        int[] indices = new int[6] { 0, 1, 2, 0, 2, 3 };
        if (flag)
        {
            Mesh mesh1 = new Mesh();
            Mesh mesh2 = new Mesh();
            mesh1.vertices = vertices1; mesh2.vertices = vertics2;
            mesh1.normals = normals;    mesh2.normals = normals;
            mesh1.uv = uvs;             mesh2.uv = uvs;
            mesh1.triangles = indices;  mesh2.triangles = indices;
            mf1.mesh = mesh1;           mf2.mesh = mesh2;
            // important!
            mr1.enabled = true;         mr2.enabled = true;
        }
        else
        {
            mr1.enabled = false;        mr2.enabled = false;
        }

    }
}
