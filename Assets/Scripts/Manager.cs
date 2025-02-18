using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***
 整个算法的控制器
 ***/
public class Manager : MonoBehaviour
{

    public bool needSample = false;
    public bool needConnection = false;
    public float sampleFilterScore = 0.5f;
    void Start()
    {
        Init();
        GameObject go = Instantiate(Resources.Load("SamplePoint")) as GameObject;
        go.transform.SetParent(GameObject.Find("SamplePoints").transform);
     
        Debug.Log("Finish Init");
        //先与python端连接
        if(needConnection)
             Connect2Python.instance.Connect2server();
        if (needSample)
        {
            //在用户行动之前，预打好所有pos + view 的评分
            SampleThroughWay.instance.GetMap();//初始化地图，在整个界面上画格子,得到Map[Point]
            SampleThroughWay.instance.GenerateSampleCenters();//得到所有的采样中心点
            Debug.Log("GetAllSampleCenters");
            SampleThroughWay.instance.StartGetSamplePointsThroughWay();//根据采样中心点采样 pos + view 并评分
            SampleThroughWay.instance.FilterSamplePoint(sampleFilterScore);//删除特别不好的采样点和采样视角，不参与后期的路径生成
            Debug.Log("FinishFilter");
        }
        //准备接收用户反馈，开始语音监听
        //Voice.instance.StartRecord();

    }
    private void Init()
    {
        //注意，这些类的instance应该在awake阶段就赋值
        SampleThroughWay.instance.Init();
        Connect2Python.instance.Init();
       // GenerateSampleCenters.instance
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
