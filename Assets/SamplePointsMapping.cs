using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CustomUI;

public class SamplePointsMapping : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] startPoints;
    private int num = 0;
    private bool add = false;
    void Start()
    {
        GameObject[] startPoints = new GameObject[1000];
    }
    public void enableAdd()
    {
        add = true;
    }
    public void AddStartPoints()
    {
        if (!add) return;
        //��������Ļ�ϵĴ�С
        Vector2 pos =  Input.mousePosition;
        GameObject go = GameObject.Instantiate(Resources.Load("CircularImage")) as GameObject;
        go.transform.parent = GameObject.Find("StartPoints").transform;
        go.transform.position = pos;
        startPoints[num++] = go;

    }
    public void RemoveStartPoints()
    {

    }
    // Update is called once per frame
    void Update()
    {
        //ui��position����������Ļ������꣬localposition�븸�����pivot�йأ�����
       // Debug.Log(GameObject.Find("CircularImage").transform.localPosition); 
    }
}
