using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveThroughWay : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 10f;// m / s��ÿ����Ӧ����ǰ�߶���
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
    //����һ��·���ؼ���
    public void chooseNextPoint()
    {
        
      /* if(lastKeyPoint != null)
        {
            GetWayPointsBetween()
        }*/ 
    }
    public void GetWayPointsBetween(Vector3 p1,Vector3 p2,Vector3 before,Vector3 next,Quaternion begin,Quaternion end)
    {
        //Ҫ���ݾ���������ƽ���������,300m��Ӧ200�ȽϺ�
        int number = 200;
        //Debug.Log((p2 - p1).magnitude);
        number = (int)(p2 - p1).magnitude * 2 / 4;
        for (int i = 0; i < number;i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //����Ҫ�ߵ�·��
            
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
    //˼��һ�����ƽ���Ƕ�
    void Update()
    {
        float s = getSpeed() * Time.deltaTime;//��һ֡���ߵ�·��
       
        
        if (toGo.Count > 0)//��Ҫ������ǰ��
        {   t = t + s;
            //��һ��Ӧ�õ���λ��
            Vector3 next =  toGo[0].pos;
            //��������ƶ�ʱ����̬��������λ�ã�
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
                //����Ŀ�귽����
                target.transform.position += (next - target.transform.position).normalized * s;
            }
        }
    }
    //���ܲ���p0-p3�ĸ�������õ�����Ϊp1��p2֮��ģ�iΪһ��λ��0-1֮���������ʾλ��p1-p2���ĸ�λ��,����ÿ��ֻ�ܵõ�һ��
    //��һ�������ʵ��ģʽ
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
       /* for (int i = 0; i < points.Length; ++i)//�������е�Ԥ���
        {
            var t1 = Waypoints[(i) % Waypoints.Length];
            var t2 = Waypoints[(i + 1) % Waypoints.Length];
            if (t1 != null && t2 != null)
            {
                Vector3 p1 = t1.position;
                Vector3 p2 = t2.position;
                //points�����Ԥ����λ�ã�distances���������ۼƵ��߶γ���
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
