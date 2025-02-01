using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net.Sockets;
using System.IO;
using System.Text;
using UnityEngine.Video;
using System;
using System.Linq;

public class Connect2Python : MonoBehaviour{
    
    public string serverIP ;
    public int serverPort;
    public DataObject readDataObject;
    TcpClient client;
    NetworkStream stream;// 网络流，用于和服务器通信  
    bool connected;    
    public List<string> responseList;
    public int responseListCount;
    public GameObject voiceRecorder;
    private sampleThroughWay stw;
    void Start(){
        
    }
    public void Init()
    {
        serverIP = "127.0.0.1";
        serverPort = 25001;
        connected = false;
        responseListCount = 0;
        readDataObject = new DataObject();
        responseList = new List<string>();
        stw = gameObject.GetComponent<sampleThroughWay>();
    }    
    public void Connect2server(){
        client = new TcpClient(serverIP, serverPort);//tell client server's inf,和服务器端建立连接
        stream = client.GetStream();
        connected = true;
        Debug.Log("成功连接到服务器");
        stream.BeginRead(readDataObject.bytes, readDataObject.writeIndex, readDataObject.remainLength, ReceiveCallBack, stream);
    }
    
    private void ReceiveCallBack(IAsyncResult iar)
    {
        try
        {
            Debug.Log("收到消息");
            NetworkStream stream = (NetworkStream)iar.AsyncState;
            //返回这次读了多少字节的数据
            int count = stream.EndRead(iar);
            Debug.Log("receiveall: " + count);
            readDataObject.writeIndex += count;
            HandleReceiveData();
            //剩余空间不够了，移动位置顺便扩容
            if (readDataObject.remainLength < 8)
            {
                readDataObject.MoveBytes();
                readDataObject.Resize(readDataObject.dataLength * 2);
            }
            Debug.Log("BeginRead");
            //继续开始异步接收消息
            stream.BeginRead(readDataObject.bytes, readDataObject.writeIndex, readDataObject.remainLength, ReceiveCallBack, stream);
        }
        catch (SocketException se)
        {
            Debug.Log("接收失败");
        }
    }
    int test = 0;
    private void HandleReceiveData()
    {
       Debug.Log("      HandleReceiveData");
       if (readDataObject.dataLength <= 4){
            return;
        }
        //是用小端存的，这是计算方式,bodylenth是不包含前面两个字节
        Int32 bodyLenth = (Int32)(readDataObject.bytes[readDataObject.readIndex] | readDataObject.bytes[readDataObject.readIndex + 1] << 8 | readDataObject.bytes[readDataObject.readIndex+2] << 16| readDataObject.bytes[readDataObject.readIndex + 3] << 24);
        Debug.Log("      receive: " + bodyLenth);
        //解决分包问题，此时数据还没完全传过来呢
        if (readDataObject.dataLength < bodyLenth + 4)
        {
            return;
        }
        //读了总长度,readIndex + 4
        readDataObject.readIndex += 4;
        //解析这一次传过来的话
        Int16 flag = (Int16)(readDataObject.bytes[readDataObject.readIndex] | readDataObject.bytes[readDataObject.readIndex + 1] << 8);
        readDataObject.readIndex += 2;
        Debug.Log("      flag is: " + flag);
        //注意，python传过来的数据为length + flag + text
        //length为 flag + text的长度，所以要减去2（flag）
        string response = Encoding.UTF8.GetString(readDataObject.bytes, readDataObject.readIndex, bodyLenth-2);
        readDataObject.readIndex += (bodyLenth-2);
        readDataObject.CheckAndMoveBytes();
        Debug.Log("      收到消息:" +test +"   " + response);
        test = test + 1;
        if(flag == 0)
        {
            float[] ans = new float[stw.sampleSolveSize * stw.sampleSolveSize];
            //将response的内容一个个填入ans数组中

            //
            if (stw!=null && stw.forScore.Count > 0)
            {
                int i = 0;
                foreach (ForPicture t in stw.forScore[0])
                {

                    var element = stw.spbl[t.bunch].spl[t.samplePoint].views[t.view];
                    element.score = ans[i++];
                    stw.spbl[t.bunch].spl[t.samplePoint].views[t.view] = element;
                }
                stw.forScore.RemoveAt(0); 
            }
        }
        //再次调用，解决粘包问题
        HandleReceiveData();
    }

    //点击send Message button会触发这个
    public void sendMessageEnable(){
        if (connected) {
             //  vc.enableFrame();
        }
    } 
    /*void Update(){
      if (ifchangestate){
            sc.changeState(response);
            origin_video.GetComponent<VideoPlayer>().Play();
            target_video.GetComponent<VideoPlayer>().Play();
            target_video2.GetComponent<VideoPlayer>().Play();
            cp.cp();
            ifchangestate = false;
        }  
        if (connected){
            if(vc.frameOK == true){
                SendFrameMessage();
                vc.frameOK = false;
            }
            if (vr.voiceClipPrepared){
                sendVoice();
                vr.voiceClipPrepared = false;
            }
        }
    }*/

