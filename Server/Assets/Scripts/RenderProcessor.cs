using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderProcessor : MonoBehaviour
{
    // quad vars
    private Vector3[] normalsQuad = {
                Vector3.forward, Vector3.forward,
                Vector3.forward, Vector3.forward
            };
    private Vector2[] uvsQuad = {
                new Vector2(0, 0), -new Vector2(1, 0),
                new Vector2(1, 1), new Vector2(0, 1)
            };
    //private int[] indicesQuad = new int[6] { 0, 1, 2, 1, 2, 3 };
    private int[] indicesQuad = new int[6] { 0, 1, 2, 1, 3, 2 };

    // double-quad vars
    //private int[] indicesDoubleQuad = new int[] { 0, 1, 2, 2, 3, 0, 4, 7, 6, 6, 5, 4 };
    private int[] indicesDoubleQuad = new int[] {   0, 1, 2, 1, 3, 2,
                                                    4, 6, 5, 5, 6, 7 };
    private Vector2[] uvsDoubleQuad = new Vector2[] {
        Vector2.zero, new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 0),//正面
        new Vector2(0.5f, 0), new Vector2(0.5f, 1),Vector2.one, new Vector2(1, 0)  //反面
        };

    // triangle vars
    private Vector3[] normalsTri = {
                -Vector3.forward, -Vector3.forward,-Vector3.forward
            };
    private Vector2[] uvsTri = {
                new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1)
            };
    private int[] indicesTri = new int[3] { 0, 1, 2 };

    // line var
    private float lineWidth = 0.1f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public LineRenderer initLineRenderer(LineRenderer lr)
    {
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.alignment = LineAlignment.TransformZ;
        lr.startWidth = lr.endWidth = lineWidth;
        lr.startColor = new Color(1, 1, 1, 0.8f);
        lr.endColor = new Color(1, 1, 1, 0.8f);
        lr.enabled = false;
        return lr;
    }
    
    public MeshRenderer initMeshRenderer(MeshRenderer mr)
    {
        //mr.material = new Material(Shader.Find("Sprites/Default"));
        mr.material.color = new Color32(255, 255, 255, 100);
        mr.enabled = false;
        return mr;
    }

    public void updateLine(bool flag, int cntPos, Vector3[] vertics,
        out LineRenderer outLr)
    {
        LineRenderer lr = new LineRenderer();
        lr.positionCount = cntPos;
        for(int i = 0; i < cntPos; i++)
        {
            lr.SetPosition(i, vertics[i]);
        }
        outLr = lr;
    }

    public void updateQuad(bool flag, Vector3[] vertices, 
        out Mesh outMesh)
    {
        Mesh mesh = new Mesh();
        if (flag)
        {
            mesh.vertices = vertices; 
            mesh.normals = normalsQuad;
            mesh.uv = uvsQuad;
            mesh.triangles = indicesQuad;
            outMesh = mesh;
        }
        else
        {
            outMesh = mesh;
        }
    }

    public void updateDoubleQuad(bool flag, Vector3[] vertices,
        out Mesh outMesh)
    {
        Mesh mesh = new Mesh();
        if (flag)
        {
            Debug.Log("dy6- len: " + vertices.Length);
            mesh.vertices = vertices;
            //mesh.normals = normalsQuad;
            mesh.uv = uvsDoubleQuad;
            mesh.triangles = indicesDoubleQuad;
            outMesh = mesh;
        }
        else
        {
            outMesh = mesh;
        }
    }

    public void updateTri(bool flag, Vector3[] vertices,
        out Mesh outMesh)
    {
        Mesh mesh = new Mesh();
        if (flag)
        {
            mesh.vertices = vertices;
            mesh.normals = normalsTri;
            mesh.uv = uvsTri;
            mesh.triangles = indicesTri;
            outMesh = mesh;
        }
        else
        {
            outMesh = mesh;
        }
    }
}
