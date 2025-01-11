using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SamplePointsGenerater : MonoBehaviour
{
    //格子个数,每行几个，每列几个
    public int heightNum = 10;
    public int widthNum = 10;
    public static int number = 30;
    private float step_w;
    private float step_h;

    //UI上的navigationMap
    private GameObject navigationMap;


    //装所有边界点的父物体
    public GameObject frameObjectFather;
    public List<GameObject> boundaryPoints; //为了排除不在博物馆内部的边界点
    //相机   
    public GameObject cameraOnUI;//在UI上代表移动中的摄像机的圆点
    public GameObject sampleCamera;//用来采样的相机
    public float tall = 1.7f; //相机的高度
    public List<Quaternion> quaternions = new List<Quaternion>();  //相机的旋转角度
    //采样点
    public SamplePoint[,,] allSamplePoints = new SamplePoint[1000, 1000, 6];
    public List<SamplePoint> choosedSamplePoints; // 根据当前相机(即中心点)的位置来添加未当过采样点的点进list

    //为了UI和真实位置的映射
    public GameObject leftDown;
    public float fixWidth;
    public float fixHeight;
    public int cameraSize = 0;
    private GameObject mapCamera;
    public GameObject[] lies;
    public GameObject[] hangs;
    private ViewManager viewManager;
    //生成路径移动
    private MoveThroughWay moveThroughWay;
    public Vector3 last;
    //显示图片
    public GameObject fatherForBag;
    // Start is called before the first frame update
    private Connect2Python connect;

    List<ForPicture> unsolvedSprite = new List<ForPicture>();
    public List<List<ForPicture>> forScore = new List<List<ForPicture>>();
    void Start()
    {

    }
    public void startGenerateNextCenter()
    {
        StartCoroutine("generateNextCenter");
    }
    public void Init()
    {
        heightNum = 10;
        widthNum = 10;

        choosedSamplePoints = new List<SamplePoint>();
        unsolvedSprite = new List<ForPicture>();
        forScore = new List<List<ForPicture>>();
        quaternions = new List<Quaternion>();
        for (int i = 0; i < 6; i++)
        {
            quaternions.Add(Quaternion.Euler(0, 360 / 6 * i, 0));
        }
        leftDown = GameObject.Find("downLeft");
        frameObjectFather = GameObject.Find("boundaryPoints");
        cameraOnUI = GameObject.Find("peopleUI"); ;//在UI上代表移动中的摄像机的圆点
        sampleCamera = GameObject.Find("SampleCamera");
        navigationMap = GameObject.Find("navigationMap");
        mapCamera = GameObject.Find("MapCamera");
        fatherForBag = GameObject.Find("fatherForCell");
        viewManager = gameObject.GetComponent<ViewManager>();
        connect = gameObject.GetComponent<Connect2Python>();
        moveThroughWay = GameObject.Find("MoveThroughWay").GetComponent<MoveThroughWay>();

        cameraSize = (int)mapCamera.GetComponent<Camera>().orthographicSize;
        fixHeight = navigationMap.GetComponent<RectTransform>().rect.height;
        fixWidth = navigationMap.GetComponent<RectTransform>().rect.width;

        getBoundaryPoints();
    }
    #region  根据边界点排除不可能的物体
    struct Point
    {
        public double x, y;
        public Point(double x = 0, double y = 0)
        {
            this.x = x;
            this.y = y;
        }

        //向量+
        public static Point operator +(Point b, Point a)
        {
            Point p = new Point(b.x, b.y);
            p.x += a.x;
            p.y += a.y;
            return p;
        }
        //向量-
        public static Point operator -(Point b, Point a)
        {
            Point p = new Point(b.x, b.y);
            p.y -= a.y;
            p.x -= a.x;
            return p;
        }
        //点积
        public static double operator *(Point b, Point a)
        {
            return a.x * b.x + a.y * b.y;
        }
        //叉积
        //P^Q>0,P在Q的顺时针方向；<0，P在Q的逆时针方向；=0，P，Q共线，可能同向或反向
        public static double operator ^(Point a, Point b)
        {
            return a.x * b.y - b.x * a.y;
        }
    };
    //得到所有的边界点
    public void getBoundaryPoints()
    {
        FindChild(frameObjectFather);
    }
    void FindChild(GameObject child)
    {
        //利用for循环 获取物体下的全部子物体
        for (int c = 0; c < child.transform.childCount; c++)
        {
            //如果子物体下还有子物体 就将子物体传入进行回调查找 直到物体没有子物体为止
            if (child.transform.GetChild(c).childCount > 0)
            {
                FindChild(child.transform.GetChild(c).gameObject);

            }
            boundaryPoints.Add(child.transform.GetChild(c).gameObject);
        }
    }
    const double eps = 1e-6;
    bool OnSegment(Point P1, Point P2, Point Q)
    {
        return dcmp((P1 - Q) ^ (P2 - Q)) == 0 && dcmp((P1 - Q) * (P2 - Q)) <= 0;
    }
    int dcmp(double x)
    {
        double y = x;
        if (x < 0) y = -x;
        if (y < eps) return 0;
        else
            return x < 0 ? -1 : 1;
    }
    bool InPolygon(Point P)
    {
        bool flag = false;
        int n = boundaryPoints.Count;
        Point P1, P2; //多边形一条边的两个顶点
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            P1 = new Point(boundaryPoints[i].transform.GetComponent<RectTransform>().anchoredPosition.x, boundaryPoints[i].transform.GetComponent<RectTransform>().anchoredPosition.y);
            P2 = new Point(boundaryPoints[j].transform.GetComponent<RectTransform>().anchoredPosition.x, boundaryPoints[j].transform.GetComponent<RectTransform>().anchoredPosition.y);


            if (OnSegment(P1, P2, P)) return true; //点在多边形一条边上
            if ((dcmp(P1.y - P.y) > 0 != dcmp(P2.y - P.y) > 0) && dcmp(P.x - (P.y - P1.y) * (P1.x - P2.x) / (P1.y - P2.y) - P1.x) < 0)
            {
                flag = !flag;
                //    Debug.Log("flip");
            }
        }
        return flag;
    }
    #endregion
    int size = 4;
    public void generateAllSamplePoints()
    {
        int num = 0;
        DateTime NowTime = DateTime.Now.ToLocalTime();
        //将时间格式化输出
        Debug.Log(NowTime.ToString("yyyy-MM-dd HH:mm:ss"));
        int w = (int)navigationMap.GetComponent<RectTransform>().rect.width;
        int h = (int)navigationMap.GetComponent<RectTransform>().rect.height;
        step_w = w * 1.0f / widthNum;
        step_h = h * 1.0f / heightNum;

        for (int i = 0; i < heightNum; i++)
        {
            for (int j = 0; j < widthNum; j++)
            {
                float min_x = i * step_w;
                float min_y = j * step_h;
                System.Random ra = new System.Random(unchecked((int)DateTime.Now.Ticks));
                for (int k = 0; k < 5; k++)//每个格子里抽 k 个
                {
                    //返回(0,1)之间的随机数
                    float tmp = (float)ra.NextDouble();
                    float tmp2 = (float)ra.NextDouble();
                    float uix = min_x + tmp * step_w;
                    float uiy = min_y + tmp2 * step_h;

                    //在我们规定的范围内
                    if (InPolygon(new Point(uix, uiy)))
                    {
                        GameObject go = show(uix, uiy);
                        //得到该采样点所有方向的图像
                        float realx = leftDown.transform.position.x + (uix / w) * mapCamera.GetComponent<Camera>().orthographicSize * 2;
                        float realz = leftDown.transform.position.z + (uiy / h) * mapCamera.GetComponent<Camera>().orthographicSize * 2;
                        //得到所有视角的sprite
                        getView(new Vector3(realx, tall, realz), i, j, k, go);
                        num = num + 6;
                    }
                    else
                    {
                        delete(i, j, k);
                    }

                    /*
                    //当没有处理过的图达到一定的数量，集中处理，并得到每个图的评分
                    if(unsolvedSprite.Count >= size * size)
                    {
                        viewManager.getMultipleView(size, unsolvedSprite);
                        //得到图之后传给llm然后得到评分再保存
                        //send
                        List<forPicture> t = new List<forPicture>();
                        for(int z = 0; z < size * size; z++)
                        {
                            t.Add(unsolvedSprite[0]);
                            unsolvedSprite.RemoveAt(0);
                        }
                        //将这次处理的所有图片的坐标信息存在 t 中，存入forscore
                        forScore.Add(t);
                        //将生成的总图片发给服务器端
                        connect.SendFrame();
                    }
                    
                    */
                }
            }
        }
        NowTime = DateTime.Now.ToLocalTime();
        //将时间格式化输出
        Debug.Log(NowTime.ToString("yyyy-MM-dd HH:mm:ss"));
        Debug.Log(num);
        sampleCamera.SetActive(false);
    }
    public void delete(int i, int j, int k)
    {
        for (int v = 0; v < 6; v++)
        {
            allSamplePoints[i, j, k].pos = new Vector3(-1, -1, -1);
            allSamplePoints[i, j, k].passed = false;
            allSamplePoints[i, j, k].valid = false;
            allSamplePoints[i, j, k].views = new List<View>();

        }
    }
    //去掉评分比较低的视角，如果全部视角的评分都低，就去掉这个点
    public void removeLowScore()
    {

        for (int i = 0; i < heightNum; i++)
        {
            for (int j = 0; j < widthNum; j++)
            {

                for (int k = 0; k < 5; k++)
                {
                    if (k == 2 || k == 4 || k == 0 || k == 1)
                    {
                        if (allSamplePoints[i, j, k].valid)
                        {
                            allSamplePoints[i, j, k].valid = false;
                            allSamplePoints[i, j, k].label.SetActive(false);
                            allSamplePoints[i, j, k].passed = true;
                        }


                    }
                }


            }
        }
    }

    //根据不同的方向拿到sprite
    public void getView(Vector3 pos, int i, int j, int k, GameObject go)
    {
        allSamplePoints[i, j, k].views = new List<View>();
        for (int v = 0; v < 6; v++)
        {
            sampleCamera.transform.position = pos;
            sampleCamera.transform.rotation = quaternions[v];
            allSamplePoints[i, j, k].pos = pos;
            allSamplePoints[i, j, k].label = go;
            allSamplePoints[i, j, k].valid = true;
            allSamplePoints[i, j, k].passed = false;
            //allSamplePoints[i, j, k].views.Add(new View() { score = 0, rot = quaternions[v] });
            allSamplePoints[i, j, k].views.Add(new View() { score = 0, sprite = ViewManager.CaptureCameraForTexture(sampleCamera.GetComponent<Camera>()), rot = quaternions[v] });
            unsolvedSprite.Add(new ForPicture() { i = i, j = j, k = k, v = v });
        }
    }
    
    //center是现实世界中的摄像机
    public void SetCenterAndSampling(GameObject center)
    {

        float x = (center.transform.position.x - leftDown.transform.position.x) / (cameraSize * 2) * fixWidth;
        float y = (center.transform.position.z - leftDown.transform.position.z) / (cameraSize * 2) * fixHeight;
        cameraOnUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

        chooseSamplePoints(30);

    }
    //按照格子顺序将待选点不停地加入chooSamplePoints中
    public void chooseSamplePoints(int number)
    {
        int width = 3;
        int height = 3;
        //根据现在中心点的位置来采样

        //判断中心点的位置处于哪个格子里
        float center_x = cameraOnUI.GetComponent<RectTransform>().anchoredPosition.x;
        float center_y = cameraOnUI.GetComponent<RectTransform>().anchoredPosition.y;
        int x_grid = (int)(center_x / step_w);
        int y_grid = (int)(center_y / step_h);
        //只在中心点附近格子处采样
        int min_grad_x = 0 > (x_grid - (int)width) ? 0 : (x_grid - (int)width);
        int min_grad_y = 0 > (y_grid - (int)height) ? 0 : (y_grid - (int)height);
        Debug.Log("x:" + x_grid);
        Debug.Log("y:" + y_grid);
        //先采自己所在格子
        for (int k = 0; k < 5; k++)//每个格子里采k个
        {
            int i = x_grid;
            int j = y_grid;
            int t = k;
            if (!allSamplePoints[i, j, t].passed && allSamplePoints[i, j, t].valid)
            {
                choosedSamplePoints.Add(allSamplePoints[i, j, t]);
                allSamplePoints[i, j, k].passed = true;
                allSamplePoints[i, j, t].label.GetComponent<CustomUI.CircularImage>().color = Color.green;
            }
        }
        for (int i = min_grad_x; i <= (widthNum - 1 > min_grad_x + width ? min_grad_x + width : widthNum - 1); i++)
            for (int j = min_grad_y; j <= (height - 1 > min_grad_y + height ? min_grad_y + height : heightNum - 1); j++)
            {
                for (int k = 0; k < 5; k++)//每个格子里采k个
                {
                    int t = k;
                    if (!allSamplePoints[i, j, t].passed && allSamplePoints[i, j, t].valid)
                    {
                        choosedSamplePoints.Add(allSamplePoints[i, j, t]);
                        allSamplePoints[i, j, k].passed = true;
                        allSamplePoints[i, j, t].label.GetComponent<CustomUI.CircularImage>().color = Color.green;
                    }
                }
            }
    }
    public Quaternion lastAngel;

    public List<Vector3> wayRecorder = new List<Vector3>(); //记录选择过的key路径，方便曲线生成
    //根据分数选择下一个应该移动到的采样点
    IEnumerator generateNextCenter()
    {
        while (true)
        {
            if (choosedSamplePoints.Count > 0)
            {
                //  int i = 0;
                //  while(choosedSamplePoints.Count > 1 && i < 3)
                //  {
                ////      choosedSamplePoints.RemoveAt(0);
                //    i++;
                // }
                Vector3 next = choosedSamplePoints[0].pos;
                //测试
                System.Random ra = new System.Random(unchecked((int)DateTime.Now.Ticks));
                Quaternion nextAngel = choosedSamplePoints[0].views[ra.Next(0, 5)].rot;

                choosedSamplePoints[0].label.GetComponent<CustomUI.CircularImage>().color = Color.blue;
                ///创建一个Image对象
                GameObject newImage = new GameObject("Imageiiii");
                //把newImage对象变成Canvas对象的子节点对象
                newImage.transform.parent = fatherForBag.transform;
                //添加Image组件
                newImage.AddComponent<Image>();
                //动态加载贴图赋值给Image

                newImage.GetComponent<Image>().sprite = choosedSamplePoints[0].views[0].sprite;

                choosedSamplePoints.RemoveAt(0);
                //将当前选择的位置加入队列中
                wayRecorder.Add(next);
                if (wayRecorder.Count >= 3)
                {
                    moveThroughWay.GetWayPointsBetween(wayRecorder[wayRecorder.Count - 2], next, wayRecorder[wayRecorder.Count - 3], next + new Vector3(19, 1, 671), lastAngel, nextAngel);
                }
                else if (wayRecorder.Count == 2)//此时只选了一个关键点
                {
                    moveThroughWay.GetWayPointsBetween(wayRecorder[wayRecorder.Count - 2], next, wayRecorder[wayRecorder.Count - 2] - new Vector3(-20, 0, 20), next + new Vector3(19, 1, 671), lastAngel, nextAngel);
                }


            }
            //Vector3 next = ;// = choosedSamplePoints[] //找出下一个点

            //生成路径
            // 
            yield return new WaitForSeconds(1);
        }

    }
    GameObject show(float x, float y)
    {
        GameObject go = GameObject.Instantiate(Resources.Load("SamplePoint")) as GameObject;
        go.transform.parent = GameObject.Find("SamplePoints").transform;
        go.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        return go;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
   



   /* public struct SamplePoint
    {
        //采样点在现实世界的位置坐标
        public Vector3 pos;
        //一个采样点不同的View
        public List<View> views;
        //在Canvas上对应的UI
        public GameObject label;
        //采样点是否已经被选择过（指移动时被纳入考虑范围）
        public bool passed;
        //采样点是否有效
        public bool valid;
        public void setPos(Vector3 p)
        {
            pos = p;
        }
    }
    public struct View
    {
          public  Quaternion rot;//代表的方向
          public  float score; //所对应的评分
          public  Sprite sprite; //当前采样点的sprite
        }
}
    public struct forPicture {
       public int i;
       public int j;
       public int k;
       public int v;
    }*/