using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 //�����㷨�Ŀ�����
public class Manager : MonoBehaviour
{

    private sampleThroughWay stw;
    //private SamplePointsGenerater spg;
    private Connect2Python connect;
    public float sampleFilterScore = 0.5f;
    float time = 3;
    float s = 0;
    void Start()
    {
        Init();
        Debug.Log("Finish Init");
        //����python������
        // connect.Connect2server();
        stw.GetMap();
        Debug.Log("Finish Init Map");
        stw.GetCourseWay();
        Debug.Log("get course way");
        //stw.GetSamplePointsThroughWay();
        //stw.FilterSamplePoint(sampleFilterScore);
        //׼�������û�����

    }
    private void Init()
    {
        stw = gameObject.GetComponent<sampleThroughWay>();
        stw.Init();
        connect = gameObject.GetComponent<Connect2Python>();
        connect.Init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
