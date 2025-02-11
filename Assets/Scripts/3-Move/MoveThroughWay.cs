using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveThroughWay : MonoBehaviour
{
    // Start is called before the first frame update
    public List<RotationAndPosition> toGo = new();
    private float speed = 100f;
    public static MoveThroughWay instance;
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {   
       
    }
    public void GetWayPointsBetween(Vector3 p1,Vector3 p2,Vector3 before,Vector3 next,Quaternion begin,Quaternion end)
    {
        //Ҫ���ݾ���������ƽ���������,300m��Ӧ200�ȽϺ�
        int number = (int)(p2 - p1).magnitude * 2 / 4;
        for (int i = 0; i < number;i++)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.parent = transform;
            go.SetActive(false);
            go.transform.localScale = new Vector3(10,10,10);
            go.transform.position = CatmullRom(before, p1, p2, next, i * (1f /( (float)number) * 1.0f));
            //��ֵ��ת�ǶȺ�λ��
            toGo.Add(new RotationAndPosition(Quaternion.Slerp(begin,end, i * (1f / ((float)number) * 1.0f)), go.transform.position));
            SampleThroughWay.instance.DrawRect(go.transform.position.x, go.transform.position.z);
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
    //˼��һ�����ƽ���Ƕ�
    void Update()
    {
        float s = GetSpeed() * Time.deltaTime;//��һ֡���ߵ�·��
       
        
        if (toGo.Count > 0)//��Ҫ������ǰ��
        {   
            t += s;
            //��һ��Ӧ�õ���λ��
            Vector3 next =  toGo[0].pos;
            //��������ƶ�ʱ����̬��������λ�ã�
            //samplePointsGenerater.SetCenterAndSampling(target);
            float gap = (next - SampleThroughWay.instance.mainCamera.transform.position).magnitude;
            if( t > gap)
            {

                // target.transform.position = next; 
                SampleThroughWay.instance.mainCamera.transform.position += (next - SampleThroughWay.instance.mainCamera.transform.position).normalized * s;
                SampleThroughWay.instance.CameraUIFollow();
                toGo.RemoveAt(0);
                t -= gap;
            }
            else
            {
                //����Ŀ�귽����
                SampleThroughWay.instance. mainCamera.transform.position += (next - SampleThroughWay.instance.mainCamera.transform.position).normalized * s;
                SampleThroughWay.instance.CameraUIFollow();
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

        public RotationAndPosition(Quaternion q,Vector3 p)
        {
            quaternion = q;
            pos = p;
        }
    }
}
