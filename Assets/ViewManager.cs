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
    
    //得到num x num个view组成的一整张图片保存下来，每个图片分别生成texture存到相应的结构体中
    public static void getMultipleView(int num,List<ForPicture> forPictures)
    {
        int gap = 10;
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        int singleWidth = screenWidth / num;
        int singleHeight = screenHeight / num;
        Texture2D combinedView = new Texture2D((int)singleWidth * num + (num-1) * gap, (int)singleHeight * num + (num - 1) * gap, TextureFormat.RGB24, false);

        // 创建一个RenderTexture对象,设定长宽，0表示不启用深度
        //RenderTexture rt = new RenderTexture((int)screenWidth, (int)screenHeight, 0);
        //Camera camera= GameObject.Find("TestCamera") .GetComponent<Camera>();

        for (int i = 0; i < num; i++)
        for(int p = 0; p < num;p++)
        {//先每行，再每列，在大图里是从下往上排的
                // 34
                // 12 
          //  camera.targetTexture = rt;
            ForPicture t = forPictures[i * num + p];
          
            //testCamera.transform.position = spg.allSamplePoints[t.i, t.j, t.k].pos;
            //testCamera.transform.rotation = spg.allSamplePoints[t.i, t.j, t.k].views[t.v].rot;
            //camera.Render();

            // 激活这个rt, 并从中中读取像素。.ReadPixels会从RenderTexture.active中读取像素
           // RenderTexture.active = rt;
            // 创建一个新的Texture2D对象，大小为 w, h
            //Texture2D screenShot = new Texture2D((int)screenWidth, screenHeight, TextureFormat.RGB24, false);
            //rect 指定了要从RenderTexture中读取的区域。
            //如果这个矩形区域的宽高与RenderTexture的尺寸相同，那么会读取整个RenderTexture的内容
           // screenShot.ReadPixels(new Rect(0, 0, screenWidth, screenHeight), 0, 0);
            //现在这个texture上是截下来的内容
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

        // 重置相关参数，以使用camera继续在屏幕上显示
        //camera.targetTexture = null;
        //ps: camera2.targetTexture = null;
        //RenderTexture.active = null; // JC: added to avoid errors
        //Destroy(rt);
        // 最后将这些纹理数据，成一个png图片文件
        byte[] bytes = combinedView.EncodeToJPG();
        string filename = "D:/CameraControl/Assets/sampleViews/" +num1+".jpg";
        num1++;
        File.WriteAllBytes(filename, bytes);
    }
    private static Texture2D ResizeTexture(Texture2D source, int width, int height)
    {
        if (source != null)
        {
            // 创建临时的RenderTexture
            RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
            // 复制source的纹理到RenderTexture里
            UnityEngine.Graphics.Blit(source, renderTex);
            // 开启当前RenderTexture激活状态
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;
            // 创建修改后的纹理，保持与源纹理相同压缩格式
            Texture2D resizedTexture = new Texture2D(width, height, source.format, false);
            // 读取像素到创建的纹理中
            resizedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            // 应用修改到GPU上
            resizedTexture.Apply();
            // 停止当前RenderTexture工作
            RenderTexture.active = previous;
            // 释放内存
            RenderTexture.ReleaseTemporary(renderTex);
            return resizedTexture;
        }
        else
        {
            return null;
        }
    }
    //保存现在相机内的图像进texture
    public static Texture2D CaptureCameraForTexture(Camera camera)
    {
        // 创建一个RenderTexture对象,设定长宽，0表示不启用深度
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 0);
        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
        camera.targetTexture = rt;
        camera.Render();
        // 激活这个rt, 并从中中读取像素。.ReadPixels会从RenderTexture.active中读取像素
        RenderTexture.active = rt;
        //创建一个新的Texture2D对象，大小为 w,h
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        //rect 指定了要从RenderTexture中读取的区域。
        //如果这个矩形区域的宽高与RenderTexture的尺寸相同，那么会读取整个RenderTexture的内容
        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
       //现在这个texture上是截下来的内容
        screenShot.Apply();
        // 重置相关参数，以使用camera继续在屏幕上显示
        camera.targetTexture = null;
        //ps: camera2.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        //用texture创建一个sprite
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
    //返回字节数组
    static private byte[] ReturnImgByte(string UIPath)
    {
        FileStream fs = new FileStream(UIPath, FileMode.Open);

        byte[] imgByte = new byte[fs.Length];
        fs.Read(imgByte, 0, imgByte.Length);
        fs.Close();
        return imgByte;
    }
}
