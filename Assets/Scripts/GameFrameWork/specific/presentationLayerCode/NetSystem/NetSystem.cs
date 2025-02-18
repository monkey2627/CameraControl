using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Linq;

public class NetSystem : INetSystem
{
    private Socket socket;
    //用来接收的缓冲区
    private ByteObject readBuff;
    //每次send会创建一个新的 send ByteOBject
    private Queue<ByteObject> writeQueue; 
    private bool isClosing = false;
    private bool isConnecting = false;
    //网络模块的事件系统
    public delegate void PTListener(PTBase pt);
    private Dictionary<string,PTListener > ptListenerDict = new Dictionary<string, PTListener>();
    private List<PTBase> ptList = new List<PTBase>();
    private int ptListCount = 0;
    private const int maxPTUpdateNum = 10;
    //心跳机制
    private bool usePingPang;
    private int pingPangTime = 30;
    private float lastPingTime, lastPangTime;
    #region
    public void RegistPTListenr(string PTname,PTListener listener)
    {
        if (ptListenerDict.ContainsKey(PTname))
        {
            ptListenerDict[PTname] += listener;
        }
        else
        {
            ptListenerDict[PTname] = listener;
        }
    }
    public void UnregistPTListenr(string PTname, PTListener listener)
    {
        if (ptListenerDict.ContainsKey(PTname))
        {
            ptListenerDict[PTname] -= listener;
        }

    }
    public void SendPTEvent(string PTname, PTBase pt) {
        if (ptListenerDict.ContainsKey(PTname))
        {
            ptListenerDict[PTname](pt);
        }
    }
#endregion
    public void Close()
    {
        if(writeQueue.Count >0)
        {
            isClosing = true;
        }
        else
        {
            socket.Close();
        }
       
    }
    private void InitNetSystemState()
    {
       socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
       socket.NoDelay = true;
       //用来接收的缓冲区
       readBuff = new ByteObject();
    //每次send会创建一个新的 send ByteOBject
       writeQueue = new Queue<ByteObject>();
       isClosing = false;
       usePingPang = true;
       lastPangTime = lastPingTime = Time.time;
       RegistPTListenr("PTPang", OnPTPang);

        this.RegistEvent<OnConnectSucceedEvent>(OnConnectSucceed);
        this.RegistEvent<OnConnectFailEvent>(OnConnectFail);
    }

    public void Connect(string ip, int port)
    {
        if(socket != null && socket.Connected)
        {
            return;
        }
        if (isConnecting)
        {
            return;
        }
        InitNetSystemState();
        isConnecting = true;
        socket.BeginConnect(ip, port,ConnectCallBack,socket);
        //目标ip和端口,自己用什么端口每次都不一样，是系统自己的
    }
    private void ConnectCallBack(IAsyncResult iar)
    {
        try
        {
            Socket socket = (Socket)iar.AsyncState;
            socket.EndConnect(iar);//结束这个异步的方法
            Receive();
            this.SendEvent<OnConnectSucceedEvent>();
            isConnecting = false;
        }
        catch (SocketException se)
        {
            Debug.Log(se);
            this.SendEvent<OnConnectFailEvent>();
        }
    }

    public void Init()
    {
    }
    
