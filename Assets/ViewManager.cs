using UnityEngine;
using System.IO;
using System.Drawing;
using System;
using System.Drawing.Imaging;
using System.Collections.Generic;

public class ViewManager : MonoBehaviour
{   
    static int num1 = 0;
    public  Quaternion[] quaternion = new Quaternion[4];
    private static sampleThroughWay stw;
    // Start is called before the first frame update
    void Start()
    {
        num1 = 0;
        //testCamera = GameObject.Find("SampleCamera");
        stw = gameObject.GetComponent<sampleThroughWay>();
    }
    
    //�õ�num x num��view��ɵ�һ����ͼƬ����������ÿ��ͼƬ�ֱ�����texture�浽��Ӧ�Ľṹ����
    public static void getMultipleView(int num,List<ForPicture> forPictures)
    {
        int gap = 10;
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        int singleWidth = screenWidth / num;
        int singleHeight = screenHeight / num;
        Texture2D combinedView = new Texture2D((int)singleWidth * num + (num-1) * gap, (int)singleHeight * num + (num - 1) * gap, TextureFormat.RGB24, false);

        // ����һ��RenderTexture����,�趨����0��ʾ���������
        //RenderTexture rt = new RenderTexture((int)screenWidth, (int)screenHeight, 0);
        //Camera camera= GameObject.Find("TestCamera") .GetComponent<Camera>();

        for (int i = 0; i < num; i++)
        for(int p = 0; p < num;p++)
        {//��ÿ�У���ÿ�У��ڴ�ͼ���Ǵ��������ŵ�
                // 34
                // 12 
          //  camera.targetTexture = rt;
            ForPicture t = forPictures[i * num + p];
          
            //testCamera.transform.position = spg.allSamplePoints[t.i, t.j, t.k].pos;
            //testCamera.transform.rotation = spg.allSamplePoints[t.i, t.j, t.k].views[t.v].rot;
            //camera.Render();

            // �������rt, �������ж�ȡ���ء�.ReadPixels���RenderTexture.active�ж�ȡ����
           // RenderTexture.active = rt;
            // ����һ���µ�Texture2D���󣬴�СΪ w, h
            //Texture2D screenShot = new Texture2D((int)screenWidth, screenHeight, TextureFormat.RGB24, false);
            //rect ָ����Ҫ��RenderTexture�ж�ȡ������
            //��������������Ŀ����RenderTexture�ĳߴ���ͬ����ô���ȡ����RenderTexture������
           // screenShot.ReadPixels(new Rect(0, 0, screenWidth, screenHeight), 0, 0);
            //�������texture���ǽ�����������
           // screenShot.Apply();
            Texture2D resize = ResizeTexture(stw.spbl[t.bunch].spl[t.samplePoint].views[t.view].texture, singleWidth, singleHeight);
            //Sprite newS =  Sprite.Create(resize, new Rect(0, 0, resize.width, resize.height), new Vector2(0.5f, 0.5f));
            for (int z = 0; z < singleWidth; z++)
            {
                for(int j = 0; j < singleHeight; j++)
                {
                    int realx = (singleWidth) * p + gap * (p) + z;
                    int realy = (singleHeight) * i + gap * (i) + j;
                    combinedView.SetPixel(realx, realy, resize.GetPixel(z, j));
                }
            }
            
        }

        // ������ز�������ʹ��camera��������Ļ����ʾ
        //camera.targetTexture = null;
        //ps: camera2.targetTexture = null;
        //RenderTexture.active = null; // JC: added to avoid errors
        //Destroy(rt);
        // �����Щ�������ݣ���һ��pngͼƬ�ļ�
        byte[] bytes = combinedView.EncodeToJPG();
        string filename = "D:/CameraControl/Assets/sampleViews/" +num1+".jpg";
        num1++;
        File.WriteAllBytes(filename, bytes);
    }
    private static Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        if (source != null)
        {
            // ������ʱ��RenderTexture
            RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            // ����source������RenderTexture��
            UnityEngine.Graphics.Blit(source, renderTex);
            // ������ǰRenderTexture����״̬
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            // �����޸ĺ������������Դ������ͬѹ����ʽ
            Texture2D resizedTexture = new Texture2D(width, height, source.format, false);
            // ��ȡ���ص�������������
            resizedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            // Ӧ���޸ĵ�GPU��
            resizedTexture.Apply();
            // ֹͣ��ǰRenderTexture����
            RenderTexture.active = previous;
            // �ͷ��ڴ�
            RenderTexture.ReleaseTemporary(renderTex);
            return resizedTexture;
        }
        else
        {
            return null;
        }
    }
    //������������ڵ�ͼ���texture
    public static Texture2D CaptureCameraForTexture(Camera camera)
    {
        // ����һ��RenderTexture����,�趨����0��ʾ���������
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);
        // ��ʱ������������targetTextureΪrt, ���ֶ���Ⱦ������
        camera.targetTexture = rt;
        camera.Render();
        // �������rt, �������ж�ȡ���ء�.ReadPixels���RenderTexture.active�ж�ȡ����
        RenderTexture.active = rt;
        //����һ���µ�Texture2D���󣬴�СΪ w,h
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //rect ָ����Ҫ��RenderTexture�ж�ȡ������
        //��������������Ŀ����RenderTexture�ĳߴ���ͬ����ô���ȡ����RenderTexture������
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
       //�������texture���ǽ�����������
        screenShot.Apply();
        // ������ز�������ʹ��camera��������Ļ����ʾ
        camera.targetTexture = null;
        //ps: camera2.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        //��texture����һ��sprite
        return screenShot;
            //Sprite.Create(screenShot, new Rect(0, 0, screenShot.width, screenShot.height), new Vector2(0.5f, 0.5f));

    }
    static public Sprite TextureToSprite(Texture2D texture2D)
    {
        return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));
    }
    static public Sprite ByteToSprite(byte[] ImgByte)
    {
        Texture2D texture2D = new Texture2D(1080, 1920);

        texture2D.LoadImage(ImgByte);

        return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f));

        //finalFrameImage.sprite = sprite;

    }
    //�����ֽ�����
    static private byte[] ReturnImgByte(string UIPath)
    {
        FileStream fs = new FileStream(UIPath, FileMode.Open);

        byte[] imgByte = new byte[fs.Length];
        fs.Read(imgByte, 0, imgByte.Length);
        fs.Close();
        return imgByte;
    }
}
