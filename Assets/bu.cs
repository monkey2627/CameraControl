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
        //Ҫ���ݾ���������ƽ���������,300m��Ӧ200�ȽϺ�
        // number = (int)(p2 - p1).magnitude * 2 / 4;
        number = 40;
        for (int i = 0; i < number; i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.parent = transform;
           // go.SetActive(false);
            go.transform.localScale = new Vector3(1, 1, 1);
            go.transform.position = CatmullRom(before, p1, p2, next, i * (1f / ((float)number) * 1.0f));
            //��ֵ��ת�ǶȺ�λ��
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
    //˼��һ�����ƽ���Ƕ�
    void Update()
    {
        Vector3 qSpeed;
        float s = GetSpeed() * Time.deltaTime;//��һ֡���ߵ�·��

        if (toGo.Count > 0)//��Ҫ������ǰ��
        {
            t += s;
            //��һ��Ӧ�õ���λ��
            Vector3 next = toGo[0].pos;
            Vector3 nextq = toGo[0].quaternion.eulerAngles;
            //��ͣ��ת�����ٶ�
            qSpeed = (nextq - target.transform.eulerAngles) / ((next - target.transform.position).magnitude / GetSpeed());
            //��������ƶ�ʱ����̬��������λ�ã�
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
                //����Ŀ�귽����
                target.transform.position += (next - target.transform.position).normalized * s;
                target.transform.eulerAngles += qSpeed * Time.deltaTime;
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
