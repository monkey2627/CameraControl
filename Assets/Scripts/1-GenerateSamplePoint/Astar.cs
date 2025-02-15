using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour
{
    public Material way;
    public Material choosed;
    public static Astar instance;
    public GameObject keyPoints;
    public  int mapWidth;//地图的长
    public  int mapHeight;//地图的宽
    public Point[,] map;//存地图，x，y表的是第几个格子

    //public GameObject Ground;//地图不可行走格子的图片
    //public GameObject Way;//地图可行走格子的图片

    [Range(0.5f, 1.5f)]
    public float lengthOfBox;//地图中每个格子大小，这个可以根据实际情况改

    public LayerMask NodeLayer;//选择障碍物所在的层
    public Transform meshOrigin;//网格地图起点对象

    private int maxDistance;
    
    RaycastHit m_Hit;
    bool isFloor;
   // ，这个地方要改，整个地图上只有一个由格子生成的map，这些map要记录point的信息，还要记录属于哪一个粗糙路径的格子
    //，格子是按照每一个Astar生成的路径上的小格子来生成！那sampleThroughWay还要改
    private void Awake()
    {
        instance = this;
        lengthOfBox = 20;
        maxDistance = 120;
    }
    //初始化grid
    //地板红色，障碍物黄色，门灰色
    public void InitGrid(float cameraSize)
    {
        int n = (int) (cameraSize * 2.0f / lengthOfBox);//行、列格子数
        mapHeight = mapWidth = n;
        map = new Point[n, n];//存地图，和point一一对应

        for (int x = 0; x < n; x++){
            for (int y = 0; y < n; y++){

                //从起始点meshOrigin开始绘制网格,meshOrigin是网格的xy轴的交汇点
                Vector2 pos = new Vector2(x * lengthOfBox, y * lengthOfBox) +  new Vector2(meshOrigin.position.x, meshOrigin.position.z);
                //绘制可视化地面背景
                GameObject gameObject = Instantiate(Resources.Load("way")) as GameObject;                
                gameObject.transform.localScale = new Vector3(lengthOfBox / 10, 1, lengthOfBox / 10);
                gameObject.transform.position = new Vector3(pos.x + lengthOfBox / 2, 0, pos.y + lengthOfBox / 2);
                gameObject.transform.SetParent(GameObject.Find("Map").transform);
                gameObject.name = string.Format("{0}_{1}", x,y);
                //初始化
                map[x, y] = new Point(pos, x, y);
                map[x, y].samplePointBunchLabel = -2;
                map[x, y].plane = gameObject;
                //方形范围检测（这里如果判断我将能检测到的方形碰撞盒的地方设为可行走路面，如果想改为障碍将感叹号去掉即可）
                isFloor = Physics.BoxCast(gameObject.transform.position, new Vector3(lengthOfBox / 2, 60, lengthOfBox / 2), new Vector3(0, -1, 0), out m_Hit, transform.rotation, maxDistance);
                if (isFloor)//如果有撞到什么东西,就全部属于有地板的地方
                {   
                   
                    //红色为地面
                    gameObject.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Material/red")) as Material;
                    //如果撞到障碍物，变为黄色
                    bool isObstacle = Physics.BoxCast(gameObject.transform.position, new Vector3(lengthOfBox / 2, 60, lengthOfBox / 2), new Vector3(0, -1, 0), out m_Hit, transform.rotation, maxDistance, LayerMask.GetMask("obstacles"));
                    if (isObstacle)
                        gameObject.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Material/yellow")) as Material;                
                    map[x, y].isObstacle = isObstacle;
                    bool isDoor = Physics.BoxCast(gameObject.transform.position, new Vector3(lengthOfBox / 2, 60, lengthOfBox / 2), new Vector3(0, -1, 0), out m_Hit, transform.rotation, maxDistance, LayerMask.GetMask("door"));
                    map[x, y].isDoor = isDoor;
                    //门是黑色
                    if (isDoor)
                        gameObject.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Material/black")) as Material;
                }
                else
                {
                    //没有撞到任何东西，不显示这个map的一块
                    gameObject.SetActive(false);
                }          
                //只有地板才有效
                map[x, y].isValid = isFloor;
                map[x, y].label = -1;
            }
        }
    }
    //找到地图上有几个房间，随机
    void FindAllRoom()
    {
        //类似广搜，每次把所有的水塘找到
        int label = 1;
        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            { 
                if(map[x,y].isValid && map[x,y].label == -1 && !map[x,y].isObstacle)
                {
                    tt = 0;
                   // Debug.Log(x + "_" + y);
                    //广搜
                     Point start = map[x,y];
                    map[x, y].label = label;
                     List<Point> t = new List<Point>();
                     t.Add(start);
                   while(t.Count > 0)
                     {
                        
                        Point point = t[0];
                        List<Point> surroundPoints = GetSurroundPointsGetArea(point);
              
                        foreach (var item in surroundPoints)
                          {

                          //  Debug.Log(item.plane.name);
                                        item.label = point.label;
                           // Debug.Log(item.label);
                            t.Add(item);
                                }
                                //移除point
                                t.RemoveAt(0);/**/ 
                    }


                    label += 1;
                }
            
            
            }
        }

        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
             if(map[x,y].isValid && map[x,y].label == 1)
                {
                   
                    map[x,y].plane.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Material/gray")) as Material;
                }else if (map[x, y].isValid && map[x, y].label == 2)
                {
                    //Debug.Log(x + "_" + y);
                    map[x, y].plane.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Material/purple")) as Material;
                }


            }
        }

    }

    private List<Point> GetSurroundPointsGetArea(Point point)
    {
        //八个方向都可以走
        Point up = null, down = null, left = null, right = null, lu = null, ru = null, ld = null, rd = null;
        if (point.y < mapHeight - 1)
        {
            up = map[point.x, point.y + 1];
        }
        if (point.y > 0)
        {
            down = map[point.x, point.y - 1];
        }
        if (point.x > 0)
        {
            left = map[point.x - 1, point.y];
        }
        if (point.x < mapWidth - 1)
        {
            right = map[point.x + 1, point.y];
        }
        if (up != null && left != null)
        {
            lu = map[point.x - 1, point.y + 1];
        }
        if (up != null && right != null)
        {
            ru = map[point.x + 1, point.y + 1];
        }
        if (down != null && left != null)
        {
            ld = map[point.x - 1, point.y - 1];
        }
        if (down != null && right != null)
        {
            rd = map[point.x + 1, point.y - 1];
        }
        List<Point> list = new List<Point>();
        if ( down != null && down.label==-1 && down.isObstacle == false && down.isValid == true && down.isDoor == false)
        {
            list.Add(down);
        }
        if ( left != null && left.label == -1&& left.isObstacle == false && left.isValid == true && left.isDoor == false)
        {
            list.Add(left);
        }
        if (right != null && right.label == -1 && right.isObstacle == false && right.isValid == true && right.isDoor == false)
        {
            list.Add(right);
        }
        if (up != null && up.label == -1 && up.isObstacle == false && up.isValid == true && up.isDoor == false)
        {
            list.Add(up);
        }
        if (lu != null && lu.label == -1 && lu.isObstacle == false && left.isObstacle == false && up.isObstacle == false && lu.isValid == true && left.isValid == true && up.isValid == true && up.isDoor == false && left.isDoor == false && lu.isDoor == false)
        {
            list.Add(lu);
        }
        if (ld != null && ld.label == -1 && ld.isObstacle == false && left.isObstacle == false && down.isObstacle == false && left.isValid == true && down.isValid == true && ld.isValid == true && ld.isDoor == false && down.isDoor == false && left.isDoor == false) 
        {
            list.Add(ld);
        }
        if (ru != null && ru.label == -1 && ru.isObstacle == false && right.isObstacle == false && up.isObstacle == false && ru.isValid == true && right.isValid == true && up.isValid == true && right.isDoor == false && ru.isDoor == false && up.isDoor == false)
        {
            list.Add(ru);
        }
        if (rd != null && rd.label == -1 && rd.isObstacle == false && right.isObstacle == false && down.isObstacle == false && rd.isValid == true && right.isValid == true && down.isValid == true && rd.isDoor == false && right.isDoor == false && down.isDoor == false)
        {
            list.Add(rd);
        }
        return list;
    }
    void OnDrawGizmos()
    {
      /* Gizmos.color = Color.red;

     
        if(GameObject.Find("15_15"))
        {
            //Draw a Ray forward from GameObject toward the maximum distance
            Gizmos.DrawRay(GameObject.Find("15_15").transform.position, new Vector3(0,-1,0) * maxDistance);
            //Draw a cube at the maximum distance
            Gizmos.DrawWireCube(GameObject.Find("15_15").transform.position + new Vector3(0, -1, 0) * maxDistance, new Vector3(lengthOfBox , 120, lengthOfBox ));
        }
      */ 
    }

    /// <summary>
    /// 绘制算法地图
    /// </summary>
    private void InitMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                //从起始点meshOrigin开始绘制网格
                Vector2 pos = new Vector2(x * lengthOfBox, y * lengthOfBox) + (Vector2)meshOrigin.position;
                //方形范围检测（这里如果判断我将能检测到的方形碰撞盒的地方设为可行走路面，如果想改为障碍将感叹号去掉即可）
                bool iswall = !Physics2D.BoxCast(pos, new Vector2(lengthOfBox, lengthOfBox), 0, new Vector2(0, 0), NodeLayer);
                map[x, y] = new Point(pos, x, y);
                map[x, y].isObstacle = iswall;
            }
        }
    }

    /// <summary>
    /// 二叉堆添加
    /// </summary>
    /// <param name="openList"></param>
    /// 
    /// <param name="point"></param>
    private void AddPoint(List<Point> openList, Point point)
    {
        openList.Add(point);
        //
        //
        //point.plane.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Material/green")) as Material;
        int last = openList.Count - 1;
        while (last >= 1)
        {
            int half = last >> 1;
            if (openList[last].f >= openList[half].f)
            {
                break;
            }
            Point temporary = openList[last];
            openList[last] = openList[half];
            openList[half] = temporary;
            last >>= 1;
        }
    }
    /// <summary>
    /// 二叉堆删除
    /// </summary>
    /// <param name="openList"></param>
    /// <param name="point"></param>
    private void RemovePoint(List<Point> openList, Point point)
    {
        int last = openList.Count;
        int head = openList.IndexOf(point) + 1;//防止索引为零，必须+1

        while ((head << 1) + 1 <= last)
        {
            int child1 = head << 1;
            int child2 = child1 + 1;
            int childMin = openList[child1 - 1].f < openList[child2 - 1].f ? child1 : child2;

            openList[head - 1] = openList[childMin - 1];

            head = childMin;

        }
        openList.Remove(openList[head - 1]);
    }
  
    /// <summary>
    /// 查找最优路径
    /// </summary>
    /// <param name="start">起点</param>
    /// <param name="end">终点</param>
    public List<Point> FindPath(Point start, Point end)
    {
        //Point start = GetIdem(startPos);
        //Point end = GetIdem(endPos);
        Debug.Log("起点算法坐标"+start.x + "+" + start.y);
        Debug.Log("终点算法坐标"+end.x + "+" + end.y+" ");
        List<Point> openList = new();
        List<Point> closeList = new();
        AddPoint(openList, start);//将起始位置添加到Open列表，二叉堆

        //终点在路面上的情况
        while (openList.Count > 0)
        {
         
            Point point = openList[0];//获取开启列表中F值最小的格子，
            if (point == end)
            {
                
                //用记录的父子关系来生成队列，变色
                return Generatepath(start, end);
            }
            //移除point
            RemovePoint(openList, point);
            closeList.Add(point);
            //四周节点列表
            //节点四周的八个方向都可以
           // ，改，现在的问题是，每开始一个新的start和end，其有可能会走之前走过的路，是完全消除吗？
            List<Point> surroundPoints = GetSurroundPoints(point);
            //去除周围节点中已经被排除的
            PointsFilter(surroundPoints, closeList);

            foreach (Point surroundPoint in surroundPoints)
            {
                //被放进周围列表中的变成绿色
                surroundPoint.plane.GetComponent<MeshRenderer>().material = choosed;
                if (openList.IndexOf(surroundPoint) > -1)//如果周围节点在起始列表中
                {
                    /*
                     public float h;//该格子与父节点的距离
    public float g;//该格子与目标点的距离 
    public float f;//h与g的综合，也是该格子在线路中的优先级
                     */
                    // 计算经过的Open列表中最小f值到周围节点的G值
                    float nowG = CalcG(surroundPoint, surroundPoint.parent);
                    if (nowG < surroundPoint.g)
                    {
                        surroundPoint.UpdateParent(point, nowG);
                    }
                }
                else
                {
                    surroundPoint.parent = point;//设置周围列表的父节点
                    CalcF(surroundPoint, end);//计算周围节点的F，G,H值  
                    AddPoint(openList, surroundPoint); //最后将周围节点添加进Open列表  
                }
            }
        }

        //终点在障碍物上，且障碍物周围至少有一格为路面的情况
        List<Point> endSurroundPoints = GetSurroundPoints(end);//终点周围格子
        Point optimal = null;//最优点
        foreach (Point surroundPoint in endSurroundPoints)
        {
            if (closeList.IndexOf(surroundPoint) > -1)
            {
                if (optimal != null)
                {
                    if (surroundPoint.g < optimal.g)
                        optimal = surroundPoint;
                }
                else
                {
                    optimal = surroundPoint;
                }
            }
        }
        if (optimal != null)
        {
            return Generatepath(start, optimal);
        }

        //终点在障碍物上，无法到达
        Debug.Log("找不到路径");
        return Generatepath(start, null);
    }
    /// <summary>
    /// 获取坐标点在算法地图中的坐标,即知道起在哪个格子里
    /// </summary>
    /// <param name="pos"></param>
    /// <returns></returns>
    public Point GetIdem(Vector2 pos)
    {
        int i = Mathf.RoundToInt((pos.x - meshOrigin.position.x) / lengthOfBox);
        int j = Mathf.RoundToInt((pos.y - meshOrigin.position.z) / lengthOfBox);
        return map[i, j];
    }
    public  bool IsFloorAndNotObtacle(Vector2 pos)
    {

        Point start =GetIdem(pos);
        if(start != null && start.isValid && !start.isObstacle)
        {
            return true;
        }
            return false;

    }
    /// <summary>
    /// 广搜，直到搜到一个可行的
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Point ChooseOnePoint(Point point)
    {
        //八个方向都可以走
        Point up = null, down = null, left = null, right = null, lu = null, ru = null, ld = null, rd = null;
        Debug.Log(mapHeight);
        if (point.y < mapHeight - 1)
        {
          
            up = map[point.x, point.y + 1];  
            Debug.Log(up.plane.name);
        }
        if (point.y > 0)
        {
          
            down = map[point.x, point.y - 1];  Debug.Log(down.plane.name);
        }
        if (point.x > 0)
        {
            left = map[point.x - 1, point.y];
            Debug.Log(down.plane.name);
        }
        if (point.x < mapWidth - 1)
        {
            right = map[point.x + 1, point.y];
        }
        if (up != null && left != null)
        {
            lu = map[point.x - 1, point.y + 1];
        }
        if (up != null && right != null)
        {
            ru = map[point.x + 1, point.y + 1];
        }
        if (down != null && left != null)
        {
            ld = map[point.x - 1, point.y - 1];
        }
        if (down != null && right != null)
        {
            rd = map[point.x + 1, point.y - 1];
        }
        if (down != null && down.isObstacle == false && down.isValid == true)
        {
            return down;
        }
        if (left != null && left.isObstacle == false && left.isValid == true)
        {
            return left;
        }
        if (right != null && right.isObstacle == false && right.isValid == true)
        {
            return right;
        }
        if (up != null && up.isObstacle == false && up.isValid == true)
        {
            return up;
        }
        if (lu != null && lu.isObstacle == false && left.isObstacle == false && up.isObstacle == false && lu.isValid == true && left.isValid == true && up.isValid == true)
        {
            return lu;
        }
        if (ld != null && ld.isObstacle == false && left.isObstacle == false && down.isObstacle == false && left.isValid == true && down.isValid == true && ld.isValid == true)
        {
            return ld;
        }
        if (ru != null && ru.isObstacle == false && right.isObstacle == false && up.isObstacle == false && ru.isValid == true && right.isValid == true && up.isValid == true)
        {
            return ru;
        }
        if (rd != null && rd.isObstacle == false && right.isObstacle == false && down.isObstacle == false && rd.isValid == true && right.isValid == true && down.isValid == true)
        {
            return rd;
        }
        if (down != null)
            return ChooseOnePoint(down);
        else if (up != null) 
            return ChooseOnePoint(up);
        else if(left != null) 
            return ChooseOnePoint(left);
        else if (right != null)
            return ChooseOnePoint(right);
        else if (lu != null)
            return ChooseOnePoint(lu);
        else if (ld != null)
            return ChooseOnePoint(ld);
        else if (ru != null)
            return ChooseOnePoint(ru);
        else if (rd != null)
            return ChooseOnePoint(rd);
        return null;
    }
    /// <summary>
    /// 获取当前节点周围的八个节点
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private List<Point> GetSurroundPoints(Point point)
    {
        //八个方向都可以走
        Point up = null, down = null, left = null, right = null,lu = null,ru = null,ld = null,rd = null;
        if (point.y < mapHeight - 1)
        {
            up = map[point.x, point.y + 1];
        }
        if (point.y > 0)
        {
            down = map[point.x, point.y - 1];
        }
        if (point.x > 0)
        {
            left = map[point.x - 1, point.y];
        }
        if (point.x < mapWidth - 1)
        {
            right = map[point.x + 1, point.y];
        }
        if (up != null && left != null)
        {
            lu = map[point.x - 1, point.y + 1];
        }
        if (up != null && right != null)
        {
            ru = map[point.x + 1, point.y + 1];
        }
        if (down != null && left != null)
        {
            ld = map[point.x - 1, point.y - 1];
        }
        if (down != null && right != null)
        {
            rd = map[point.x + 1, point.y - 1];
        }
        List<Point> list = new List<Point>();
        if (down != null && down.isObstacle == false && down.isValid == true)
        {
            list.Add(down);
        }
        if (left != null && left.isObstacle == false && left.isValid == true)
        {
            list.Add(left);
        }
        if (right != null && right.isObstacle == false && right.isValid == true)
        {
            list.Add(right);
        }
        if (up != null && up.isObstacle == false && up.isValid == true)
        {
            list.Add(up);
        }
        if (lu != null && lu.isObstacle == false && left.isObstacle == false && up.isObstacle == false && lu.isValid == true && left.isValid == true && up.isValid == true)
        {
            list.Add(lu);
        }
        if (ld != null && ld.isObstacle == false && left.isObstacle == false && down.isObstacle == false && left.isValid == true && down.isValid == true && ld.isValid == true)
        {
            list.Add(ld);
        }
        if (ru != null && ru.isObstacle == false && right.isObstacle == false && up.isObstacle == false && ru.isValid == true && right.isValid == true && up.isValid == true)
        {
            list.Add(ru);
        }
        if (rd != null && rd.isObstacle == false && right.isObstacle == false && down.isObstacle == false && rd.isValid == true && right.isValid == true && down.isValid == true)
        {
            list.Add(rd);
        }
        return list;
    }
    /// <summary>
    /// 将关闭列表中已经存在的节点从周围节点中移除
    /// </summary>
    /// <param name="surroundPoints">四周节点列表</param>
    /// <param name="closeList">关闭列表</param>
    private void PointsFilter(List<Point> surroundPoints, List<Point> closeList)
    {
        foreach (Point p in closeList)
        {
            if (surroundPoints.IndexOf(p) > -1)
            {
                surroundPoints.Remove(p);
            }
        }
    }

    /// <summary>
    /// 计算经过起始列表中最小f值到周围节点的G值
    /// </summary>
    /// <param name="surroundPoint"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private float CalcG(Point surroundPoint, Point parent)
    {
        return Vector2.Distance(new Vector2(surroundPoint.x, surroundPoint.y), new Vector2(parent.x, parent.y)) + parent.g;
    }
    /// <summary>
    /// 计算周围节点的F，G，H值
    /// </summary>
    /// <param name="surroundPoint"></param>
    /// <param name="end"></param>
    private void CalcF(Point surroundPoint, Point end)
    {
        //F = G + H  
        float h = Mathf.Abs(end.x - surroundPoint.x) + Mathf.Abs(end.y - surroundPoint.y);
        float g = 0;
        if (surroundPoint.parent == null)
        {
            g = 0;
        }
        else
        {
            g = Vector2.Distance(new Vector2(surroundPoint.x, surroundPoint.y), new Vector2(surroundPoint.parent.x, surroundPoint.parent.y)) + surroundPoint.parent.g;
        }
        float f = g + h;
        surroundPoint.f = f;
        surroundPoint.g = g;
        surroundPoint.h = h;
    }

    /// <summary>
    /// 生成路径泛型队列
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    private List<Point> Generatepath(Point start, Point end)
    {
        List<Point> path = new List<Point>();
        if (end != null)
        {
            Point node = end;//从终点开始生成路线
            while (node != start)
            {
                node.plane.GetComponent<MeshRenderer>().material = way;
                path.Add(node);//将节点放入路径队列中
                node = node.parent;//下一个节点为该节点的父节点
            }
            path.Reverse();//将路径节点倒序
        }
        //Debug.LogWarning(path.Count);
        return path;
    }

    private void Update()
    {
        //当地图发生改变后重新绘制算法地图
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InitMap();
        }
    }
}
public class Point
{
    public Point parent { get; set; }//该格子的父节点
    public Vector2 worldpos;//该格子在场景中的坐标
    public int x;//算法中的x方向第几格
    public int y;//算法中的y方向第几格
    public float h;//该格子与目标点的距离
    public float g;//该格子与父节点的距离 
    public float f;//h与g的综合，也是该格子在线路中的优先级
    public bool isObstacle;//是否是障碍物，能不能通过
    public bool isValid;//是否是地板，能否走过
    public GameObject plane;//该格子所对应的plane，用于可视化
    public int label;//标识属于哪个房间
    public bool isDoor;//表示是否处于开口的位置
    public int samplePointBunchLabel;//当前格子属于哪个samplePointBunchLabel

    public Point(Vector2 worldpos, int x, int y, Point parent = null)
    {
        this.parent = parent;
        this.worldpos = worldpos;
        this.x = x;
        this.y = y;
    }

    public void UpdateParent(Point parent, float g)
    {
        this.parent = parent;
        this.g = g;
        this.f = this.g + h;
    }
}