using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SamplePointsGenerater : MonoBehaviour
{
    //���Ӹ���,ÿ�м�����ÿ�м���
    public int heightNum = 10;
    public int widthNum = 10;
    public static int number = 30;
    private float step_w;
    private float step_h;

    //UI�ϵ�navigationMap
    private GameObject navigationMap;


    //װ���б߽��ĸ�����
    public GameObject frameObjectFather;
    public List<GameObject> boundaryPoints; //Ϊ���ų����ڲ�����ڲ��ı߽��
    //���   
    public GameObject cameraOnUI;//��UI�ϴ����ƶ��е��������Բ��
    public GameObject sampleCamera;//�������������
    public float tall = 1.7f; //����ĸ߶�
    public List<Quaternion> quaternions = new List<Quaternion>();  //�������ת�Ƕ�
    //������
    public SamplePoint[,,] allSamplePoints = new SamplePoint[1000, 1000, 6];
    public List<SamplePoint> choosedSamplePoints; // ���ݵ�ǰ���(�����ĵ�)��λ�������δ����������ĵ��list

    //Ϊ��UI����ʵλ�õ�ӳ��
    public GameObject leftDown;
    public float fixWidth;
    public float fixHeight;
    public int cameraSize = 0;
    private GameObject mapCamera;
    public GameObject[] lies;
    public GameObject[] hangs;
    private ViewManager viewManager;
    //����·���ƶ�
    private MoveThroughWay moveThroughWay;
    public Vector3 last;
    //��ʾͼƬ
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
        cameraOnUI = GameObject.Find("peopleUI"); ;//��UI�ϴ����ƶ��е��������Բ��
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
    #region  ���ݱ߽���ų������ܵ�����
    struct Point
    {
        public double x, y;
        public Point(double x = 0, double y = 0)
        {
            this.x = x;
            this.y = y;
        }

        //����+
        public static Point operator +(Point b, Point a)
        {
            Point p = new Point(b.x, b.y);
            p.x += a.x;
            p.y += a.y;
            return p;
        }
        //����-
        public static Point operator -(Point b, Point a)
        {
            Point p = new Point(b.x, b.y);
            p.y -= a.y;
            p.x -= a.x;
            return p;
        }
        //���
        public static double operator *(Point b, Point a)
        {
            return a.x * b.x + a.y * b.y;
        }
        //���
        //P^Q>0,P��Q��˳ʱ�뷽��<0��P��Q����ʱ�뷽��=0��P��Q���ߣ�����ͬ�����
        public static double operator ^(Point a, Point b)
        {
            return a.x * b.y - b.x * a.y;
        }
    };
    //�õ����еı߽��
    public void getBoundaryPoints()
    {
        FindChild(frameObjectFather);
    }
    void FindChild(GameObject child)
    {
        //����forѭ�� ��ȡ�����µ�ȫ��������
        for (int c = 0; c < child.transform.childCount; c++)
        {
            //����������»��������� �ͽ������崫����лص����� ֱ������û��������Ϊֹ
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
        Point P1, P2; //�����һ���ߵ���������
        for (int i = 0, j = n - 1; i < n; j = i++)
        {
            P1 = new Point(boundaryPoints[i].transform.GetComponent<RectTransform>().anchoredPosition.x, boundaryPoints[i].transform.GetComponent<RectTransform>().anchoredPosition.y);
            P2 = new Point(boundaryPoints[j].transform.GetComponent<RectTransform>().anchoredPosition.x, boundaryPoints[j].transform.GetComponent<RectTransform>().anchoredPosition.y);


            if (OnSegment(P1, P2, P)) return true; //���ڶ����һ������
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
        //��ʱ���ʽ�����
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
                for (int k = 0; k < 5; k++)//ÿ��������� k ��
                {
                    //����(0,1)֮��������
                    float tmp = (float)ra.NextDouble();
                    float tmp2 = (float)ra.NextDouble();
                    float uix = min_x + tmp * step_w;
                    float uiy = min_y + tmp2 * step_h;

                    //�����ǹ涨�ķ�Χ��
                    if (InPolygon(new Point(uix, uiy)))
                    {
                        GameObject go = show(uix, uiy);
                        //�õ��ò��������з����ͼ��
                        float realx = leftDown.transform.position.x + (uix / w) * mapCamera.GetComponent<Camera>().orthographicSize * 2;
                        float realz = leftDown.transform.position.z + (uiy / h) * mapCamera.GetComponent<Camera>().orthographicSize * 2;
                        //�õ������ӽǵ�sprite
                        getView(new Vector3(realx, tall, realz), i, j, k, go);
                        num = num + 6;
                    }
                    else
                    {
                        delete(i, j, k);
                    }

                    /*
                    //��û�д������ͼ�ﵽһ�������������д������õ�ÿ��ͼ������
                    if(unsolvedSprite.Count >= size * size)
                    {
                        viewManager.getMultipleView(size, unsolvedSprite);
                        //�õ�ͼ֮�󴫸�llmȻ��õ������ٱ���
                        //send
                        List<forPicture> t = new List<forPicture>();
                        for(int z = 0; z < size * size; z++)
                        {
                            t.Add(unsolvedSprite[0]);
                            unsolvedSprite.RemoveAt(0);
                        }
                        //����δ��������ͼƬ��������Ϣ���� t �У�����forscore
                        forScore.Add(t);
                        //�����ɵ���ͼƬ������������
                        connect.SendFrame();
                    }
                    
                    */
                }
            }
        }
        NowTime = DateTime.Now.ToLocalTime();
        //��ʱ���ʽ�����
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
    //ȥ�����ֱȽϵ͵��ӽǣ����ȫ���ӽǵ����ֶ��ͣ���ȥ�������
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

    //���ݲ�ͬ�ķ����õ�sprite
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
    
    //center����ʵ�����е������
    public void SetCenterAndSampling(GameObject center)
    {

        float x = (center.transform.position.x - leftDown.transform.position.x) / (cameraSize * 2) * fixWidth;
        float y = (center.transform.position.z - leftDown.transform.position.z) / (cameraSize * 2) * fixHeight;
        cameraOnUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);

        chooseSamplePoints(30);

    }
    //���ո���˳�򽫴�ѡ�㲻ͣ�ؼ���chooSamplePoints��
    public void chooseSamplePoints(int number)
    {
        int width = 3;
        int height = 3;
        //�����������ĵ��λ��������

        //�ж����ĵ��λ�ô����ĸ�������
        float center_x = cameraOnUI.GetComponent<RectTransform>().anchoredPosition.x;
        float center_y = cameraOnUI.GetComponent<RectTransform>().anchoredPosition.y;
        int x_grid = (int)(center_x / step_w);
        int y_grid = (int)(center_y / step_h);
        //ֻ�����ĵ㸽�����Ӵ�����
        int min_grad_x = 0 > (x_grid - (int)width) ? 0 : (x_grid - (int)width);
        int min_grad_y = 0 > (y_grid - (int)height) ? 0 : (y_grid - (int)height);
        Debug.Log("x:" + x_grid);
        Debug.Log("y:" + y_grid);
        //�Ȳ��Լ����ڸ���
        for (int k = 0; k < 5; k++)//ÿ���������k��
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
                for (int k = 0; k < 5; k++)//ÿ���������k��
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

    public List<Vector3> wayRecorder = new List<Vector3>(); //��¼ѡ�����key·����������������
    //���ݷ���ѡ����һ��Ӧ���ƶ����Ĳ�����
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
                //����
                System.Random ra = new System.Random(unchecked((int)DateTime.Now.Ticks));
                Quaternion nextAngel = choosedSamplePoints[0].views[ra.Next(0, 5)].rot;

                choosedSamplePoints[0].label.GetComponent<CustomUI.CircularImage>().color = Color.blue;
                ///����һ��Image����
                GameObject newImage = new GameObject("Imageiiii");
                //��newImage������Canvas������ӽڵ����
                newImage.transform.parent = fatherForBag.transform;
                //���Image���
                newImage.AddComponent<Image>();
                //��̬������ͼ��ֵ��Image

                newImage.GetComponent<Image>().sprite = choosedSamplePoints[0].views[0].sprite;

                choosedSamplePoints.RemoveAt(0);
                //����ǰѡ���λ�ü��������
                wayRecorder.Add(next);
                if (wayRecorder.Count >= 3)
                {
                    moveThroughWay.GetWayPointsBetween(wayRecorder[wayRecorder.Count - 2], next, wayRecorder[wayRecorder.Count - 3], next + new Vector3(19, 1, 671), lastAngel, nextAngel);
                }
                else if (wayRecorder.Count == 2)//��ʱֻѡ��һ���ؼ���
                {
                    moveThroughWay.GetWayPointsBetween(wayRecorder[wayRecorder.Count - 2], next, wayRecorder[wayRecorder.Count - 2] - new Vector3(-20, 0, 20), next + new Vector3(19, 1, 671), lastAngel, nextAngel);
                }


            }
            //Vector3 next = ;// = choosedSamplePoints[] //�ҳ���һ����

            //����·��
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
        //����������ʵ�����λ������
        public Vector3 pos;
        //һ�������㲻ͬ��View
        public List<View> views;
        //��Canvas�϶�Ӧ��UI
        public GameObject label;
        //�������Ƿ��Ѿ���ѡ�����ָ�ƶ�ʱ�����뿼�Ƿ�Χ��
        public bool passed;
        //�������Ƿ���Ч
        public bool valid;
        public void setPos(Vector3 p)
        {
            pos = p;
        }
    }
    public struct View
    {
          public  Quaternion rot;//����ķ���
          public  float score; //����Ӧ������
          public  Sprite sprite; //��ǰ�������sprite
        }
}
    public struct forPicture {
       public int i;
       public int j;
       public int k;
       public int v;
    }*/