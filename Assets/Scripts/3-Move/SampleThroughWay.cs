using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 //这个类用来沿着粗糙路径生成采样点
public class SampleThroughWay : MonoBehaviour
{
    public static SampleThroughWay instance;
    private readonly int sampleNum = 5;
    private readonly List<Point> sampleCenters = new();
    private static readonly System.Random ra = new System.Random(unchecked((int)DateTime.Now.Ticks));
    void Awake()
    {
        instance = this;
    }
    public void GetMap()
    {
        //初始化地图，在整个界面上画格子,得到Map[Point]
        Astar.instance.InitGrid((int)mapCamera.GetComponent<Camera>().orthographicSize);
        Debug.Log("Finish Init Map");
    }
    public void GenerateSampleCenters()
    {
        //根据TSP问题的遗传算法，根据所有的展览物的位置生成一个访问回路（访问顺序-getcourseway）
        //然后根据得到的访问顺序、场景环境信息，用Astar算法生成粗糙路径上所有关键点
        List<Point> points =  GetCourseWay.instance.GetOrderAndWayPoints();   
        Debug.Log("get course way");
        //根据粗糙路径点再画格子，每个关键点代表一个格子，修改map[x,y]属于的采样格子标记(label)
        GetSampleCenters(points);
     
    }
    public void Init()
    {
            fatherForBag = GameObject.Find("fatherForCell");
            mainCamera = GameObject.Find("Main Camera");
            cameraOnUI = GameObject.Find("peopleUI"); ;//在UI上代表移动中的摄像机的圆点
            for (int i = 0; i < 6; i++)
            {
                quaternions.Add(Quaternion.Euler(0, 360 / 6 * i, 0));
            }
            //StartCoroutine("GenerateNextCenter");
            //采样点对应ui
            leftDown = GameObject.Find("leftDown");
            mapCamera = GameObject.Find("mapCamera");
            navigationMap = GameObject.Find("navigationMap");
            dl = navigationMap.GetComponent<drawline>();
            sampleCamera = GameObject.Find("sampleCamera");
}   
    public List<SamplePointBunch> spbl = new();
    private GameObject sampleCamera;//用来采样的相机
    public List<Quaternion> quaternions = new();  //相机的旋转角度
    List<ForPicture> unsolvedSprite = new();//存图片对应的是哪个bunch哪个samplePoint哪个View
    public List<List<ForPicture>> forScore = new();//每一次共同处理，都放入forScore中
    public float tall = 1.7f;
    public int sampleSolveSize;
    int sampleHalfNum = 2;//bunch由中心格子和其周围一圈格子组成
    
