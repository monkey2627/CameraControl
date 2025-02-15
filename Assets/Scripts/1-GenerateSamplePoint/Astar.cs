using System.Collections.Generic;
using UnityEngine;

public class Astar : MonoBehaviour
{
    public Material way;
    public Material choosed;
    public static Astar instance;
    public GameObject keyPoints;
    public  int mapWidth;//��ͼ�ĳ�
    public  int mapHeight;//��ͼ�Ŀ�
    public Point[,] map;//���ͼ��x��y����ǵڼ�������

    //public GameObject Ground;//��ͼ�������߸��ӵ�ͼƬ
    //public GameObject Way;//��ͼ�����߸��ӵ�ͼƬ

    [Range(0.5f, 1.5f)]
    public float lengthOfBox;//��ͼ��ÿ�����Ӵ�С��������Ը���ʵ�������

    public LayerMask NodeLayer;//ѡ���ϰ������ڵĲ�
    public Transform meshOrigin;//�����ͼ������

    private int maxDistance;
    
    RaycastHit m_Hit;
    bool isFloor;
   // ������ط�Ҫ�ģ�������ͼ��ֻ��һ���ɸ������ɵ�map����ЩmapҪ��¼point����Ϣ����Ҫ��¼������һ���ֲ�·���ĸ���
    //�������ǰ���ÿһ��Astar���ɵ�·���ϵ�С���������ɣ���sampleThroughWay��Ҫ��
    private void Awake()
    {
        instance = this;
        lengthOfBox = 20;
        maxDistance = 120;
    }
    //��ʼ��grid
    //�ذ��ɫ���ϰ����ɫ���Ż�ɫ
    public void InitGrid(float cameraSize)
    {
        int n = (int) (cameraSize * 2.0f / lengthOfBox);//�С��и�����
        mapHeight = mapWidth = n;
        map = new Point[n, n];//���ͼ����pointһһ��Ӧ

        for (int x = 0; x < n; x++){
            for (int y = 0; y < n; y++){

                //����ʼ��meshOrigin��ʼ��������,meshOrigin�������xy��Ľ����
                Vector2 pos = new Vector2(x * lengthOfBox, y * lengthOfBox) +  new Vector2(meshOrigin.position.x, meshOrigin.position.z);
                //���ƿ��ӻ����汳��
                GameObject gameObject = Instantiate(Resources.Load("way")) as GameObject;                
                gameObject.transform.localScale = new Vector3(lengthOfBox / 10, 1, lengthOfBox / 10);
                gameObject.transform.position = new Vector3(pos.x + lengthOfBox / 2, 0, pos.y + lengthOfBox / 2);
                gameObject.transform.SetParent(GameObject.Find("Map").transform);
                gameObject.name = string.Format("{0}_{1}", x,y);
                //��ʼ��
                map[x, y] = new Point(pos, x, y);
                map[x, y].samplePointBunchLabel = -2;
                map[x, y].plane = gameObject;
                //���η�Χ��⣨��������ж��ҽ��ܼ�⵽�ķ�����ײ�еĵط���Ϊ������·�棬������Ϊ�ϰ�����̾��ȥ�����ɣ�
                isFloor = Physics.BoxCast(gameObject.transform.position, new Vector3(lengthOfBox / 2, 60, lengthOfBox / 2), new Vector3(0, -1, 0), out m_Hit, transform.rotation, maxDistance);
                if (isFloor)//�����ײ��ʲô����,��ȫ�������еذ�ĵط�
                {   
                   
                    //��ɫΪ����
                    gameObject.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Material/red")) as Material;
                    //���ײ���ϰ����Ϊ��ɫ
                    bool isObstacle = Physics.BoxCast(gameObject.transform.position, new Vector3(lengthOfBox / 2, 60, lengthOfBox / 2), new Vector3(0, -1, 0), out m_Hit, transform.rotation, maxDistance, LayerMask.GetMask("obstacles"));
                    if (isObstacle)
                        gameObject.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Material/yellow")) as Material;                
                    map[x, y].isObstacle = isObstacle;
                    bool isDoor = Physics.BoxCast(gameObject.transform.position, new Vector3(lengthOfBox / 2, 60, lengthOfBox / 2), new Vector3(0, -1, 0), out m_Hit, transform.rotation, maxDistance, LayerMask.GetMask("door"));
                    map[x, y].isDoor = isDoor;
                    //���Ǻ�ɫ
                    if (isDoor)
                        gameObject.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Material/black")) as Material;
                }
                else
                {
                    //û��ײ���κζ���������ʾ���map��һ��
                    gameObject.SetActive(false);
                }          
                //ֻ�еذ����Ч
                map[x, y].isValid = isFloor;
                map[x, y].label = -1;
            }
        }
    }
    //�ҵ���ͼ���м������䣬���
    void FindAllRoom()
    {
        //���ƹ��ѣ�ÿ�ΰ����е�ˮ���ҵ�
        int label = 1;
        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            { 
                if(map[x,y].isValid && map[x,y].label == -1 && !map[x,y].isObstacle)
                {
                    tt = 0;
                   // Debug.Log(x + "_" + y);
                    //����
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
                                //�Ƴ�point
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
        //�˸����򶼿�����
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
    /// �����㷨��ͼ
    /// </summary>
    private void InitMap()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                //����ʼ��meshOrigin��ʼ��������
                Vector2 pos = new Vector2(x * lengthOfBox, y * lengthOfBox) + (Vector2)meshOrigin.position;
                //���η�Χ��⣨��������ж��ҽ��ܼ�⵽�ķ�����ײ�еĵط���Ϊ������·�棬������Ϊ�ϰ�����̾��ȥ�����ɣ�
                bool iswall = !Physics2D.BoxCast(pos, new Vector2(lengthOfBox, lengthOfBox), 0, new Vector2(0, 0), NodeLayer);
                map[x, y] = new Point(pos, x, y);
                map[x, y].isObstacle = iswall;
            }
        }
    }

