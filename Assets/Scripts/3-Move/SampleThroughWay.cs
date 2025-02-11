using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
 //������������Ŵֲ�·�����ɲ�����
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
        //��ʼ����ͼ�������������ϻ�����,�õ�Map[Point]
        Astar.instance.InitGrid((int)mapCamera.GetComponent<Camera>().orthographicSize);
        Debug.Log("Finish Init Map");
    }
    public void GenerateSampleCenters()
    {
        //����TSP������Ŵ��㷨���������е�չ�����λ������һ�����ʻ�·������˳��-getcourseway��
        //Ȼ����ݵõ��ķ���˳�򡢳���������Ϣ����Astar�㷨���ɴֲ�·�������йؼ���
        List<Point> points =  GetCourseWay.instance.GetOrderAndWayPoints();   
        Debug.Log("get course way");
        //���ݴֲ�·�����ٻ����ӣ�ÿ���ؼ������һ�����ӣ��޸�map[x,y]���ڵĲ������ӱ��(label)
        GetSampleCenters(points);
     
    }
    public void Init()
    {
            fatherForBag = GameObject.Find("fatherForCell");
            mainCamera = GameObject.Find("Main Camera");
            cameraOnUI = GameObject.Find("peopleUI"); ;//��UI�ϴ����ƶ��е��������Բ��
            for (int i = 0; i < 6; i++)
            {
                quaternions.Add(Quaternion.Euler(0, 360 / 6 * i, 0));
            }
            //StartCoroutine("GenerateNextCenter");
            //�������Ӧui
            leftDown = GameObject.Find("leftDown");
            mapCamera = GameObject.Find("mapCamera");
            navigationMap = GameObject.Find("navigationMap");
            dl = navigationMap.GetComponent<drawline>();
            sampleCamera = GameObject.Find("sampleCamera");
}   
    public List<SamplePointBunch> spbl = new();
    private GameObject sampleCamera;//�������������
    public List<Quaternion> quaternions = new();  //�������ת�Ƕ�
    List<ForPicture> unsolvedSprite = new();//��ͼƬ��Ӧ�����ĸ�bunch�ĸ�samplePoint�ĸ�View
    public List<List<ForPicture>> forScore = new();//ÿһ�ι�ͬ����������forScore��
    public float tall = 1.7f;
    public int sampleSolveSize;
    int sampleHalfNum = 2;//bunch�����ĸ��Ӻ�����ΧһȦ�������
    
    //���ݴֲ�·����õ�������Ҫ�Ĳ�������
    public void GetSampleCenters(List<Point> points)
    {
        int label = 0;
        //���ֲ�·���ع̶�����������
       for(int i = 0; i < points.Count; i++)
        {
            if (Astar.instance.map[points[i].x, points[i].y].samplePointBunchLabel == label - 1 || Astar.instance.map[points[i].x, points[i].y].samplePointBunchLabel!=-2)
            {
                //�Ѿ��б�ŵľ�������������ǰ������Ѿ�����ĳ������������
                continue;
            }
            //��ǰ��������Ϊ����������ģ����뼯���У�����һ���µĲ�����������
            sampleCenters.Add(points[i]);
            //�����η�Χ�ڶ������bunch,�����bunch�����ѡK���������ɲ�����
            for(int j = -sampleHalfNum; j <= sampleHalfNum; j++)
            {
                for(int k = -sampleHalfNum; k <= sampleHalfNum; k++)
                {
                    if(Astar.instance.map[points[i].x + j, points[i].y + k].isValid)
                    {
                        //��������������Ч�ģ�����block��ŷ���ȥ
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
            //�������½�
            Vector2 pos = point.worldpos;
            //����һ���µĲ����㼯��
            SamplePointBunch bunch = new SamplePointBunch();
            bunch.pos = new Vector3(pos.x,1.7f,pos.y);
            bunch.spl = new List<SamplePoint>();
            bunch.label = point.samplePointBunchLabel;

            //bunch�����½�
            Vector2 ld = new Vector2(pos.x - sampleHalfNum * Astar.instance.lengthOfBox , pos.y - sampleHalfNum * Astar.instance.lengthOfBox);

            //�趨sampleNum���������λ��
            int k = 0;
            while(k < sampleNum){
                //����(0,1)֮��������
                float tmpWidth = (float)ra.NextDouble();
                float tmpHeight = (float)ra.NextDouble();
                float realx = ld.x + tmpWidth * (2 * sampleHalfNum + 1) * Astar.instance.lengthOfBox;
                float realy = ld.y + tmpHeight * (2 * sampleHalfNum + 1) * Astar.instance.lengthOfBox;
                //���ò���������map�ĸ�����
                int x = (int)( (realx - leftDown.transform.position.x) / Astar.instance.lengthOfBox);
                int y = (int)( (realy - leftDown.transform.position.z) / Astar.instance.lengthOfBox);
                //��Ӧ�ĸ����ڵ�ǰbunch֮�ڣ�����Ч��
                if (Astar.instance.map[x,y].samplePointBunchLabel == bunch.label)
                {
                    SamplePoint samplePoint = new SamplePoint();
                    samplePoint.pos = new Vector3(realx, 1.7f, realy);
                    samplePoint.label = ShowUI(GetUIPos(realx, realy));
                    //�õ��ò��������з����ͼ��,�õ������ӽǵ�sprite
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
                    //����ǰ���samplePoint���뵱ǰ��bunch��
                    bunch.spl.Add(samplePoint);
                }
                sampleSolveSize = 6;
                //��û�д������ͼ�ﵽһ�������������д������õ�ÿ��ͼ������
                if(unsolvedSprite.Count >= sampleSolveSize * sampleSolveSize)
                {
                    ViewManager.getMultipleView(sampleSolveSize, unsolvedSprite);
                    //�õ�ͼ֮�󴫸�llmȻ��õ������ٱ���
                    List<ForPicture> t = new List<ForPicture>();
                    for(int z = 0; z < sampleSolveSize * sampleSolveSize; z++)
                    {
                        t.Add(unsolvedSprite[0]);
                        unsolvedSprite.RemoveAt(0);
                    }
                    //����δ��������ͼƬ��������Ϣ���� t �У�����forscore
                    forScore.Add(t);
                    //�������û���⣬�����ɵ���ͼƬ������������
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
                        //����ӽ����ֲ�����ɾ������ӽ�
                        var  temp =   spbl[z].spl[i].views[j];
                        temp.valid = false;
                        spbl[z].spl[i].views[j] = temp;
                    }
                    else
                    {
                        t++;
                    }
                }
                if(t == 0)//�����ȫû�к��ʵ��ӽǣ�ɾ�����������
                {
                    var temp = spbl[z].spl[i];
                    temp.valid = false;
                    spbl[z].spl[i] = temp;
                    spbl[z].spl[i].label.SetActive(false);//ͼ��Ҳ��ȥ
                    Destroy(spbl[z].spl[i].label);
                }


            }



        }
    }
#region ��ʼ����ɺ�ʼ����·��
    public Quaternion lastAngel;//��¼��һ�������λ��
    private List<Vector3> wayRecorder = new(); //��¼ѡ�����key·����Ϊ����������
    public void Add2WayRecorder(Vector3 v)
    {
        wayRecorder.Add(v);
    }
    public GameObject mainCamera;//�û���Ұ���
    /*public void setCenterAndStartSampling(GameObject center){
        float x = (center.transform.position.x - leftDown.transform.position.x) / (cameraSize * 2) * mapWidth;
        float y = (center.transform.position.z - leftDown.transform.position.z) / (cameraSize * 2) * mapHeight;
        cameraOnUI.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
        //�ҵ���ǰ�����������Ĳ����㲢����
        //�ж����ĵ��λ�ô����ĸ�������
        int gridx = (int)((mainCamera.transform.position.x - leftDown.transform.position.x) / Astar.instance.lengthOfBox);
        int gridy = (int)((mainCamera.transform.position.z - leftDown.transform.position.z) / Astar.instance.lengthOfBox);
        if (Astar.instance.map[gridx, gridy].samplePointBunchLabel == -2)
        {
            Debug.Log("��ǰѡ��ĵ㲻���κ�һ��bunch��");
            return; //˵����ǰ�����ڵĵ㲻���κ�һ��bunch��
        }
        int next = Astar.instance.map[gridx, gridy].samplePointBunchLabel;
        SamplePointBunch bunch = spbl[next];
        if (!bunch.valid)
        {//�����Ѿ���ɾ���ˣ���δ���
            while (!bunch.valid)
            {
                //�ҵ���һ����Ч��
                next = next + 1 >= spbl.Count ? 0 : next + 1;
                bunch = spbl[next];

            }
            return;
        }
        int maxSamplePoint = 0;
        int maxView = 0;
        float _maxScore = -1;
        //ѡ������������ӵ�
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
            //����ǰ�����ѡ��Ĳ����㼯���У����ҽ���Ӧ��UIͼ����ɫ����
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
    //��ĳ������Ϊ��㣬����ʣ�µ�ȫ��·��

    //����·���ƶ�
    
    public Vector3 last;
    //Ԥ����֮��һ������������·����ÿ��ѡ���������ߵľ����£������Ǹı�����������ѡ������
    public void GenerateWay()
    {
        //ÿ�ζ���������������
        finalWay.Clear();
        //�ж����ĵ��λ�ô����ĸ�������
        int x = (int)((mainCamera.transform.position.x - leftDown.transform.position.x) / Astar.instance.lengthOfBox);
        int y = (int)((mainCamera.transform.position.z - leftDown.transform.position.z) / Astar.instance.lengthOfBox);
        int bunchLabel = Astar.instance.map[x, y].samplePointBunchLabel;
        if (bunchLabel == -2)
        {
            Debug.Log("��ǰѡ��ĵ㲻���κ�һ��bunch��");
            return; //˵����ǰ�����ڵĵ㲻���κ�һ��bunch��
        } 
        //�Ե�ǰbunchLabelΪ��㣬��������bunch
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
                    {//ѡ������bunch�����������ӵ�
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
                        //�ı䵱ǰ��point״̬Ϊpassed�����ҽ�UI��Ϊ��ɫ
                        var point = bunch.spl[maxSamplePoint];
                        point.passed = true;
                        var ui = point.label;
                        ui.GetComponent<CustomUI.CircularImage>().color = Color.green;
                        point.label = ui;
                        bunch.spl[maxSamplePoint] = point;
                        spbl[i] = bunch;                        
                        finalWay.Add(new PosAndView() { quaternion = spbl[i].spl[maxSamplePoint].views[maxView].rot, point = spbl[i].spl[maxSamplePoint]});
                        //�����view��ͼ����ʾ����
                        //showSprite(spbl[i].spl[maxSamplePoint].views[maxView].texture);
                        ////����ǰ�����ѡ��Ĳ����㼯���У����ҽ���Ӧ��UIͼ����ɫ����
                        //choosedSamplePoints.Add(bunch.spl[maxSamplePoint]);
                    }
                }
            }
            k++;
        }

        //�������ɵ�·�����õ�ƽ���㣬����ƽ���ƶ�
        while (finalWay.Count > 0)
        {

            Vector3 next = finalWay[0].point.pos;
            Quaternion nextAngel = finalWay[0].quaternion;
            finalWay[0].point.label.GetComponent<CustomUI.CircularImage>().color = Color.blue;

            ShowSprite(finalWay[0].point.views[finalWay[0].view].texture);

            finalWay.RemoveAt(0);
            //����ǰѡ���λ�ü��������
            wayRecorder.Add(next);
            if (wayRecorder.Count >= 3)
            {
                MoveThroughWay.instance.GetWayPointsBetween(wayRecorder[wayRecorder.Count - 2], next, wayRecorder[wayRecorder.Count - 3], next + new Vector3(19, 1, 671), lastAngel, nextAngel);
            }
            else if (wayRecorder.Count == 2)//��ʱֻѡ��һ���ؼ���
            {
                MoveThroughWay.instance.GetWayPointsBetween(wayRecorder[wayRecorder.Count - 2], next, wayRecorder[wayRecorder.Count - 2] - new Vector3(-20, 0, 20), next + new Vector3(19, 1, 671), lastAngel, nextAngel);
            }

        }
    }    
    private GameObject fatherForBag;
    private void ShowSprite(Texture2D texture)
    {
        //����һ��Image����
        GameObject newImage = new GameObject("Image");
        //��newImage������Canvas������ӽڵ����
        newImage.transform.parent = fatherForBag.transform;
        //���Image���
        newImage.AddComponent<Image>();
        //��̬������ͼ��ֵ��Image
        newImage.GetComponent<Image>().sprite = ViewManager.TextureToSprite(texture);
    } 
    /*public List<SamplePoint> choosedSamplePoints; // ���ݵ�ǰ���(�����ĵ�)��λ�������δ����������ĵ��list
     * public void chooseSamplePoints(int number)
    {   
        //�ж����ĵ��λ�ô����ĸ�������
        int x = (int)((mainCamera.transform.position.x - leftDown.transform.position.x) / Astar.instance.lengthOfBox);
        int y = (int)((mainCamera.transform.position.z - leftDown.transform.position.z) / Astar.instance.lengthOfBox);
        if (Astar.instance.map[x,y].samplePointBunchLabel == -2){
            Debug.Log("��ǰѡ��ĵ㲻���κ�һ��bunch��");
            return; //˵����ǰ�����ڵĵ㲻���κ�һ��bunch��
        }


        //����ǰbunch������valid���������
        SamplePointBunch bunch = spbl[Astar.instance.map[x, y].samplePointBunchLabel];
        if (!bunch.valid)
        {
            Debug.Log("��ǰ����û�к��ʲ�����");
             next = Astar.instance.map[x, y].samplePointBunchLabel;
            //��������һ��
            while (!bunch.valid)
            {
                //�ҵ���һ����Ч��
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
        {//�����Ѿ���ɾ���ˣ���δ���
            while (!bunch.valid)
            {
                //�ҵ���һ����Ч��
                next = next + 1 >= spbl.Count ? 0 : next + 1;
                bunch = spbl[next];

            }
            return;
        }
        int maxSamplePoint=0;
        int maxView=0;
        float _maxScore= -1;
        //ѡ������������ӵ�
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
                //����ǰ�����ѡ��Ĳ����㼯���У����ҽ���Ӧ��UIͼ����ɫ����
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

    //����ѡ������·���Լ������ߵ���������·�����Լ����û�����������Ӧ
    public bool userResponse;
    /*
    IEnumerator GenerateNextCenter()
    {//.ѡ���˲���ʾsprite�����һ��
        while (true)
        {
            if (userResponse){
                //��������

                //��������·��
                GenerateWay();
            }

            if (finalWay.Count > 0){
              
                Vector3 next = finalWay[0].point.pos;
                Quaternion nextAngel = finalWay[0].quaternion;
                finalWay[0].point.label.GetComponent<CustomUI.CircularImage>().color = Color.blue;
                
                ShowSprite(finalWay[0].point.views[finalWay[0].view].texture);

                finalWay.RemoveAt(0);
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
            yield return new WaitForSeconds(1);
        }

    }*/
    #endregion
   

#region ���ɲ���ʾ�������Ӧ��UI    
    private  GameObject leftDown;
    private  GameObject mapCamera;
    private  GameObject navigationMap;
    private drawline dl;
    private GameObject cameraOnUI;//��UI�ϴ����ƶ��е��������Բ��
    //����3ά�����λ�õõ���Ӧ��UIλ��
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
   
#region ���ݽṹ
public struct SamplePoint
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
public struct PosAndView
{
    public Quaternion quaternion;
    public SamplePoint point;
    public int view;
}
public struct View
{
    public Quaternion rot;//����ķ���
    public float score; //����Ӧ������
    public Sprite sprite; //��ǰ�������sprite
    public Texture2D texture;//��ǰ������sprite��Ӧ��texture
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