    //根据粗糙路径点得到采样需要的采样中心
    public void GetSampleCenters(List<Point> points)
    {
        int label = 0;
        //将粗糙路径沿固定格子数扩大
       for(int i = 0; i < points.Count; i++)
        {
            if (Astar.instance.map[points[i].x, points[i].y].samplePointBunchLabel == label - 1 || Astar.instance.map[points[i].x, points[i].y].samplePointBunchLabel!=-2)
            {
                //已经有标号的就跳过，表明当前这个点已经属于某个采样集合了
                continue;
            }
            //当前的中心作为采样点的中心，加入集合中，代表一个新的采样集合中心
            sampleCenters.Add(points[i]);
            //正方形范围内都是这个bunch,在这个bunch内随机选K个格子生成采样点
            for(int j = -sampleHalfNum; j <= sampleHalfNum; j++)
            {
                for(int k = -sampleHalfNum; k <= sampleHalfNum; k++)
                {
                    if(Astar.instance.map[points[i].x + j, points[i].y + k].isValid)
                    {
                        //如果这个格子是有效的，将其block标号放上去
                        Astar.instance.map[points[i].x + j, points[i].y + k].samplePointBunchLabel = label;
                    }
                }
            }
            label++;
        }
    }
    public void GetSamplePointsThroughWay()
    {
        foreach (Point point in sampleCenters){
            //格子左下角
            Vector2 pos = point.worldpos;
            //创建一个新的采样点集合
            SamplePointBunch bunch = new SamplePointBunch();
            bunch.pos = new Vector3(pos.x,1.7f,pos.y);
            bunch.spl = new List<SamplePoint>();
            bunch.label = point.samplePointBunchLabel;

            //bunch的左下角
            Vector2 ld = new Vector2(pos.x - sampleHalfNum * Astar.instance.lengthOfBox , pos.y - sampleHalfNum * Astar.instance.lengthOfBox);

            //设定sampleNum个采样点的位置
            int k = 0;
            while(k < sampleNum){
                //返回(0,1)之间的随机数
                float tmpWidth = (float)ra.NextDouble();
                float tmpHeight = (float)ra.NextDouble();
                float realx = ld.x + tmpWidth * (2 * sampleHalfNum + 1) * Astar.instance.lengthOfBox;
                float realy = ld.y + tmpHeight * (2 * sampleHalfNum + 1) * Astar.instance.lengthOfBox;
                //看该采样点属于map哪个格子
                int x = (int)( (realx - leftDown.transform.position.x) / Astar.instance.lengthOfBox);
                int y = (int)( (realy - leftDown.transform.position.z) / Astar.instance.lengthOfBox);
                //对应的格子在当前bunch之内，是有效的
                if (Astar.instance.map[x,y].samplePointBunchLabel == bunch.label)
                {
                    SamplePoint samplePoint = new SamplePoint();
                    samplePoint.pos = new Vector3(realx, 1.7f, realy);
                    samplePoint.label = ShowUI(GetUIPos(realx, realy));
                    //得到该采样点所有方向的图像,得到所有视角的sprite
                    samplePoint.views = new List<View>();
                    for (int v = 0; v < 6; v++)
                    {
                        sampleCamera.transform.position = pos;
                        sampleCamera.transform.rotation = quaternions[v];
                               
                        samplePoint.valid = true;
                        samplePoint.passed = false;
                        
                        unsolvedSprite.Add(new ForPicture() {bunch = spbl.Count, samplePoint = bunch.spl.Count, view = v});
                        samplePoint.views.Add(new View() {valid = true, score = 0, texture = ViewManager.CaptureCameraForTexture(sampleCamera.GetComponent<Camera>()), rot = quaternions[v] });
                    }
                    //将当前这个samplePoint加入当前的bunch中
                    bunch.spl.Add(samplePoint);
                }
                sampleSolveSize = 6;
                //当没有处理过的图达到一定的数量，集中处理，并得到每个图的评分
                if(unsolvedSprite.Count >= sampleSolveSize * sampleSolveSize)
                {
                    ViewManager.getMultipleView(sampleSolveSize, unsolvedSprite);
                    //得到图之后传给llm然后得到评分再保存
                    List<ForPicture> t = new List<ForPicture>();
                    for(int z = 0; z < sampleSolveSize * sampleSolveSize; z++)
                    {
                        t.Add(unsolvedSprite[0]);
                        unsolvedSprite.RemoveAt(0);
                    }
                    //将这次处理的所有图片的坐标信息存在 t 中，存入forscore
                    forScore.Add(t);
                    //如果连接没问题，将生成的总图片发给服务器端
                    if(Connect2Python.instance.connected)
                        Connect2Python.instance.SendFrame();
                }
                k++;
            }

        }
    }
    public void FilterSamplePoint(float score)
    {
        for(int z = 0; z < spbl.Count; z++)
        {
            for(int i = 0; i < spbl[z].spl.Count; i++)
            {
                int t = 0;
                for(int j = 0; j < spbl[z].spl[i].views.Count; j++)
                {
                    if(spbl[z].spl[i].views[j].score < score)
                    {   
                        //这个视角评分不够，删掉这个视角
                        var  temp =   spbl[z].spl[i].views[j];
                        temp.valid = false;
                        spbl[z].spl[i].views[j] = temp;
                    }
                    else
                    {
                        t++;
                    }
                }
                if(t == 0)//如果完全没有合适的视角，删掉这个采样点
                {
                    var temp = spbl[z].spl[i];
                    temp.valid = false;
                    spbl[z].spl[i] = temp;
                    spbl[z].spl[i].label.SetActive(false);//图标也消去
                    Destroy(spbl[z].spl[i].label);
                }


            }



        }
    }
#region 初始化完成后开始生成路径
    public Quaternion lastAngel;//记录上一个相机的位姿
    private List<Vector3> wayRecorder = new(); //记录选择过的key路径，为了曲线生成
    public void Add2WayRecorder(Vector3 v)
    {
        wayRecorder.Add(v);
    }
    public GameObject mainCamera;//用户视野相机
    /*public void setCenterAndStartSampling(GameObject center){
        float x = (center.transform.position.x - leftDown.transform.position.x) / (cameraSize * 2) * mapWidth;
        float y = (center.transform.position.z - leftDown.transform.position.z) / (cameraSize * 2) * mapHeight;
        cameraOnUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        //找到当前格子里面最大的采样点并加入
        //判断中心点的位置处于哪个格子里
        int gridx = (int)((mainCamera.transform.position.x - leftDown.transform.position.x) / Astar.instance.lengthOfBox);
        int gridy = (int)((mainCamera.transform.position.z - leftDown.transform.position.z) / Astar.instance.lengthOfBox);
        if (Astar.instance.map[gridx, gridy].samplePointBunchLabel == -2)
        {
            Debug.Log("当前选择的点不在任何一个bunch内");
            return; //说明当前正处于的点不在任何一个bunch内
        }
        int next = Astar.instance.map[gridx, gridy].samplePointBunchLabel;
        SamplePointBunch bunch = spbl[next];
        if (!bunch.valid)
        {//格子已经被删除了，如何处理？
            while (!bunch.valid)
            {
                //找到下一个有效的
                next = next + 1 >= spbl.Count ? 0 : next + 1;
                bunch = spbl[next];

            }
            return;
        }
        int maxSamplePoint = 0;
        int maxView = 0;
        float _maxScore = -1;
        //选择出最大分数的视点
        for (int i = 0; i < bunch.spl.Count; i++)
        {
            if (bunch.spl[i].valid)
            {
                for (int j = 0; j < bunch.spl[i].views.Count; j++)
                {
                    if (bunch.spl[i].views[j].valid)
                    {
                        if (_maxScore < bunch.spl[i].views[j].score)
                        {
                            maxSamplePoint = i;
                            maxView = j;
                            _maxScore = (bunch.spl[i].views[j].score);
                        }
                    }
                }
            }
        }
        if (!bunch.spl[maxSamplePoint].passed)
        {
            //将当前点加入选择的采样点集合中，并且将对应的UI图标颜色变绿
            choosedSamplePoints.Add(bunch.spl[maxSamplePoint]);
            var point = bunch.spl[maxSamplePoint];
            point.passed = true;
            var ui = point.label;
            ui.GetComponent<CustomUI.CircularImage>().color = Color.green;
            point.label = ui;
            bunch.spl[maxSamplePoint] = point;
        }


        spbl[next] = bunch;


        //chooseSamplePoints(30);
    }*/
    

