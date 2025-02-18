using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System;
using System.Linq;

public class Connect2Python : MonoBehaviour{
    
    private string serverIP ;
    private int serverPort;
    public DataObject readDataObject;
    TcpClient client;
    NetworkStream stream;// �����������ںͷ�����ͨ��  
    public bool connected;    
    public List<string> responseList;
    public int responseListCount;
    public TMPro.TMP_Text score;
    public UnityEngine.UI.Image scoreImage;
    public static Connect2Python instance;
    private void Awake()
    {
        instance = this;
    }
    public void Init()
    {
        serverIP = "127.0.0.1";
        serverPort = 25001;
        connected = false;
        responseListCount = 0;
        readDataObject = new DataObject();
        responseList = new List<string>();
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
            Debug.Log("����ʧ��:"+se);
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
            float[] ans = new float[SampleThroughWay.instance.sampleSolveSize * SampleThroughWay.instance.sampleSolveSize];
            //��response������һ��������ans������

            //
            if (SampleThroughWay.instance!=null && SampleThroughWay.instance.forScore.Count > 0)
            {
                int i = 0;
                foreach (ForPicture t in SampleThroughWay.instance.forScore[0])
                {

                    var element = SampleThroughWay.instance.spbl[t.bunch].spl[t.samplePoint].views[t.view];
                    element.score = ans[i++];
                    SampleThroughWay.instance.spbl[t.bunch].spl[t.samplePoint].views[t.view] = element;
                }
                SampleThroughWay.instance.forScore.RemoveAt(0); 
            }
        }
        //�ٴε��ã����ճ������
        HandleReceiveData();
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
    //���ݲ��ֵĳ���4�ֽ� +  2�ֽ�flag(0/1/2)��������֣���ͼƬ���֣�ͼƬ+����  + 0 ��������/1 ͼƬ����/2 �����ֲ��ֵĳ���2�ֽ� + �������� + ͼƬ���ֵĳ���4�ֽ� + ͼƬ���ݣ� 

    private Queue<DataObject> writeQueue = new Queue<DataObject>();
    private Byte[] GetTextBytes(string text)//
    {

        byte[] flagBytes = BitConverter.GetBytes(0);
        if (!BitConverter.IsLittleEndian)
        {
            flagBytes.Reverse();
        }
        //����text,ǰ���Ǳ��
        byte[] textBytes = Encoding.UTF8.GetBytes(text);
       

        Int32 length = (Int32)textBytes.Length;
        byte[] lenthBytes = BitConverter.GetBytes(length); 
        if (!BitConverter.IsLittleEndian)
        {
            lenthBytes.Reverse();
        }        

        textBytes = flagBytes.Concat(textBytes).ToArray();
        byte[] sendBytes = lenthBytes.Concat(textBytes).ToArray();
        return sendBytes;
    }
    private Byte[] GetFrameBytes(string framePath)
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
        if (!BitConverter.IsLittleEndian)
        {
            lenthBytes.Reverse();
        }

        frameBytes = flagBytes.Concat(frameBytes).ToArray();//,���ֽڲ������ĳ��ļ���
        //����4�ֽ�(ͼƬframe�ĳ���) + 2�ֽ�flag + framebyte
        byte[] sendBytes = lenthBytes.Concat(frameBytes).ToArray();
        return sendBytes;
    }
    int num = 0;
    public void SendFrame()
    {
        Send("D:\\CameraControl\\Assets\\sampleViews\\" + num + ".jpg", 1);
        num++;
    }
    //���ݲ��ֵĳ���4�ֽ� +  2�ֽ�flag(0/1/2)��������֣���ͼƬ���֣�ͼƬ+����  + 0 ��������/1 ͼƬ����/2 �����ֲ��ֵĳ���2�ֽ� + �������� + ͼƬ���ֵĳ���4�ֽ� + ͼƬ���ݣ� 


    public byte[] GetTextAndFrameByte()
    {
        //flag 2��
        byte[] flagBytes = BitConverter.GetBytes(2);
        if (!BitConverter.IsLittleEndian)
        {
            flagBytes.Reverse();
        }
        //ͼƬ����+ͼƬ����
        byte[] frameBytes = ViewManager.GetByte(scoreImage.sprite);
        //frame�ĳ��� 4��
        Int32 framelength = (Int32)frameBytes.Length;
        byte[] framelenthBytes = BitConverter.GetBytes(framelength);
        if (!BitConverter.IsLittleEndian)
        {
            framelenthBytes.Reverse();
        }
        frameBytes = framelenthBytes.Concat(frameBytes).ToArray();



        //text����+ text����
        byte[] textBytes = Encoding.UTF8.GetBytes(score.text);
        //text�ĳ��� 2��
        Int16 textlength = (Int16)textBytes.Length;
        byte[] textlenthBytes = BitConverter.GetBytes(textlength);
        if (!BitConverter.IsLittleEndian)
        {
            textlenthBytes.Reverse();
        }

        textBytes = textlenthBytes.Concat(textBytes).ToArray();

        byte[] allLengthBytes= BitConverter.GetBytes(textlength+2+framelength+4);
        if (!BitConverter.IsLittleEndian)
        {
            allLengthBytes.Reverse();
        }
        //�����ݳ���4�ֽ�() + 2�ֽ�flag + ����
        byte[] sendBytes = allLengthBytes.Concat(flagBytes).ToArray().Concat(textBytes).ToArray().Concat(frameBytes).ToArray();
        return sendBytes;
    }
    public void Send()
    {
        //���أ��������û���llm���������ֵ���Ƭ���Ľ�llm���ֱ�׼
        if (!connected) return;
        byte[] sendBytes = new Byte[1024];
        sendBytes = GetTextAndFrameByte();
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
    public void Send(string data,int mode)
    {   
        if (!connected) return;
        byte[] sendBytes = new Byte[1024];
        if(mode == 0){
            sendBytes = GetTextBytes(data);
            Debug.Log("send to server: " + sendBytes.Length);
        }
        else if(mode == 1){
            //data Ϊ ͼƬ��·��
            sendBytes = GetFrameBytes(data);
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
    void OnDestroy()
    {
        Debug.Log("CLOSE BY CLIENT");

        if (connected)
        {
           // stream.Flush();  
            stream.Close();
          //  stream.Dispose();
            client.Close();
        }

       /* t.Abort();//����Thread.Abort������ͼǿ����ֹthread�߳�  

        //�������Thread.Abort�������߳�thread��һ�����Ͼͱ���ֹ�ˣ���������������д�˸�ѭ��������飬���߳�thread�Ƿ��Ѿ�����ֹͣ����ʵҲ����������ʹ��Thread.Join�������ȴ��߳�thread��ֹ��Thread.Join�����������������������д��ѭ��Ч����һ���ģ������������߳�ֱ��thread�߳���ֹΪֹ  
        while (t.ThreadState != ThreadState.Aborted)
        {
            //������Abort���������thread�̵߳�״̬��ΪAborted�����߳̾�һֱ��������ѭ����ֱ��thread�̵߳�״̬��ΪAbortedΪֹ  
            Thread.Sleep(100);
        }*/
    }
}