    private Queue<DataObject> writeQueue = new Queue<DataObject>();
    private Byte[] getTextBytes(string text)
    {
        //发送text,前面是标记
        byte[] textBytes = Encoding.UTF8.GetBytes("text" + text);
        Int16 length = (Int16)textBytes.Length;
        byte[] lenthBytes = BitConverter.GetBytes(length);
        Debug.Log(length);
        if (!BitConverter.IsLittleEndian)
        {
            lenthBytes.Reverse();
        }
        //总长度 + name 长度 +name+ 本体
        byte[] sendBytes = lenthBytes.Concat(textBytes).ToArray();
        //申请一个新的dataObject，此时其大小由发送的数据大小决定
        return sendBytes;
    }
    private Byte[] getFrameBytes(string framePath)
    {
        FileStream fs = new FileStream(framePath, FileMode.OpenOrCreate, FileAccess.Read);
        BinaryReader strread = new BinaryReader(fs);
        Int16 flag = 1;
        byte[] flagBytes = BitConverter.GetBytes(flag);
        if (!BitConverter.IsLittleEndian)
        {
            flagBytes.Reverse();
        }
        byte[] frameBytes = new byte[fs.Length];
        //读取文件内容并保存在字节数组中
        strread.Read(frameBytes, 0, frameBytes.Length);       
        
        Int32 length = (Int32)frameBytes.Length;
        byte[] lenthBytes = BitConverter.GetBytes(length);

        frameBytes = flagBytes.Concat(frameBytes).ToArray();//,两字节不够，改成四季皆
 
        if (!BitConverter.IsLittleEndian)
        {
            lenthBytes.Reverse();
        }
        //长度4字节(图片frame的长度) + 2字节flag + framebyte
        byte[] sendBytes = lenthBytes.Concat(frameBytes).ToArray();
        return sendBytes;
    }
    int num = 0;
    public void SendFrame()
    {
        send("D:\\CameraControl\\Assets\\sampleViews\\"+num+".jpg", 1);
        num++;
    }
    public void testSendFrame()
    {
      //  Debug.Log("send");
        send("D:\\CameraControl\\Assets\\sampleViews\\9.jpg", 1);
    }
    public void send(string data,int mode)
    {   
        if (!connected) return;
        byte[] sendBytes = new Byte[1024];
        if(mode == 0){
            sendBytes = getTextBytes(data);
            Debug.Log("send to server: " + sendBytes.Length);
        }
        else if(mode == 1){
            //data 为 图片的路径
            sendBytes = getFrameBytes(data);
            Debug.Log("send  to server: " + sendBytes.Length);
        }
        //申请一个新的dataObject，此时其大小由发送的数据大小决定
        DataObject newSend = new DataObject(sendBytes);
        int count = 0;
        //放到消息发送队列里
        lock (writeQueue)
        {
            writeQueue.Enqueue(newSend);
            count = writeQueue.Count;
        }
        if (count == 1)
        {
            stream.BeginWrite(newSend.bytes, newSend.readIndex, newSend.dataLength, SendCallBack, stream);
        }
    }
    //发送成功后调用
    private void SendCallBack(IAsyncResult iar)
    {
        try
        {
            NetworkStream s = (NetworkStream)iar.AsyncState;
            //发送了 lenth，所以发送缓冲区偏移量往后移 lenth
            s.EndWrite(iar);
            DataObject bo;
            lock (writeQueue)//避免异步线程同时访问，用到队列的时候都给他锁起来
            {
                bo = writeQueue.First();
            }
            //bo.readIndex += lenth;
            int count = 0;
            //数据发完了
            //if (bo.dataLength == 0){
                lock (writeQueue){
                    //当前的出队
                    writeQueue.Dequeue();
                    //发送队列里还有多少条消息没发
                    count = writeQueue.Count();
                }
            //}
            //发现发送队列里面还有待发送的消息
            if (count > 0)
            {
                bo = writeQueue.First();//取出第一个
                stream.BeginWrite(bo.bytes, bo.readIndex, bo.dataLength, SendCallBack, stream);
            }
            else if (!connected)
            {
               stream.Close();
            }

        }
        catch (SocketException se)
        {
            Debug.Log("发送失败："+ se);
        }
    }









    private string[] statemMark = {"","<info>","<info>","<ppt>","<bb>","","" };



    private byte[] fssize;
    void SendFrameMessage(){

       /* sendMessage = false;
        imageName = "/final_frame4" + videoController.picture_num.ToString() + ".jpg"; 
        Debug.Log("send imag:"+ imageName);
        videoController.picture_num++;
        FileStream fs = new FileStream(imageFolder+imageName, FileMode.OpenOrCreate, FileAccess.Read);
        fssize = new byte[fs.Length];
        BinaryReader strread = new BinaryReader(fs);
        //读取文件内容并保存在字节数组中
        strread.Read(fssize, 0, fssize.Length);*/
       // fssize = vc.bytes;
        Debug.Log(fssize.Length);
        stream.Write(fssize, 0, fssize.Length);
       

    }
    bool ifchangestate = false;
    string totalReadString = "";
    void OnDestroy()
    {
        Debug.Log("CLOSE BY CLIENT");
      
       // stream.Flush();  
        stream.Close();
      //  stream.Dispose();
        client.Close();
       /* t.Abort();//调用Thread.Abort方法试图强制终止thread线程  

        //上面调用Thread.Abort方法后线程thread不一定马上就被终止了，所以我们在这里写了个循环来做检查，看线程thread是否已经真正停止。其实也可以在这里使用Thread.Join方法来等待线程thread终止，Thread.Join方法做的事情和我们在这里写的循环效果是一样的，都是阻塞主线程直到thread线程终止为止  
        while (t.ThreadState != ThreadState.Aborted)
        {
            //当调用Abort方法后，如果thread线程的状态不为Aborted，主线程就一直在这里做循环，直到thread线程的状态变为Aborted为止  
            Thread.Sleep(100);
        }*/
    }
}