    public List<PosAndView> finalWay = new();
    //以某个格子为起点，生成剩下的全部路径

    //生成路径移动
    
    public Vector3 last;
    //预处理之后一次性生成完整路径，每次选择格子里最高的就完事，后面是改变评分再重新选就完事
    public void GenerateWay()
    {
        //每次都是整个重新生成
        finalWay.Clear();
        //判断中心点的位置处于哪个格子里
        int x = (int)((mainCamera.transform.position.x - leftDown.transform.position.x) / Astar.instance.lengthOfBox);
        int y = (int)((mainCamera.transform.position.z - leftDown.transform.position.z) / Astar.instance.lengthOfBox);
        int bunchLabel = Astar.instance.map[x, y].samplePointBunchLabel;
        if (bunchLabel == -2)
        {
            Debug.Log("当前选择的点不在任何一个bunch内");
            return; //说明当前正处于的点不在任何一个bunch内
        } 
        //以当前bunchLabel为起点，遍历所有bunch
        for(int k = bunchLabel,p=0 ; p < spbl.Count; p++)
        {
            if (k == spbl.Count) 
                k = 0;
            if (spbl[k].valid)
            {
                SamplePointBunch bunch = spbl[k];

                int maxSamplePoint = 0;

                int maxView = 0;

                float _maxScore = -1;

                
                for (int i = 0; i < bunch.spl.Count; i++)
                {
                    if (bunch.spl[i].valid)
                    {//选择出这个bunch中最大分数的视点
                        for (int j = 0; j < bunch.spl[i].views.Count; j++)
                        {
                            if (bunch.spl[i].views[j].valid)
                            {
                                if (_maxScore < bunch.spl[i].views[j].score)
                                {
                                    maxSamplePoint = i;
                                    maxView = j;
                                    _maxScore = (bunch.spl[i].views[j].score);
                                }
                            }
                        }
                        //改变当前的point状态为passed，并且将UI变为绿色
                        var point = bunch.spl[maxSamplePoint];
                        point.passed = true;
                        var ui = point.label;
                        ui.GetComponent<CustomUI.CircularImage>().color = Color.green;
                        point.label = ui;
                        bunch.spl[maxSamplePoint] = point;
                        spbl[i] = bunch;                        
                        finalWay.Add(new PosAndView() { quaternion = spbl[i].spl[maxSamplePoint].views[maxView].rot, point = spbl[i].spl[maxSamplePoint]});
                        //将这个view的图像显示出来
                        //showSprite(spbl[i].spl[maxSamplePoint].views[maxView].texture);
                        ////将当前点加入选择的采样点集合中，并且将对应的UI图标颜色变绿
                        //choosedSamplePoints.Add(bunch.spl[maxSamplePoint]);
                    }
                }
            }
            k++;
        }

        //根据生成的路径，得到平滑点，进行平滑移动
        while (finalWay.Count > 0)
        {

            Vector3 next = finalWay[0].point.pos;
            Quaternion nextAngel = finalWay[0].quaternion;
            finalWay[0].point.label.GetComponent<CustomUI.CircularImage>().color = Color.blue;

            ShowSprite(finalWay[0].point.views[finalWay[0].view].texture);

            finalWay.RemoveAt(0);
            //将当前选择的位置加入队列中
            wayRecorder.Add(next);
            if (wayRecorder.Count >= 3)
            {
                MoveThroughWay.instance.GetWayPointsBetween(wayRecorder[wayRecorder.Count - 2], next, wayRecorder[wayRecorder.Count - 3], next + new Vector3(19, 1, 671), lastAngel, nextAngel);
            }
            else if (wayRecorder.Count == 2)//此时只选了一个关键点
            {
                MoveThroughWay.instance.GetWayPointsBetween(wayRecorder[wayRecorder.Count - 2], next, wayRecorder[wayRecorder.Count - 2] - new Vector3(-20, 0, 20), next + new Vector3(19, 1, 671), lastAngel, nextAngel);
            }

        }
    }    
    private GameObject fatherForBag;
    private void ShowSprite(Texture2D texture)
    {
        //创建一个Image对象
        GameObject newImage = new GameObject("Image");
        //把newImage对象变成Canvas对象的子节点对象
        newImage.transform.parent = fatherForBag.transform;
        //添加Image组件
        newImage.AddComponent<Image>();
        //动态加载贴图赋值给Image
        newImage.GetComponent<Image>().sprite = ViewManager.TextureToSprite(texture);
    } 
    /*public List<SamplePoint> choosedSamplePoints; // 根据当前相机(即中心点)的位置来添加未当过采样点的点进list
     * public void chooseSamplePoints(int number)
    {   
        //判断中心点的位置处于哪个格子里
        int x = (int)((mainCamera.transform.position.x - leftDown.transform.position.x) / Astar.instance.lengthOfBox);
        int y = (int)((mainCamera.transform.position.z - leftDown.transform.position.z) / Astar.instance.lengthOfBox);
        if (Astar.instance.map[x,y].samplePointBunchLabel == -2){
            Debug.Log("当前选择的点不在任何一个bunch内");
            return; //说明当前正处于的点不在任何一个bunch内
        }


        //将当前bunch的所有valid采样点放入
        SamplePointBunch bunch = spbl[Astar.instance.map[x, y].samplePointBunchLabel];
        if (!bunch.valid)
        {
            Debug.Log("当前集合没有合适采样点");
             next = Astar.instance.map[x, y].samplePointBunchLabel;
            //继续找下一个
            while (!bunch.valid)
            {
                //找到下一个有效的
                next = next + 1 >= spbl.Count ? 0 : next + 1;
                bunch = spbl[next];

            }
        }
        for (int i = 0; i < bunch.spl.Count; i++)
        {
            if (bunch.spl[i].valid)
            {
                choosedSamplePoints.Add(bunch.spl[i]);
            }
        }

                int next = Astar.instance.map[x, y].samplePointBunchLabel + 1 >= spbl.Count ? 0 : Astar.instance.map[x, y].samplePointBunchLabel + 1;
        SamplePointBunch bunch = spbl[next];
        if (!bunch.valid)
        {//格子已经被删除了，如何处理？
            while (!bunch.valid)
            {
                //找到下一个有效的
                next = next + 1 >= spbl.Count ? 0 : next + 1;
                bunch = spbl[next];

            }
            return;
        }
        int maxSamplePoint=0;
        int maxView=0;
        float _maxScore= -1;
        //选择出最大分数的视点
        for(int i = 0; i < bunch.spl.Count; i++)
        {
            if (bunch.spl[i].valid)
            {
                for(int j = 0; j < bunch.spl[i].views.Count; j++)
                {
                    if (bunch.spl[i].views[j].valid)
                    {
                        if(_maxScore < bunch.spl[i].views[j].score)
                        {
                            maxSamplePoint = i;
                            maxView = j;
                            _maxScore = (bunch.spl[i].views[j].score);
                        }
                    }
                }
            }
        }
        if (!bunch.spl[maxSamplePoint].passed){
                //将当前点加入选择的采样点集合中，并且将对应的UI图标颜色变绿
                choosedSamplePoints.Add(bunch.spl[maxSamplePoint]);
            var point = bunch.spl[maxSamplePoint];
                point.passed = true;
            var ui = point.label;
                ui.GetComponent<CustomUI.CircularImage>().color = Color.green;
                point.label = ui;
                bunch.spl[maxSamplePoint] = point;
            }


        spbl[next] = bunch;
    }*/