    /// <summary>
    /// ��������
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
    /// �����ɾ��
    /// </summary>
    /// <param name="openList"></param>
    /// <param name="point"></param>
    private void RemovePoint(List<Point> openList, Point point)
    {
        int last = openList.Count;
        int head = openList.IndexOf(point) + 1;//��ֹ����Ϊ�㣬����+1

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
    /// ��������·��
    /// </summary>
    /// <param name="start">���</param>
    /// <param name="end">�յ�</param>
    public List<Point> FindPath(Point start, Point end)
    {
        //Point start = GetIdem(startPos);
        //Point end = GetIdem(endPos);
        Debug.Log("����㷨����"+start.x + "+" + start.y);
        Debug.Log("�յ��㷨����"+end.x + "+" + end.y+" ");
        List<Point> openList = new();
        List<Point> closeList = new();
        AddPoint(openList, start);//����ʼλ����ӵ�Open�б������

        //�յ���·���ϵ����
        while (openList.Count > 0)
        {
         
            Point point = openList[0];//��ȡ�����б���Fֵ��С�ĸ��ӣ�
            if (point == end)
            {
                
                //�ü�¼�ĸ��ӹ�ϵ�����ɶ��У���ɫ
                return Generatepath(start, end);
            }
            //�Ƴ�point
            RemovePoint(openList, point);
            closeList.Add(point);
            //���ܽڵ��б�
            //�ڵ����ܵİ˸����򶼿���
           // ���ģ����ڵ������ǣ�ÿ��ʼһ���µ�start��end�����п��ܻ���֮ǰ�߹���·������ȫ������
            List<Point> surroundPoints = GetSurroundPoints(point);
            //ȥ����Χ�ڵ����Ѿ����ų���
            PointsFilter(surroundPoints, closeList);

            foreach (Point surroundPoint in surroundPoints)
            {
                //���Ž���Χ�б��еı����ɫ
                surroundPoint.plane.GetComponent<MeshRenderer>().material = choosed;
                if (openList.IndexOf(surroundPoint) > -1)//�����Χ�ڵ�����ʼ�б���
                {
                    /*
                     public float h;//�ø����븸�ڵ�ľ���
    public float g;//�ø�����Ŀ���ľ��� 
    public float f;//h��g���ۺϣ�Ҳ�Ǹø�������·�е����ȼ�
                     */
                    // ���㾭����Open�б�����Сfֵ����Χ�ڵ��Gֵ
                    float nowG = CalcG(surroundPoint, surroundPoint.parent);
                    if (nowG < surroundPoint.g)
                    {
                        surroundPoint.UpdateParent(point, nowG);
                    }
                }
                else
                {
                    surroundPoint.parent = point;//������Χ�б�ĸ��ڵ�
                    CalcF(surroundPoint, end);//������Χ�ڵ��F��G,Hֵ  
                    AddPoint(openList, surroundPoint); //�����Χ�ڵ���ӽ�Open�б�  
                }
            }
        }

        //�յ����ϰ����ϣ����ϰ�����Χ������һ��Ϊ·������
        List<Point> endSurroundPoints = GetSurroundPoints(end);//�յ���Χ����
        Point optimal = null;//���ŵ�
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

        //�յ����ϰ����ϣ��޷�����
        Debug.Log("�Ҳ���·��");
        return Generatepath(start, null);
    }
    /// <summary>
    /// ��ȡ��������㷨��ͼ�е�����,��֪�������ĸ�������
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
    /// ���ѣ�ֱ���ѵ�һ�����е�
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public Point ChooseOnePoint(Point point)
    {
        //�˸����򶼿�����
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
    /// ��ȡ��ǰ�ڵ���Χ�İ˸��ڵ�
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private List<Point> GetSurroundPoints(Point point)
    {
        //�˸����򶼿�����
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
    /// ���ر��б����Ѿ����ڵĽڵ����Χ�ڵ����Ƴ�
    /// </summary>
    /// <param name="surroundPoints">���ܽڵ��б�</param>
    /// <param name="closeList">�ر��б�</param>
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
    /// ���㾭����ʼ�б�����Сfֵ����Χ�ڵ��Gֵ
    /// </summary>
    /// <param name="surroundPoint"></param>
    /// <param name="parent"></param>
    /// <returns></returns>
    private float CalcG(Point surroundPoint, Point parent)
    {
        return Vector2.Distance(new Vector2(surroundPoint.x, surroundPoint.y), new Vector2(parent.x, parent.y)) + parent.g;
    }
    /// <summary>
    /// ������Χ�ڵ��F��G��Hֵ
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
    /// ����·�����Ͷ���
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    private List<Point> Generatepath(Point start, Point end)
    {
        List<Point> path = new List<Point>();
        if (end != null)
        {
            Point node = end;//���յ㿪ʼ����·��
            while (node != start)
            {
                node.plane.GetComponent<MeshRenderer>().material = way;
                path.Add(node);//���ڵ����·��������
                node = node.parent;//��һ���ڵ�Ϊ�ýڵ�ĸ��ڵ�
            }
            path.Reverse();//��·���ڵ㵹��
        }
        //Debug.LogWarning(path.Count);
        return path;
    }

    private void Update()
    {
        //����ͼ�����ı�����»����㷨��ͼ
        if (Input.GetKeyDown(KeyCode.Q))
        {
            InitMap();
        }
    }
}
public class Point
{
    public Point parent { get; set; }//�ø��ӵĸ��ڵ�
    public Vector2 worldpos;//�ø����ڳ����е�����
    public int x;//�㷨�е�x����ڼ���
    public int y;//�㷨�е�y����ڼ���
    public float h;//�ø�����Ŀ���ľ���
    public float g;//�ø����븸�ڵ�ľ��� 
    public float f;//h��g���ۺϣ�Ҳ�Ǹø�������·�е����ȼ�
    public bool isObstacle;//�Ƿ����ϰ���ܲ���ͨ��
    public bool isValid;//�Ƿ��ǵذ壬�ܷ��߹�
    public GameObject plane;//�ø�������Ӧ��plane�����ڿ��ӻ�
    public int label;//��ʶ�����ĸ�����
    public bool isDoor;//��ʾ�Ƿ��ڿ��ڵ�λ��
    public int samplePointBunchLabel;//��ǰ���������ĸ�samplePointBunchLabel

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