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
    NetworkStream stream;// �����������ںͷ�����ͨ��  
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
        client = new TcpClient(serverIP, serverPort);//tell client server's inf,�ͷ������˽�������
        stream = client.GetStream();
        connected = true;
        Debug.Log("�ɹ����ӵ�������");
        stream.BeginRead(readDataObject.bytes, readDataObject.writeIndex, readDataObject.remainLength, ReceiveCallBack, stream);
    }
    
    private void ReceiveCallBack(IAsyncResult iar)
    {
        try
        {
            Debug.Log("�յ���Ϣ");
            NetworkStream stream = (NetworkStream)iar.AsyncState;
            //������ζ��˶����ֽڵ�����
            int count = stream.EndRead(iar);
            Debug.Log("receiveall: " + count);
            readDataObject.writeIndex += count;
            HandleReceiveData();
            //ʣ��ռ䲻���ˣ��ƶ�λ��˳������
            if (readDataObject.remainLength < 8)
            {
                readDataObject.MoveBytes();
                readDataObject.Resize(readDataObject.dataLength * 2);
            }
            Debug.Log("BeginRead");
            //������ʼ�첽������Ϣ
            stream.BeginRead(readDataObject.bytes, readDataObject.writeIndex, readDataObject.remainLength, ReceiveCallBack, stream);
        }
        catch (SocketException se)
        {
            Debug.Log("����ʧ��");
        }
    }
    int test = 0;
    private void HandleReceiveData()
    {
       Debug.Log("      HandleReceiveData");
       if (readDataObject.dataLength <= 4){
            return;
        }
        //����С�˴�ģ����Ǽ��㷽ʽ,bodylenth�ǲ�����ǰ�������ֽ�
        Int32 bodyLenth = (Int32)(readDataObject.bytes[readDataObject.readIndex] | readDataObject.bytes[readDataObject.readIndex + 1] << 8 | readDataObject.bytes[readDataObject.readIndex+2] << 16| readDataObject.bytes[readDataObject.readIndex + 3] << 24);
        Debug.Log("      receive: " + bodyLenth);
        //����ְ����⣬��ʱ���ݻ�û��ȫ��������
        if (readDataObject.dataLength < bodyLenth + 4)
        {
            return;
        }
        //�����ܳ���,readIndex + 4
        readDataObject.readIndex += 4;
        //������һ�δ������Ļ�
        Int16 flag = (Int16)(readDataObject.bytes[readDataObject.readIndex] | readDataObject.bytes[readDataObject.readIndex + 1] << 8);
        readDataObject.readIndex += 2;
        Debug.Log("      flag is: " + flag);
        //ע�⣬python������������Ϊlength + flag + text
        //lengthΪ flag + text�ĳ��ȣ�����Ҫ��ȥ2��flag��
        string response = Encoding.UTF8.GetString(readDataObject.bytes, readDataObject.readIndex, bodyLenth-2);
        readDataObject.readIndex += (bodyLenth-2);
        readDataObject.CheckAndMoveBytes();
        Debug.Log("      �յ���Ϣ:" +test +"   " + response);
        test = test + 1;
        if(flag == 0)
        {
            float[] ans = new float[stw.sampleSolveSize * stw.sampleSolveSize];
            //��response������һ��������ans������

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
        //�ٴε��ã����ճ������
        HandleReceiveData();
    }

    //���send Message button�ᴥ�����
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
        //����text,ǰ���Ǳ��
        byte[] textBytes = Encoding.UTF8.GetBytes("text" + text);
        Int16 length = (Int16)textBytes.Length;
        byte[] lenthBytes = BitConverter.GetBytes(length);
        Debug.Log(length);
        if (!BitConverter.IsLittleEndian)
        {
            lenthBytes.Reverse();
        }
        //�ܳ��� + name ���� +name+ ����
        byte[] sendBytes = lenthBytes.Concat(textBytes).ToArray();
        //����һ���µ�dataObject����ʱ���С�ɷ��͵����ݴ�С����
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
        //��ȡ�ļ����ݲ��������ֽ�������
        strread.Read(frameBytes, 0, frameBytes.Length);       
        
        Int32 length = (Int32)frameBytes.Length;
        byte[] lenthBytes = BitConverter.GetBytes(length);

        frameBytes = flagBytes.Concat(frameBytes).ToArray();//,���ֽڲ������ĳ��ļ���
 
        if (!BitConverter.IsLittleEndian)
        {
            lenthBytes.Reverse();
        }
        //����4�ֽ�(ͼƬframe�ĳ���) + 2�ֽ�flag + framebyte
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
            //data Ϊ ͼƬ��·��
            sendBytes = getFrameBytes(data);
            Debug.Log("send  to server: " + sendBytes.Length);
        }
        //����һ���µ�dataObject����ʱ���С�ɷ��͵����ݴ�С����
        DataObject newSend = new DataObject(sendBytes);
        int count = 0;
        //�ŵ���Ϣ���Ͷ�����
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
    //���ͳɹ������
    private void SendCallBack(IAsyncResult iar)
    {
        try
        {
            NetworkStream s = (NetworkStream)iar.AsyncState;
            //������ lenth�����Է��ͻ�����ƫ���������� lenth
            s.EndWrite(iar);
            DataObject bo;
            lock (writeQueue)//�����첽�߳�ͬʱ���ʣ��õ����е�ʱ�򶼸���������
            {
                bo = writeQueue.First();
            }
            //bo.readIndex += lenth;
            int count = 0;
            //���ݷ�����
            //if (bo.dataLength == 0){
                lock (writeQueue){
                    //��ǰ�ĳ���
                    writeQueue.Dequeue();
                    //���Ͷ����ﻹ�ж�������Ϣû��
                    count = writeQueue.Count();
                }
            //}
            //���ַ��Ͷ������滹�д����͵���Ϣ
            if (count > 0)
            {
                bo = writeQueue.First();//ȡ����һ��
                stream.BeginWrite(bo.bytes, bo.readIndex, bo.dataLength, SendCallBack, stream);
            }
            else if (!connected)
            {
               stream.Close();
            }

        }
        catch (SocketException se)
        {
            Debug.Log("����ʧ�ܣ�"+ se);
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
        //��ȡ�ļ����ݲ��������ֽ�������
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
       /* t.Abort();//����Thread.Abort������ͼǿ����ֹthread�߳�  

        //�������Thread.Abort�������߳�thread��һ�����Ͼͱ���ֹ�ˣ���������������д�˸�ѭ��������飬���߳�thread�Ƿ��Ѿ�����ֹͣ����ʵҲ����������ʹ��Thread.Join�������ȴ��߳�thread��ֹ��Thread.Join�����������������������д��ѭ��Ч����һ���ģ������������߳�ֱ��thread�߳���ֹΪֹ  
        while (t.ThreadState != ThreadState.Aborted)
        {
            //������Abort���������thread�̵߳�״̬��ΪAborted�����߳̾�һֱ��������ѭ����ֱ��thread�̵߳�״̬��ΪAbortedΪֹ  
            Thread.Sleep(100);
        }*/
    }
}
