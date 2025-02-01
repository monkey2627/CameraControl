using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class goToTargetPlace : MonoBehaviour
{
    private float fixWidth;
    private float fixHeight;
    private int cameraSize = 0;
    public Camera camera;
    public GameObject mapCamera;
    public GameObject center;
    private sampleThroughWay stw;
    private 

    // Start is called before the first frame update
    void Start()
    {
        mapCamera = GameObject.Find("mapCamera");
        stw = GameObject.Find("Main-Manager").GetComponent<sampleThroughWay>();
        cameraSize = (int) mapCamera.GetComponent<Camera>().orthographicSize;
        center.transform.position = new Vector3(-cameraSize, center.transform.position.y, -cameraSize);
        fixHeight = gameObject.transform.parent.parent.GetComponent<RectTransform>().rect.height;
        fixWidth = gameObject.transform.parent.parent.GetComponent<RectTransform>().rect.width;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Go()
    {
        //以左下角为0，0
        float x = fixWidth / 2 + transform.localPosition.x;
        float y = fixHeight / 2 + transform.localPosition.y;

        float new_x = center.transform.position.x + mapCamera.GetComponent<Camera>().orthographicSize * 2 * (x / fixWidth);
        float new_z = center.transform.position.z + mapCamera.GetComponent<Camera>().orthographicSize * 2 * (y / fixHeight);
        camera.transform.position = new Vector3(new_x,stw.tall ,new_z);
        
        //将起始点作为路径的第一个关键点
        stw.Add2WayRecorder(camera.transform.position);

        stw.lastAngel = camera.transform.rotation;
        //设置相机的位置为起始点，然后开始采样
        stw.GenerateWay();

    }
}
