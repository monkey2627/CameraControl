using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveThroughWay : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 10f;// m / s，每秒钟应该往前走多少
    private List<Vector3> points = new List<Vector3>();
    private List<float> distances = new List<float>();
    public List<GameObject> wayPoints = new List<GameObject>();
    public List<RotationAndPosition> toGo = new List<RotationAndPosition>();
    public List<Vector2> toGOUI = new List<Vector2>();
    public int num = 50;
    public GameObject target;
    public GameObject SamplePointsGenerater;
    private SamplePointsGenerater samplePointsGenerater;
    private drawline dl;
    void Start()
    {   
        num = 50;
        samplePointsGenerater =  SamplePointsGenerater.GetComponent<SamplePointsGenerater>();
        dl = GameObject.Find("navigationMap").GetComponent<drawline>();
        CachePositionsAndDistances();
      //  GetWayPgointsBetween(points[1], points[2], points[0], points[3]);
    }
    //找下一个路径关键点
    public void chooseNextPoint()
    {
        
      /* if(lastKeyPoint != null)
        {
            GetWayPointsBetween()
        }*/ 
    }
    public void GetWayPointsBetween(Vector3 p1,Vector3 p2,Vector3 before,Vector3 next,Quaternion begin,Quaternion end)
    {
        //要根据距离来调整平滑点的数量,300m对应200比较好
        int number = 200;
        //Debug.Log((p2 - p1).magnitude);
        number = (int)(p2 - p1).magnitude * 2 / 4;
        for (int i = 0; i < number;i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //加入要走的路中
            
            go.transform.parent = transform;
            go.SetActive(false);
            go.transform.localScale = new Vector3(10,10,10);
            go.transform.position = CatmullRom(before, p1, p2, next, i * (1f /( (float)number) * 1.0f));
            toGo.Add(new RotationAndPosition(Quaternion.Slerp(begin,end, i * (1f / ((float)number) * 1.0f)), go.transform.position));
            float uix = -samplePointsGenerater.fixWidth / 2 +(go.transform.position.x - samplePointsGenerater.leftDown.transform.position.x) / (samplePointsGenerater.cameraSize * 2) * samplePointsGenerater.fixWidth;
            float uiy = -samplePointsGenerater.fixHeight/2 + (go.transform.position.z - samplePointsGenerater.leftDown.transform.position.z) / (samplePointsGenerater.cameraSize * 2) * samplePointsGenerater.fixHeight;
            //  toGOUI.Add(); 
            //Debug.Log(dl);
            dl.addRect(new Vector2(uix, uiy));
        }
       
    }
    private float getSpeed()
    {
        return 100f;
    }
    // Update is called once per frame
    private float t = 0;
    //思考一下如何平滑角度
    void Update()
    {
        float s = getSpeed() * Time.deltaTime;//这一帧该走的路程
       
        
        if (toGo.Count > 0)//需要继续往前走
        {   t = t + s;
            //下一个应该到的位置
            Vector3 next =  toGo[0].pos;
            //在摄像机移动时，动态更新中心位置，
            samplePointsGenerater.SetCenterAndSampling(target);
            float gap = (next - target.transform.position).magnitude;
            if( t > gap)
            {

                // target.transform.position = next; 
                target.transform.position += (next - target.transform.position).normalized * s;
                toGo.RemoveAt(0);
                t = t - gap;
            }
            else
            {
                //朝着目标方向走
                target.transform.position += (next - target.transform.position).normalized * s;
            }
        }
    }
    //接受参数p0-p3四个点后，所得的曲线为p1和p2之间的，i为一个位于0-1之间的数，表示位于p1-p2间哪个位置,所以每次只能得到一段
    //想一下这里的实现模式
    private Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float i)
    {
        // comments are no use here... it's the catmull-rom equation.
        // Un-magic this, lord vector!
        return 0.5f *
               ((2 * p1) + (-p0 + p2) * i + (2 * p0 - 5 * p1 + 4 * p2 - p3) * i * i +
                (-p0 + 3 * p1 - 3 * p2 + p3) * i * i * i);
    }
    private void CachePositionsAndDistances()
    {
        // transfer the position of each point and distances between points to arrays for
        // speed of lookup at runtime
        for (int c = 0; c < gameObject.transform.childCount; c++)
        {
            GameObject t = gameObject.transform.GetChild(c).gameObject;
            wayPoints.Add(t);
            //Debug.Log(t.transform.position);
            points.Add(t.transform.position);
            //distances.Add()
        }
        float accumulateDistance = 0;
       /* for (int i = 0; i < points.Length; ++i)//遍历所有的预设点
        {
            var t1 = Waypoints[(i) % Waypoints.Length];
            var t2 = Waypoints[(i + 1) % Waypoints.Length];
            if (t1 != null && t2 != null)
            {
                Vector3 p1 = t1.position;
                Vector3 p2 = t2.position;
                //points里面存预设点的位置，distances里面存的是累计的线段长度
                points[i] = Waypoints[i % Waypoints.Length].position;
                distances[i] = accumulateDistance;
                accumulateDistance += (p1 - p2).magnitude;
            }
        }*/
    }
    public struct RotationAndPosition
    {
        public Quaternion quaternion;
        public Vector3 pos;

        public RotationAndPosition(Quaternion q,Vector3 p)
        {
            quaternion = q;
            pos = p;
        }
    }
}
