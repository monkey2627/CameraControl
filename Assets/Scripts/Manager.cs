using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***
 �����㷨�Ŀ�����
 ***/
public class Manager : MonoBehaviour
{

    public float sampleFilterScore = 0.5f;
    float time = 3;
    float s = 0;
    void Start()
    {
        Init();
        Debug.Log("hahah");
        GameObject go = Instantiate(Resources.Load("SamplePoint")) as GameObject;
        go.transform.SetParent(GameObject.Find("SamplePoints").transform);
     
        Debug.Log("Finish Init");
        //����python������
        // connect.Connect2server();

        //���û��ж�֮ǰ��Ԥ�������pos + view ������
        SampleThroughWay.instance.GetMap();//��ʼ����ͼ�������������ϻ�����,�õ�Map[Point]
        SampleThroughWay.instance.GenerateSampleCenters();//�õ����еĲ������ĵ�
       Debug.Log("GetAllSampleCenters");
        SampleThroughWay.instance.StartGetSamplePointsThroughWay();//���ݲ������ĵ���� pos + view ������
       // SampleThroughWay.instance.FilterSamplePoint(sampleFilterScore);//ɾ���ر𲻺õĲ�����Ͳ����ӽǣ���������ڵ�·������

        //׼�������û���������ʼ��������
        //Voice.instance.StartRecord();

    }
    private void Init()
    {
        //ע�⣬��Щ���instanceӦ����awake�׶ξ͸�ֵ
        SampleThroughWay.instance.Init();
        Connect2Python.instance.Init();
       // GenerateSampleCenters.instance
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
