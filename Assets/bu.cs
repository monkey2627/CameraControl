using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bu : MonoBehaviour
{
    // Start is called before the first frame update
    public List<RotationAndPosition> toGo = new();
    public int count = 0;
    private float speed = 10f;
    int number;
    public GameObject g1;
    public GameObject g2;
    public GameObject g3;
    public GameObject g4;
    public GameObject g5;
    public GameObject target;
    void Start()
    {
        GetWayPointsBetween(g2.transform.position, g3.transform.position, g1.transform.position, g4.transform.position, new Vector3(0, 0, 0), new Vector3(0, 60, 0));
        GetWayPointsBetween(g3.transform.position, g4.transform.position, g2.transform.position, g5.transform.position, new Vector3(0, 60, 0), new Vector3(0, 90, 0));
    }
    public void GetWayPointsBetween(Vector3 p1, Vector3 p2, Vector3 before, Vector3 next, Vector3 begin, Vector3 end)
    {
        //要根据距离来调整平滑点的数量,300m对应200比较好
        // number = (int)(p2 - p1).magnitude * 2 / 4;
        number = 40;
        for (int i = 0; i < number; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.parent = transform;
           // go.SetActive(false);
            go.transform.localScale = new Vector3(1, 1, 1);
            go.transform.position = CatmullRom(before, p1, p2, next, i * (1f / ((float)number) * 1.0f));
            //插值旋转角度和位置
            toGo.Add(new RotationAndPosition(Quaternion.Slerp(Quaternion.Euler(begin), Quaternion.Euler(end), i * (1f / ((float)number) * 1.0f)), go.transform.position));
          
        }

    }

    private void SetSpeed(float sp)
    {
        speed = sp;
    }
    private float GetSpeed()
    {
        return speed;
    }
    // Update is called once per frame
    private float t = 0;
    private int rot = 0;
    //思考一下如何平滑角度
    void Update()
    {
        Vector3 qSpeed;
        float s = GetSpeed() * Time.deltaTime;//这一帧该走的路程

        if (toGo.Count > 0)//需要继续往前走
        {
            t += s;
            //下一个应该到的位置
            Vector3 next = toGo[0].pos;
            Vector3 nextq = toGo[0].quaternion.eulerAngles;
            //不停算转动的速度
            qSpeed = (nextq - target.transform.eulerAngles) / ((next - target.transform.position).magnitude / GetSpeed());
            //在摄像机移动时，动态更新中心位置，
            //samplePointsGenerater.SetCenterAndSampling(target);
            float gap = (next - target.transform.position).magnitude;
            if (t > gap)
            {
                target.transform.position += (next - target.transform.position).normalized * s;
                target.transform.eulerAngles = nextq;
                
                toGo.RemoveAt(0);
               t -= gap;

            }
            else
            {
                //朝着目标方向走
                target.transform.position += (next - target.transform.position).normalized * s;
                target.transform.eulerAngles += qSpeed * Time.deltaTime;
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

    public struct RotationAndPosition
    {
        public Quaternion quaternion;
        public Vector3 pos;

        public RotationAndPosition(Quaternion q, Vector3 p)
        {
            quaternion = q;
            pos = p;
        }
    }
}
