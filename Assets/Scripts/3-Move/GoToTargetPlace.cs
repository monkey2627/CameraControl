using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//挂载在每个起点上，点击起点自动从起点开始移动
public class GoToTargetPlace : MonoBehaviour
{
    private float fixWidth;
    private float fixHeight;
    private int cameraSize = 0;
    public Camera travelCamera;
    public GameObject mapCamera;
    public GameObject meshOrigin;

    // Start is called before the first frame update
    void Start()
    {
        travelCamera = GameObject.Find("Main Camera").GetComponent<Camera>();
        mapCamera = GameObject.Find("mapCamera");
        meshOrigin = GameObject.Find("meshOrigin");
        cameraSize = (int) mapCamera.GetComponent<Camera>().orthographicSize;
        meshOrigin.transform.position = new Vector3(-cameraSize, meshOrigin.transform.position.y, -cameraSize);
        fixHeight = gameObject.transform.parent.parent.GetComponent<RectTransform>().rect.height;
        fixWidth = gameObject.transform.parent.parent.GetComponent<RectTransform>().rect.width;
    }
    public void Go()
    {
        //以左下角为0，0
        float x = fixWidth / 2 + transform.localPosition.x;
        float y = fixHeight / 2 + transform.localPosition.y;

        float new_x = meshOrigin.transform.position.x + mapCamera.GetComponent<Camera>().orthographicSize * 2 * (x / fixWidth);
        float new_z = meshOrigin.transform.position.z + mapCamera.GetComponent<Camera>().orthographicSize * 2 * (y / fixHeight);
        travelCamera.transform.position = new Vector3(new_x,SampleThroughWay.instance.tall ,new_z);
        Debug.Log("go");
        //将起始点作为路径的第一个关键点
        SampleThroughWay.instance.Add2WayRecorder(travelCamera.transform.position);
        //设置初始的角度
        SampleThroughWay.instance.lastAngel = travelCamera.transform.eulerAngles;
        //设置相机的位置为起始点，生成路径
        SampleThroughWay.instance.GenerateWay();

    }
}
