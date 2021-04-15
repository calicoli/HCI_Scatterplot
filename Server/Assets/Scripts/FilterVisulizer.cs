using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterVisulizer : MonoBehaviour
{
    //public GameObject lineContainer;
    //public GameObject meshContainer;
    public GameObject renderProcessor;

    private GameObject xquad1, xquad2, yquad1, yquad2, zquad1, zquad2;
    private MeshRenderer xmr1, xmr2, ymr1, ymr2, zmr1, zmr2;
    private MeshFilter   xmf1, xmf2, ymf1, ymf2, zmf1, zmf2;

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
        xquad1 = this.transform.GetChild(0).gameObject;
        xquad2 = this.transform.GetChild(1).gameObject;
        yquad1 = this.transform.GetChild(2).gameObject;
        yquad2 = this.transform.GetChild(3).gameObject;
        zquad1 = this.transform.GetChild(4).gameObject;
        zquad2 = this.transform.GetChild(5).gameObject;

        xmr1 = xquad1.GetComponent<MeshRenderer>();
        xmr2 = xquad2.GetComponent<MeshRenderer>();
        ymr1 = yquad1.GetComponent<MeshRenderer>();
        ymr2 = yquad2.GetComponent<MeshRenderer>();
        zmr1 = zquad1.GetComponent<MeshRenderer>();
        zmr2 = zquad2.GetComponent<MeshRenderer>();

        xmf1 = xquad1.GetComponent<MeshFilter>();
        xmf2 = xquad2.GetComponent<MeshFilter>();
        ymf1 = yquad1.GetComponent<MeshFilter>();
        ymf2 = yquad2.GetComponent<MeshFilter>();
        zmf1 = zquad1.GetComponent<MeshFilter>();
        zmf2 = zquad2.GetComponent<MeshFilter>();

        renderProcessor.GetComponent<RenderProcessor>().initMeshRenderer(xmr1);
        renderProcessor.GetComponent<RenderProcessor>().initMeshRenderer(xmr2);
        renderProcessor.GetComponent<RenderProcessor>().initMeshRenderer(ymr1);
        renderProcessor.GetComponent<RenderProcessor>().initMeshRenderer(ymr2);
        renderProcessor.GetComponent<RenderProcessor>().initMeshRenderer(zmr1);
        renderProcessor.GetComponent<RenderProcessor>().initMeshRenderer(zmr2);
    }

    public void enableQuad(char ch, bool flag)
    {
        if(ch == 'x')
        {
            xmr1.enabled = xmr2.enabled = flag;
        }
        if (ch == 'y')
        {
            ymr1.enabled = ymr2.enabled = flag;
        }
        if (ch == 'z')
        {
            zmr1.enabled = zmr2.enabled = flag;
        }
    }

    public void updateQuad(char ch, bool flag, Vector3[] vertices1, Vector3[] vertices2)
    {
        int[] indices = new int[6] { 0, 1, 2, 0, 2, 3 };
        Mesh mesh1 = new Mesh();
        Mesh mesh2 = new Mesh();
        if (flag)
        {
            renderProcessor.GetComponent<RenderProcessor>().
                updateQuad(true, vertices1, out mesh1);
            renderProcessor.GetComponent<RenderProcessor>().
                updateQuad(true, vertices2, out mesh2);
        } else
        {
            renderProcessor.GetComponent<RenderProcessor>().
                updateQuad(false, vertices1, out mesh1);
            renderProcessor.GetComponent<RenderProcessor>().
                updateQuad(false, vertices2, out mesh2);
        }

        if (ch == 'x')
        {
            mesh1.name = "x-quad1";
            mesh2.name = "x-quad2";
            xmf1.mesh = mesh1; xmf2.mesh = mesh2;
            
        }
        else if (ch == 'y')
        {
            mesh1.name = "y-quad1";
            mesh2.name = "y-quad2";
            ymf1.mesh = mesh1; ymf2.mesh = mesh2;
           
        }
        else if (ch == 'z')
        {
            mesh1.name = "z-quad1";
            mesh2.name = "z-quad2";
            zmf1.mesh = mesh1; zmf2.mesh = mesh2;

        }

    }
}