    //根据选出来的路径以及现在走到哪了生成路径，以及对用户反馈作出反应
    public bool userResponse;
    /*
    IEnumerator GenerateNextCenter()
    {//.选中了才显示sprite，会快一点
        while (true)
        {
            if (userResponse){
                //重新评分

                //重新生成路径
                GenerateWay();
            }

            if (finalWay.Count > 0){
              
                Vector3 next = finalWay[0].point.pos;
                Quaternion nextAngel = finalWay[0].quaternion;
                finalWay[0].point.label.GetComponent<CustomUI.CircularImage>().color = Color.blue;
                
                ShowSprite(finalWay[0].point.views[finalWay[0].view].texture);

                finalWay.RemoveAt(0);
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
            yield return new WaitForSeconds(1);
        }

    }*/
    #endregion
   

#region 生成并显示采样点对应的UI    
    private  GameObject leftDown;
    private  GameObject mapCamera;
    private  GameObject navigationMap;
    private drawline dl;
    private GameObject cameraOnUI;//在UI上代表移动中的摄像机的圆点
    //根据3维世界的位置得到对应的UI位置
    private Vector2 GetUIPos(float x,float y)
    {
        int w = (int)navigationMap.GetComponent<RectTransform>().rect.width;
        int h = (int)navigationMap.GetComponent<RectTransform>().rect.height;
        Vector2 uiPos = new(x, y)
        {
            x = (x - leftDown.transform.position.x) / (mapCamera.GetComponent<Camera>().orthographicSize * 2) * w,
            y = (y - leftDown.transform.position.z) / (mapCamera.GetComponent<Camera>().orthographicSize * 2) * h
        };
        return uiPos;
    }
    public void DrawRect(float x,float y)
    {
        int w = (int)navigationMap.GetComponent<RectTransform>().rect.width;
        int h = (int)navigationMap.GetComponent<RectTransform>().rect.height;
        float cameraSize = mapCamera.GetComponent<Camera>().orthographicSize;
        float uix = -w / 2 + (x - leftDown.transform.position.x) / (cameraSize * 2) * w;
        float uiy = -h / 2 + (y - leftDown.transform.position.z) / (cameraSize * 2) * h;
        //  toGOUI.Add(); 
        //Debug.Log(dl);
        dl.addRect(new Vector2(uix, uiy));
    }
    GameObject ShowUI(Vector2 pos)
    {
        GameObject go = Instantiate(Resources.Load("SamplePoint")) as GameObject;
        go.transform.parent = GameObject.Find("SamplePoints").transform;
        go.GetComponent<RectTransform>().anchoredPosition = pos;
        return go;
    } 
    public void CameraUIFollow()
    {
        float cameraSize = mapCamera.GetComponent<Camera>().orthographicSize;
        float w = navigationMap.GetComponent<RectTransform>().rect.width;
        float h = navigationMap.GetComponent<RectTransform>().rect.height;
        float x = (mainCamera.transform.position.x - leftDown.transform.position.x) / (cameraSize * 2) * w;
        float y = (mainCamera.transform.position.z - leftDown.transform.position.z) / (cameraSize * 2) * h;
        cameraOnUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
    }
}
#endregion
   
#region 数据结构
public struct SamplePoint
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
public struct PosAndView
{
    public Quaternion quaternion;
    public SamplePoint point;
    public int view;
}
public struct View
{
    public Quaternion rot;//代表的方向
    public float score; //所对应的评分
    public Sprite sprite; //当前采样点的sprite
    public Texture2D texture;//当前采样点sprite对应的texture
    public bool valid;
}
public struct SamplePointBunch
{
    public int label;
    public Vector3 pos;
    public List<SamplePoint> spl;
    public bool valid;
}
public struct ForPicture
{
    public int bunch;
    public int samplePoint;
    public int view;
}
# endregion
