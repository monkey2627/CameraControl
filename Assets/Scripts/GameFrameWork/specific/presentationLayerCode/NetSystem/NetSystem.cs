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
    //�������յĻ�����
    private ByteObject readBuff;
    //ÿ��send�ᴴ��һ���µ� send ByteOBject
    private Queue<ByteObject> writeQueue; 
    private bool isClosing = false;
    private bool isConnecting = false;
    //����ģ����¼�ϵͳ
    public delegate void PTListener(PTBase pt);
    private Dictionary<string,PTListener > ptListenerDict = new Dictionary<string, PTListener>();
    private List<PTBase> ptList = new List<PTBase>();
    private int ptListCount = 0;
    private const int maxPTUpdateNum = 10;
    //��������
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
       //�������յĻ�����
       readBuff = new ByteObject();
    //ÿ��send�ᴴ��һ���µ� send ByteOBject
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
        //Ŀ��ip�Ͷ˿�,�Լ���ʲô�˿�ÿ�ζ���һ������ϵͳ�Լ���
    }
    private void ConnectCallBack(IAsyncResult iar)
    {
        try
        {
            Socket socket = (Socket)iar.AsyncState;
            socket.EndConnect(iar);//��������첽�ķ���
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

        byte[] nameBytes = PT.EncodeName(msg);//name���� + name����
        byte[] bodyBytes = PT.EncodeBody(msg);//body����
        byte[] ptBytes = nameBytes.Concat(bodyBytes).ToArray();
       
        Int16 length = (Int16)ptBytes.Length;
        Debug.Log(length);
        byte[] lenthBytes = BitConverter.GetBytes(length);
        if (!BitConverter.IsLittleEndian)
        {
            lenthBytes.Reverse();
        } 
        //�ܳ��� + name ���� +name+ ����
        byte[] sendBytes = lenthBytes.Concat(ptBytes).ToArray();
        //����һ���µ�bo
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
    //���ͳɹ������
    private void SendCallBack(IAsyncResult iar)
    {
        try
        {
            Socket socket = (Socket)iar.AsyncState;  
            //������ lenth�����Է��ͻ�����ƫ���������� lenth
            int lenth = socket.EndSend(iar);              
            ByteObject bo;
            
            lock (writeQueue)//�����첽�߳�ͬʱ���ʣ��õ����е�ʱ�򶼸���������
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
            Debug.Log("����ʧ��" + se);
        }
    }       
    private void ReceiveCallBack(IAsyncResult iar)
    {
        try
        {
            Debug.Log("�յ���Ϣ");
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
            Debug.Log("����ʧ��:"+se);
        }
    }
    private void HandleReceiveData(){
        Debug.Log(readBuff.dataLength);
        if (readBuff.dataLength <= 2){
            return;
        }
        //����С�˴�ģ����Ǽ��㷽ʽ,bodylenth�ǲ�����ǰ�������ֽ�
        Int16 bodyLenth = (Int16)(readBuff.bytes[readBuff.readIndex] | readBuff.bytes[readBuff.readIndex + 1] << 8);
        //���ճ��
        if(readBuff.dataLength < bodyLenth + 2)
        {
            return;
        }
        //�����ܳ���
        readBuff.readIndex += 2;
        
        int nameCount = 0;
        //����Э����
        Debug.Log("��ʼ����Э����");
        string protoName = PT.DecodeName(readBuff.bytes, readBuff.readIndex,out nameCount);
        if (protoName == "")
        {
            Debug.Log("Э�����ʧ��");
            return;
        }
        readBuff.readIndex += nameCount;
        //����Э����
        int bodyCount = bodyLenth - nameCount;
        PTBase ptBase = PT.DecodeBody(protoName, readBuff.bytes,readBuff.readIndex,bodyCount);     
        readBuff.readIndex += bodyCount;
        readBuff.CheckAndMoveBytes();
        Debug.Log("�յ�Э��:"+protoName+" \n"+ JsonUtility.ToJson(ptBase));
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
        //�ͻ����������pingЭ��
        if(Time.time-lastPingTime > pingPangTime)
        {
            Send(new PTPing());
            lastPingTime = Time.time;
        }
        //�ͻ��˼���Ƿ��յ�pong
        if(Time.time - lastPangTime > pingPangTime * 4)
        {
            Close();
        }
    }
    //�յ�Э���ʱ�򴥷��ķ���
    private void OnPTPang(PTBase pt)
    {
        lastPangTime = Time.time;
    }
    public void Update()
    {
        UpdatePT();
    }
    //�첽����
    private void Receive()
    {
 
        socket.BeginReceive(readBuff.bytes, readBuff.writeIndex,readBuff.remainLength, SocketFlags.None, ReceiveCallBack, socket);
        
    }
    private void OnConnectSucceed(object obj)
    {
        Debug.Log("�ͻ������ӳɹ�");
    }
    private void OnConnectFail(object obj)
    {
        Debug.Log("�ͻ�������ʧ��");
    }
}
