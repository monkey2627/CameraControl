using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{
    private SamplePointsGenerater spg;
    private Connect2Python connect;

    float time = 3;
    float s = 0;
    void Start()
    {
        Init();
        //先与python端连接
       // connect.Connect2server();
        spg.generateAllSamplePoints();
        //spg.removeLowScore();
        spg.startGenerateNextCenter();
      

    }
    private void Init()
    {
        spg = gameObject.GetComponent<SamplePointsGenerater>();
        spg.Init();
        connect = gameObject.GetComponent<Connect2Python>();
        connect.Init();

    }

    // Update is called once per frame
    void Update()
    {
        s += Time.deltaTime;
        if(s > 10)
        {
            spg.removeLowScore();
        }
    }
}