    public void Send(PTBase msg)
    {
        if (isClosing) return;

        byte[] nameBytes = PT.EncodeName(msg);//name长度 + name本体
        byte[] bodyBytes = PT.EncodeBody(msg);//body本体
        byte[] ptBytes = nameBytes.Concat(bodyBytes).ToArray();
       
        Int16 length = (Int16)ptBytes.Length;
        Debug.Log(length);
        byte[] lenthBytes = BitConverter.GetBytes(length);
        if (!BitConverter.IsLittleEndian)
        {
            lenthBytes.Reverse();
        } 
        //总长度 + name 长度 +name+ 本体
        byte[] sendBytes = lenthBytes.Concat(ptBytes).ToArray();
        //申请一个新的bo
        ByteObject bo = new ByteObject(sendBytes);
        int count = 0;
        lock (writeQueue) { 
       // readIndex = 0;
           writeQueue.Enqueue(bo);
           count = writeQueue.Count;
        }
        if(count == 1){
             socket.BeginSend(bo.bytes, bo.readIndex, bo.dataLength, SocketFlags.None, SendCallBack, socket); 
        }
       

    }
    //发送成功后调用
    private void SendCallBack(IAsyncResult iar)
    {
        try
        {
            Socket socket = (Socket)iar.AsyncState;  
            //发送了 lenth，所以发送缓冲区偏移量往后移 lenth
            int lenth = socket.EndSend(iar);              
            ByteObject bo;
            
            lock (writeQueue)//避免异步线程同时访问，用到队列的时候都给他锁起来
            {
                bo = writeQueue.First();
            } 
            bo.readIndex += lenth;
            int count = 0;
            if(bo.dataLength == 0) {
                lock (writeQueue)
                {
                    writeQueue.Dequeue();   
                    count = writeQueue.Count();  
                }     


            }
            if(count > 0){
             //  bo = writeQueue.First();
               socket.BeginSend(bo.bytes, bo.readIndex, bo.dataLength, SocketFlags.None, SendCallBack, socket);
            }else if (isClosing)
            {
                socket.Close();
            }
           
        }
        catch (SocketException se)
        {
            Debug.Log("发送失败" + se);
        }
    }       
    private void ReceiveCallBack(IAsyncResult iar)
    {
        try
        {
            Debug.Log("收到消息");
            Socket socket = (Socket)iar.AsyncState;
            int count = socket.EndReceive(iar);
            readBuff.writeIndex += count;
            HandleReceiveData();
            if (readBuff.remainLength < 8)
            {
                readBuff.MoveBytes();
                readBuff.Resize(readBuff.dataLength * 2);
            }
            Receive();
            
        }
        catch (SocketException se)
        {
            Debug.Log("接收失败:"+se);
        }
    }
    private void HandleReceiveData(){
        Debug.Log(readBuff.dataLength);
        if (readBuff.dataLength <= 2){
            return;
        }
        //是用小端存的，这是计算方式,bodylenth是不包含前面两个字节
        Int16 bodyLenth = (Int16)(readBuff.bytes[readBuff.readIndex] | readBuff.bytes[readBuff.readIndex + 1] << 8);
        //解决粘包
        if(readBuff.dataLength < bodyLenth + 2)
        {
            return;
        }
        //读了总长度
        readBuff.readIndex += 2;
        
        int nameCount = 0;
        //解析协议名
        Debug.Log("开始解析协议名");
        string protoName = PT.DecodeName(readBuff.bytes, readBuff.readIndex,out nameCount);
        if (protoName == "")
        {
            Debug.Log("协议解析失败");
            return;
        }
        readBuff.readIndex += nameCount;
        //解析协议体
        int bodyCount = bodyLenth - nameCount;
        PTBase ptBase = PT.DecodeBody(protoName, readBuff.bytes,readBuff.readIndex,bodyCount);     
        readBuff.readIndex += bodyCount;
        readBuff.CheckAndMoveBytes();
        Debug.Log("收到协议:"+protoName+" \n"+ JsonUtility.ToJson(ptBase));
        lock (ptList)
        {
            ptList.Add(ptBase);
            ptListCount++;
        }
        HandleReceiveData();
    }
    private void UpdatePT()
    {
        if (ptListCount <= 0) return;
        for (int i = 0; i < maxPTUpdateNum; i++)
        {
            PTBase ptBase = null;
            lock (ptList)
            {
                if (ptList.Count > 0)
                {
                    ptBase = ptList[0];
                    ptList.RemoveAt(0);
                    ptListCount--;
                }
            }
            if(ptBase != null)
            {
                SendPTEvent(ptBase.ptName, ptBase);
            }
            else
            {
                break;
            }
        }
    }
    private void UpdatePP()
    {
        if (!usePingPang) return;
        //客户端向服务器ping协议
        if(Time.time-lastPingTime > pingPangTime)
        {
            Send(new PTPing());
            lastPingTime = Time.time;
        }
        //客户端检测是否收到pong
        if(Time.time - lastPangTime > pingPangTime * 4)
        {
            Close();
        }
    }
    //收到协议的时候触发的方法
    private void OnPTPang(PTBase pt)
    {
        lastPangTime = Time.time;
    }
    public void Update()
    {
        UpdatePT();
    }
    //异步接收
    private void Receive()
    {
 
        socket.BeginReceive(readBuff.bytes, readBuff.writeIndex,readBuff.remainLength, SocketFlags.None, ReceiveCallBack, socket);
        
    }
    private void OnConnectSucceed(object obj)
    {
        Debug.Log("客户端连接成功");
    }
    private void OnConnectFail(object obj)
    {
        Debug.Log("客户端连接失败");
    }
}
