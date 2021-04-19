using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChinarMergeMesh : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        MergeMesh();
    }



    private void MergeMesh()
    {
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();   //获取 所有子物体的网格
        CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length]; //新建一个合并组，长度与 meshfilters一致
        for (int i = 0; i < meshFilters.Length; i++)                                  //遍历
        {
            combineInstances[i].mesh = meshFilters[i].sharedMesh;                   //将共享mesh，赋值
            combineInstances[i].transform = meshFilters[i].transform.localToWorldMatrix; //本地坐标转矩阵，赋值
        }
        Mesh newMesh = new Mesh();                                  //声明一个新网格对象
        newMesh.CombineMeshes(combineInstances);                    //将combineInstances数组传入函数
        gameObject.AddComponent<MeshFilter>().sharedMesh = newMesh; //给当前空物体，添加网格组件；将合并后的网格，给到自身网格
        //到这里，新模型的网格就已经生成了。运行模式下，可以点击物体的 MeshFilter 进行查看网格

        #region 以下是对新模型做的一些处理：添加材质，关闭所有子物体，添加自转脚本和控制相机的脚本

        //gameObject.AddComponent<MeshRenderer>().material = Resources.Load<Material>("Materials/Koala"); //给当前空物体添加渲染组件，给新模型网格上色;
        //foreach (Transform t in transform)                                                              //禁用掉所有子物体
        //{
        //    t.gameObject.SetActive(false);
        //}
        //gameObject.AddComponent<BallRotate>();
        //Camera.main.gameObject.AddComponent<ChinarCamera>().pivot = transform;

        #endregion
    }
}
